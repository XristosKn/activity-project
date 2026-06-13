using ActivityProjectApp.Models;
using Microsoft.EntityFrameworkCore;

namespace ActivityProjectApp.Database
{
    public class AppDbContext : DbContext
    {
        public DbSet<ActivityEvent> ActivityEvents { get; set; }
        public DbSet<Course> Courses { get; set; }
        public DbSet<Announcement> Announcements { get; set; }
        public DbSet<SavedItem> SavedItems { get; set; }
        public DbSet<Enrollment> Enrollments { get; set; }
        public DbSet<AnnouncementTarget> AnnouncementTargets { get; set; }
        public DbSet<AppUser> Users { get; set; }
        public string DatabasePath { get; }

        public AppDbContext()
        {
            DatabasePath = "activity_project.db";
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite($"Data Source={DatabasePath}");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<AppUser>().HasKey(user => user.Id);
            modelBuilder.Entity<ActivityEvent>().HasKey(activityEvent => activityEvent.Id);
            modelBuilder.Entity<Course>().HasKey(course => course.Id);
            modelBuilder.Entity<Announcement>().HasKey(announcement => announcement.Id);
            modelBuilder.Entity<AnnouncementTarget>().HasKey(announcementTarget => announcementTarget.Id);
            modelBuilder.Entity<SavedItem>().HasKey(savedItem => savedItem.Id);
            modelBuilder.Entity<Enrollment>().HasKey(enrollment => enrollment.Id);
            
            
        }
    }
}