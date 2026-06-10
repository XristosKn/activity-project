using System;

namespace ActivityProjectApp.Models
{
    public class ServiceProvider : User
    {
        public string TypeofService { get; set; } = string.Empty;

        public ServiceProvider()
        {
        }

        public ServiceProvider(
            int id,
            string name,
            string lastName,
            string username,
            string password,
            string email,
            string phone,
            DateTime enrollmentDate,
            string typeofservice)
        {
            Id = id;
            Name = name;
            LastName = lastName;
            Username = username;
            Password = password;
            Email = email;
            Phone = phone;
            EnrollmentDate = enrollmentDate;
            TypeofService = typeofservice;
        }
    }
}