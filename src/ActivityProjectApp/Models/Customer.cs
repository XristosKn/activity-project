using System;

namespace ActivityProjectApp.Models
{
    public class Customer : User
    {
        public string Gender { get; set; } = string.Empty;

        public DateTime DateBirth { get; set; }

        public double? CurrentLatitude { get; set; }

        public double? CurrentLongitude { get; set; }

        public string LocationPermissionStatus { get; set; } = "NotRequested";

        public Customer()
        {
        }

        public Customer(
            int id,
            string name,
            string lastName,
            string username,
            string password,
            string email,
            string phone,
            DateTime enrollmentDate,
            string gender,
            DateTime dateBirth,
            double? currentLatitude,
            double? currentLongitude,
            string locationPermissionStatus)
        {
            Id = id;
            Name = name;
            LastName = lastName;
            Username = username;
            Password = password;
            Email = email;
            Phone = phone;
            EnrollmentDate = enrollmentDate;
            Gender = gender;
            DateBirth = dateBirth;
            CurrentLatitude = currentLatitude;
            CurrentLongitude = currentLongitude;
            LocationPermissionStatus = locationPermissionStatus;
        }
    }
}