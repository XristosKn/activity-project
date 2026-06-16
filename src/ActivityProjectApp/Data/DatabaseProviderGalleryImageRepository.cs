using ActivityProjectApp.Database;
using ActivityProjectApp.Models;
using System.Collections.Generic;
using System.Linq;

namespace ActivityProjectApp.Data
{
    public class DatabaseProviderGalleryImageRepository
    {
        public ProviderGalleryImage AddImage(ProviderGalleryImage providerGalleryImage)
        {
            using AppDbContext dbContext = new AppDbContext();

            dbContext.ProviderGalleryImages.Add(providerGalleryImage);
            dbContext.SaveChanges();

            return providerGalleryImage;
        }

        public List<ProviderGalleryImage> GetImagesByServiceProviderId(int serviceProviderId)
        {
            using AppDbContext dbContext = new AppDbContext();

            return dbContext.ProviderGalleryImages
                .Where(image => image.ServiceProviderId == serviceProviderId)
                .OrderByDescending(image => image.UploadedAt)
                .ToList();
        }

        public ProviderGalleryImage? GetImageById(int imageId)
        {
            using AppDbContext dbContext = new AppDbContext();

            return dbContext.ProviderGalleryImages
                .FirstOrDefault(image => image.Id == imageId);
        }

        public void DeleteImage(int imageId)
        {
            using AppDbContext dbContext = new AppDbContext();

            ProviderGalleryImage? existingImage = dbContext.ProviderGalleryImages
                .FirstOrDefault(image => image.Id == imageId);

            if (existingImage == null)
            {
                return;
            }

            dbContext.ProviderGalleryImages.Remove(existingImage);
            dbContext.SaveChanges();
        }

        public bool ImageBelongsToProvider(int imageId, int serviceProviderId)
        {
            using AppDbContext dbContext = new AppDbContext();

            return dbContext.ProviderGalleryImages
                .Any(image =>
                    image.Id == imageId &&
                    image.ServiceProviderId == serviceProviderId);
        }
    }
}