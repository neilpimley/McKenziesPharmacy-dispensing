using Pharmacy.Dispensing.Models;
using System;
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
#if !DEBUG
    [RequireHttps] 
#endif
    public class PracticeController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public PracticeController(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        //
        // GET: /Practice/

        public async Task<IActionResult> Index()
        {
            var model = new PracticesModel();
            model.Practices = await _unitOfWork.PracticeRepository.Get(orderBy: p => p.OrderBy(x => x.PracticeName));
            model.Doctors = await _unitOfWork.DoctorRepository.Get(orderBy: d => d.OrderBy(x => x.Surname));

            return View(model);
        }

        //
        // GET: /Practice/Details/5

        public async Task<IActionResult> Details(Guid id)
        {
            var practice = await  _unitOfWork.PracticeRepository.GetByID(id);
            return View(practice);
        }

        //
        // GET: /Practice/Create

        public async Task<IActionResult> Create()
        {
            ViewBag.AddressID = new SelectList(await _unitOfWork.AddressRepository.Get(), "AddressID", "AddressLine1");
            return View();
        }

        //
        // POST: /Practice/Create

        [HttpPost]
        public async Task<IActionResult> Create(Practice practice)
        {
            if (ModelState.IsValid)
            {
                practice.PracticeId = Guid.NewGuid();
                _unitOfWork.PracticeRepository.Insert(practice);
                await _unitOfWork.SaveAsync();
                return RedirectToAction("Index");
            }

            ViewBag.AddressID = new SelectList(await _unitOfWork.AddressRepository.Get(), "AddressID", "AddressLine1", practice.AddressId);
            return View(practice);
        }

        //
        // GET: /Practice/Edit/5

        public async Task<IActionResult> Edit(Guid id)
        {
            var model = new PracticesModel();
            model.Practice = await _unitOfWork.PracticeRepository.GetByID(id);
            ViewBag.AddressID = new SelectList(await _unitOfWork.AddressRepository.Get(), "AddressID", "AddressLine1", model.Practice.AddressId);
            var doctors = (from d in await _unitOfWork.DoctorRepository.Get()
                           select new
                           {
                               Text = "Dr. " + d.Surname,
                               Value = d.DoctorId
                           }).ToList();

            model.DoctorsDropdown = (from d in doctors
                                     select new SelectListItem
                                     {
                                         Text = d.Text,
                                         Value = d.Value.ToString()
                                     }).ToList();

            model.Doctors = await _unitOfWork.DoctorRepository.Get(filter: d => d.PracticeId == id).toLis

            return View(model);
        }

        //
        // POST: /Practice/Edit/5

        [HttpPost]
        public async Task<IActionResult> Edit(PracticesModel model)
        {
            if (ModelState.IsValid)
            {
                _unitOfWork.PracticeRepository.Update(model.Practice);
                //db.ObjectStateManager.ChangeObjectState(practice, EntityState.Modified);
                _unitOfWork.Save();
                return RedirectToAction("Index");
            }
            ViewBag.AddressID = new SelectList(await _unitOfWork.AddressRepository.Get(), "AddressID", "AddressLine1", model.Practice.AddressId);
            return View(model);
        }

        //
        // GET: /Practice/Delete/5

        public async Task<IActionResult> Delete(Guid id)
        {
            var practice = await _unitOfWork.PracticeRepository.GetByID(id);
            return View(practice);
        }

        //
        // POST: /Practice/Delete/5

        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var practice = await _unitOfWork.PracticeRepository.GetByID(id);
            _unitOfWork.PracticeRepository.Delete(practice);
            await _unitOfWork.SaveAsync();
            return RedirectToAction("Index");
        }


        public ActionResult RemoveFromPractice(Guid doctorId, Guid practiceId)
        {
            var doctor = _mapper.Map<DoctorPoco>(await _unitOfWork.DoctorRepository.GetByID(doctorId));
            doctor.PracticeId = Guid.Empty;
            _unitOfWork.Save();

            var practice = _unitOfWork.PracticeRepository.GetByID(practiceId);
            return View(practice);
        }

    }
}