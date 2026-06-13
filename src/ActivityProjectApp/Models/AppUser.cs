using System;

namespace ActivityProjectApp.Models
{
    public class AppUser
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public string LastName { get; set; } = string.Empty;

        public string Username { get; set; } = string.Empty;

        public string Password { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string Phone { get; set; } = string.Empty;

        public DateTime EnrollmentDate { get; set; }

        public string Role { get; set; } = string.Empty;

        public string Gender { get; set; } = string.Empty;

        public DateTime? DateBirth { get; set; }

        public string TypeofService { get; set; } = string.Empty;

        public double? CurrentLatitude { get; set; }

        public double? CurrentLongitude { get; set; }

        public string LocationPermissionStatus { get; set; } = "NotRequested";
    }
}