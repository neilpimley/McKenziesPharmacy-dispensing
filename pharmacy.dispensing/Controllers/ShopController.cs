using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Pharmacy.Dispensing.Models;
using Pharmacy.Models;
using Pharmacy.Repositories.Interfaces;

namespace Pharmacy.dispensing.Controllers
{
    [Authorize]
    public class ShopController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public ShopController(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        //
        // GET: /Shop/

        public async Task<IActionResult> Index()
        {
            // TODO: include Addres
            var shops = _mapper.Map<IEnumerable<ShopPoco>>(await _unitOfWork.ShopRepository.Get());
            return View(shops.ToList());
        }

        //
        // GET: /Shop/Details/5

        public async Task<IActionResult> Details(Guid id)
        {
            var shop = _mapper.Map<ShopPoco>(await _unitOfWork.ShopRepository.GetByID(id));
            shop.Address = await _unitOfWork.AddressRepository.GetByID(shop.AddressId);
            return View(shop);
        }

        //
        // GET: /Shop/Create

        public async Task<IActionResult> Create()
        {
            ViewBag.AddressId = new SelectList(await _unitOfWork.AddressRepository.Get(), "AddressId", "AddressLine1");
            return View();
        } 

        //
        // POST: /Shop/Create

        [HttpPost]
        public async Task<IActionResult> Create(Shop shop)
        {
            if (ModelState.IsValid)
            {
                shop.ShopId = Guid.NewGuid();
                _unitOfWork.ShopRepository.Insert(shop);
                await _unitOfWork.SaveAsync();
                return RedirectToAction("Index");  
            }

            ViewBag.AddressID = new SelectList(_unitOfWork.AddressRepository.Get().Result, "AddressId", "AddressLine1", shop.AddressId);
            return View(shop);
        }
        
        //
        // GET: /Shop/Edit/5
 
        public async Task<IActionResult> Edit(Guid id)
        {
            var shop = _mapper.Map<ShopPoco>(await _unitOfWork.ShopRepository.GetByID(id));
            ViewBag.AddressID = new SelectList(await _unitOfWork.AddressRepository.Get(), "AddressId", "AddressLine1", shop.AddressId);
            return View(shop);
        }

        //
        // POST: /Shop/Edit/5

        [HttpPost]
        public async Task<IActionResult> Edit(Shop shop)
        {
            if (ModelState.IsValid)
            {
                _unitOfWork.ShopRepository.Update(shop);
                //db.ObjectStateManager.ChangeObjectState(shop, EntityState.Modified);
                await _unitOfWork.SaveAsync();
                return RedirectToAction("Index");
            }
            ViewBag.AddressID = new SelectList(_unitOfWork.AddressRepository.Get().Result, "AddressID", "AddressLine1", shop.AddressId);
            return View(shop);
        }

        //
        // GET: /Shop/Delete/5
 
        public async Task<IActionResult> Delete(Guid id)
        {
            Shop shop = await _unitOfWork.ShopRepository.GetByID(id);
            return View(shop);
        }

        //
        // POST: /Shop/Delete/5

        [HttpPost, ActionName("Delete")]
        public async Task<ActionResult> DeleteConfirmed(Guid id)
        {
            var shop = await _unitOfWork.ShopRepository.GetByID(id);
            _unitOfWork.ShopRepository.Delete(shop);
            _unitOfWork.SaveAsync();
            return RedirectToAction("Index");
        }

    }
}