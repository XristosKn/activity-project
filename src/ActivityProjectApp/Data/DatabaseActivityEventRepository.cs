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
    }
}