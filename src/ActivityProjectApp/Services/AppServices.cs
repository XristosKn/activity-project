using ActivityProjectApp.Data;

namespace ActivityProjectApp.Services
{
    public static class AppServices
    {
        public static FakeUserRepository UserRepository { get; } = new FakeUserRepository();

        public static FakeActivityEventRepository ActivityEventRepository { get; } = new FakeActivityEventRepository();

        public static SessionService SessionService { get; } = new SessionService();

        public static AuthService AuthService { get; } = new AuthService(UserRepository, SessionService);
    }
}