namespace ActivityProjectApp.Models
{
    public class AnnouncementTargetSelectionItem
    {
        public int ItemId { get; set; }

        public AnnouncementItemType ItemType { get; set; }

        public string DisplayText { get; set; } = string.Empty;

        public bool IsSelected { get; set; }
    }
}