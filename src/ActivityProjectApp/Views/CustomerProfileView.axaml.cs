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
        private List<CustomerProfileItem> _allSavedItems = new List<CustomerProfileItem>();
        private List<CustomerProfileItem> _allMyItems = new List<CustomerProfileItem>();

        private List<Announcement> _allAnnouncements = new List<Announcement>();
        private int _announcementStartIndex = 0;
        private const int AnnouncementsPageSize = 5;

        public CustomerProfileView()
        {
            InitializeComponent();

            if (AppServices.AuthService.GetCurrentUser() is not Customer)
            {
                NavigateToLogin();
                return;
            }

            LoadCustomerInfo();
            LoadAnnouncements();
            LoadSavedItems();
            LoadMyEnrolledItems();

            BackButton.Click += BackButton_Click;
            SettingsButton.Click += SettingsButton_Click;
            LogoutButton.Click += LogoutButton_Click;

            PreviousAnnouncementsButton.Click += PreviousAnnouncementsButton_Click;
            NextAnnouncementsButton.Click += NextAnnouncementsButton_Click;

            SavedItemsFilterComboBox.SelectionChanged += SavedItemsFilterComboBox_SelectionChanged;
            MyItemsFilterComboBox.SelectionChanged += MyItemsFilterComboBox_SelectionChanged;
            SavedItemsControl.AddHandler(Button.ClickEvent, SavedItemsControl_ButtonClick, RoutingStrategies.Bubble);

            ShowProfileMainContent();
        }

        private void LoadCustomerInfo()
        {
            User? currentUser = AppServices.AuthService.GetCurrentUser();

            if (currentUser is Customer customer)
            {
                CustomerInfoTextBlock.Text = $"{customer.Name} {customer.LastName} | {customer.Email}";
                return;
            }

            CustomerInfoTextBlock.Text = "Profile overview";
        }

        private void LoadAnnouncements()
        {
            _allAnnouncements = AppServices.AnnouncementRepository
                .GetActiveAnnouncements()
                .OrderByDescending(announcement => announcement.AnnouncementDate)
                .ToList();

            _announcementStartIndex = 0;

            RefreshAnnouncementsCarousel();
        }

        private void RefreshAnnouncementsCarousel()
        {
            List<Announcement> visibleAnnouncements = _allAnnouncements
                .Skip(_announcementStartIndex)
                .Take(AnnouncementsPageSize)
                .ToList();

            AnnouncementsItemsControl.ItemsSource = visibleAnnouncements;
            AnnouncementsCountTextBlock.Text = $"{_allAnnouncements.Count} announcements";

            bool hasAnnouncements = _allAnnouncements.Count > 0;

            AnnouncementsEmptyTextBlock.IsVisible = !hasAnnouncements;
            AnnouncementsItemsControl.IsVisible = hasAnnouncements;

            PreviousAnnouncementsButton.IsEnabled = _announcementStartIndex > 0;
            NextAnnouncementsButton.IsEnabled =
                _announcementStartIndex + AnnouncementsPageSize < _allAnnouncements.Count;
        }

        private void PreviousAnnouncementsButton_Click(object? sender, RoutedEventArgs e)
        {
            _announcementStartIndex -= AnnouncementsPageSize;

            if (_announcementStartIndex < 0)
            {
                _announcementStartIndex = 0;
            }

            RefreshAnnouncementsCarousel();
        }

        private void NextAnnouncementsButton_Click(object? sender, RoutedEventArgs e)
        {
            if (_announcementStartIndex + AnnouncementsPageSize >= _allAnnouncements.Count)
            {
                return;
            }

            _announcementStartIndex += AnnouncementsPageSize;

            RefreshAnnouncementsCarousel();
        }

        private void LoadSavedItems()
        {
            User? currentUser = AppServices.AuthService.GetCurrentUser();

            if (currentUser is not Customer customer)
            {
                _allSavedItems = new List<CustomerProfileItem>();
                SavedItemsControl.ItemsSource = _allSavedItems;
                SavedItemsCountTextBlock.Text = "0 saved";
                return;
            }

            List<SavedItem> savedItems = AppServices.SavedItemRepository
                .GetSavedItemsByCustomerId(customer.Id);

            _allSavedItems = savedItems
                .Select(ConvertSavedItemToCustomerProfileItem)
                .Where(item => item != null)
                .Select(item => item!)
                .ToList();

            RefreshSavedItems();
        }

        private CustomerProfileItem? ConvertSavedItemToCustomerProfileItem(SavedItem savedItem)
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

        private void LoadMyEnrolledItems()
        {
            User? currentUser = AppServices.AuthService.GetCurrentUser();

            if (currentUser is not Customer customer)
            {
                _allMyItems = new List<CustomerProfileItem>();
                MyItemsControl.ItemsSource = _allMyItems;
                MyItemsCountTextBlock.Text = "0 enrolled";
                return;
            }

            List<Enrollment> confirmedEnrollments = AppServices.EnrollmentRepository
                .GetEnrollmentsByCustomerId(customer.Id)
                .Where(enrollment => enrollment.Status == EnrollmentStatus.Confirmed)
                .ToList();

            _allMyItems = confirmedEnrollments
                .Select(ConvertEnrollmentToCustomerProfileItem)
                .Where(item => item != null)
                .Select(item => item!)
                .ToList();

            RefreshMyItems();
        }

        private CustomerProfileItem? ConvertEnrollmentToCustomerProfileItem(Enrollment enrollment)
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

        private void SavedItemsFilterComboBox_SelectionChanged(object? sender, SelectionChangedEventArgs e)
        {
            RefreshSavedItems();
        }

        private void MyItemsFilterComboBox_SelectionChanged(object? sender, SelectionChangedEventArgs e)
        {
            RefreshMyItems();
        }

        private void RefreshSavedItems()
        {
            string selectedFilter = GetSelectedSavedItemsFilter();

            List<CustomerProfileItem> filteredItems = _allSavedItems;

            if (selectedFilter == "Events")
            {
                filteredItems = _allSavedItems
                    .Where(item => item.ItemType == EnrollmentItemType.Event)
                    .ToList();
            }
            else if (selectedFilter == "Courses")
            {
                filteredItems = _allSavedItems
                    .Where(item => item.ItemType == EnrollmentItemType.Course)
                    .ToList();
            }

            SavedItemsControl.ItemsSource = filteredItems;
            SavedItemsCountTextBlock.Text = $"{filteredItems.Count} saved";
        }

        private void SavedItemsControl_ButtonClick(object? sender, RoutedEventArgs e)
        {
            if (e.Source is not Button button)
            {
                return;
            }

            if (!button.Classes.Contains("unsave-button"))
            {
                return;
            }

            if (button.Tag is not CustomerProfileItem savedItem)
            {
                return;
            }

            UnsaveCustomerProfileItem(savedItem);

            e.Handled = true;
        }

        private void UnsaveCustomerProfileItem(CustomerProfileItem savedItem)
        {
            User? currentUser = AppServices.AuthService.GetCurrentUser();

            if (currentUser is not Customer customer)
            {
                return;
            }

            SavedItemType savedItemType = ConvertToSavedItemType(savedItem.ItemType);

            AppServices.SavedItemRepository.RemoveSavedItem(
                customer.Id,
                savedItem.ItemId,
                savedItemType);

            LoadSavedItems();
        }

        private SavedItemType ConvertToSavedItemType(EnrollmentItemType itemType)
        {
            if (itemType == EnrollmentItemType.Course)
            {
                return SavedItemType.Course;
            }

            return SavedItemType.Event;
        }

        private void RefreshMyItems()
        {
            string selectedFilter = GetSelectedMyItemsFilter();

            List<CustomerProfileItem> filteredItems = _allMyItems;

            if (selectedFilter == "Events")
            {
                filteredItems = _allMyItems
                    .Where(item => item.ItemType == EnrollmentItemType.Event)
                    .ToList();
            }
            else if (selectedFilter == "Courses")
            {
                filteredItems = _allMyItems
                    .Where(item => item.ItemType == EnrollmentItemType.Course)
                    .ToList();
            }

            MyItemsControl.ItemsSource = filteredItems;
            MyItemsCountTextBlock.Text = $"{filteredItems.Count} enrolled";
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

        private void ShowProfileMainContent()
        {
            AnnouncementsSectionPanel.IsVisible = true;
            SavedItemsSectionPanel.IsVisible = true;
            MyItemsSectionPanel.IsVisible = true;

            SettingsSectionPanel.IsVisible = false;
        }

        private void ShowSettingsContent()
        {
            AnnouncementsSectionPanel.IsVisible = false;
            SavedItemsSectionPanel.IsVisible = false;
            MyItemsSectionPanel.IsVisible = false;

            SettingsSectionPanel.IsVisible = true;
        }

        private void SettingsButton_Click(object? sender, RoutedEventArgs e)
        {
            ShowSettingsContent();
        }

        private void BackButton_Click(object? sender, RoutedEventArgs e)
        {
            if (SettingsSectionPanel.IsVisible)
            {
                ShowProfileMainContent();
                return;
            }

            MainWindow? mainWindow = this.VisualRoot as MainWindow;

            if (mainWindow == null)
            {
                return;
            }

            mainWindow.Content = new CustomerDashboardView();
        }

        private void LogoutButton_Click(object? sender, RoutedEventArgs e)
        {
            AppServices.AuthService.Logout();
            NavigateToLogin();
        }

        private void NavigateToLogin()
        {
            MainWindow? mainWindow = this.VisualRoot as MainWindow;

            if (mainWindow == null)
            {
                return;
            }

            mainWindow.Content = new LoginView();
        }
    }
}