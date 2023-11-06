using Microsoft.AspNetCore.Mvc;
using WhiteLagoon.Domain.Entities;
using WhiteLagoon.Infrastructure.Data;

namespace WhiteLagoon.Web.Controllers
{
    public class VillaController : Controller
    {
        private readonly ApplicationDbContext _db;

        public VillaController(ApplicationDbContext db)
        {
            _db = db;
        }

        public IActionResult Index()
        {
            var villas = _db.Villas.ToList();
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
                _db.Villas.Add(obj);
                _db.SaveChanges();
                TempData["success"] = "Villa created successfully";
                return RedirectToAction("Index");
            }
            TempData["error"] = "Villa could not be created";
            return View();
        }

        public IActionResult Update(int? villaId)
        {
            Villa? obj = _db.Villas.FirstOrDefault(u=> u.Id == villaId);
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
                _db.Villas.Update(obj);
                _db.SaveChanges();
                TempData["success"] = "Villa updated successfully";
                return RedirectToAction("Index");
            }
            TempData["error"] = "Villa could not be updated";
            return View();
        }

        public IActionResult Delete(int? villaId)
        {
            Villa? obj = _db.Villas.FirstOrDefault(u => u.Id == villaId);
            if (obj is null || villaId == 0)
            {
                return RedirectToAction("Error", "Home");
            }
            return View(obj);
        }

        [HttpPost]
        public IActionResult Delete(Villa obj)
        {
            Villa? objFromDb =_db.Villas.FirstOrDefault(u => u.Id==obj.Id);
            if (objFromDb is not null)
            {
                _db.Villas.Remove(objFromDb);
                _db.SaveChanges();
                TempData["success"] = "Villa deleted successfully";
                return RedirectToAction("Index");
            }
            TempData["error"] = "Villa could not be deleted/found";
            return View();
        }
    }
}
