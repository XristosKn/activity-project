using System;

namespace ActivityProjectApp.Models
{
    public class Customer : User
    {
        public string Gender { get; set; } = string.Empty;

        public DateTime DateBirth { get; set; }

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
            DateTime dateBirth)
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
        }
    }
}