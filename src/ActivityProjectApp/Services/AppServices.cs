using ActivityProjectApp.Data;

namespace ActivityProjectApp.Services
{
    public static class AppServices
    {
        public static DatabaseUserRepository UserRepository { get; } = new DatabaseUserRepository();

        public static DatabaseActivityEventRepository ActivityEventRepository { get; } = new DatabaseActivityEventRepository();

        public static DatabaseCourseRepository CourseRepository { get; } = new DatabaseCourseRepository();

        public static DatabaseSavedItemRepository SavedItemRepository { get; } = new DatabaseSavedItemRepository();

        public static DatabaseEnrollmentRepository EnrollmentRepository { get; } = new DatabaseEnrollmentRepository();

        public static DatabaseAnnouncementRepository AnnouncementRepository { get; } = new DatabaseAnnouncementRepository();

        public static DatabaseActivityImageRepository ActivityImageRepository { get; } = new DatabaseActivityImageRepository();

        public static DatabaseProviderGalleryImageRepository ProviderGalleryImageRepository { get; } = new DatabaseProviderGalleryImageRepository();

        public static PaymentSimulationService PaymentSimulationService { get; } = new PaymentSimulationService();

        public static SessionService SessionService { get; } = new SessionService();

        public static AuthService AuthService { get; } = new AuthService(UserRepository, SessionService);
    }
}
