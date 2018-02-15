using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pharmacy.Dispensing.Models;
using Pharmacy.Models;
using Pharmacy.Repositories.Interfaces;

namespace Pharmacy.Dispensing.Controllers
{
    [Authorize]
    public class DrugController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        public DrugController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        //
        // GET: /Drug/

        public async Task<IActionResult> Index(string currentFilter, string searchString, int? page)
        {
            if (searchString != null)
            {
                page = 1;
            }
            else
            {
                searchString = currentFilter;
            }

            ViewData["CurrentFilter"] = searchString;

            var drugs = from d in await _unitOfWork.DrugRepository.Get()
                        select d;

            if (!String.IsNullOrEmpty(searchString))
            {
                drugs = drugs.Where(s => s.DrugName.ToUpper().Contains(searchString.ToUpper())).Distinct();
            }
            drugs = drugs.OrderBy(p => p.DrugName);
            return View(drugs);

            //int pageSize = 3;
            //return View(await PaginatedList<Drug>.CreateAsync(drugs.AsQueryable(), page ?? 1, pageSize));
        }

        //
        // GET: /Drug/Details/5

        public async Task<IActionResult> Details(Guid id)
        {
            var drug = await _unitOfWork.DrugRepository.GetByID(id);
            return View(drug);
        }

        //
        // GET: /Drug/Create

        public IActionResult Create()
        {
            return View();
        }

        //
        // POST: /Drug/Create

        [HttpPost]
        public async Task<IActionResult> Create(Drug drug)
        {
            if (ModelState.IsValid)
            {
                drug.DrugId = Guid.NewGuid();
                _unitOfWork.DrugRepository.Insert(drug);
                _unitOfWork.Save();
                return RedirectToAction("Index");
            }

            return View(drug);
        }

        //
        // GET: /Drug/Edit/5

        public async Task<IActionResult> Edit(Guid id)
        {
            var drug = await _unitOfWork.DrugRepository.GetByID(id);
            return View(drug);
        }

        //
        // POST: /Drug/Edit/5

        [HttpPost]
        public async Task<IActionResult> Edit(Drug drug)
        {
            if (ModelState.IsValid)
            {
                _unitOfWork.DrugRepository.Update(drug);
                //db.ObjectStateManager.ChangeObjectState(drug, EntityState.Modified);
                await _unitOfWork.SaveAsync();
                return RedirectToAction("Index");
            }
            return View(drug);
        }

        //
        // GET: /Drug/Delete/5

        public async Task<IActionResult> Delete(Guid id)
        {
            var drug = await _unitOfWork.DrugRepository.GetByID(id);
            return View(drug);
        }

        //
        // POST: /Drug/Delete/5

        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var drug = await _unitOfWork.DrugRepository.GetByID(id);
            _unitOfWork.DrugRepository.Delete(drug);
            await _unitOfWork.SaveAsync();
            return RedirectToAction("Index");
        }

    }
}