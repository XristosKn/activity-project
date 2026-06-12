using System;

namespace ActivityProjectApp.Models
{
    public enum AnnouncementItemType
    {
        Event,
        Course
    }

    public class Announcement
    {
        public int Id { get; set; }

        public string Title { get; set; } = string.Empty;

        public string Text { get; set; } = string.Empty;

        public AnnouncementItemType ItemType { get; set; }

        public int ItemId { get; set; }

        public DateTime AnnouncementDate { get; set; }

        public string Gallery { get; set; } = string.Empty;

        public bool IsActive { get; set; }
    }
}