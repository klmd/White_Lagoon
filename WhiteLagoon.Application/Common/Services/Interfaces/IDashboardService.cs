using WhiteLagoon.Application.Common.Interfaces;
using WhiteLagoon.Web.Models.ViewModels;

namespace WhiteLagoon.Application.Common.Services.Interfaces;

public interface IDashboardService
{
    Task<RadialBarChartDto> GetTotalBookingRadialChartData();
    Task<RadialBarChartDto> GetTotalRevenueRadialChartData();
    Task<RadialBarChartDto> GetRegisteredUserChartData();
    Task<PieChartDto> GetBookingPieChartData();
    Task<LineChartDto> GetMemberAndBookingLineChartData();
}