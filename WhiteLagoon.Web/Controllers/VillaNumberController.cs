using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;
using WhiteLagoon.Application.Common.Interfaces;
using WhiteLagoon.Domain.Entities;
using WhiteLagoon.Infrastructure.Data;
using WhiteLagoon.Web.Models.ViewModels;

namespace WhiteLagoon.Web.Controllers
{
    public class VillaNumberController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        public VillaNumberController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public IActionResult Index()
        {
            //seznam Vill s čísly pro tabulku
            //var villaNumbers = _db.VillaNumber.Include(u => u.Villa).ToList();
            //var villaNumbers = _unitOfWork.VillaNumber.GetAll();
            var villaNumberVM = _unitOfWork.VillaNumber.GetAll(includeProperties: "Villa");
            return View(villaNumberVM);
        }
        public IActionResult Create()
        {
            // rozbalovací seznam pro přřazení čísla vily k typu vily
            VillaNumberVM villaNumberVM = new ()
            {
                VillaList = _unitOfWork.Villa.GetAll().Select(u => new SelectListItem
                {
                    Text = u.Name,
                    Value = u.Id.ToString()
                })
            };
            return View(villaNumberVM);
        }

        [HttpPost]
        public IActionResult Create(VillaNumberVM villaNumberVM)
        {
            //kontrola, že číslo vily už neexistuje
            bool roomNumberexist = _unitOfWork.VillaNumber.Any(u => u.Villa_Number == villaNumberVM.VillaNumber.Villa_Number);
                       
            //ModelState.Remove("Villa");
            if (ModelState.IsValid && !roomNumberexist)
            {
                _unitOfWork.VillaNumber.Add(villaNumberVM.VillaNumber);
                _unitOfWork.Save();
                TempData["success"] = "Villa Number created successfully";
                return RedirectToAction(nameof(Index));
            }
            
            if (roomNumberexist)
            {
                TempData["error"] = "Villa Number could not be created room number already exist";                
            }
            villaNumberVM.VillaList = _unitOfWork.Villa.GetAll().Select(u => new SelectListItem
            {
                Text = u.Name,
                Value = u.Id.ToString()
            });
            return View(villaNumberVM);
        }

        public IActionResult Update(int? villaNumberId)
        {
            VillaNumberVM villaNumberVM = new()
            {
                VillaList = _unitOfWork.Villa.GetAll().Select(u => new SelectListItem
                {
                    Text = u.Name,
                    Value = u.Id.ToString()
                }),
                VillaNumber = _unitOfWork.VillaNumber.Get(u => u.Villa_Number == villaNumberId)
            };
            
            //Villa? obj = _db.Villas.FirstOrDefault(u=> u.Id == villaId);
            if (villaNumberVM.VillaNumber is null || villaNumberId == 0)
            {
                return RedirectToAction("Error","Home");
            }
            return View(villaNumberVM);
        }

        [HttpPost]
        public IActionResult Update(VillaNumberVM villaNumberVM)
        {
            //ModelState.Remove("Villa");
            if (ModelState.IsValid)
            {
                _unitOfWork.VillaNumber.Update(villaNumberVM.VillaNumber);
                _unitOfWork.Save();
                TempData["success"] = "Villa Number updated successfully";
                return RedirectToAction(nameof(Index));
            }

            villaNumberVM.VillaList = _unitOfWork.Villa.GetAll().Select(u => new SelectListItem
            {
                Text = u.Name,
                Value = u.Id.ToString()
            });
            return View(villaNumberVM);
        }

        public IActionResult Delete(int? villaNumberId)
        {
            VillaNumberVM villaNumberVM = new()
            {
                VillaList = _unitOfWork.Villa.GetAll().Select(u => new SelectListItem
                {
                    Text = u.Name,
                    Value = u.Id.ToString()
                }),
                VillaNumber = _unitOfWork.VillaNumber.Get(u => u.Villa_Number == villaNumberId)
            };

            //Villa? obj = _db.Villas.FirstOrDefault(u=> u.Id == villaId);
            if (villaNumberVM.VillaNumber is null || villaNumberId == 0)
            {
                return RedirectToAction("Error", "Home");
            }
            return View(villaNumberVM);
        }

        [HttpPost]
        public IActionResult Delete(VillaNumberVM villaNumberVM)
        {
            VillaNumber? objFromDb =_unitOfWork.VillaNumber.Get(u => u.Villa_Number == villaNumberVM.VillaNumber.Villa_Number);
            if (objFromDb is not null)
            {
                _unitOfWork.VillaNumber.Remove(objFromDb);
                _unitOfWork.Save();
                TempData["success"] = "Villa Number has been deleted successfully";
                return RedirectToAction(nameof(Index));
            }
            TempData["error"] = "Villa Number could not be deleted/found";
            return View();
        }
    }
}
