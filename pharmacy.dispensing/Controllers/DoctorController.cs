using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Pharmacy.Models;
using Pharmacy.Repositories.Interfaces;

namespace Pharmacy.Dispensing.Controllers
{
    
    [Authorize]
    public class DoctorController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        public DoctorController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        //
        // GET: /Doctor/

        public async Task<IActionResult> Index(Guid? PracticeId, bool? OrderByPractice)
        {
            if (PracticeId == null)
            {
                var doctors = (from d in await  _unitOfWork.DoctorRepository.Get()
                               join p in await  _unitOfWork.PracticeRepository.Get() on d.PracticeId equals p.PracticeId
                               select d).OrderBy(d => d.Surname).ThenBy(d => d.Firstname);
                               
                
                if (OrderByPractice ?? false)
                    doctors = doctors.OrderBy(d => d.Surname).ThenBy(d => d.Firstname);

                return View(doctors.ToList());
            }
            else
            {
                var doctors = (from d in await _unitOfWork.DoctorRepository.Get()
                               join p in await _unitOfWork.PracticeRepository.Get() on d.PracticeId equals p.PracticeId
                               orderby d.Surname, d.Firstname, p.PracticeName
                               select d).Where(p => p.PracticeId == PracticeId).OrderBy(d => d.Surname).ThenBy(d => d.Firstname);

                return View(doctors.ToList());
            }
        }

        //
        // GET: /Doctor/Details/5

        public async Task<IActionResult> Details(Guid id)
        {
            var doctor = await  _unitOfWork.DoctorRepository.GetByID(id);
            return View(doctor);
        }

        //
        // GET: /Doctor/Create

        public async Task<IActionResult> Create()
        {
            ViewBag.PracticeId = new SelectList(await _unitOfWork.PracticeRepository.Get(), "PracticeId", "PracticeName");
            ViewBag.TitleId = new SelectList(await _unitOfWork.TitleRepository.Get(), "TitleId", "TitleName");
            return View();
        } 

        //
        // POST: /Doctor/Create

        [HttpPost]
        public async Task<IActionResult> Create(Doctor doctor)
        {
            if (ModelState.IsValid)
            {
                doctor.DoctorId = Guid.NewGuid();
                _unitOfWork.DoctorRepository.Insert(doctor);
                await _unitOfWork.SaveAsync();
                return RedirectToAction("Index");  
            }

            ViewBag.PracticeID = new SelectList(await _unitOfWork.PracticeRepository.Get(), "PracticeId", "PracticeName", doctor.PracticeId);
            ViewBag.TitleID = new SelectList(await _unitOfWork.TitleRepository.Get(), "TitleId", "TitleName", doctor.TitleId);
            return View(doctor);
        }

        //
        // GET: /Doctor/Edit/5

        public async Task<IActionResult> Edit(Guid id)
        {
            var doctor = await _unitOfWork.DoctorRepository.GetByID(id);
            ViewBag.PracticeId = new SelectList(await _unitOfWork.PracticeRepository.Get(), "PracticeId", "PracticeName", doctor.PracticeId);
            ViewBag.TitleId = new SelectList(await _unitOfWork.TitleRepository.Get(), "TitleId", "TitleName", doctor.TitleId);
            return View(doctor);
        }

        //
        // POST: /Doctor/Edit/5

        [HttpPost]
        public async Task<IActionResult> Edit(Doctor doctor)
        {
            if (ModelState.IsValid)
            {
                _unitOfWork.DoctorRepository.Update(doctor);
                //db.ObjectStateManager.ChangeObjectState(doctor, EntityState.Modified);
                _unitOfWork.SaveAsync();
                return RedirectToAction("Index");
            }
            ViewBag.PracticeID = new SelectList(await _unitOfWork.PracticeRepository.Get(), "PracticeId", "PracticeName", doctor.PracticeId);
            ViewBag.TitleID = new SelectList(await _unitOfWork.TitleRepository.Get(), "TitleId", "TitleName", doctor.TitleId);
            return View(doctor);
        }

        //
        // GET: /Doctor/Delete/5

        public async Task<IActionResult> Delete(Guid id)
        {
            var doctor = await _unitOfWork.DoctorRepository.GetByID(id);
            return View(doctor);
        }

        //
        // POST: /Doctor/Delete/5

        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var  doctor = await _unitOfWork.DoctorRepository.GetByID(id);
            _unitOfWork.DoctorRepository.Delete(doctor);
            await _unitOfWork.SaveAsync();
            return RedirectToAction("Index");
        }

        
    }
}