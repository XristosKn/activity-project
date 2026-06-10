using System.Collections.Generic;
using System.Linq;
using ActivityProjectApp.Models;

namespace ActivityProjectApp.Data
{
    public class FakeActivityEventRepository
    {
        private readonly List<ActivityEvent> _events = new List<ActivityEvent>();

        public void AddEvent(ActivityEvent activityEvent)
        {
            activityEvent.Id = GetNextId();
            _events.Add(activityEvent);
        }

        public List<ActivityEvent> GetAllEvents()
        {
            return _events;
        }

        public List<ActivityEvent> GetEventsByServiceProviderId(int serviceProviderId)
        {
            return _events
                .Where(activityEvent => activityEvent.ServiceProviderId == serviceProviderId)
                .ToList();
        }

        public ActivityEvent? GetEventById(int id)
        {
            return _events.FirstOrDefault(activityEvent => activityEvent.Id == id);
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