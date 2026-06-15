namespace ActivityProjectApp.Models
{
    public class CustomerProfileItem
    {
        public int ItemId { get; set; }

        public EnrollmentItemType ItemType { get; set; }

        public string ItemTypeText { get; set; } = string.Empty;

        public string Title { get; set; } = string.Empty;

        public string Category { get; set; } = string.Empty;

        public string ScheduleText { get; set; } = string.Empty;

        public string StatusText { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public string Address { get; set; } = string.Empty;

        public decimal Price { get; set; }

        public string PriceText { get; set; } = string.Empty;

        public int MaxSpace { get; set; }

        public double Latitude { get; set; }

        public double Longitude { get; set; }

        public bool IsExpanded { get; set; }
    }
}