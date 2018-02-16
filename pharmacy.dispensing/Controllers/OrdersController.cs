using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using System.Diagnostics;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Pharmacy.Dispensing.Models;
using Pharmacy.Models;
using Pharmacy.Models.Pocos;
using Pharmacy.Repositories.Interfaces;
using SendGrid;
using SendGrid.Helpers.Mail;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Protocols;

namespace Pharmacy.Dispensing.Controllers
{
    [Authorize]
    public class OrdersController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly string _apiKey;

        public OrdersController(IUnitOfWork unitOfWork, IMapper mapper ,IOptions<ServiceSettings> serviceSettings)
        {
            _unitOfWork = unitOfWork;
            _apiKey = serviceSettings.Value.SendGridApiKey;
            _mapper = mapper;
        }

        //
        // GET: /Order/

        public ViewResult CreateOrder()
        {

            return View("Index");
        }

        public async Task<IActionResult> AddScriptToCollect()
        {
            var model = new AddScriptModel();

            var shops = (from s in await _unitOfWork.ShopRepository.Get() select new { Id = s.ShopId, Name = s.ShopName  }).ToList();
            model.Shops = shops.Select(a => new SelectListItem { Value = a.Id.ToString(), Text = a.Name }).ToList();

            var titles = (from s in await _unitOfWork.TitleRepository.Get() select new { Id = s.TitleId, Name = s.TitleName }).ToList();
            model.Titles = titles.Select(a => new SelectListItem { Value = a.Id.ToString(), Text = a.Name }).ToList();

            var doctors = (from s in await _unitOfWork.DoctorRepository.Get() orderby s.Surname select new { Id = s.DoctorId, Name = "Dr." + s.Surname }).ToList();
            model.Doctors = doctors.Select(a => new SelectListItem { Value = a.Id.ToString(), Text = a.Name }).ToList();

            // TODO: get the users in the specificied role
            /*var drivers = Roles.GetUsersInRole("Driver");
            var driverList = new List<SelectListItem>();
            foreach (var driver in drivers)  {
                driverList.Add(new SelectListItem() { Value = driver, Text = driver });
            }
            model.Drivers = driverList;
            */

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> AddScriptToCollect(AddScriptModel model)
        {
            try
            {
                var script = new CollectScript();
                script.OrderId = Guid.NewGuid();
                script.Customer = model.Script.Customer;
                script.CustomerAddress = model.Script.CustomerAddress;
                script.OrderDate = DateTime.Now;
                script.NumItems = model.Script.NumItems;
                script.Notes = model.Script.Notes;
                script.DoctorId = model.Script.DoctorId;
                script.ShopId = model.Script.ShopId;

                _unitOfWork.CollectScriptRepository.Insert(script);
                await _unitOfWork.SaveAsync();

                if (model.SendAlert)
                    if (!SendAlertToDriver(model.Driver, script).Result)
                        TempData["senderror"] = "Script has been added to system but alert has not been sent to driver.";

                return RedirectToAction("Collections");
            }
            catch(Exception ex) 
            {
                ModelState.AddModelError("", ex.Message);
            }

            var shops = (from s in _unitOfWork.ShopRepository.Get().Result select new { Id = s.ShopId, Name = s.ShopName }).ToList();
            model.Shops = shops.Select(a => new SelectListItem { Value = a.Id.ToString(), Text = a.Name, Selected = model.Script.ShopId == a.Id ? true : false }).ToList();

            var titles = (from s in _unitOfWork.TitleRepository.Get().Result select new { Id = s.TitleId, Name = s.TitleName }).ToList();
            model.Titles = titles.Select(a => new SelectListItem { Value = a.Id.ToString(), Text = a.Name, Selected = model.TitleId == a.Id ? true : false }).ToList();

            var doctors = (from s in _unitOfWork.DoctorRepository.Get().Result orderby s.Surname select new { Id = s.DoctorId, Name = "Dr." + s.Surname }).ToList();
            model.Doctors = doctors.Select(a => new SelectListItem { Value = a.Id.ToString(), Text = a.Name, Selected = model.Script.DoctorId == a.Id ? true : false }).ToList();
            return View(model);
        }

        public async Task<IActionResult> Index(int? status)
        {
            
            if (status == null)
            {
                // Default to ordered status
                status = 2;
            }
            var titles = _unitOfWork.TitleRepository.Get().Result;
            var orders = from o in await _unitOfWork.OrderRepository.Get()
                         join c in await _unitOfWork.CustomerRepository.Get() on o.CustomerId equals c.CustomerId
                         join s in await _unitOfWork.ShopRepository.Get() on c.ShopId equals s.ShopId
                         join t in titles on c.TitleId equals t.TitleId
                         join a in await _unitOfWork.AddressRepository.Get() on c.AddressId equals a.AddressId
                         join d in await _unitOfWork.DoctorRepository.Get() on c.DoctorId equals d.DoctorId
                         join p in await  _unitOfWork.PracticeRepository.Get() on d.PracticeId equals p.PracticeId
                         where o.OrderStatus == status
                         select new ViewOrder()
                         {
                            Order = new Models.OrderPoco() {
                                OrderId = o.OrderId,
                                OrderDate = o.OrderDate,
                                OrderStatus = o.OrderStatus,
                                OrderLines = from ol in _unitOfWork.OrderLineRepository.Get(ol => ol.OrderId == o.OrderId).Result
                                              join od in _unitOfWork.DrugRepository.Get().Result on ol.DrugId equals od.DrugId
                                              select new OrderLinePoco()
                                              {
                                                  DrugName = od.DrugName,
                                                  Qty = ol.Qty
                                              }
                            },
                            StatusChangedDate = (from os in _unitOfWork.OrderStatusRepository.Get(os => os.OrderId == o.OrderId).Result
                                orderby os.StatusSetDate select os).FirstOrDefault().StatusSetDate,
                            Customer = new CustomerPoco()
                            {
                                 CustomerId = c.CustomerId,
                                 UserId = c.UserId,
                                 Email = c.Email,
                                 Title = t,
                                 Firstname = c.Firstname,
                                 Lastname = c.Lastname,
                                 Sex = c.Sex,
                                 Mobile = c.Mobile,
                                 Home = c.Home,
                                 Dob = c.Dob,
                                 Address = a,
                                 Doctor = new DoctorPoco {
                                     Title = titles.FirstOrDefault(tt => tt.TitleId == d.TitleId),
                                     Surname = d.Surname,
                                     Practice = p
                                 },
                                 Shop = s
                             }
                         };
            return View(orders);
        }

        private async Task<bool> SendAlertToDriver(string drivername, CollectScript script)
        {
            var driver = User.Identity.Name;

            var practice = (from p in await _unitOfWork.PracticeRepository.Get()
                                join d in await _unitOfWork.DoctorRepository.Get() on p.PracticeId equals d.PracticeId
                                join a in await _unitOfWork.AddressRepository.Get() on p.AddressId equals a.AddressId
                                where d.DoctorId == script.DoctorId
                                select p).FirstOrDefault();

            var address = await _unitOfWork.AddressRepository.GetByID(practice.AddressId);

            var shop = (from s in await _unitOfWork.ShopRepository.Get()
                        where s.ShopId == script.ShopId select s).First();

            var body = new StringBuilder();
            body.AppendFormat("Script for {0} of {1} containing {2}",script.Customer, script.CustomerAddress, script.NumItems);
            body.AppendLine("===================================");
            body.AppendFormat("Leave in {0}", shop.ShopName);
            body.AppendLine("===================================");
            body.AppendLine("Notes:");
            body.AppendLine(script.Notes);

            var client = new SendGridClient(_apiKey);
            var from = new EmailAddress("prescriptions@mckenziespharmacy.com", "McKenzies Pharmacy");
            var subject = "Pick up script at " + practice.PracticeName + ", " + address.AddressLine1;
            var to = new EmailAddress(driver);
            var plainTextContent = "not implemented";

            var msg = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent, body.ToString());


            await client.SendEmailAsync(msg);
            return true;
        }

        public async Task<IActionResult> Process(Guid id, int status)
        {
             /*MembershipUser user = Membership.GetUser(User.Identity.Name);
             Guid userid = (Guid)user.ProviderUserKey;

            var order = await _unitOfWork.OrderRepository.GetByID(id);
            order.OrderStatus = status;
            _unitOfWork.OrderStatusRepository.Insert(new OrderStatus() {
                OrderId = id,
                OrderStatusId = Guid.NewGuid(),
                Status = status,
                UserId = userid,
                StatusSetDate = DateTime.Now
            });*/
            await _unitOfWork.SaveAsync();

            return RedirectToAction("Index", new { status = status });
        }

        [HttpPost]
        public async Task<JsonResult> GetOrderTotals()
        {
            // Get orders added by customer via the website that scripts need collected for
            var allScripts = (from o in await _unitOfWork.OrderRepository.Get()
                              join c in await _unitOfWork.CustomerRepository.Get() on o.CustomerId equals c.CustomerId
                              join d in await _unitOfWork.DoctorRepository.Get() on c.DoctorId equals d.DoctorId
                              select new
                              {
                                  OrderStatus = o.OrderStatus,
                                  Customer = c.Firstname + " " + c.Lastname,
                                  NumItems = _unitOfWork.OrderLineRepository.Get(n => n.OrderId == o.OrderId).Result.Count()
                              }).ToList().Select(x => new CollectScript
                              {
                                  OrderStatus = x.OrderStatus,
                                  Customer = x.Customer,
                                  NumItems = x.NumItems
                              }).ToList();

            // Get scripts that need collected for shops
            var shopScripts = (from col in await _unitOfWork.CollectScriptRepository.Get()
                               join d in await _unitOfWork.DoctorRepository.Get() on col.DoctorId equals d.DoctorId
                               join p in await _unitOfWork.PracticeRepository.Get() on d.PracticeId equals p.PracticeId
                               select col).ToList();


            var totals = new
            {
                ScriptRequested = allScripts.Concat(shopScripts).Where(s => s.OrderStatus == (int)Status.ScriptRequested).Select(x => new { name = x.Customer, items = x.NumItems }).ToList(),
                ScriptOrdered = allScripts.Where(s => s.OrderStatus == (int)Status.ScriptOrdered).Select(x => new { name = x.Customer, items = x.NumItems }).ToList(),
                Collected = allScripts.Where(s => s.OrderStatus == (int)Status.Collected).Select(x => new { name = x.Customer, items = x.NumItems }).ToList(),
                NotCollected = allScripts.Where(s => s.OrderStatus == (int)Status.NotCollected).Select(x => new { name = x.Customer, items = x.NumItems }).ToList(),
                MissingItems = allScripts.Where(s => s.OrderStatus == (int)Status.MissingItems).Select(x => new { name = x.Customer, items = x.NumItems }).ToList()
            };

            return Json(totals);
        }

        [HttpPost]
        public async Task<JsonResult> AddNote(Guid orderId, string noteText)
        {
            

            var note = new Note();
            note.NoteId = Guid.NewGuid();
            note.CreatedOn = DateTime.Now;
            note.Text = noteText;

            var order = await  _unitOfWork.OrderRepository.GetByID(orderId);
            order.NoteId = note.NoteId;

            try
            {
                //_unitOfWork.NoteRepository.Insert(note);
                _unitOfWork.OrderRepository.Update(order);
                await _unitOfWork.SaveAsync();
                return Json(new { status = 1, msg = "" });
            }
            catch (Exception ex)
            {
                return Json(new { status = 0, msg = ex.Message });
            }
        }

        //
        // GET: /Order/Collections

        public async Task<IActionResult> Collections(DateTime? date)
        {
            var model = new CollectionsModel();
            
            var queryDate = date == null ? new DateTime(1900,1,1) : (DateTime)date;

            var scriptsFromAllPractices = new List<CollectScript>();

            var practicesWithScriptToCollect = new List<ScriptCollection>(); 
            foreach (var practice in _unitOfWork.PracticeRepository.Get().Result.OrderBy(p => p.PracticeName).ToList())
            {            
                
                
                // Get orders added by customer via the website that scripts need collected for
                var orderScripts = (from o in await _unitOfWork.OrderRepository.Get()
                                    join c in await _unitOfWork.CustomerRepository.Get() on o.CustomerId equals c.CustomerId
                                                join a in await _unitOfWork.AddressRepository.Get() on c.AddressId equals a.AddressId
                                                join d in await _unitOfWork.DoctorRepository.Get() on c.DoctorId equals d.DoctorId
                                                where d.PracticeId == practice.PracticeId
                                                select new 
                                                {
                                                    OrderId = o.OrderId,
                                                    OrderDate = o.OrderDate,
                                                    Customer = c.Firstname + " " + c.Lastname,
                                                    CustomerAddress = a.AddressLine1 + " " + a.Postcode,
                                                    NumItems = _unitOfWork.OrderLineRepository.Get(os => os.OrderId == o.OrderId).Result.Count()
                                                }).ToList().Select(x => new CollectScript
                                                                    {
                                                                        OrderId = x.OrderId,
                                                                        OrderDate = x.OrderDate,
                                                                        Customer = x.Customer,
                                                                        CustomerAddress = x.CustomerAddress,
                                                                        NumItems = x.NumItems
                                                                    }).ToList();

                // Get scripts that need collected for shops
                var shopScripts = (from col in await _unitOfWork.CollectScriptRepository.Get()
                                   join d in await _unitOfWork.DoctorRepository.Get() on col.DoctorId equals d.DoctorId
                                           join p in await _unitOfWork.PracticeRepository.Get() on d.PracticeId equals p.PracticeId
                                            where p.PracticeId == practice.PracticeId
                                           select col).ToList();

                var allScripts = orderScripts.Concat(shopScripts).OrderByDescending(s => s.OrderDate);

                // Add the customer scripts to the shop scripts
                var dateFilteredScripts = allScripts.Where(s => s.OrderDate >= queryDate);
                if (dateFilteredScripts.Count() > 0)
                {
                    var collection = new ScriptCollection();
                    collection.Practice = practice.PracticeName;
                    collection.Scripts = _mapper.Map<IEnumerable<CollectScriptPoco>>(dateFilteredScripts);
                    practicesWithScriptToCollect.Add(collection);
                }

                // Add all the scripts to a collection so we can get the total uncollected
                var uncollectedScripts = allScripts.Where(s => s.OrderStatus == 0);
                if (uncollectedScripts.Count() > 0)
                    scriptsFromAllPractices.AddRange(uncollectedScripts);
            }

            model.PracticesWithScriptToCollect = practicesWithScriptToCollect;
            model.NoScriptsToday = scriptsFromAllPractices.Where(s => s.OrderDate.Date == DateTime.Today).Count();
            model.NoScripts7Days = scriptsFromAllPractices.Where(s => s.OrderDate.Date >= DateTime.Today.AddDays(-7)).Count();
            model.NoScripts2Weeks = scriptsFromAllPractices.Where(s => s.OrderDate.Date >= DateTime.Today.AddDays(-14)).Count();
            model.NoScriptsMonth = scriptsFromAllPractices.Where(s => s.OrderDate.Date >= DateTime.Today.AddMonths(-1)).Count();
            model.NoScriptsOlder = scriptsFromAllPractices.Count();

            return View(model);
        }
        /*
        public ViewResult CollectionsToPrint()
        {
            var model = new CollectionsModel();

            var practicesWithScriptToCollect = new List<ScriptCollection>();
            foreach (var practice in db.Practices.OrderBy(p => p.PracticeName).ToList())
            {
                var collection = new ScriptCollection();
                collection.Practice = practice.PracticeName;

                // Get orders added by customer via the website that scripts need collected for
                var orderScripts = (from o in db.Orders
                                    join c in db.Customers on o.CustomerID equals c.CustomerID
                                    join d in db.Doctors on c.DoctorID equals d.DoctorID
                                    where d.PracticeID == practice.PracticeID
                                    select new
                                    {
                                        OrderID = o.OrderID,
                                        OrderDate = o.OrderDate,
                                        Customer = c.Firstname + " " + c.Lastname,
                                        NumItems = db.Orders.Where(a => a.OrderID == o.OrderID).Count()
                                    }).ToList().Select(x => new CollectScript
                                    {
                                        OrderID = x.OrderID,
                                        OrderDate = x.OrderDate,
                                        Customer = x.Customer,
                                        NumItems = x.NumItems
                                    }).Where(x => x.NumItems > 0).ToList();

                // Get scripts that need collected for shops
                var shopScripts = (from col in db.CollectScripts
                                   join d in db.Doctors on col.DoctorID equals d.DoctorID
                                   join p in db.Practices on d.PracticeID equals p.PracticeID
                                   where p.PracticeID == practice.PracticeID
                                   select col).ToList();

                // Add the customer scripts to the shop scripts
                collection.Scripts = orderScripts.Concat(shopScripts).OrderByDescending(s => s.OrderDate);

                practicesWithScriptToCollect.Add(collection);
            }
            model.PracticesWithScriptToCollect = practicesWithScriptToCollect;
            return View(model);
        }
        */
        
    }
}