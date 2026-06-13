using ActivityProjectApp.Database;
using ActivityProjectApp.Models;
using System.Collections.Generic;
using System.Linq;

namespace ActivityProjectApp.Data
{
    public class DatabaseSavedItemRepository
    {
        public List<SavedItem> GetSavedItemsByCustomerId(int customerId)
        {
            using AppDbContext dbContext = new AppDbContext();

            return dbContext.SavedItems
                .Where(savedItem => savedItem.CustomerId == customerId)
                .ToList();
        }

        public bool IsItemSaved(int customerId, int itemId, SavedItemType itemType)
        {
            using AppDbContext dbContext = new AppDbContext();

            return dbContext.SavedItems
                .Any(savedItem =>
                    savedItem.CustomerId == customerId &&
                    savedItem.ItemId == itemId &&
                    savedItem.ItemType == itemType);
        }

        public void SaveItem(int customerId, int itemId, SavedItemType itemType)
        {
            using AppDbContext dbContext = new AppDbContext();

            bool alreadySaved = dbContext.SavedItems
                .Any(savedItem =>
                    savedItem.CustomerId == customerId &&
                    savedItem.ItemId == itemId &&
                    savedItem.ItemType == itemType);

            if (alreadySaved)
            {
                return;
            }

            SavedItem savedItem = new SavedItem
            {
                CustomerId = customerId,
                ItemId = itemId,
                ItemType = itemType
            };

            dbContext.SavedItems.Add(savedItem);
            dbContext.SaveChanges();
        }

        public void RemoveSavedItem(int customerId, int itemId, SavedItemType itemType)
        {
            using AppDbContext dbContext = new AppDbContext();

            SavedItem? savedItem = dbContext.SavedItems
                .FirstOrDefault(item =>
                    item.CustomerId == customerId &&
                    item.ItemId == itemId &&
                    item.ItemType == itemType);

            if (savedItem == null)
            {
                return;
            }

            dbContext.SavedItems.Remove(savedItem);
            dbContext.SaveChanges();
        }
    }
}