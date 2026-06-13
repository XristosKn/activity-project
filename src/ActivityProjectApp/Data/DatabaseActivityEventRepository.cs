using ActivityProjectApp.Database;
using ActivityProjectApp.Models;
using System.Collections.Generic;
using System.Linq;

namespace ActivityProjectApp.Data
{
    public class DatabaseActivityEventRepository
    {
        public void AddEvent(ActivityEvent activityEvent)
        {
            using AppDbContext dbContext = new AppDbContext();

            dbContext.ActivityEvents.Add(activityEvent);
            dbContext.SaveChanges();
        }

        public List<ActivityEvent> GetAllEvents()
        {
            using AppDbContext dbContext = new AppDbContext();

            return dbContext.ActivityEvents
                .ToList();
        }

        public List<ActivityEvent> GetActiveEvents()
        {
            using AppDbContext dbContext = new AppDbContext();

            return dbContext.ActivityEvents
                .Where(activityEvent => activityEvent.Status == EventStatus.Active)
                .ToList();
        }

        public List<ActivityEvent> GetEventsByServiceProviderId(int serviceProviderId)
        {
            using AppDbContext dbContext = new AppDbContext();

            return dbContext.ActivityEvents
                .Where(activityEvent => activityEvent.ServiceProviderId == serviceProviderId)
                .ToList();
        }

        public List<ActivityEvent> SearchActiveEvents(string searchText)
        {
            using AppDbContext dbContext = new AppDbContext();

            if (string.IsNullOrWhiteSpace(searchText))
            {
                return dbContext.ActivityEvents
                    .Where(activityEvent => activityEvent.Status == EventStatus.Active)
                    .ToList();
            }

            string normalizedSearchText = searchText.ToLower();

            return dbContext.ActivityEvents
                .Where(activityEvent => activityEvent.Status == EventStatus.Active)
                .Where(activityEvent =>
                    activityEvent.Title.ToLower().Contains(normalizedSearchText) ||
                    activityEvent.Category.ToLower().Contains(normalizedSearchText) ||
                    activityEvent.Description.ToLower().Contains(normalizedSearchText) ||
                    activityEvent.Address.ToLower().Contains(normalizedSearchText))
                .ToList();
        }

        public ActivityEvent? GetEventById(int eventId)
        {
            using AppDbContext dbContext = new AppDbContext();

            return dbContext.ActivityEvents
                .FirstOrDefault(activityEvent => activityEvent.Id == eventId);
        }

        public void UpdateEvent(ActivityEvent updatedEvent)
        {
            using AppDbContext dbContext = new AppDbContext();

            ActivityEvent? existingEvent = dbContext.ActivityEvents
                .FirstOrDefault(activityEvent => activityEvent.Id == updatedEvent.Id);

            if (existingEvent == null)
            {
                return;
            }

            existingEvent.Title = updatedEvent.Title;
            existingEvent.Category = updatedEvent.Category;
            existingEvent.Description = updatedEvent.Description;
            existingEvent.Latitude = updatedEvent.Latitude;
            existingEvent.Longitude = updatedEvent.Longitude;
            existingEvent.Address = updatedEvent.Address;
            existingEvent.Date = updatedEvent.Date;
            existingEvent.Time = updatedEvent.Time;
            existingEvent.Price = updatedEvent.Price;
            existingEvent.MaxSpace = updatedEvent.MaxSpace;
            existingEvent.MainImagePath = updatedEvent.MainImagePath;
            existingEvent.Gallery = updatedEvent.Gallery;
            existingEvent.ServiceProviderId = updatedEvent.ServiceProviderId;
            existingEvent.Status = updatedEvent.Status;

            dbContext.SaveChanges();
        }

        public void DeleteEvent(int eventId)
        {
            using AppDbContext dbContext = new AppDbContext();

            ActivityEvent? existingEvent = dbContext.ActivityEvents
                .FirstOrDefault(activityEvent => activityEvent.Id == eventId);

            if (existingEvent == null)
            {
                return;
            }

            dbContext.ActivityEvents.Remove(existingEvent);
            dbContext.SaveChanges();
        }
    }
}