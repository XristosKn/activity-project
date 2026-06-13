using System;

namespace ActivityProjectApp.Models
{
    public class Announcement
    {
        public int Id { get; set; }

        public string Title { get; set; } = string.Empty;

        public string Text { get; set; } = string.Empty;

        public DateTime AnnouncementDate { get; set; }

        public string Gallery { get; set; } = string.Empty;

        public bool IsActive { get; set; }

        public int ServiceProviderId { get; set; }
    }
}