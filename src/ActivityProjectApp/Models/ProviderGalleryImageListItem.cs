using Avalonia.Media.Imaging;

namespace ActivityProjectApp.Models
{
    public class ProviderGalleryImageListItem
    {
        public int Id { get; set; }

        public string ImagePath { get; set; } = string.Empty;

        public string OriginalFileName { get; set; } = string.Empty;

        public string UploadedAtText { get; set; } = string.Empty;

        public Bitmap? PreviewImage { get; set; }
    }
}