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
                Gender = "Not specified",
                DateBirth = new DateTime(2000, 1, 1)
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
                TypeofService = "Sports"
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
            user.Id = _users.Count + 1;
            _users.Add(user);
        }

        public List<User> GetAllUsers()
        {
            return _users;
        }
    }
}