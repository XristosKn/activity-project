using ActivityProjectApp.Data;

namespace ActivityProjectApp.Services
{
    public static class AppServices
    {
        public static FakeUserRepository UserRepository { get; } = new FakeUserRepository();

        public static FakeActivityEventRepository ActivityEventRepository { get; } = new FakeActivityEventRepository();

        public static FakeSavedItemRepository SavedItemRepository { get; } = new FakeSavedItemRepository();

        public static FakeEnrollmentRepository EnrollmentRepository { get; } = new FakeEnrollmentRepository();

        public static PaymentSimulationService PaymentSimulationService { get; } = new PaymentSimulationService();

        public static FakeCourseRepository CourseRepository { get; } = new FakeCourseRepository();

        public static SessionService SessionService { get; } = new SessionService();

        public static AuthService AuthService { get; } = new AuthService(UserRepository, SessionService);
    }
}