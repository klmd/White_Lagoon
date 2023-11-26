using Microsoft.AspNetCore.Mvc;
using WhiteLagoon.Application.Common.Interfaces;
using WhiteLagoon.Domain.Entities;
using WhiteLagoon.Infrastructure.Data;

namespace WhiteLagoon.Web.Controllers
{
    public class VillaController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWebHostEnvironment _hostEnvironment;

        public VillaController(IUnitOfWork unitOfWork, IWebHostEnvironment hostEnvironment)
        {
            _unitOfWork = unitOfWork;
            _hostEnvironment = hostEnvironment;
        }

        public IActionResult Index()
        {
            var villas = _unitOfWork.Villa.GetAll();
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
            if (ModelState.IsValid)
            {
                if (obj.Image != null)
                {
                    string imageName = Guid.NewGuid().ToString() + Path.GetExtension(obj.Image.FileName);
                    string imagePath = Path.Combine(_hostEnvironment.WebRootPath, @"images\VillaImages");

                    using var fileStream = new FileStream(Path.Combine(imagePath, imageName), FileMode.Create);

                    obj.Image.CopyTo(fileStream);
                    obj.ImageUrl = @"\images\VillaImages\" + imageName;
                    
                }
                else
                {
                    obj.ImageUrl = "https://placehold.co/600x400";
                }

                _unitOfWork.Villa.Add(obj);
                _unitOfWork.Save();
                TempData["success"] = "Villa created successfully";
                return RedirectToAction(nameof(Index));
            }
            TempData["error"] = "Villa could not be created";
            return View();
        }

        public IActionResult Update(int? villaId)
        {
            Villa? obj = _unitOfWork.Villa.Get(u => u.Id == villaId);
            if (obj is null)
            {
                return RedirectToAction("Error", "Home");
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
            if (ModelState.IsValid && obj.Id > 0)
            {
                if (obj.Image != null)
                {
                    string imageName = Guid.NewGuid().ToString() + Path.GetExtension(obj.Image.FileName);
                    string imagePath = Path.Combine(_hostEnvironment.WebRootPath, @"images\VillaImages");

                    if (!string.IsNullOrEmpty(obj.ImageUrl))
                    {
                        string imageOldPath = Path.Combine(_hostEnvironment.WebRootPath, obj.ImageUrl.TrimStart('\\'));

                        if (System.IO.File.Exists(imageOldPath))
                        {
                            System.IO.File.Delete(imageOldPath);
                        }
                    }

                    using var fileStream = new FileStream(Path.Combine(imagePath, imageName), FileMode.Create);

                    obj.Image.CopyTo(fileStream);
                    obj.ImageUrl = @"\images\VillaImages\" + imageName;
                }
                _unitOfWork.Villa.Update(obj);
                _unitOfWork.Save();
                TempData["success"] = "Villa updated successfully";
                return RedirectToAction(nameof(Index));
            }
            //TempData["error"] = "Villa could not be updated";
            return View();
        }

        public IActionResult Delete(int? villaId)
        {
            Villa? obj = _unitOfWork.Villa.Get(u => u.Id == villaId);
            if (obj is null)
            {
                return RedirectToAction("Error", "Home");
            }
            return View(obj);
        }

        [HttpPost]
        public IActionResult Delete(Villa obj)
        {
            Villa? objFromDb = _unitOfWork.Villa.Get(u => u.Id == obj.Id);
            if (objFromDb is not null)
            {
                if (!string.IsNullOrEmpty(objFromDb.ImageUrl))
                {
                    string imageOldPath = Path.Combine(_hostEnvironment.WebRootPath, objFromDb.ImageUrl.TrimStart('\\'));

                    if (System.IO.File.Exists(imageOldPath))
                    {
                        System.IO.File.Delete(imageOldPath);
                    }
                }

                _unitOfWork.Villa.Remove(objFromDb);
                _unitOfWork.Save();
                TempData["success"] = "Villa deleted successfully";
                return RedirectToAction(nameof(Index));
            }
            TempData["error"] = "Villa could not be deleted/found";
            return View();
        }
    }
}
