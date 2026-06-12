using Avalonia.Controls;
using Avalonia.Interactivity;
using ActivityProjectApp.Models;
using ActivityProjectApp.Services;
using System.Collections.Generic;
using System.Linq;

namespace ActivityProjectApp.Views
{
    public partial class CustomerProfileView : UserControl
    {
        private readonly AuthService _authService;

        public CustomerProfileView()
        {
            InitializeComponent();

            _authService = AppServices.AuthService;

            LoadCustomerInfo();
            LoadSavedItems();
            LoadMyEnrolledItems();

            BackButton.Click += BackButton_Click;
            LogoutButton.Click += LogoutButton_Click;

            SavedItemsFilterComboBox.SelectionChanged += SavedItemsFilterComboBox_SelectionChanged;
            MyItemsFilterComboBox.SelectionChanged += MyItemsFilterComboBox_SelectionChanged;
        }

        private void LoadCustomerInfo()
        {
            User? currentUser = _authService.GetCurrentUser();

            if (currentUser is Customer customer)
            {
                CustomerInfoTextBlock.Text = $"{customer.Name} {customer.LastName} | {customer.Email}";
            }
            else
            {
                CustomerInfoTextBlock.Text = "Customer profile";
            }
        }

        private void LoadSavedItems()
        {
            User? currentUser = _authService.GetCurrentUser();

            if (currentUser is not Customer customer)
            {
                SavedItemsControl.ItemsSource = null;
                SavedItemsCountTextBlock.Text = "0 saved";
                return;
            }

            List<SavedItem> savedItems = AppServices.SavedItemRepository
                .GetSavedItemsByCustomerId(customer.Id);

            List<CustomerProfileItem> profileItems = new List<CustomerProfileItem>();

            foreach (SavedItem savedItem in savedItems)
            {
                CustomerProfileItem? profileItem = BuildProfileItemFromSavedItem(savedItem);

                if (profileItem != null)
                {
                    profileItems.Add(profileItem);
                }
            }

            string selectedFilter = GetSelectedSavedItemsFilter();

            if (selectedFilter == "Events")
            {
                profileItems = profileItems
                    .Where(item => item.ItemType == EnrollmentItemType.Event)
                    .ToList();
            }

            if (selectedFilter == "Courses")
            {
                profileItems = profileItems
                    .Where(item => item.ItemType == EnrollmentItemType.Course)
                    .ToList();
            }

            SavedItemsControl.ItemsSource = profileItems;
            SavedItemsCountTextBlock.Text = $"{profileItems.Count} saved";
        }

        private void SavedItemsFilterComboBox_SelectionChanged(object? sender, SelectionChangedEventArgs e)
        {
            LoadSavedItems();
        }

        private void MyItemsFilterComboBox_SelectionChanged(object? sender, SelectionChangedEventArgs e)
        {
            LoadMyEnrolledItems();
        }

        private void LoadMyEnrolledItems()
        {
            User? currentUser = _authService.GetCurrentUser();

            if (currentUser is not Customer customer)
            {
                MyItemsControl.ItemsSource = null;
                MyItemsCountTextBlock.Text = "0 enrolled";
                return;
            }

            List<Enrollment> enrollments = AppServices.EnrollmentRepository
                .GetEnrollmentsByCustomerId(customer.Id)
                .Where(enrollment => enrollment.Status == EnrollmentStatus.Confirmed)
                .ToList();

            List<CustomerProfileItem> profileItems = new List<CustomerProfileItem>();

            foreach (Enrollment enrollment in enrollments)
            {
                CustomerProfileItem? profileItem = BuildProfileItemFromEnrollment(enrollment);

                if (profileItem != null)
                {
                    profileItems.Add(profileItem);
                }
            }

            string selectedFilter = GetSelectedMyItemsFilter();

            if (selectedFilter == "Events")
            {
                profileItems = profileItems
                    .Where(item => item.ItemType == EnrollmentItemType.Event)
                    .ToList();
            }

            if (selectedFilter == "Courses")
            {
                profileItems = profileItems
                    .Where(item => item.ItemType == EnrollmentItemType.Course)
                    .ToList();
            }

            MyItemsControl.ItemsSource = profileItems;
            MyItemsCountTextBlock.Text = $"{profileItems.Count} enrolled";
        }

        private CustomerProfileItem? BuildProfileItemFromSavedItem(SavedItem savedItem)
        {
            if (savedItem.ItemType == SavedItemType.Event)
            {
                ActivityEvent? activityEvent = AppServices.ActivityEventRepository
                    .GetAllEvents()
                    .FirstOrDefault(item => item.Id == savedItem.ItemId);

                if (activityEvent == null)
                {
                    return null;
                }

                return new CustomerProfileItem
                {
                    ItemId = activityEvent.Id,
                    ItemType = EnrollmentItemType.Event,
                    ItemTypeText = "Event",
                    Title = activityEvent.Title,
                    Category = activityEvent.Category,
                    ScheduleText = $"{activityEvent.Date:dd/MM/yyyy} at {activityEvent.Time:hh\\:mm}",
                    StatusText = "Saved"
                };
            }

            if (savedItem.ItemType == SavedItemType.Course)
            {
                Course? course = AppServices.CourseRepository
                    .GetAllCourses()
                    .FirstOrDefault(item => item.Id == savedItem.ItemId);

                if (course == null)
                {
                    return null;
                }

                return new CustomerProfileItem
                {
                    ItemId = course.Id,
                    ItemType = EnrollmentItemType.Course,
                    ItemTypeText = "Course",
                    Title = course.Title,
                    Category = course.Category,
                    ScheduleText = $"{course.Days}, {course.StartTime:hh\\:mm} - {course.EndTime:hh\\:mm}",
                    StatusText = "Saved"
                };
            }

            return null;
        }

        private CustomerProfileItem? BuildProfileItemFromEnrollment(Enrollment enrollment)
        {
            if (enrollment.ItemType == EnrollmentItemType.Event)
            {
                ActivityEvent? activityEvent = AppServices.ActivityEventRepository
                    .GetAllEvents()
                    .FirstOrDefault(item => item.Id == enrollment.ItemId);

                if (activityEvent == null)
                {
                    return null;
                }

                return new CustomerProfileItem
                {
                    ItemId = activityEvent.Id,
                    ItemType = EnrollmentItemType.Event,
                    ItemTypeText = "Event",
                    Title = activityEvent.Title,
                    Category = activityEvent.Category,
                    ScheduleText = $"{activityEvent.Date:dd/MM/yyyy} at {activityEvent.Time:hh\\:mm}",
                    StatusText = enrollment.Status.ToString()
                };
            }

            if (enrollment.ItemType == EnrollmentItemType.Course)
            {
                Course? course = AppServices.CourseRepository
                    .GetAllCourses()
                    .FirstOrDefault(item => item.Id == enrollment.ItemId);

                if (course == null)
                {
                    return null;
                }

                return new CustomerProfileItem
                {
                    ItemId = course.Id,
                    ItemType = EnrollmentItemType.Course,
                    ItemTypeText = "Course",
                    Title = course.Title,
                    Category = course.Category,
                    ScheduleText = $"{course.Days}, {course.StartTime:hh\\:mm} - {course.EndTime:hh\\:mm}",
                    StatusText = enrollment.Status.ToString()
                };
            }

            return null;
        }

        private string GetSelectedSavedItemsFilter()
        {
            if (SavedItemsFilterComboBox.SelectedItem is ComboBoxItem selectedItem)
            {
                return selectedItem.Content?.ToString() ?? "All";
            }

            return "All";
        }

        private string GetSelectedMyItemsFilter()
        {
            if (MyItemsFilterComboBox.SelectedItem is ComboBoxItem selectedItem)
            {
                return selectedItem.Content?.ToString() ?? "All";
            }

            return "All";
        }

        private void BackButton_Click(object? sender, RoutedEventArgs e)
        {
            MainWindow? mainWindow = this.VisualRoot as MainWindow;

            if (mainWindow == null)
            {
                return;
            }

            mainWindow.Content = new CustomerDashboardView();
        }

        private void LogoutButton_Click(object? sender, RoutedEventArgs e)
        {
            _authService.Logout();

            MainWindow? mainWindow = this.VisualRoot as MainWindow;

            if (mainWindow == null)
            {
                return;
            }

            mainWindow.Content = new LoginView();
        }
    }
}