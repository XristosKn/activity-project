namespace ActivityProjectApp.Models
{
    public enum SavedItemType
    {
        Event,
        Course
    }

    public class SavedItem
    {
        public int Id { get; set; }

        public int CustomerId { get; set; }

        public int ItemId { get; set; }

        public SavedItemType ItemType { get; set; }

    }
}