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
            LoadDashboardCounts();

            LogoutButton.Click += LogoutButton_Click;

            CreateEventButton.Click += CreateEventButton_Click;
            MyActivitiesButton.Click += MyActivitiesButton_Click;
            AnnouncementsButton.Click += AnnouncementsButton_Click;
            MyAnnouncementsButton.Click += MyAnnouncementsButton_Click;
            SettingsButton.Click += SettingsButton_Click;
            ProfileSettingsButton.Click += ProfileSettingsButton_Click;
            ManagementSettingsButton.Click += ManagementSettingsButton_Click;

            MyEventsSummaryButton.Click += MyEventsSummaryButton_Click;
            MyCoursesSummaryButton.Click += MyCoursesSummaryButton_Click;
            MyAnnouncementsSummaryButton.Click += MyAnnouncementsSummaryButton_Click;

            SaveEventButton.Click += SaveEventButton_Click;
            CancelCreateEventButton.Click += CancelCreateEventButton_Click;

            PublishAnnouncementButton.Click += PublishAnnouncementButton_Click;
            CancelAnnouncementButton.Click += CancelAnnouncementButton_Click;

            CreateItemTypeComboBox.SelectionChanged += CreateItemTypeComboBox_SelectionChanged;
            ProviderActivityFilterComboBox.SelectionChanged += ProviderActivityFilterComboBox_SelectionChanged;
            AnnouncementTargetModeComboBox.SelectionChanged += AnnouncementTargetModeComboBox_SelectionChanged;
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

        private void HideDashboardPanels()
        {
            CreateEventFormPanel.IsVisible = false;
            CreateAnnouncementFormPanel.IsVisible = false;
            ProviderActivitiesPanel.IsVisible = false;
            ProviderAnnouncementsPanel.IsVisible = false;
            SettingsPanel.IsVisible = false;
        }

        private void ShowProviderActivitiesPanel(string title, int filterIndex)
        {
            HideDashboardPanels();

            ProviderActivitiesPanel.IsVisible = true;
            ProviderActivitiesTitleTextBlock.Text = title;

            ProviderActivityFilterComboBox.SelectedIndex = filterIndex;

            LoadProviderEvents();

            DashboardMessageTextBlock.Text = $"Viewing {title.ToLower()}.";
        }

        private void ShowProviderAnnouncementsPanel()
        {
            HideDashboardPanels();

            ProviderAnnouncementsPanel.IsVisible = true;

            LoadProviderAnnouncements();

            DashboardMessageTextBlock.Text = "Viewing your announcements.";
        }

        private void LoadDashboardCounts()
        {
            User? currentUser = _authService.GetCurrentUser();

            if (currentUser is not ServiceProvider serviceProvider)
            {
                MyEventsCountTextBlock.Text = "0";
                MyCoursesCountTextBlock.Text = "0";
                MyAnnouncementsCountTextBlock.Text = "0";
                return;
            }

            int eventsCount = AppServices.ActivityEventRepository
                .GetEventsByServiceProviderId(serviceProvider.Id)
                .Count;

            int coursesCount = AppServices.CourseRepository
                .GetCoursesByServiceProviderId(serviceProvider.Id)
                .Count;

            int announcementsCount = GetProviderAnnouncements(serviceProvider)
                .Count;

            MyEventsCountTextBlock.Text = eventsCount.ToString();
            MyCoursesCountTextBlock.Text = coursesCount.ToString();
            MyAnnouncementsCountTextBlock.Text = announcementsCount.ToString();
        }

        private void MyEventsSummaryButton_Click(object? sender, RoutedEventArgs e)
        {
            SetActiveSummaryCard(MyEventsSummaryButton);
            ShowProviderActivitiesPanel("My Events", 1);
        }

        private void MyCoursesSummaryButton_Click(object? sender, RoutedEventArgs e)
        {
            SetActiveSummaryCard(MyCoursesSummaryButton);
            ShowProviderActivitiesPanel("My Courses", 2);
        }

        private void MyAnnouncementsSummaryButton_Click(object? sender, RoutedEventArgs e)
        {
            SetActiveSummaryCard(MyAnnouncementsSummaryButton);
            ShowProviderAnnouncementsPanel();
        }

        private void MyAnnouncementsButton_Click(object? sender, RoutedEventArgs e)
        {
            SetActiveSummaryCard(MyAnnouncementsSummaryButton);
            ShowProviderAnnouncementsPanel();
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
                return;
            }

            List<ActivityEvent> providerEvents = AppServices.ActivityEventRepository
                .GetEventsByServiceProviderId(serviceProvider.Id);

            List<Course> providerCourses = AppServices.CourseRepository
                .GetCoursesByServiceProviderId(serviceProvider.Id);

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
            SetActiveSummaryCard(null);
            HideDashboardPanels();

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
            SetActiveSummaryCard(null);
            ShowProviderActivitiesPanel("My Courses / Events", 0);
        }

        private void AnnouncementsButton_Click(object? sender, RoutedEventArgs e)
        {
            SetActiveSummaryCard(null);
            HideDashboardPanels();

            CreateAnnouncementFormPanel.IsVisible = true;

            DashboardMessageTextBlock.Text = "Create announcements for your events or courses.";
            AnnouncementMessageTextBlock.Text = string.Empty;

            if (AnnouncementTargetModeComboBox.SelectedItem == null)
            {
                AnnouncementTargetModeComboBox.SelectedIndex = 0;
            }

            if (AnnouncementStatusComboBox.SelectedItem == null)
            {
                AnnouncementStatusComboBox.SelectedIndex = 0;
            }

            LoadAnnouncementTargetItems();
        }

        private void SettingsButton_Click(object? sender, RoutedEventArgs e)
        {
            SetActiveSummaryCard(null);
            HideDashboardPanels();

            SettingsPanel.IsVisible = true;

            ShowProfileSettings();

            DashboardMessageTextBlock.Text = "Manage your account and activity settings.";
        }

        private void ProfileSettingsButton_Click(object? sender, RoutedEventArgs e)
        {
            ShowProfileSettings();
        }

        private void ManagementSettingsButton_Click(object? sender, RoutedEventArgs e)
        {
            ShowManagementSettings();
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

        private void ShowProfileSettings()
        {
            ProfileSettingsContentPanel.IsVisible = true;
            ManagementSettingsContentPanel.IsVisible = false;

            ProfileSettingsButton.Classes.Add("active");
            ManagementSettingsButton.Classes.Remove("active");
        }

        private void ShowManagementSettings()
        {
            ProfileSettingsContentPanel.IsVisible = false;
            ManagementSettingsContentPanel.IsVisible = true;

            ManagementSettingsButton.Classes.Add("active");
            ProfileSettingsButton.Classes.Remove("active");
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
                MainImagePath = string.Empty,
                Gallery = string.Empty,
                ServiceProviderId = serviceProvider.Id,
                Status = status
            };

            AppServices.ActivityEventRepository.AddEvent(activityEvent);

            ClearCreateEventForm();
            LoadProviderEvents();
            LoadDashboardCounts();

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
            LoadDashboardCounts();

            CreateEventMessageTextBlock.Foreground = Avalonia.Media.Brushes.Green;
            CreateEventMessageTextBlock.Text = "Course created successfully.";

            DashboardMessageTextBlock.Text = "The course was created successfully.";
        }

        private void AnnouncementItemTypeComboBox_SelectionChanged(object? sender, SelectionChangedEventArgs e)
        {
            LoadAnnouncementTargetItems();
        }

        private void AnnouncementTargetModeComboBox_SelectionChanged(object? sender, SelectionChangedEventArgs e)
        {
            LoadAnnouncementTargetItems();
        }

        private void LoadAnnouncementTargetItems()
        {
            User? currentUser = _authService.GetCurrentUser();

            if (currentUser is not ServiceProvider serviceProvider)
            {
                AnnouncementTargetItemsControl.ItemsSource = null;
                return;
            }

            string selectedMode = GetSelectedAnnouncementTargetMode();

            bool isCustomSelection = selectedMode == "Custom Selection";

            AnnouncementTargetItemsControl.IsEnabled = isCustomSelection;
            AnnouncementTargetsPanel.Opacity = isCustomSelection ? 1.0 : 0.65;

            AnnouncementTargetsHelpTextBlock.Text = isCustomSelection
                ? "Manually select one or more events/courses for this announcement."
                : "Targets are selected automatically based on the option above.";

            List<AnnouncementTargetSelectionItem> targetItems = new List<AnnouncementTargetSelectionItem>();

            List<ActivityEvent> providerEvents = AppServices.ActivityEventRepository
                .GetEventsByServiceProviderId(serviceProvider.Id);

            List<Course> providerCourses = AppServices.CourseRepository
                .GetCoursesByServiceProviderId(serviceProvider.Id);

            targetItems.AddRange(providerEvents.Select(activityEvent => new AnnouncementTargetSelectionItem
            {
                ItemId = activityEvent.Id,
                ItemType = AnnouncementItemType.Event,
                DisplayText = $"Event - {activityEvent.Title}",
                IsSelected = selectedMode == "All Events and Courses" || selectedMode == "All Events"
            }));

            targetItems.AddRange(providerCourses.Select(course => new AnnouncementTargetSelectionItem
            {
                ItemId = course.Id,
                ItemType = AnnouncementItemType.Course,
                DisplayText = $"Course - {course.Title}",
                IsSelected = selectedMode == "All Events and Courses" || selectedMode == "All Courses"
            }));

            if (selectedMode == "Custom Selection")
            {
                foreach (AnnouncementTargetSelectionItem targetItem in targetItems)
                {
                    targetItem.IsSelected = false;
                }
            }

            AnnouncementTargetItemsControl.ItemsSource = targetItems;
        }

        private string GetSelectedAnnouncementTargetMode()
        {
            if (AnnouncementTargetModeComboBox.SelectedItem is ComboBoxItem selectedItem)
            {
                return selectedItem.Content?.ToString() ?? "All Events and Courses";
            }

            return "All Events and Courses";
        }

        private void PublishAnnouncementButton_Click(object? sender, RoutedEventArgs e)
        {
            string title = AnnouncementTitleTextBox.Text?.Trim() ?? string.Empty;
            string text = AnnouncementTextTextBox.Text?.Trim() ?? string.Empty;
            string gallery = AnnouncementGalleryTextBox.Text?.Trim() ?? string.Empty;

            AnnouncementMessageTextBlock.Foreground = Avalonia.Media.Brushes.Red;

            if (string.IsNullOrWhiteSpace(title))
            {
                AnnouncementMessageTextBlock.Text = "Please enter announcement title.";
                return;
            }

            if (string.IsNullOrWhiteSpace(text))
            {
                AnnouncementMessageTextBlock.Text = "Please enter announcement text.";
                return;
            }

            List<AnnouncementTarget> targets = GetSelectedAnnouncementTargets();

            if (targets.Count == 0)
            {
                AnnouncementMessageTextBlock.Text = "Please select at least one related event or course.";
                return;
            }

            User? currentUser = _authService.GetCurrentUser();

            if (currentUser is not ServiceProvider serviceProvider)
            {
                AnnouncementMessageTextBlock.Text = "Only service providers can create announcements.";
                return;
            }

            Announcement announcement = new Announcement
            {
                Title = title,
                Text = text,
                AnnouncementDate = DateTime.Now,
                Gallery = gallery,
                IsActive = GetSelectedAnnouncementIsActive(),
                ServiceProviderId = serviceProvider.Id
            };

            AppServices.AnnouncementRepository.AddAnnouncement(announcement, targets);

            ClearAnnouncementForm();
            LoadDashboardCounts();

            if (ProviderAnnouncementsPanel.IsVisible)
            {
                LoadProviderAnnouncements();
            }

            AnnouncementMessageTextBlock.Foreground = Avalonia.Media.Brushes.Green;
            AnnouncementMessageTextBlock.Text = "Announcement published successfully.";

            DashboardMessageTextBlock.Text = "The announcement was published and linked to the selected events/courses.";
        }

        private List<AnnouncementTarget> GetSelectedAnnouncementTargets()
        {
            List<AnnouncementTarget> targets = new List<AnnouncementTarget>();

            if (AnnouncementTargetItemsControl.ItemsSource is not IEnumerable<AnnouncementTargetSelectionItem> targetItems)
            {
                return targets;
            }

            targets = targetItems
                .Where(targetItem => targetItem.IsSelected)
                .Select(targetItem => new AnnouncementTarget
                {
                    ItemId = targetItem.ItemId,
                    ItemType = targetItem.ItemType
                })
                .ToList();

            return targets;
        }
        private void CancelAnnouncementButton_Click(object? sender, RoutedEventArgs e)
        {
            ClearAnnouncementForm();

            CreateAnnouncementFormPanel.IsVisible = false;
            AnnouncementMessageTextBlock.Text = string.Empty;

            DashboardMessageTextBlock.Text = "Select an option from the menu.";
        }

        private void ClearAnnouncementForm()
        {
            AnnouncementTitleTextBox.Text = string.Empty;
            AnnouncementTextTextBox.Text = string.Empty;
            AnnouncementGalleryTextBox.Text = string.Empty;

            AnnouncementTargetModeComboBox.SelectedIndex = 0;
            AnnouncementStatusComboBox.SelectedIndex = 0;

            LoadAnnouncementTargetItems();
        }

        private List<Announcement> GetProviderAnnouncements(ServiceProvider serviceProvider)
        {
            return AppServices.AnnouncementRepository
                .GetAllAnnouncements()
                .Where(announcement => announcement.ServiceProviderId == serviceProvider.Id)
                .OrderByDescending(announcement => announcement.AnnouncementDate)
                .ToList();
        }

        private void LoadProviderAnnouncements()
        {
            User? currentUser = _authService.GetCurrentUser();

            if (currentUser is not ServiceProvider serviceProvider)
            {
                ProviderAnnouncementsItemsControl.ItemsSource = null;
                ProviderAnnouncementsCountTextBlock.Text = "0 announcements";
                return;
            }

            List<Announcement> announcements = GetProviderAnnouncements(serviceProvider);

            ProviderAnnouncementsItemsControl.ItemsSource = announcements;
            ProviderAnnouncementsCountTextBlock.Text = $"{announcements.Count} announcements";
        }


        private bool GetSelectedAnnouncementIsActive()
        {
            if (AnnouncementStatusComboBox.SelectedItem is ComboBoxItem selectedItem)
            {
                string selectedValue = selectedItem.Content?.ToString() ?? "Active";

                return selectedValue == "Active";
            }

            return true;
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

        private void SetActiveSummaryCard(Button? activeButton)
        {
            MyEventsSummaryButton.Classes.Remove("active");
            MyCoursesSummaryButton.Classes.Remove("active");
            MyAnnouncementsSummaryButton.Classes.Remove("active");

            if (activeButton != null)
            {
                activeButton.Classes.Add("active");
            }
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