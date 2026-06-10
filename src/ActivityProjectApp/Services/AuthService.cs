using ActivityProjectApp.Data;
using ActivityProjectApp.Models;

namespace ActivityProjectApp.Services
{
    public class AuthService
    {
        private readonly FakeUserRepository _userRepository;
        private readonly SessionService _sessionService;

        public AuthService(FakeUserRepository userRepository, SessionService sessionService)
        {
            _userRepository = userRepository;
            _sessionService = sessionService;
        }

        public bool Login(string identifier, string password, out string message)
        {
            User? user = _userRepository.FindByUsernameOrEmail(identifier);

            if (user == null)
            {
                message = "User not found.";
                return false;
            }

            if (user.Password != password)
            {
                message = "Invalid password.";
                return false;
            }

            _sessionService.StartSession(user);

            if (user is Customer)
            {
                message = "Customer login successful.";
            }
            else if (user is ServiceProvider)
            {
                message = "Service provider login successful.";
            }
            else
            {
                message = "Login successful.";
            }

            return true;
        }

        public void Logout()
        {
            _sessionService.EndSession();
        }

        public User? GetCurrentUser()
        {
            return _sessionService.CurrentUser;
        }
    }
}