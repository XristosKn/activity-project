using System;
using System.Collections.Generic;
using System.Linq;
using ActivityProjectApp.Models;

namespace ActivityProjectApp.Data
{
    public class FakeUserRepository
    {
        private readonly List<User> _users = new List<User>
        {
            new Customer
            {
                Id = 1,
                Name = "Test",
                LastName = "Customer",
                Username = "customer",
                Password = "1234",
                Email = "customer@test.com",
                Phone = "6900000000",
                EnrollmentDate = DateTime.Now,
                Gender = "Prefer not to say",
                DateBirth = new DateTime(2000, 1, 1),

                CurrentLatitude = null,
                CurrentLongitude = null,
                LocationPermissionStatus = "NotRequested"
            },

            new ServiceProvider
            {
                Id = 2,
                Name = "Test",
                LastName = "Provider",
                Username = "provider",
                Password = "1234",
                Email = "provider@test.com",
                Phone = "6911111111",
                EnrollmentDate = DateTime.Now,
                TypeofService = "Water Sports, Outdoor Adventure"
            }
        };

        public User? FindByUsernameOrEmail(string identifier)
        {
            return _users.FirstOrDefault(user =>
                user.Username.Equals(identifier, StringComparison.OrdinalIgnoreCase)
                || user.Email.Equals(identifier, StringComparison.OrdinalIgnoreCase));
        }

        public bool UsernameExists(string username)
        {
            return _users.Any(user =>
                user.Username.Equals(username, StringComparison.OrdinalIgnoreCase));
        }

        public bool EmailExists(string email)
        {
            return _users.Any(user =>
                user.Email.Equals(email, StringComparison.OrdinalIgnoreCase));
        }

        public void AddUser(User user)
        {
            user.Id = GetNextId();
            user.EnrollmentDate = DateTime.Now;

            _users.Add(user);
        }

        public List<User> GetAllUsers()
        {
            return _users;
        }

        public List<Customer> GetAllCustomers()
        {
            return _users
                .OfType<Customer>()
                .ToList();
        }

        public List<ServiceProvider> GetAllServiceProviders()
        {
            return _users
                .OfType<ServiceProvider>()
                .ToList();
        }

        public User? GetUserById(int id)
        {
            return _users.FirstOrDefault(user => user.Id == id);
        }

        private int GetNextId()
        {
            if (_users.Count == 0)
            {
                return 1;
            }

            return _users.Max(user => user.Id) + 1;
        }
    }
}