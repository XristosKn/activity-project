using ActivityProjectApp.Models;
using System.Collections.Generic;
using System.Linq;

namespace ActivityProjectApp.Data
{
    public class FakeSavedItemRepository
    {
        private readonly List<SavedItem> _savedItems = new List<SavedItem>();

        private int _nextId = 1;

        public List<SavedItem> GetSavedItemsByCustomerId(int customerId)
        {
            return _savedItems
                .Where(savedItem => savedItem.CustomerId == customerId)
                .ToList();
        }

        public bool IsItemSaved(int customerId, int itemId, SavedItemType itemType)
        {
            return _savedItems.Any(savedItem =>
                savedItem.CustomerId == customerId &&
                savedItem.ItemId == itemId &&
                savedItem.ItemType == itemType);
        }

        public void SaveItem(int customerId, int itemId, SavedItemType itemType)
        {
            bool alreadySaved = IsItemSaved(customerId, itemId, itemType);

            if (alreadySaved)
            {
                return;
            }

            SavedItem savedItem = new SavedItem
            {
                Id = _nextId,
                CustomerId = customerId,
                ItemId = itemId,
                ItemType = itemType
            };

            _savedItems.Add(savedItem);

            _nextId++;
        }

        public void RemoveSavedItem(int customerId, int itemId, SavedItemType itemType)
        {
            SavedItem? savedItem = _savedItems.FirstOrDefault(item =>
                item.CustomerId == customerId &&
                item.ItemId == itemId &&
                item.ItemType == itemType);

            if (savedItem == null)
            {
                return;
            }

            _savedItems.Remove(savedItem);
        }
    }
}