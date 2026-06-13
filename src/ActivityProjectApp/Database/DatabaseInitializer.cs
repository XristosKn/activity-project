using ActivityProjectApp.Models;
using System;
using System.Linq;

namespace ActivityProjectApp.Database
{
    public static class DatabaseInitializer
    {
        public static void Initialize()
        {
            using AppDbContext dbContext = new AppDbContext();

            dbContext.Database.EnsureCreated();

            SeedActivityEvents(dbContext);
            SeedCourses(dbContext);
        }

        private static void SeedActivityEvents(AppDbContext dbContext)
        {
            if (dbContext.ActivityEvents.Any())
            {
                return;
            }

            dbContext.ActivityEvents.Add(new ActivityEvent
            {
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
                MainImagePath = "Assets/EventImages/kayak_main.jpg",
                Gallery = "Assets/EventImages/kayak_1.jpg;Assets/EventImages/kayak_2.jpg",
                ServiceProviderId = 2,
                Status = EventStatus.Active
            });

            dbContext.ActivityEvents.Add(new ActivityEvent
            {
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
                MainImagePath = "Assets/EventImages/fitness_main.jpg",
                Gallery = "Assets/EventImages/fitness_1.jpg",
                ServiceProviderId = 2,
                Status = EventStatus.Active
            });

            dbContext.SaveChanges();
        }

        private static void SeedCourses(AppDbContext dbContext)
        {
            if (dbContext.Courses.Any())
            {
                return;
            }

            dbContext.Courses.Add(new Course
            {
                Title = "Kick Boxing Beginner Course",
                Category = "Fitness & Wellness",
                Description = "A beginner-friendly kick boxing course focused on basic techniques, stamina and coordination.",
                Latitude = 37.9838,
                Longitude = 23.7275,
                Address = "Athens Center, Greece",
                Days = "Monday, Wednesday, Friday",
                StartTime = new TimeSpan(18, 0, 0),
                EndTime = new TimeSpan(19, 30, 0),
                Price = 45,
                MaxSpace = 20,
                AgeRestriction = "16+",
                MainImagePath = "Assets/CourseImages/kickboxing_main.jpg",
                Gallery = "Assets/CourseImages/kickboxing_1.jpg;Assets/CourseImages/kickboxing_2.jpg",
                ServiceProviderId = 2,
                Status = CourseStatus.Active
            });

            dbContext.Courses.Add(new Course
            {
                Title = "Swimming Technique Course",
                Category = "Water Sports",
                Description = "A structured swimming course for improving breathing, technique and endurance.",
                Latitude = 37.9400,
                Longitude = 23.6400,
                Address = "Piraeus, Greece",
                Days = "Tuesday, Thursday",
                StartTime = new TimeSpan(17, 30, 0),
                EndTime = new TimeSpan(18, 30, 0),
                Price = 55,
                MaxSpace = 15,
                AgeRestriction = "12+",
                MainImagePath = "Assets/CourseImages/swimming_main.jpg",
                Gallery = "Assets/CourseImages/swimming_1.jpg;Assets/CourseImages/swimming_2.jpg",
                ServiceProviderId = 2,
                Status = CourseStatus.Active
            });

            dbContext.SaveChanges();
        }
    }
}