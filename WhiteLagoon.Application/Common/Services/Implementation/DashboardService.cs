using WhiteLagoon.Application.Common.Interfaces;
using WhiteLagoon.Application.Common.Services.Interfaces;
using WhiteLagoon.Application.Utility;
using WhiteLagoon.Web.Models.ViewModels;

namespace WhiteLagoon.Application.Common.Services.Implementation;

public class DashboardService : IDashboardService
{
    private readonly IUnitOfWork _unitOfWork;
    static int previousMonth = DateTime.Now.Month == 1 ? 12 : DateTime.Now.Month - 1;
    private readonly DateTime previousMonthStartDate = new(DateTime.Now.Year, previousMonth, 1);
    private readonly DateTime currentMonthStartDate = new(DateTime.Now.Year, DateTime.Now.Month, 1);
    public DashboardService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }
    public async Task<PieChartDto> GetBookingPieChartData()
    {
        var totalBooking =
            _unitOfWork.Booking.GetAll(u => u.BookingDate >= DateTime.Now.AddDays(-30) && (u.Status != SD.StatusPending || u.Status != SD.StatusCancelled));

        var customerWithOneBooking =
            totalBooking.GroupBy(b => b.UserId).Where(x => x.Count() == 1).Select(x => x.Key).ToList();

        var bookingsByNewCustomer = customerWithOneBooking.Count();
        var bookingsByReturningCustomer = totalBooking.Count() - bookingsByNewCustomer;

        PieChartDto PieChartDto = new()
        {
            Labels = new string[] { "New Customer", "Returning Customer" },
            Series = new decimal[] { bookingsByNewCustomer, bookingsByReturningCustomer }
        };

        return PieChartDto;
    }

    public async Task<LineChartDto> GetMemberAndBookingLineChartData()
    {
        var bookingData = _unitOfWork.Booking.GetAll(u => u.BookingDate >= DateTime.Now.AddDays(-30) &&
                                                          u.BookingDate.Date <= DateTime.Now)
            .GroupBy(b => b.BookingDate.Date)
            .Select(u => new {
                DateTime = u.Key,
                NewBookingCount = u.Count()
            });

        var customerData = _unitOfWork.User.GetAll(u => u.CreatedAt >= DateTime.Now.AddDays(-30) &&
                                                        u.CreatedAt.Date <= DateTime.Now)
            .GroupBy(b => b.CreatedAt.Date)
            .Select(u => new {
                DateTime = u.Key,
                NewCustomerCount = u.Count()
            });


        var leftJoin = bookingData.GroupJoin(customerData, booking => booking.DateTime, customer => customer.DateTime,
            (booking, customer) => new
            {
                booking.DateTime,
                booking.NewBookingCount,
                NewCustomerCount = customer.Select(x => x.NewCustomerCount).FirstOrDefault()
            });


        var rightJoin = customerData.GroupJoin(bookingData, customer => customer.DateTime, booking => booking.DateTime,
            (customer, booking) => new
            {
                customer.DateTime,
                NewBookingCount = booking.Select(x => x.NewBookingCount).FirstOrDefault(),
                customer.NewCustomerCount
            });

        var mergedData = leftJoin.Union(rightJoin).OrderBy(x => x.DateTime).ToList();

        var newBookingData = mergedData.Select(x => x.NewBookingCount).ToArray();
        var newCustomerData = mergedData.Select(x => x.NewCustomerCount).ToArray();
        var categories = mergedData.Select(x => x.DateTime.ToString("MM/dd/yyyy")).ToArray();

        List<ChartData> chartDataList = new()
        {
            new ChartData
            {
                Name = "New Booking",
                Data = newBookingData
            },
            new ChartData
            {
                Name = "New Customer",
                Data = newCustomerData
            }
        };

        LineChartDto LineChartDto = new()
        {
            Series = chartDataList,
            Categories = string.Join(",", categories)
        };

        return LineChartDto;
    }

    public async Task<RadialBarChartDto> GetRegisteredUserChartData()
    {
        var totalUsers =
            _unitOfWork.User.GetAll();

        var countByCurrentMonth =
            totalUsers.Count(u => u.CreatedAt >= currentMonthStartDate && u.CreatedAt <= DateTime.Now);
        var countByPreviousMonth = totalUsers.Count(u =>
            u.CreatedAt >= previousMonthStartDate &&
            u.CreatedAt <= currentMonthStartDate.AddDays(-1));

        return SD.GetRadialBarChartDto(totalUsers.Count(), countByCurrentMonth, countByPreviousMonth);
    }

    public async Task<RadialBarChartDto> GetTotalBookingRadialChartData()
    {
        var totalBooking =
            _unitOfWork.Booking.GetAll(u => u.Status != SD.StatusPending || u.Status != SD.StatusCancelled);

        var countByCurrentMonth =
            totalBooking.Count(u => u.BookingDate >= currentMonthStartDate && u.BookingDate <= DateTime.Now);
        var countByPreviousMonth = totalBooking.Count(u =>
            u.BookingDate >= previousMonthStartDate &&
            u.BookingDate <= currentMonthStartDate.AddDays(-1));

        return SD.GetRadialBarChartDto(totalBooking.Count(), countByCurrentMonth, countByPreviousMonth);
    }

    public async Task<RadialBarChartDto> GetTotalRevenueRadialChartData()
    {
        var totalBooking =
            _unitOfWork.Booking.GetAll(u => u.Status != SD.StatusPending || u.Status != SD.StatusCancelled);
        var totalRevenue = Convert.ToInt32(totalBooking.Sum(u => u.TotalCost));

        var countByCurrentMonth =
            totalBooking.Where(u => u.BookingDate >= currentMonthStartDate && u.BookingDate <= DateTime.Now).Sum(u => u.TotalCost);
        var countByPreviousMonth = totalBooking.Where(u =>
            u.BookingDate >= previousMonthStartDate &&
            u.BookingDate <= currentMonthStartDate.AddDays(-1)).Sum(u => u.TotalCost);

        return SD.GetRadialBarChartDto(totalRevenue, countByCurrentMonth, countByPreviousMonth);
    }
}