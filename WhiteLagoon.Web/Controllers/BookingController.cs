using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Stripe.Checkout;
using WhiteLagoon.Application.Common.Interfaces;
using WhiteLagoon.Application.Utility;
using WhiteLagoon.Domain.Entities;
using Stripe;
using Stripe.Checkout;

namespace WhiteLagoon.Web.Controllers
{
    public class BookingController : Controller
    {
        public readonly IUnitOfWork _UnitOfWork;

        public BookingController(IUnitOfWork unitOfWork)
        {
            _UnitOfWork = unitOfWork;
        }
        [Authorize]
        public IActionResult Index()
        {
            return View();
        }

        [Authorize]
        public IActionResult FinalizeBooking(int villaId, DateOnly checkInDate, int nights)
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

            ApplicationUser user = _UnitOfWork.User.Get(u => u.Id == userId);

            Booking booking = new()
            {
                VillaId = villaId,
                Villa = _UnitOfWork.Villa.Get(u => u.Id==villaId, includeProperties:"VillaAmenity"),
                CheckInDate = checkInDate,
                Nights = nights,
                CheckOutDate = checkInDate.AddDays(nights), //proč toto nefunguje v kookingdetails.cshtml a FinalizeBooking.cshtml
                UserId = userId,
                Phone = user.PhoneNumber,
                Email = user.Email,
                Name = user.Name,
            };
            booking.TotalCost = booking.Villa.Price * nights;
            return View(booking);
        }
       
        [Authorize]
        [HttpPost]
        public IActionResult FinalizeBooking(Booking booking)
        {
            var villa = _UnitOfWork.Villa.Get(u => u.Id == booking.VillaId);

            booking.TotalCost = villa.Price * booking.Nights;

            booking.Status = SD.StatusPending;
            booking.BookingDate = DateTime.Now; //DateTime.Now; - nefunguje - už funguje

            _UnitOfWork.Booking.Add(booking);
            _UnitOfWork.Save();

            //stripe payment
            var domain = Request.Scheme+"://"+Request.Host.Value+"/";
            var options = new SessionCreateOptions
            {
                LineItems = new List<SessionLineItemOptions>(),
                Mode = "payment",
                SuccessUrl = domain + $"booking/BookingConfirmation?bookingId={booking.Id}",
                CancelUrl = domain + $"booking/FinalizeBooking?villaId={booking.VillaId}& checkInDate={booking.CheckInDate}&nights={booking.Nights}",
            };

            options.LineItems.Add(new SessionLineItemOptions
            {
                PriceData = new SessionLineItemPriceDataOptions
                {
                    UnitAmount = (long)booking.TotalCost * 100,
                    Currency = "usd",
                    ProductData = new SessionLineItemPriceDataProductDataOptions
                    {
                        Name = villa.Name,
                        //Images = new List<string> { villa.ImageUrl }
                    },
                },
                Quantity = 1,
            });

            var service = new SessionService();
            Session session = service.Create(options);

            _UnitOfWork.Booking.UpdateStripePaymentId(booking.Id, session.Id, session.PaymentIntentId);
            _UnitOfWork.Save();

            Response.Headers.Add("Location", session.Url);
            return new StatusCodeResult(303);
            //stripe payment

            //return RedirectToAction(nameof(BookingConfirmation), new { bookingId = booking.Id}); - nefunguje resp. není třeba
        }

        [Authorize]
        public IActionResult BookingConfirmation(int bookingId) //nefunguje zjistit co je špatně - funguje
        {
            Booking bookingFromDb = _UnitOfWork.Booking.Get(u => u.Id == bookingId, includeProperties:"User, Villa");

            if (bookingFromDb.Status == SD.StatusPending)
            {
                //pending order we need to confirm if payment is successeful;
                var service = new SessionService();
                Session session = service.Get(bookingFromDb.StripeSessionId);

                if (session.PaymentStatus == "paid")
                {
                    _UnitOfWork.Booking.UpdateStatus(bookingFromDb.Id, SD.StatusApproved, 0);
                    _UnitOfWork.Booking.UpdateStripePaymentId(bookingFromDb.Id, session.Id, session.PaymentIntentId);
                    _UnitOfWork.Save();
                }
                //else
                //{
                //    bookingFromDb.Status = SD.StatusCancelled;
                //}
            }
            return View(bookingId);
        }

        [Authorize]
        public IActionResult BookingDetails(int bookingId)
        {
            Booking bookingFromDb = _UnitOfWork.Booking.Get(u => u.Id == bookingId, includeProperties: "User, Villa");

            if (bookingFromDb.VillaNumber == 0 && bookingFromDb.Status == SD.StatusApproved)
            {
                var availableVillaNumber = AssignAvailableVillaNumbersByVilla(bookingFromDb.VillaId);

                bookingFromDb.VillaNumbers = _UnitOfWork.VillaNumber.GetAll(u =>
                    u.VillaId == bookingFromDb.VillaId && availableVillaNumber.Any(v => v == u.Villa_Number)).ToList();
            }

            return View(bookingFromDb);
        }

        [HttpPost]
        [Authorize(Roles = SD.Role_Admin)]
        public IActionResult CheckIn(Booking booking)
        {
            _UnitOfWork.Booking.UpdateStatus(booking.Id, SD.StatusCheckedIn, booking.VillaNumber);
            _UnitOfWork.Save();
            TempData["Success"] = "Booking has been updated successfully";
            return RedirectToAction(nameof(BookingDetails), new { bookingId = booking.Id });
        }

        [HttpPost]
        [Authorize(Roles = SD.Role_Admin)]
        public IActionResult CheckOut(Booking booking)
        {
            _UnitOfWork.Booking.UpdateStatus(booking.Id, SD.StatusCompleted, booking.VillaNumber);
            _UnitOfWork.Save();
            TempData["Success"] = "Booking has been completed successfully";
            return RedirectToAction(nameof(BookingDetails), new { bookingId = booking.Id });
        }

        [HttpPost]
        [Authorize(Roles = SD.Role_Admin)]
        public IActionResult CancelBooking(Booking booking)
        {
            _UnitOfWork.Booking.UpdateStatus(booking.Id, SD.StatusCancelled, 0);
            _UnitOfWork.Save();
            TempData["Success"] = "Booking has been cancelled successfully";
            return RedirectToAction(nameof(BookingDetails), new { bookingId = booking.Id });
        }


        private List<int> AssignAvailableVillaNumbersByVilla(int villaId)
        {
            List<int> AvailableVillaNumbers = new List<int>();
            var villaNumbers = _UnitOfWork.VillaNumber.GetAll(u => u.VillaId == villaId);

            var checkedInVilla = _UnitOfWork.Booking.GetAll(u => u.VillaId == villaId && u.Status == SD.StatusCheckedIn).Select(u => u.VillaNumber);

            foreach (var villaNumber in villaNumbers)
            {
                if (!checkedInVilla.Contains(villaNumber.Villa_Number))
                {
                    AvailableVillaNumbers.Add(villaNumber.Villa_Number);
                }
            }
            return AvailableVillaNumbers;
        }

        #region API Calls
        [HttpGet]
        [Authorize]
        public IActionResult GetAll(string status)
        {
            IEnumerable<Booking> objBookings;

            if (User.IsInRole(SD.Role_Admin))
            {
                objBookings = _UnitOfWork.Booking.GetAll(includeProperties: "User,Villa");
            }
            else
            {
                var claimsIdentity = (ClaimsIdentity)User.Identity;
                var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

                objBookings = _UnitOfWork.Booking.GetAll(u => u.UserId == userId, includeProperties: "User,Villa");
            }

            if (!string.IsNullOrEmpty(status))
            {
                objBookings = objBookings.Where(u => u.Status.ToLower().Equals(status.ToLower()));
            }

            return Json(new { data = objBookings });
        }

        #endregion
    }
}
