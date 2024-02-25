namespace WhiteLagoon.Web.Models.ViewModels
{
    public class RadialBarChartDto
    {
        public decimal TotalCount { get; set; }
        public decimal CountInCurrentMonth { get; set; }
        public int[] Series { get; set; }
        public bool HasRatioIncreased { get; set; }
    }
}
