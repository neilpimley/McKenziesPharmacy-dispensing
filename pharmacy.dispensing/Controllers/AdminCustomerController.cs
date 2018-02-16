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
                join dt in titles on d.TitleId equals dt.TitleId
                join s in await  _unitOfWork.ShopRepository.Get() on c.ShopId equals s.ShopId
                select new CustomerPoco {
                    CustomerId = c.CustomerId,
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
                });

            return View(customers);
        }

        //
        // GET: /Customer/Details/5

        public async Task<IActionResult> Details(Guid id)
        {
            var customer = _mapper.Map<CustomerPoco>(await _unitOfWork.CustomerRepository.GetByID(id));
            customer.Title = await _unitOfWork.TitleRepository.GetByID(customer.TitleId);
            customer.Address = await _unitOfWork.AddressRepository.GetByID(customer.AddressId);
            customer.Shop = await _unitOfWork.ShopRepository.GetByID(customer.ShopId);
            customer.Doctor = await _unitOfWork.DoctorRepository.GetByID(customer.DoctorId);
            return View(customer);
        }

        
        //
        // GET: /Customer/Edit/5
 
        public async Task<IActionResult> Edit(Guid id)
        {
            var customer = _mapper.Map<CustomerPoco>(await _unitOfWork.CustomerRepository.GetByID(id));
            customer.Address = await _unitOfWork.AddressRepository.GetByID(customer.AddressId);
            ViewBag.Titles = new SelectList(await _unitOfWork.TitleRepository.Get(), "TitleId", "TitleName", customer.TitleId);
            ViewBag.Doctors = new SelectList(await _unitOfWork.DoctorRepository.Get(), "DoctorId", "Surname", customer.DoctorId);
            return View(customer);
        }

        //
        // POST: /Customer/Edit/5

        [HttpPost]
        public async Task<IActionResult> Edit(CustomerPoco customer)
        {
            if (ModelState.IsValid)
            {
                var _customer = _mapper.Map<Customer>(customer);
                _customer.ModifiedOn = DateTime.Now;
                _unitOfWork.CustomerRepository.Update(_customer);
                _unitOfWork.AddressRepository.Update(customer.Address);
                await _unitOfWork.SaveAsync();
                return RedirectToAction("Index");
            }
            ViewBag.Titles = new SelectList(await _unitOfWork.TitleRepository.Get(), "TitleId", "TitleName", customer.TitleId);
            ViewBag.Doctors = new SelectList(await _unitOfWork.DoctorRepository.Get(), "DoctorId", "Surname", customer.DoctorId);
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