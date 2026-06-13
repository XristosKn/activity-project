using System;
using System.Collections.Generic;
using ActivityProjectApp.Data;
using ActivityProjectApp.Models;

namespace ActivityProjectApp.Services
{
    public class AuthService
    {
        private readonly DatabaseUserRepository _userRepository;
        private readonly SessionService _sessionService;

        public AuthService(DatabaseUserRepository userRepository, SessionService sessionService)
        {
            _userRepository = userRepository;
            _sessionService = sessionService;
        }

        public bool Login(string identifier, string password, out string message)
        {
            User? user = _userRepository.FindByUsernameOrEmail(identifier);

            if (user == null)
            {
                message = "User not found.";
                return false;
            }

            if (user.Password != password)
            {
                message = "Invalid password.";
                return false;
            }

            _sessionService.StartSession(user);

            if (user is Customer)
            {
                message = "Customer login successful.";
            }
            else if (user is ServiceProvider)
            {
                message = "Service provider login successful.";
            }
            else
            {
                message = "Login successful.";
            }

            return true;
        }

        public bool RegisterCustomer(
            string name,
            string lastName,
            string username,
            string email,
            string phone,
            string password,
            string gender,
            DateTime dateOfBirth,
            out string message)
        {
            if (_userRepository.UsernameExists(username))
            {
                message = "Username already exists.";
                return false;
            }

            if (_userRepository.EmailExists(email))
            {
                message = "Email already exists.";
                return false;
            }

            Customer customer = new Customer
            {
                Name = name,
                LastName = lastName,
                Username = username,
                Email = email,
                Phone = phone,
                Password = password,
                EnrollmentDate = DateTime.Now,
                Gender = gender,
                DateBirth = dateOfBirth,

                CurrentLatitude = null,
                CurrentLongitude = null,
                LocationPermissionStatus = "NotRequested"
            };

            _userRepository.AddUser(customer);

            message = "Customer account created successfully.";
            return true;
        }

        public bool RegisterServiceProvider(
            string name,
            string lastName,
            string username,
            string email,
            string phone,
            string password,
            List<string> selectedTypesOfService,
            out string message)
        {
            if (_userRepository.UsernameExists(username))
            {
                message = "Username already exists.";
                return false;
            }

            if (_userRepository.EmailExists(email))
            {
                message = "Email already exists.";
                return false;
            }

            ServiceProvider serviceProvider = new ServiceProvider
            {
                Name = name,
                LastName = lastName,
                Username = username,
                Email = email,
                Phone = phone,
                Password = password,
                EnrollmentDate = DateTime.Now,
                TypeofService = string.Join(", ", selectedTypesOfService)
            };

            _userRepository.AddUser(serviceProvider);

            message = "Service Provider account created successfully.";
            return true;
        }

        public void Logout()
        {
            _sessionService.EndSession();
        }

        public User? GetCurrentUser()
        {
            return _sessionService.CurrentUser;
        }
    }
}