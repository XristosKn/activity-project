using Avalonia.Controls;
using Avalonia.Interactivity;
using ActivityProjectApp.Models;
using ActivityProjectApp.Services;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace ActivityProjectApp.Views
{
    public partial class ServiceProviderDashboardView : UserControl
    {
        private readonly AuthService _authService;

        public ServiceProviderDashboardView()
        {
            InitializeComponent();

            _authService = AppServices.AuthService;

            LoadCurrentUser();
            LoadProviderEvents();

            LogoutButton.Click += LogoutButton_Click;
            CreateEventButton.Click += CreateEventButton_Click;
            MyActivitiesButton.Click += MyActivitiesButton_Click;
            BookingsButton.Click += BookingsButton_Click;
            ProfileButton.Click += ProfileButton_Click;
            SaveEventButton.Click += SaveEventButton_Click;
            CancelCreateEventButton.Click += CancelCreateEventButton_Click;
            CreateItemTypeComboBox.SelectionChanged += CreateItemTypeComboBox_SelectionChanged;
            ProviderActivityFilterComboBox.SelectionChanged += ProviderActivityFilterComboBox_SelectionChanged;
        }

        private void LoadCurrentUser()
        {
            User? currentUser = _authService.GetCurrentUser();

            if (currentUser != null)
            {
                WelcomeTextBlock.Text = $"Welcome, {currentUser.Name} {currentUser.LastName}";
            }
            else
            {
                WelcomeTextBlock.Text = "Welcome, Service Provider";
            }
        }
        private void ProviderActivityFilterComboBox_SelectionChanged(object? sender, SelectionChangedEventArgs e)
        {
            LoadProviderEvents();
        }

        private void LoadProviderEvents()
        {
            User? currentUser = _authService.GetCurrentUser();

            if (currentUser is not ServiceProvider serviceProvider)
            {
                ProviderEventsItemsControl.ItemsSource = null;
                ActiveActivitiesCountTextBlock.Text = "0";
                InactiveActivitiesCountTextBlock.Text = "0";
                return;
            }

            List<ActivityEvent> providerEvents = AppServices.ActivityEventRepository
                .GetEventsByServiceProviderId(serviceProvider.Id);

            List<Course> providerCourses = AppServices.CourseRepository
                .GetCoursesByServiceProviderId(serviceProvider.Id);

            int activeEventsCount = providerEvents
                .Count(activityEvent => activityEvent.Status == EventStatus.Active);

            int activeCoursesCount = providerCourses
                .Count(course => course.Status == CourseStatus.Active);

            int inactiveEventsCount = providerEvents
                .Count(activityEvent => activityEvent.Status == EventStatus.Inactive);

            int inactiveCoursesCount = providerCourses
                .Count(course => course.Status == CourseStatus.Inactive);

            ActiveActivitiesCountTextBlock.Text = (activeEventsCount + activeCoursesCount).ToString();
            InactiveActivitiesCountTextBlock.Text = (inactiveEventsCount + inactiveCoursesCount).ToString();

            List<ProviderActivityListItem> providerActivities = new List<ProviderActivityListItem>();

            string selectedFilter = GetSelectedProviderActivityFilter();

            if (selectedFilter == "All" || selectedFilter == "Events")
            {
                providerActivities.AddRange(providerEvents.Select(activityEvent => new ProviderActivityListItem
                {
                    ActivityType = "Event",
                    Title = activityEvent.Title,
                    Category = activityEvent.Category,
                    Address = activityEvent.Address,
                    ScheduleText = $"{activityEvent.Date:dd/MM/yyyy} at {activityEvent.Time:hh\\:mm}",
                    Status = activityEvent.Status.ToString()
                }));
            }

            if (selectedFilter == "All" || selectedFilter == "Courses")
            {
                providerActivities.AddRange(providerCourses.Select(course => new ProviderActivityListItem
                {
                    ActivityType = "Course",
                    Title = course.Title,
                    Category = course.Category,
                    Address = course.Address,
                    ScheduleText = $"{course.Days}, {course.StartTime:hh\\:mm} - {course.EndTime:hh\\:mm}",
                    Status = course.Status.ToString()
                }));
            }

            ProviderEventsItemsControl.ItemsSource = providerActivities;
        }

        private void CreateEventButton_Click(object? sender, RoutedEventArgs e)
        {
            CreateEventFormPanel.IsVisible = true;
            DashboardMessageTextBlock.Text = "Fill in the form below to create a new event or course.";
            CreateEventMessageTextBlock.Text = string.Empty;

            if (CreateItemTypeComboBox.SelectedItem == null)
            {
                CreateItemTypeComboBox.SelectedIndex = 0;
            }

            UpdateCreateFormMode();
        }

        private void MyActivitiesButton_Click(object? sender, RoutedEventArgs e)
        {
            CreateEventFormPanel.IsVisible = false;
            DashboardMessageTextBlock.Text = "Below you can see the events and courses created by this service provider.";
            LoadProviderEvents();
        }

        private void BookingsButton_Click(object? sender, RoutedEventArgs e)
        {
            CreateEventFormPanel.IsVisible = false;
            DashboardMessageTextBlock.Text = "Bookings page will be added later.";
        }

        private void ProfileButton_Click(object? sender, RoutedEventArgs e)
        {
            CreateEventFormPanel.IsVisible = false;

            User? currentUser = _authService.GetCurrentUser();

            if (currentUser is ServiceProvider serviceProvider)
            {
                DashboardMessageTextBlock.Text =
                    $"Profile: {serviceProvider.Name} {serviceProvider.LastName} | Services: {serviceProvider.TypeofService}";
            }
            else
            {
                DashboardMessageTextBlock.Text = "Profile information is not available.";
            }
        }

        private void CreateItemTypeComboBox_SelectionChanged(object? sender, SelectionChangedEventArgs e)
        {
            UpdateCreateFormMode();
        }

        private void UpdateCreateFormMode()
        {
            string selectedType = GetSelectedCreateItemType();

            bool isCourse = selectedType == "Course";

            EventSchedulePanel.IsVisible = !isCourse;
            CourseSchedulePanel.IsVisible = isCourse;

            SaveEventButton.Content = isCourse ? "Save Course" : "Save Event";

            CreateEventMessageTextBlock.Text = string.Empty;
        }

        private void SaveEventFromForm()
        {
            string title = EventTitleTextBox.Text?.Trim() ?? string.Empty;
            string category = GetSelectedEventCategory();
            string description = EventDescriptionTextBox.Text?.Trim() ?? string.Empty;
            string latitudeText = EventLatitudeTextBox.Text?.Trim() ?? string.Empty;
            string longitudeText = EventLongitudeTextBox.Text?.Trim() ?? string.Empty;
            string address = EventAddressTextBox.Text?.Trim() ?? string.Empty;
            DateTimeOffset? selectedDate = EventDatePicker.SelectedDate;
            string timeText = EventTimeTextBox.Text?.Trim() ?? string.Empty;
            string priceText = EventPriceTextBox.Text?.Trim() ?? string.Empty;
            string maxSpaceText = EventMaxSpaceTextBox.Text?.Trim() ?? string.Empty;
            string galleryText = EventGalleryTextBox.Text?.Trim() ?? string.Empty;
            string selectedStatus = GetSelectedEventStatus();

            CreateEventMessageTextBlock.Foreground = Avalonia.Media.Brushes.Red;

            if (string.IsNullOrWhiteSpace(title))
            {
                CreateEventMessageTextBlock.Text = "Please enter event title.";
                return;
            }

            if (string.IsNullOrWhiteSpace(category))
            {
                CreateEventMessageTextBlock.Text = "Please select event category.";
                return;
            }

            if (string.IsNullOrWhiteSpace(description))
            {
                CreateEventMessageTextBlock.Text = "Please enter event description.";
                return;
            }

            if (!double.TryParse(latitudeText, NumberStyles.Any, CultureInfo.InvariantCulture, out double latitude))
            {
                CreateEventMessageTextBlock.Text = "Please enter a valid latitude.";
                return;
            }

            if (!double.TryParse(longitudeText, NumberStyles.Any, CultureInfo.InvariantCulture, out double longitude))
            {
                CreateEventMessageTextBlock.Text = "Please enter a valid longitude.";
                return;
            }

            if (string.IsNullOrWhiteSpace(address))
            {
                CreateEventMessageTextBlock.Text = "Please enter address.";
                return;
            }

            if (selectedDate == null)
            {
                CreateEventMessageTextBlock.Text = "Please select event date.";
                return;
            }

            if (!TimeSpan.TryParse(timeText, out TimeSpan time))
            {
                CreateEventMessageTextBlock.Text = "Please enter a valid time, for example 10:30.";
                return;
            }

            if (!decimal.TryParse(priceText, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal price))
            {
                CreateEventMessageTextBlock.Text = "Please enter a valid price.";
                return;
            }

            if (!int.TryParse(maxSpaceText, out int maxSpace))
            {
                CreateEventMessageTextBlock.Text = "Please enter a valid max space.";
                return;
            }

            if (maxSpace <= 0)
            {
                CreateEventMessageTextBlock.Text = "Max space must be greater than zero.";
                return;
            }

            if (string.IsNullOrWhiteSpace(selectedStatus))
            {
                CreateEventMessageTextBlock.Text = "Please select event status.";
                return;
            }

            User? currentUser = _authService.GetCurrentUser();

            if (currentUser is not ServiceProvider serviceProvider)
            {
                CreateEventMessageTextBlock.Text = "Only service providers can create events.";
                return;
            }

            EventStatus status = selectedStatus == "Active"
                ? EventStatus.Active
                : EventStatus.Inactive;

            List<string> gallery = galleryText
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(item => item.Trim())
                .Where(item => !string.IsNullOrWhiteSpace(item))
                .ToList();

            ActivityEvent activityEvent = new ActivityEvent
            {
                Title = title,
                Category = category,
                Description = description,
                Latitude = latitude,
                Longitude = longitude,
                Address = address,
                Date = selectedDate.Value.DateTime,
                Time = time,
                Price = price,
                MaxSpace = maxSpace,
                Gallery = gallery,
                ServiceProviderId = serviceProvider.Id,
                Status = status
            };

            AppServices.ActivityEventRepository.AddEvent(activityEvent);

            ClearCreateEventForm();
            LoadProviderEvents();

            CreateEventMessageTextBlock.Foreground = Avalonia.Media.Brushes.Green;
            CreateEventMessageTextBlock.Text = "Event created successfully.";

            DashboardMessageTextBlock.Text = "The event was created and added to your events list.";
        }

        private void SaveCourseFromForm()
        {
            string title = EventTitleTextBox.Text?.Trim() ?? string.Empty;
            string category = GetSelectedEventCategory();
            string description = EventDescriptionTextBox.Text?.Trim() ?? string.Empty;
            string latitudeText = EventLatitudeTextBox.Text?.Trim() ?? string.Empty;
            string longitudeText = EventLongitudeTextBox.Text?.Trim() ?? string.Empty;
            string address = EventAddressTextBox.Text?.Trim() ?? string.Empty;
            string days = CourseDaysTextBox.Text?.Trim() ?? string.Empty;
            string startTimeText = CourseStartTimeTextBox.Text?.Trim() ?? string.Empty;
            string endTimeText = CourseEndTimeTextBox.Text?.Trim() ?? string.Empty;
            string priceText = EventPriceTextBox.Text?.Trim() ?? string.Empty;
            string maxSpaceText = EventMaxSpaceTextBox.Text?.Trim() ?? string.Empty;
            string ageRestriction = CourseAgeRestrictionTextBox.Text?.Trim() ?? string.Empty;
            string galleryText = EventGalleryTextBox.Text?.Trim() ?? string.Empty;
            string selectedStatus = GetSelectedEventStatus();

            CreateEventMessageTextBlock.Foreground = Avalonia.Media.Brushes.Red;

            if (string.IsNullOrWhiteSpace(title))
            {
                CreateEventMessageTextBlock.Text = "Please enter course title.";
                return;
            }

            if (string.IsNullOrWhiteSpace(category))
            {
                CreateEventMessageTextBlock.Text = "Please select course category.";
                return;
            }

            if (string.IsNullOrWhiteSpace(description))
            {
                CreateEventMessageTextBlock.Text = "Please enter course description.";
                return;
            }

            if (!double.TryParse(latitudeText, NumberStyles.Any, CultureInfo.InvariantCulture, out double latitude))
            {
                CreateEventMessageTextBlock.Text = "Please enter a valid latitude.";
                return;
            }

            if (!double.TryParse(longitudeText, NumberStyles.Any, CultureInfo.InvariantCulture, out double longitude))
            {
                CreateEventMessageTextBlock.Text = "Please enter a valid longitude.";
                return;
            }

            if (string.IsNullOrWhiteSpace(address))
            {
                CreateEventMessageTextBlock.Text = "Please enter address.";
                return;
            }

            if (string.IsNullOrWhiteSpace(days))
            {
                CreateEventMessageTextBlock.Text = "Please enter course days.";
                return;
            }

            if (!TimeSpan.TryParse(startTimeText, out TimeSpan startTime))
            {
                CreateEventMessageTextBlock.Text = "Please enter a valid start time, for example 18:00.";
                return;
            }

            if (!TimeSpan.TryParse(endTimeText, out TimeSpan endTime))
            {
                CreateEventMessageTextBlock.Text = "Please enter a valid end time, for example 19:30.";
                return;
            }

            if (endTime <= startTime)
            {
                CreateEventMessageTextBlock.Text = "End time must be after start time.";
                return;
            }

            if (!decimal.TryParse(priceText, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal price))
            {
                CreateEventMessageTextBlock.Text = "Please enter a valid price.";
                return;
            }

            if (!int.TryParse(maxSpaceText, out int maxSpace))
            {
                CreateEventMessageTextBlock.Text = "Please enter a valid max space.";
                return;
            }

            if (maxSpace <= 0)
            {
                CreateEventMessageTextBlock.Text = "Max space must be greater than zero.";
                return;
            }

            if (string.IsNullOrWhiteSpace(ageRestriction))
            {
                CreateEventMessageTextBlock.Text = "Please enter age restriction.";
                return;
            }

            if (string.IsNullOrWhiteSpace(selectedStatus))
            {
                CreateEventMessageTextBlock.Text = "Please select course status.";
                return;
            }

            User? currentUser = _authService.GetCurrentUser();

            if (currentUser is not ServiceProvider serviceProvider)
            {
                CreateEventMessageTextBlock.Text = "Only service providers can create courses.";
                return;
            }

            CourseStatus status = selectedStatus == "Active"
                ? CourseStatus.Active
                : CourseStatus.Inactive;

            Course course = new Course
            {
                Title = title,
                Category = category,
                Description = description,
                Latitude = latitude,
                Longitude = longitude,
                Address = address,
                Days = days,
                StartTime = startTime,
                EndTime = endTime,
                Price = price,
                MaxSpace = maxSpace,
                AgeRestriction = ageRestriction,
                Gallery = galleryText,
                ServiceProviderId = serviceProvider.Id,
                Status = status
            };

            AppServices.CourseRepository.AddCourse(course);

            ClearCreateEventForm();
            LoadProviderEvents();

            CreateEventMessageTextBlock.Foreground = Avalonia.Media.Brushes.Green;
            CreateEventMessageTextBlock.Text = "Course created successfully.";

            DashboardMessageTextBlock.Text = "The course was created successfully.";
        }

        private string GetSelectedEventCategory()
        {
            if (EventCategoryComboBox.SelectedItem is ComboBoxItem selectedItem)
            {
                return selectedItem.Content?.ToString() ?? string.Empty;
            }

            return string.Empty;
        }

        private string GetSelectedEventStatus()
        {
            if (EventStatusComboBox.SelectedItem is ComboBoxItem selectedItem)
            {
                return selectedItem.Content?.ToString() ?? string.Empty;
            }

            return string.Empty;
        }

        private string GetSelectedCreateItemType()
        {
            if (CreateItemTypeComboBox.SelectedItem is ComboBoxItem selectedItem)
            {
                return selectedItem.Content?.ToString() ?? "Event";
            }

            return "Event";
        }

        private string GetSelectedProviderActivityFilter()
        {
            if (ProviderActivityFilterComboBox.SelectedItem is ComboBoxItem selectedItem)
            {
                return selectedItem.Content?.ToString() ?? "All";
            }

            return "All";
        }

        private void ClearCreateEventForm()
        {
            EventTitleTextBox.Text = string.Empty;
            EventCategoryComboBox.SelectedItem = null;
            EventDescriptionTextBox.Text = string.Empty;
            EventLatitudeTextBox.Text = string.Empty;
            EventLongitudeTextBox.Text = string.Empty;
            EventAddressTextBox.Text = string.Empty;
            EventDatePicker.SelectedDate = null;
            EventTimeTextBox.Text = string.Empty;
            EventPriceTextBox.Text = string.Empty;
            EventMaxSpaceTextBox.Text = string.Empty;
            EventGalleryTextBox.Text = string.Empty;
            EventStatusComboBox.SelectedItem = null;
            CreateItemTypeComboBox.SelectedIndex = 0;
            CourseDaysTextBox.Text = string.Empty;
            CourseStartTimeTextBox.Text = string.Empty;
            CourseEndTimeTextBox.Text = string.Empty;
            CourseAgeRestrictionTextBox.Text = string.Empty;
            UpdateCreateFormMode();
        }

        private void SaveEventButton_Click(object? sender, RoutedEventArgs e)
        {
            string selectedType = GetSelectedCreateItemType();

            if (selectedType == "Course")
            {
                SaveCourseFromForm();
                return;
            }

            SaveEventFromForm();
        }

        private void CancelCreateEventButton_Click(object? sender, RoutedEventArgs e)
        {
            ClearCreateEventForm();
            CreateEventFormPanel.IsVisible = false;
            CreateEventMessageTextBlock.Text = string.Empty;
            DashboardMessageTextBlock.Text = "Select an option from the menu.";
        }

        private void LogoutButton_Click(object? sender, RoutedEventArgs e)
        {
            _authService.Logout();

            Window? window = TopLevel.GetTopLevel(this) as Window;

            if (window != null)
            {
                window.Content = new LoginView();
            }
        }
    }
}