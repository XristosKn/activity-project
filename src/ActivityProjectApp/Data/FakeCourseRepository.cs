using ActivityProjectApp.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ActivityProjectApp.Data
{
    public class FakeCourseRepository
    {
        private readonly List<Course> _courses = new List<Course>();

        private int _nextId = 1;

        public FakeCourseRepository()
        {
            SeedCourses();
        }

        private void SeedCourses()
        {
            AddCourse(new Course
            {
                Title = "Kick Boxing Beginner Course",
                Category = "Fitness & Wellness",
                Description = "A beginner-friendly kick boxing course focused on basic techniques, stamina and coordination.",
                Latitude = 37.9838,
                Longitude = 23.7275,
                Address = "Athens Center, Greece",
                Days = "Monday, Wednesday, Friday",
                StartTime = new TimeSpan(18, 00, 00),
                EndTime = new TimeSpan(19, 30, 00),
                Price = 45,
                MaxSpace = 20,
                AgeRestriction = "16+",
                Gallery = "",
                ServiceProviderId = 2,
                Status = CourseStatus.Active
            });

            AddCourse(new Course
            {
                Title = "Swimming Technique Course",
                Category = "Water Sports",
                Description = "A structured swimming course for improving breathing, technique and endurance.",
                Latitude = 37.9400,
                Longitude = 23.6400,
                Address = "Piraeus, Greece",
                Days = "Tuesday, Thursday",
                StartTime = new TimeSpan(17, 30, 00),
                EndTime = new TimeSpan(18, 30, 00),
                Price = 55,
                MaxSpace = 15,
                AgeRestriction = "12+",
                Gallery = "",
                ServiceProviderId = 2,
                Status = CourseStatus.Active
            });
        }

        public void AddCourse(Course course)
        {
            course.Id = _nextId;
            _nextId++;

            _courses.Add(course);
        }

        public List<Course> GetAllCourses()
        {
            return _courses.ToList();
        }

        public List<Course> GetActiveCourses()
        {
            return _courses
                .Where(course => course.Status == CourseStatus.Active)
                .ToList();
        }

        public List<Course> GetCoursesByServiceProviderId(int serviceProviderId)
        {
            return _courses
                .Where(course => course.ServiceProviderId == serviceProviderId)
                .ToList();
        }

        public List<Course> SearchActiveCourses(string searchText)
        {
            if (string.IsNullOrWhiteSpace(searchText))
            {
                return GetActiveCourses();
            }

            string normalizedSearchText = searchText.ToLower();

            return _courses
                .Where(course =>
                    course.Status == CourseStatus.Active &&
                    (
                        course.Title.ToLower().Contains(normalizedSearchText) ||
                        course.Category.ToLower().Contains(normalizedSearchText) ||
                        course.Description.ToLower().Contains(normalizedSearchText) ||
                        course.Address.ToLower().Contains(normalizedSearchText) ||
                        course.Days.ToLower().Contains(normalizedSearchText) ||
                        course.AgeRestriction.ToLower().Contains(normalizedSearchText)
                    ))
                .ToList();
        }
    }
}