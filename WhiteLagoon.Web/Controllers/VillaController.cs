using Microsoft.AspNetCore.Mvc;
using WhiteLagoon.Application.Common.Interfaces;
using WhiteLagoon.Domain.Entities;
using WhiteLagoon.Infrastructure.Data;

namespace WhiteLagoon.Web.Controllers
{
    public class VillaController : Controller
    {
        private readonly IVillaRepository _villaRepo;

        public VillaController(IVillaRepository villaRepo)
        {
            _villaRepo = villaRepo;
        }

        public IActionResult Index()
        {
            var villas = _villaRepo.GetAll();
            return View(villas);
        }
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Create(Villa obj)
        {
            if (obj.Name == obj.Description)
            {
                ModelState.AddModelError("name", "Name and Description cannot be the same");
            }
            if(ModelState.IsValid) {
                _villaRepo.Add(obj)    ;
                _villaRepo.Save();
                TempData["success"] = "Villa created successfully";
                return RedirectToAction(nameof(Index));
            }
            TempData["error"] = "Villa could not be created";
            return View();
        }

        public IActionResult Update(int? villaId)
        {
            Villa? obj = _villaRepo.Get(u=> u.Id == villaId);
            if (obj is null || villaId == 0)
            {
                return RedirectToAction("Error","Home");
            }
            return View(obj);
        }

        [HttpPost]
        public IActionResult Update(Villa obj)
        {
            if (obj.Name == obj.Description)
            {
                ModelState.AddModelError("name", "Name and Description cannot be the same");
            }
            if (ModelState.IsValid)
            {
                _villaRepo.Update(obj);
                _villaRepo.Save();
                TempData["success"] = "Villa updated successfully";
                return RedirectToAction(nameof(Index));
            }
            TempData["error"] = "Villa could not be updated";
            return View();
        }

        public IActionResult Delete(int? villaId)
        {
            Villa? obj = _villaRepo.Get(u => u.Id == villaId);
            if (obj is null || villaId == 0)
            {
                return RedirectToAction("Error", "Home");
            }
            return View(obj);
        }

        [HttpPost]
        public IActionResult Delete(Villa obj)
        {
            Villa? objFromDb = _villaRepo.Get(u => u.Id==obj.Id);
            if (objFromDb is not null)
            {
                _villaRepo.Remove(objFromDb);
                _villaRepo.Save();
                TempData["success"] = "Villa deleted successfully";
                return RedirectToAction(nameof(Index));
            }
            TempData["error"] = "Villa could not be deleted/found";
            return View();
        }
    }
}
