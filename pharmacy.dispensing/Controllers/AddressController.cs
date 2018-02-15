using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pharmacy.Models;
using Pharmacy.Repositories.Interfaces;

namespace Pharmacy.Dispensing.Controllers
{

    [Authorize]
    public class AddressController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        public AddressController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }


        //
        // GET: /Address/Details/5
        public async Task<IActionResult> Details(Guid id)
        {
            Address address = await _unitOfWork.AddressRepository.GetByID(id);
            return View(address);
        }

        //
        // GET: /Address/Create
        public IActionResult Create()
        {
            return View();
        } 

        //
        // POST: /Address/Create

        [HttpPost]
        public async Task<IActionResult> Create(Address address)
        {
            if (ModelState.IsValid)
            {
                address.AddressId = Guid.NewGuid();
                _unitOfWork.AddressRepository.Insert(address);
                await _unitOfWork.SaveAsync();
                return RedirectToAction("Details");  
            }

            return View(address);
        }
        
        //
        // GET: /Address/Edit/5
        public async Task<IActionResult> Edit(Guid id)
        {
            var address = await _unitOfWork.AddressRepository.GetByID(id);
            return View(address);
        }

        //
        // POST: /Address/Edit/5

        [HttpPost]
        public async Task<IActionResult> Edit(Address address)
        {
            if (ModelState.IsValid)
            {
                _unitOfWork.AddressRepository.Update(address);
                //db.ObjectStateManager.ChangeObjectState(address, EntityState.Modified);
                await _unitOfWork.SaveAsync();
                return RedirectToAction("Details", new { id = address.AddressId });
            }
            return View(address);
        }



        
    }
}