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
        public Course? GetCourseById(int courseId)
        {
            using AppDbContext dbContext = new AppDbContext();

            return dbContext.Courses
                .FirstOrDefault(course => course.Id == courseId);
        }

        public void UpdateCourse(Course updatedCourse)
        {
            using AppDbContext dbContext = new AppDbContext();

            Course? existingCourse = dbContext.Courses
                .FirstOrDefault(course => course.Id == updatedCourse.Id);

            if (existingCourse == null)
            {
                return;
            }

            existingCourse.Title = updatedCourse.Title;
            existingCourse.Category = updatedCourse.Category;
            existingCourse.Description = updatedCourse.Description;
            existingCourse.Latitude = updatedCourse.Latitude;
            existingCourse.Longitude = updatedCourse.Longitude;
            existingCourse.Address = updatedCourse.Address;
            existingCourse.Days = updatedCourse.Days;
            existingCourse.StartTime = updatedCourse.StartTime;
            existingCourse.EndTime = updatedCourse.EndTime;
            existingCourse.Price = updatedCourse.Price;
            existingCourse.MaxSpace = updatedCourse.MaxSpace;
            existingCourse.AgeRestriction = updatedCourse.AgeRestriction;
            existingCourse.MainImagePath = updatedCourse.MainImagePath;
            existingCourse.Gallery = updatedCourse.Gallery;
            existingCourse.ServiceProviderId = updatedCourse.ServiceProviderId;
            existingCourse.Status = updatedCourse.Status;

            dbContext.SaveChanges();
        }

        public void DeleteCourse(int courseId)
        {
            using AppDbContext dbContext = new AppDbContext();

            Course? existingCourse = dbContext.Courses
                .FirstOrDefault(course => course.Id == courseId);

            if (existingCourse == null)
            {
                return;
            }

            dbContext.Courses.Remove(existingCourse);
            dbContext.SaveChanges();
        }
    }
}