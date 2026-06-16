using System;

namespace ActivityProjectApp.Models
{
    public class ProviderGalleryImage
    {
        public int Id { get; set; }

        public int ServiceProviderId { get; set; }

        public string ImagePath { get; set; } = string.Empty;

        public string OriginalFileName { get; set; } = string.Empty;

        public DateTime UploadedAt { get; set; } = DateTime.Now;
    }
}