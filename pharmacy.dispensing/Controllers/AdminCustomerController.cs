using System;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Pharmacy.Models;
using Pharmacy.Dispensing.Models;
using Pharmacy.Models.Pocos;
using Pharmacy.Repositories.Interfaces;

namespace Pharmacy.Dispensing.Controllers
{
    [Authorize]
    public class AdminCustomerController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public AdminCustomerController(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        //
        // GET: /Customer/

        public async Task<IActionResult> Index()
        {
            //var customers = db.Customers.Include("Address").Include("Doctor").Include("Title");
            var titles = await _unitOfWork.TitleRepository.Get();
            var customers = (from c in await _unitOfWork.CustomerRepository.Get() 
                join a in await _unitOfWork.AddressRepository.Get() on c.AddressId equals a.AddressId
                join d in await _unitOfWork.DoctorRepository.Get() on c.DoctorId equals d.DoctorId
                join t in titles on c.TitleId equals t.TitleId
                join dt in titles on c.TitleId equals dt.TitleId
                join s in await  _unitOfWork.ShopRepository.Get() on c.ShopId equals s.ShopId
                select new CustomerPoco() {
                    Email = c.Email,
                    Firstname = c.Firstname,
                    Lastname = c.Lastname,
                    Sex = c.Sex,
                    Mobile = c.Mobile,
                    Home = c.Home,
                    Dob = c.Dob,
                    CreatedOn = c.CreatedOn,
                    ModifiedOn = c.ModifiedOn,
                    Active = c.Active,
                    Address = a,
                    Title = t,
                    Doctor = new DoctorPoco()
                    {
                        Title = dt,
                        Surname = d.Surname
                    },
                    Shop = s
                }).ToList();

            return View(customers.ToList());
        }

        //
        // GET: /Customer/Details/5

        public async Task<IActionResult> Details(Guid id)
        {
            var customer = await _unitOfWork.CustomerRepository.GetByID(id);
            return View(customer);
        }

        
        //
        // GET: /Customer/Edit/5
 
        public async Task<IActionResult> Edit(Guid id)
        {
            var customer = _mapper.Map<CustomerPoco>(await _unitOfWork.CustomerRepository.GetByID(id));
            ViewBag.AddressID = new SelectList(await _unitOfWork.AddressRepository.Get(), "AddressID", "AddressLine1", customer.AddressId);
            ViewBag.DoctorID = new SelectList(await _unitOfWork.DoctorRepository.Get(), "DoctorID", "Firstname", customer.DoctorId);
            ViewBag.TitleID = new SelectList(await _unitOfWork.TitleRepository.Get(), "TitleID", "TitleName", customer.TitleId);
            return View(customer);
        }

        //
        // POST: /Customer/Edit/5

        [HttpPost]
        public async Task<IActionResult> Edit(Customer customer)
        {
            if (ModelState.IsValid)
            {
                _unitOfWork.CustomerRepository.Update(customer);
                //db.ObjectStateManager.ChangeObjectState(customer, EntityState.Modified);
                _unitOfWork.Save();
                return RedirectToAction("Index");
            }
            ViewBag.AddressID = new SelectList(await _unitOfWork.AddressRepository.Get(), "AddressId", "AddressLine1", customer.AddressId);
            ViewBag.DoctorID = new SelectList(await _unitOfWork.DoctorRepository.Get(), "DoctorId", "Firstname", customer.DoctorId);
            ViewBag.TitleID = new SelectList(await _unitOfWork.TitleRepository.Get(), "TitleId", "TitleName", customer.TitleId);
            return View(customer);
        }

        //
        // GET: /Customer/Delete/5
 
        public async Task<IActionResult> Delete(Guid id)
        {
            var customer = await _unitOfWork.CustomerRepository.GetByID(id);
            return View(customer);
        }

        //
        // POST: /Customer/Delete/5

        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var customer = await _unitOfWork.CustomerRepository.GetByID(id);
            _unitOfWork.CustomerRepository.Delete(customer);
            await _unitOfWork.SaveAsync();
            return RedirectToAction("Index");
        }

    }
}