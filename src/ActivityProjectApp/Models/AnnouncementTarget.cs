namespace ActivityProjectApp.Models
{
    public enum AnnouncementItemType
    {
        Event,
        Course
    }

    public class AnnouncementTarget
    {
        public int Id { get; set; }

        public int AnnouncementId { get; set; }

        public int ItemId { get; set; }

        public AnnouncementItemType ItemType { get; set; }
    }
}