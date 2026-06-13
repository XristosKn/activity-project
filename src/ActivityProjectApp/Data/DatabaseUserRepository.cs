using ActivityProjectApp.Database;
using ActivityProjectApp.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ActivityProjectApp.Data
{
    public class DatabaseUserRepository
    {
        public User? FindByUsernameOrEmail(string identifier)
        {
            using AppDbContext dbContext = new AppDbContext();

            string normalizedIdentifier = identifier.Trim().ToLower();

            AppUser? appUser = dbContext.Users
                .FirstOrDefault(user =>
                    user.Username.ToLower() == normalizedIdentifier ||
                    user.Email.ToLower() == normalizedIdentifier);

            if (appUser == null)
            {
                return null;
            }

            return ConvertAppUserToUser(appUser);
        }

        public bool UsernameExists(string username)
        {
            using AppDbContext dbContext = new AppDbContext();

            string normalizedUsername = username.Trim().ToLower();

            return dbContext.Users
                .Any(user => user.Username.ToLower() == normalizedUsername);
        }

        public bool EmailExists(string email)
        {
            using AppDbContext dbContext = new AppDbContext();

            string normalizedEmail = email.Trim().ToLower();

            return dbContext.Users
                .Any(user => user.Email.ToLower() == normalizedEmail);
        }

        public void AddUser(User user)
        {
            using AppDbContext dbContext = new AppDbContext();

            user.EnrollmentDate = DateTime.Now;

            AppUser appUser = ConvertUserToAppUser(user);

            dbContext.Users.Add(appUser);
            dbContext.SaveChanges();

            user.Id = appUser.Id;
        }

        public List<User> GetAllUsers()
        {
            using AppDbContext dbContext = new AppDbContext();

            return dbContext.Users
                .ToList()
                .Select(ConvertAppUserToUser)
                .Where(user => user != null)
                .Select(user => user!)
                .ToList();
        }

        public List<Customer> GetAllCustomers()
        {
            return GetAllUsers()
                .OfType<Customer>()
                .ToList();
        }

        public List<ServiceProvider> GetAllServiceProviders()
        {
            return GetAllUsers()
                .OfType<ServiceProvider>()
                .ToList();
        }

        public User? GetUserById(int id)
        {
            using AppDbContext dbContext = new AppDbContext();

            AppUser? appUser = dbContext.Users
                .FirstOrDefault(user => user.Id == id);

            if (appUser == null)
            {
                return null;
            }

            return ConvertAppUserToUser(appUser);
        }

        private AppUser ConvertUserToAppUser(User user)
        {
            if (user is Customer customer)
            {
                return new AppUser
                {
                    Name = customer.Name,
                    LastName = customer.LastName,
                    Username = customer.Username,
                    Password = customer.Password,
                    Email = customer.Email,
                    Phone = customer.Phone,
                    EnrollmentDate = customer.EnrollmentDate,
                    Role = "Customer",
                    Gender = customer.Gender,
                    DateBirth = customer.DateBirth,
                    TypeofService = string.Empty,
                    CurrentLatitude = customer.CurrentLatitude,
                    CurrentLongitude = customer.CurrentLongitude,
                    LocationPermissionStatus = customer.LocationPermissionStatus
                };
            }

            if (user is ServiceProvider serviceProvider)
            {
                return new AppUser
                {
                    Name = serviceProvider.Name,
                    LastName = serviceProvider.LastName,
                    Username = serviceProvider.Username,
                    Password = serviceProvider.Password,
                    Email = serviceProvider.Email,
                    Phone = serviceProvider.Phone,
                    EnrollmentDate = serviceProvider.EnrollmentDate,
                    Role = "ServiceProvider",
                    Gender = string.Empty,
                    DateBirth = null,
                    TypeofService = serviceProvider.TypeofService,
                    CurrentLatitude = null,
                    CurrentLongitude = null,
                    LocationPermissionStatus = "NotRequested"
                };
            }

            return new AppUser
            {
                Name = user.Name,
                LastName = user.LastName,
                Username = user.Username,
                Password = user.Password,
                Email = user.Email,
                Phone = user.Phone,
                EnrollmentDate = user.EnrollmentDate,
                Role = "User",
                Gender = string.Empty,
                DateBirth = null,
                TypeofService = string.Empty,
                CurrentLatitude = null,
                CurrentLongitude = null,
                LocationPermissionStatus = "NotRequested"
            };
        }

        private User? ConvertAppUserToUser(AppUser appUser)
        {
            if (appUser.Role == "Customer")
            {
                return new Customer
                {
                    Id = appUser.Id,
                    Name = appUser.Name,
                    LastName = appUser.LastName,
                    Username = appUser.Username,
                    Password = appUser.Password,
                    Email = appUser.Email,
                    Phone = appUser.Phone,
                    EnrollmentDate = appUser.EnrollmentDate,
                    Gender = appUser.Gender,
                    DateBirth = appUser.DateBirth ?? DateTime.MinValue,
                    CurrentLatitude = appUser.CurrentLatitude,
                    CurrentLongitude = appUser.CurrentLongitude,
                    LocationPermissionStatus = appUser.LocationPermissionStatus
                };
            }

            if (appUser.Role == "ServiceProvider")
            {
                return new ServiceProvider
                {
                    Id = appUser.Id,
                    Name = appUser.Name,
                    LastName = appUser.LastName,
                    Username = appUser.Username,
                    Password = appUser.Password,
                    Email = appUser.Email,
                    Phone = appUser.Phone,
                    EnrollmentDate = appUser.EnrollmentDate,
                    TypeofService = appUser.TypeofService
                };
            }

            return null;
        }
    }
}