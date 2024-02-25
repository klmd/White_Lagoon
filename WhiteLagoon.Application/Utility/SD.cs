using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WhiteLagoon.Domain.Entities;
using WhiteLagoon.Web.Models.ViewModels;

namespace WhiteLagoon.Application.Utility
{
    public static class SD //static details - SD
    {
        public const string Role_Customer = "Customer";
        public const string Role_Admin = "Admin";

        public const string StatusPending = "Pending";     //default value - before payment
        public const string StatusApproved = "Approved";   //after payment
        public const string StatusCancelled = "Cancelled"; //zrušená rezervace
        public const string StatusCheckedIn = "CheckedIn"; //přišel na recepci
        public const string StatusRefunded = "Refunded";   //vrácení peněz
        public const string StatusCompleted = "Completed"; //ukončený pobyt


        public static int VillaRoomsAvailable_Count(Villa villa, DateOnly checkInDate, List <VillaNumber>villaNumberList, int nights, List<Booking> bookings)
        {
            List<int> bookingInDate = new();
            int finalAvailableRoomForAllNights = int.MaxValue;
            var roomsInVilla = villaNumberList.Where(u => u.VillaId == villa.Id).Count();

            for (int i = 0; i < nights; i++)
            {
                var villaBooked = bookings.Where((b => b.CheckInDate.AddDays(i) >= b.CheckInDate && b.CheckInDate.AddDays(i) < b.CheckOutDate && b.VillaId == villa.Id));

                foreach (var booking in villaBooked)
                {
                    if (!bookingInDate.Contains(booking.Id))
                    {
                        bookingInDate.Add(booking.Id);
                    }

                }
                var totalAvailableRoom = roomsInVilla - bookingInDate.Count();
                if (totalAvailableRoom == 0)
                {
                    return 0;
                }
                else 
                {
                    if (totalAvailableRoom > finalAvailableRoomForAllNights)
                    {
                        finalAvailableRoomForAllNights = totalAvailableRoom;
                    }
                    
                }
            }
            return finalAvailableRoomForAllNights;
        }


        public static RadialBarChartDto GetRadialBarChartDto(int totalCount, double currentMonthCount, double prevMonthCount)
        {
            RadialBarChartDto RadialBarChartDto = new RadialBarChartDto();
            int increaseDecreaseAmount = 100;
            if (prevMonthCount != 0)
            {
                increaseDecreaseAmount = Convert.ToInt32((currentMonthCount - prevMonthCount) / prevMonthCount);
            }

            RadialBarChartDto.TotalCount = totalCount;
            RadialBarChartDto.CountInCurrentMonth = Convert.ToInt32(currentMonthCount);
            RadialBarChartDto.HasRatioIncreased = currentMonthCount > prevMonthCount;
            RadialBarChartDto.Series = new int[] { increaseDecreaseAmount };

            return RadialBarChartDto;
        }
    }
}
