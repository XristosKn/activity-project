using System;

namespace ActivityProjectApp.Models
{
    public enum EventStatus
    {
        Active,
        Inactive
    }

    public class ActivityEvent
    {
        public int Id { get; set; }

        public string Title { get; set; } = string.Empty;

        public string Category { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public double Latitude { get; set; }

        public double Longitude { get; set; }

        public string Address { get; set; } = string.Empty;

        public DateTime Date { get; set; }

        public TimeSpan Time { get; set; }

        public decimal Price { get; set; }

        public int MaxSpace { get; set; }

        public string MainImagePath { get; set; } = string.Empty;

        public string Gallery { get; set; } = string.Empty;

        public int ServiceProviderId { get; set; }

        public EventStatus Status { get; set; } = EventStatus.Active;

        public ActivityEvent()
        {
        }

        public ActivityEvent(
            int id,
            string title,
            string category,
            string description,
            double latitude,
            double longitude,
            string address,
            DateTime date,
            TimeSpan time,
            decimal price,
            int maxSpace,
            string mainImagePath,
            string gallery,
            int serviceProviderId,
            EventStatus status)
        {
            Id = id;
            Title = title;
            Category = category;
            Description = description;
            Latitude = latitude;
            Longitude = longitude;
            Address = address;
            Date = date;
            Time = time;
            Price = price;
            MaxSpace = maxSpace;
            MainImagePath = mainImagePath;
            Gallery = gallery;
            ServiceProviderId = serviceProviderId;
            Status = status;
        }
    }
}