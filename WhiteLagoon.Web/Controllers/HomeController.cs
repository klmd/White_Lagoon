using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using Syncfusion.Presentation;
using WhiteLagoon.Application.Common.Interfaces;
using WhiteLagoon.Application.Utility;
using WhiteLagoon.Web.Models;
using WhiteLagoon.Web.Models.ViewModels;

namespace WhiteLagoon.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWebHostEnvironment _webHostEnvironment;
        
        public HomeController(IUnitOfWork unitOfWork, IWebHostEnvironment webHostEnvironment)
        {
            _unitOfWork = unitOfWork;
            _webHostEnvironment = webHostEnvironment;
        }

        public IActionResult Index()
        {
            HomeVM homeVM = new()
            {
                VillaList = _unitOfWork.Villa.GetAll(includeProperties: "VillaAmenity"),
                Nights = 1,
                CheckInDate = DateOnly.FromDateTime(DateTime.Now),
            };
            return View(homeVM);
        }

        [HttpPost]
        //this is not needed because of the ajax call
        //public IActionResult Index(HomeVM homeVM)
        //{
        //    homeVM.VillaList = _unitOfWork.Villa.GetAll(includeProperties: "VillaAmenity");

        //    return View(homeVM);
        //}
        public IActionResult GetVillasByDate(int nights, DateOnly checkInDate)
        {
            //múže se použít pro spinner by byl alespoò 1s vidìt
            //Thread.Sleep(1000);
            var villaList = _unitOfWork.Villa.GetAll(includeProperties: "VillaAmenity").ToList();
            var villaNumberList = _unitOfWork.VillaNumber.GetAll().ToList();
            var bookedVillas = _unitOfWork.Booking.GetAll(u => u.Status == SD.StatusApproved ||u.Status == SD.StatusCheckedIn).ToList();
            
            foreach (var villa in villaList)
            {
                int roomsAvailable = SD.VillaRoomsAvailable_Count(villa, checkInDate, villaNumberList, nights, bookedVillas);
            }
            HomeVM homeVM = new()
            {
                VillaList = villaList,
                Nights = nights,
                CheckInDate = checkInDate,
            };

            return PartialView("_VillaList",homeVM);
        }

        [HttpPost]
        public IActionResult GeneratePPTExport(int id)
        {
            var villa = _unitOfWork.Villa.GetAll(u => u.Id == id, includeProperties: "VillaAmenity").FirstOrDefault(x => x.Id==id);

            if (villa is null)
            {
                return RedirectToAction(nameof(Error));
            }
            
            string basePath = _webHostEnvironment.WebRootPath;
            string filePath = basePath + @"/exports/ExportVillaDetails.pptx";

            using IPresentation presentation = Presentation.Open(filePath);
            
            ISlide slide = presentation.Slides[0];
            IShape? shape = slide.Shapes.FirstOrDefault(u => u.ShapeName == "txtVillaName") as IShape;

            if (shape is not null)
            {
                shape.TextBody.Text = villa.Name;
            }
            
            shape = slide.Shapes.FirstOrDefault(u => u.ShapeName == "txtVillaDescription") as IShape;

            if (shape is not null)
            {
                shape.TextBody.Text = villa.Description;
            }

            shape = slide.Shapes.FirstOrDefault(u => u.ShapeName == "txtOccupancy") as IShape;

            if (shape is not null)
            {
                shape.TextBody.Text = "Max Occupancy: " + villa.Occupancy.ToString() + " adult";
            }

            shape = slide.Shapes.FirstOrDefault(u => u.ShapeName == "txtVillaSize") as IShape;

            if (shape is not null)
            {
                shape.TextBody.Text = "Villa size: " + villa.Sqft.ToString() + "sqft";
            }

            shape = slide.Shapes.FirstOrDefault(u => u.ShapeName == "txtPricePerNight") as IShape;

            if (shape is not null)
            {
                shape.TextBody.Text = villa.Price.ToString("C") + " / night";
            }

            shape = slide.Shapes.FirstOrDefault(u => u.ShapeName == "txtVillaAmenitiesHeading") as IShape;

            if (shape is not null)
            {
                List<string> listItems = villa.VillaAmenity.Select(u => u.Name).ToList();
                
                shape.TextBody.Text = "";

                foreach (var item in listItems)
                {
                    IParagraph paragraph = shape.TextBody.AddParagraph();
                    ITextPart textPart = paragraph.AddTextPart(item);
                    
                    paragraph.ListFormat.Type = ListType.Bulleted;
                    paragraph.ListFormat.BulletCharacter = '\u2022';
                    paragraph.Font.FontName = "system-ui";
                    paragraph.Font.FontSize = 14;
                    paragraph.Font.Color = ColorObject.FromArgb(144, 148, 152);
                    
                }
            }

            shape = slide.Shapes.FirstOrDefault(u => u.ShapeName == "imgVilla") as IShape;

            if (shape is not null)
            {
                byte[] imageData;
                string imageUrl;
                try
                {
                    imageUrl = string.Format("{0}{1}", basePath, villa.ImageUrl);
                    imageData = System.IO.File.ReadAllBytes(imageUrl);
                }
                catch (Exception e)
                {
                    imageUrl = string.Format("{0}{1}", basePath, "/images/placeholder.png");
                    imageData = System.IO.File.ReadAllBytes(imageUrl);
                }
                slide.Shapes.Remove(shape);
                using MemoryStream imageStream = new(imageData);
                IPicture picture = slide.Pictures.AddPicture(imageStream, 60, 120, 300, 200);
            }

            MemoryStream memoryStream = new();
            presentation.Save(memoryStream);
            memoryStream.Position = 0;
            return File(memoryStream, "application/pptx", "VillaDetails.pptx");
        }


        public IActionResult Privacy()
        {
            return View();
        }

        //[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View();
        }
    }
}
