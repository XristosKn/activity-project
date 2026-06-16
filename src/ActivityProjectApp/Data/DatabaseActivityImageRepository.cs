using ActivityProjectApp.Database;
using ActivityProjectApp.Models;
using System.Collections.Generic;
using System.Linq;

namespace ActivityProjectApp.Data
{
    public class DatabaseActivityImageRepository
    {
        public void AddImage(ActivityImage activityImage)
        {
            using AppDbContext dbContext = new AppDbContext();

            if (activityImage.IsMainImage)
            {
                ClearExistingMainImage(
                    dbContext,
                    activityImage.ItemId,
                    activityImage.ItemType);
            }

            dbContext.ActivityImages.Add(activityImage);
            dbContext.SaveChanges();
        }

        public void AddImages(List<ActivityImage> activityImages)
        {
            using AppDbContext dbContext = new AppDbContext();

            foreach (ActivityImage activityImage in activityImages)
            {
                if (activityImage.IsMainImage)
                {
                    ClearExistingMainImage(
                        dbContext,
                        activityImage.ItemId,
                        activityImage.ItemType);
                }

                dbContext.ActivityImages.Add(activityImage);
            }

            dbContext.SaveChanges();
        }

        public List<ActivityImage> GetImagesByItem(int itemId, ActivityImageItemType itemType)
        {
            using AppDbContext dbContext = new AppDbContext();

            return dbContext.ActivityImages
                .Where(image =>
                    image.ItemId == itemId &&
                    image.ItemType == itemType)
                .OrderByDescending(image => image.IsMainImage)
                .ThenBy(image => image.DisplayOrder)
                .ToList();
        }

        public ActivityImage? GetMainImage(int itemId, ActivityImageItemType itemType)
        {
            using AppDbContext dbContext = new AppDbContext();

            return dbContext.ActivityImages
                .Where(image =>
                    image.ItemId == itemId &&
                    image.ItemType == itemType &&
                    image.IsMainImage)
                .OrderBy(image => image.DisplayOrder)
                .FirstOrDefault();
        }

        public List<ActivityImage> GetGalleryImages(int itemId, ActivityImageItemType itemType)
        {
            using AppDbContext dbContext = new AppDbContext();

            return dbContext.ActivityImages
                .Where(image =>
                    image.ItemId == itemId &&
                    image.ItemType == itemType &&
                    !image.IsMainImage)
                .OrderBy(image => image.DisplayOrder)
                .ToList();
        }

        public void SetAsMainImage(int imageId)
        {
            using AppDbContext dbContext = new AppDbContext();

            ActivityImage? selectedImage = dbContext.ActivityImages
                .FirstOrDefault(image => image.Id == imageId);

            if (selectedImage == null)
            {
                return;
            }

            ClearExistingMainImage(
                dbContext,
                selectedImage.ItemId,
                selectedImage.ItemType);

            selectedImage.IsMainImage = true;

            dbContext.SaveChanges();
        }

        public void UpdateImage(ActivityImage updatedImage)
        {
            using AppDbContext dbContext = new AppDbContext();

            ActivityImage? existingImage = dbContext.ActivityImages
                .FirstOrDefault(image => image.Id == updatedImage.Id);

            if (existingImage == null)
            {
                return;
            }

            if (updatedImage.IsMainImage)
            {
                ClearExistingMainImage(
                    dbContext,
                    updatedImage.ItemId,
                    updatedImage.ItemType);
            }

            existingImage.ItemId = updatedImage.ItemId;
            existingImage.ItemType = updatedImage.ItemType;
            existingImage.ProviderGalleryImageId = updatedImage.ProviderGalleryImageId;
            existingImage.ImagePath = updatedImage.ImagePath;
            existingImage.IsMainImage = updatedImage.IsMainImage;
            existingImage.DisplayOrder = updatedImage.DisplayOrder;
            existingImage.Caption = updatedImage.Caption;

            dbContext.SaveChanges();
        }

        public void DeleteImage(int imageId)
        {
            using AppDbContext dbContext = new AppDbContext();

            ActivityImage? existingImage = dbContext.ActivityImages
                .FirstOrDefault(image => image.Id == imageId);

            if (existingImage == null)
            {
                return;
            }

            dbContext.ActivityImages.Remove(existingImage);
            dbContext.SaveChanges();
        }

        public void DeleteImagesByItem(int itemId, ActivityImageItemType itemType)
        {
            using AppDbContext dbContext = new AppDbContext();

            List<ActivityImage> existingImages = dbContext.ActivityImages
                .Where(image =>
                    image.ItemId == itemId &&
                    image.ItemType == itemType)
                .ToList();

            dbContext.ActivityImages.RemoveRange(existingImages);
            dbContext.SaveChanges();
        }

        public void DeleteActivityImageLinksByProviderGalleryImageId(int providerGalleryImageId)
        {
            using AppDbContext dbContext = new AppDbContext();

            List<ActivityImage> linkedImages = dbContext.ActivityImages
                .Where(image => image.ProviderGalleryImageId == providerGalleryImageId)
                .ToList();

            dbContext.ActivityImages.RemoveRange(linkedImages);
            dbContext.SaveChanges();
        }

        private void ClearExistingMainImage(
            AppDbContext dbContext,
            int itemId,
            ActivityImageItemType itemType)
        {
            List<ActivityImage> existingMainImages = dbContext.ActivityImages
                .Where(image =>
                    image.ItemId == itemId &&
                    image.ItemType == itemType &&
                    image.IsMainImage)
                .ToList();

            foreach (ActivityImage existingMainImage in existingMainImages)
            {
                existingMainImage.IsMainImage = false;
            }
        }
    }
}