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
    }
}