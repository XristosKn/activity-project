using ActivityProjectApp.Database;
using ActivityProjectApp.Models;
using System.Collections.Generic;
using System.Linq;

namespace ActivityProjectApp.Data
{
    public class DatabaseCourseRepository
    {
        public void AddCourse(Course course)
        {
            using AppDbContext dbContext = new AppDbContext();

            dbContext.Courses.Add(course);
            dbContext.SaveChanges();
        }

        public List<Course> GetAllCourses()
        {
            using AppDbContext dbContext = new AppDbContext();

            return dbContext.Courses
                .ToList();
        }

        public List<Course> GetActiveCourses()
        {
            using AppDbContext dbContext = new AppDbContext();

            return dbContext.Courses
                .Where(course => course.Status == CourseStatus.Active)
                .ToList();
        }

        public List<Course> GetCoursesByServiceProviderId(int serviceProviderId)
        {
            using AppDbContext dbContext = new AppDbContext();

            return dbContext.Courses
                .Where(course => course.ServiceProviderId == serviceProviderId)
                .ToList();
        }

        public List<Course> SearchActiveCourses(string searchText)
        {
            using AppDbContext dbContext = new AppDbContext();

            if (string.IsNullOrWhiteSpace(searchText))
            {
                return dbContext.Courses
                    .Where(course => course.Status == CourseStatus.Active)
                    .ToList();
            }

            string normalizedSearchText = searchText.ToLower();

            return dbContext.Courses
                .Where(course => course.Status == CourseStatus.Active)
                .Where(course =>
                    course.Title.ToLower().Contains(normalizedSearchText) ||
                    course.Category.ToLower().Contains(normalizedSearchText) ||
                    course.Description.ToLower().Contains(normalizedSearchText) ||
                    course.Address.ToLower().Contains(normalizedSearchText) ||
                    course.Days.ToLower().Contains(normalizedSearchText) ||
                    course.AgeRestriction.ToLower().Contains(normalizedSearchText))
                .ToList();
        }
    }
}