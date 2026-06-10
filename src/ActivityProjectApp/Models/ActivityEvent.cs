using System;
using System.Collections.Generic;

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

        public List<string> Gallery { get; set; } = new List<string>();

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
            List<string> gallery,
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
            Gallery = gallery;
            ServiceProviderId = serviceProviderId;
            Status = status;
        }
    }
}