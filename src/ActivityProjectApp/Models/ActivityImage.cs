using System;

namespace ActivityProjectApp.Models
{
    public enum ActivityImageItemType
    {
        Event,
        Course
    }

    public class ActivityImage
    {
        public int Id { get; set; }

        public int ItemId { get; set; }

        public ActivityImageItemType ItemType { get; set; }

        public int? ProviderGalleryImageId { get; set; }

        public string ImagePath { get; set; } = string.Empty;

        public bool IsMainImage { get; set; }

        public int DisplayOrder { get; set; }

        public string Caption { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}