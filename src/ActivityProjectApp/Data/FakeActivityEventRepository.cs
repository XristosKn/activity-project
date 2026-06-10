using System;
using System.Collections.Generic;
using System.Linq;
using ActivityProjectApp.Models;

namespace ActivityProjectApp.Data
{
    public class FakeActivityEventRepository
    {
        private readonly List<ActivityEvent> _events = new List<ActivityEvent>
        {
            new ActivityEvent
            {
                Id = 1,
                Title = "Beginner Kayak Lesson",
                Category = "Water Sports",
                Description = "A beginner-friendly kayak activity near the coast.",
                Latitude = 37.9400,
                Longitude = 23.6400,
                Address = "Piraeus, Greece",
                Date = DateTime.Today.AddDays(3),
                Time = new TimeSpan(10, 30, 0),
                Price = 25.00m,
                MaxSpace = 12,
                Gallery = new List<string>
                {
                    "kayak_1.jpg",
                    "kayak_2.jpg"
                },
                ServiceProviderId = 2,
                Status = EventStatus.Active
            },

            new ActivityEvent
            {
                Id = 2,
                Title = "Outdoor Fitness Session",
                Category = "Fitness & Wellness",
                Description = "Group outdoor training session for all fitness levels.",
                Latitude = 37.9715,
                Longitude = 23.7257,
                Address = "Athens National Garden, Greece",
                Date = DateTime.Today.AddDays(5),
                Time = new TimeSpan(18, 0, 0),
                Price = 15.00m,
                MaxSpace = 20,
                Gallery = new List<string>
                {
                    "fitness_1.jpg"
                },
                ServiceProviderId = 2,
                Status = EventStatus.Active
            }
        };

        public void AddEvent(ActivityEvent activityEvent)
        {
            activityEvent.Id = GetNextId();
            _events.Add(activityEvent);
        }

        public List<ActivityEvent> GetAllEvents()
        {
            return _events;
        }

        public List<ActivityEvent> GetActiveEvents()
        {
            return _events
                .Where(activityEvent => activityEvent.Status == EventStatus.Active)
                .ToList();
        }

        public List<ActivityEvent> GetEventsByServiceProviderId(int serviceProviderId)
        {
            return _events
                .Where(activityEvent => activityEvent.ServiceProviderId == serviceProviderId)
                .ToList();
        }

        public List<ActivityEvent> SearchActiveEvents(string searchText)
        {
            if (string.IsNullOrWhiteSpace(searchText))
            {
                return GetActiveEvents();
            }

            string normalizedSearchText = searchText.ToLower();

            return _events
                .Where(activityEvent => activityEvent.Status == EventStatus.Active)
                .Where(activityEvent =>
                    activityEvent.Title.ToLower().Contains(normalizedSearchText)
                    || activityEvent.Category.ToLower().Contains(normalizedSearchText)
                    || activityEvent.Description.ToLower().Contains(normalizedSearchText)
                    || activityEvent.Address.ToLower().Contains(normalizedSearchText))
                .ToList();
        }

        private int GetNextId()
        {
            if (_events.Count == 0)
            {
                return 1;
            }

            return _events.Max(activityEvent => activityEvent.Id) + 1;
        }
    }
}