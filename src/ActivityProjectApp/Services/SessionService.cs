using ActivityProjectApp.Models;

namespace ActivityProjectApp.Services
{
    public class SessionService
    {
        public User? CurrentUser { get; private set; }

        public bool IsLoggedIn => CurrentUser != null;

        public void StartSession(User user)
        {
            CurrentUser = user;
        }

        public void EndSession()
        {
            CurrentUser = null;
        }
    }
}