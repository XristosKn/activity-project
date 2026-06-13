namespace ActivityProjectApp.Models
{
    public class ProviderActivityListItem
    {
        public int ItemId { get; set; }

        public string ActivityType { get; set; } = string.Empty;

        public string Title { get; set; } = string.Empty;

        public string Category { get; set; } = string.Empty;

        public string Address { get; set; } = string.Empty;

        public string ScheduleText { get; set; } = string.Empty;

        public string Status { get; set; } = string.Empty;
    }
}