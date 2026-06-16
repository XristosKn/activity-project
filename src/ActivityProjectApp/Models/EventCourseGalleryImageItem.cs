using Avalonia.Media.Imaging;

namespace ActivityProjectApp.Models
{
    public class EventCourseGalleryImageItem
    {
        public string ImagePath { get; set; } = string.Empty;

        public string Caption { get; set; } = string.Empty;

        public Bitmap? PreviewImage { get; set; }
    }
}