using System;

namespace ActivityProjectApp.Models
{
    public enum CourseStatus
    {
        Active,
        Inactive
    }

    public class Course
    {
        public int Id { get; set; }

        public string Title { get; set; } = string.Empty;

        public string Category { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public double Latitude { get; set; }

        public double Longitude { get; set; }

        public string Address { get; set; } = string.Empty;

        public string Days { get; set; } = string.Empty;

        public TimeSpan StartTime { get; set; }

        public TimeSpan EndTime { get; set; }

        public decimal Price { get; set; }

        public int MaxSpace { get; set; }

        public string AgeRestriction { get; set; } = string.Empty;

        public string Gallery { get; set; } = string.Empty;

        public int ServiceProviderId { get; set; }

        public CourseStatus Status { get; set; }
    }
}