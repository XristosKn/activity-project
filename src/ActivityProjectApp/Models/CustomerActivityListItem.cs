using ActivityProjectApp.Models;

namespace ActivityProjectApp.Models
{
    public class CustomerActivityListItem
    {
        public int ItemId { get; set; }

        public EnrollmentItemType ItemType { get; set; }

        public string ActivityType { get; set; } = string.Empty;

        public string Title { get; set; } = string.Empty;

        public string Category { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public double Latitude { get; set; }

        public double Longitude { get; set; }

        public string Address { get; set; } = string.Empty;

        public string ScheduleText { get; set; } = string.Empty;

        public decimal Price { get; set; }

        public string PriceText { get; set; } = string.Empty;

        public int MaxSpace { get; set; }

        public string ExtraInfoText { get; set; } = string.Empty;
    }
}