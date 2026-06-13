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
        private bool _isEditMode = false;
        private int _editingItemId = 0;
        private string _editingActivityType = string.Empty;
        private ProviderActivityListItem? _pendingDeleteItem = null;
        

        private bool _isAnnouncementEditMode = false;
        private int _editingAnnouncementId = 0;
        private Announcement? _pendingDeleteAnnouncement = null;

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

            ProviderEventsItemsControl.AddHandler(Button.ClickEvent, ProviderEventsItemsControl_ButtonClick, RoutingStrategies.Bubble);
            ProviderAnnouncementsItemsControl.AddHandler(Button.ClickEvent, ProviderAnnouncementsItemsControl_ButtonClick, RoutingStrategies.Bubble);

            ConfirmDeleteButton.Click += ConfirmDeleteButton_Click;
            CancelDeleteButton.Click += CancelDeleteButton_Click;
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
                    ItemId = activityEvent.Id,
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
                    ItemId = course.Id,
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
            _isEditMode = false;
            _editingItemId = 0;
            _editingActivityType = string.Empty;
            CreateItemTypeComboBox.IsEnabled = true;

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

            _isAnnouncementEditMode = false;
            _editingAnnouncementId = 0;
            PublishAnnouncementButton.Content = "Publish";

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
            string selectedType = _isEditMode
                ? _editingActivityType
                : GetSelectedCreateItemType();

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
                Id = _isEditMode ? _editingItemId : 0,
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
                Gallery = galleryText,
                ServiceProviderId = serviceProvider.Id,
                Status = status
            };

            if (_isEditMode)
            {
                AppServices.ActivityEventRepository.UpdateEvent(activityEvent);
            }
            else
            {
                AppServices.ActivityEventRepository.AddEvent(activityEvent);
            }

            ClearCreateEventForm();
            LoadProviderEvents();
            LoadDashboardCounts();

            CreateEventMessageTextBlock.Text = _isEditMode
            ? "Event updated successfully."
            : "Event created successfully.";

        DashboardMessageTextBlock.Text = _isEditMode
            ? "The event was updated successfully."
            : "The event was created and added to your events list.";
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
                Id = _isEditMode ? _editingItemId : 0,
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
                MainImagePath = string.Empty,
                Gallery = galleryText,
                ServiceProviderId = serviceProvider.Id,
                Status = status
            };

            if (_isEditMode)
            {
                AppServices.CourseRepository.UpdateCourse(course);
            }
            else
            {
                AppServices.CourseRepository.AddCourse(course);
            }

            ClearCreateEventForm();
            LoadProviderEvents();
            LoadDashboardCounts();

            CreateEventMessageTextBlock.Text = _isEditMode
            ? "Course updated successfully."
            : "Course created successfully.";

        DashboardMessageTextBlock.Text = _isEditMode
            ? "The course was updated successfully."
            : "The course was created successfully.";
        }

        private void AnnouncementItemTypeComboBox_SelectionChanged(object? sender, SelectionChangedEventArgs e)
        {
            LoadAnnouncementTargetItems();
        }

        private void AnnouncementTargetModeComboBox_SelectionChanged(object? sender, SelectionChangedEventArgs e)
        {
            LoadAnnouncementTargetItems();
        }

        private void ProviderEventsItemsControl_ButtonClick(object? sender, RoutedEventArgs e)
        {
            if (e.Source is not Button button)
            {
                return;
            }

            if (button.Tag is not ProviderActivityListItem activityItem)
            {
                return;
            }

            if (button.Classes.Contains("edit-activity-button"))
            {
                OpenEditActivityForm(activityItem);
                e.Handled = true;
                return;
            }

            if (button.Classes.Contains("delete-activity-button"))
            {
                ShowDeleteConfirmation(activityItem);
                e.Handled = true;
            }
        }

        private void OpenEditActivityForm(ProviderActivityListItem activityItem)
        {
            HideDashboardPanels();

            CreateEventFormPanel.IsVisible = true;
            DeleteConfirmationPanel.IsVisible = false;

            _isEditMode = true;
            _editingItemId = activityItem.ItemId;
            _editingActivityType = activityItem.ActivityType;

            CreateItemTypeComboBox.IsEnabled = false;

            if (activityItem.ActivityType == "Event")
            {
                ActivityEvent? activityEvent = AppServices.ActivityEventRepository.GetEventById(activityItem.ItemId);

                if (activityEvent == null)
                {
                    DashboardMessageTextBlock.Text = "The selected event could not be found.";
                    return;
                }

                CreateItemTypeComboBox.SelectedIndex = 0;
                UpdateCreateFormMode();

                EventTitleTextBox.Text = activityEvent.Title;
                SelectComboBoxItemByContent(EventCategoryComboBox, activityEvent.Category);
                EventDescriptionTextBox.Text = activityEvent.Description;
                EventLatitudeTextBox.Text = activityEvent.Latitude.ToString(CultureInfo.InvariantCulture);
                EventLongitudeTextBox.Text = activityEvent.Longitude.ToString(CultureInfo.InvariantCulture);
                EventAddressTextBox.Text = activityEvent.Address;
                EventDatePicker.SelectedDate = activityEvent.Date;
                EventTimeTextBox.Text = activityEvent.Time.ToString(@"hh\:mm");
                EventPriceTextBox.Text = activityEvent.Price.ToString(CultureInfo.InvariantCulture);
                EventMaxSpaceTextBox.Text = activityEvent.MaxSpace.ToString();
                EventGalleryTextBox.Text = activityEvent.Gallery;
                SelectComboBoxItemByContent(EventStatusComboBox, activityEvent.Status.ToString());

                SaveEventButton.Content = "Update Event";
                CreateEventMessageTextBlock.Text = string.Empty;
                DashboardMessageTextBlock.Text = "Editing selected event.";

                return;
            }

            if (activityItem.ActivityType == "Course")
            {
                Course? course = AppServices.CourseRepository.GetCourseById(activityItem.ItemId);

                if (course == null)
                {
                    DashboardMessageTextBlock.Text = "The selected course could not be found.";
                    return;
                }

                CreateItemTypeComboBox.SelectedIndex = 1;
                UpdateCreateFormMode();

                EventTitleTextBox.Text = course.Title;
                SelectComboBoxItemByContent(EventCategoryComboBox, course.Category);
                EventDescriptionTextBox.Text = course.Description;
                EventLatitudeTextBox.Text = course.Latitude.ToString(CultureInfo.InvariantCulture);
                EventLongitudeTextBox.Text = course.Longitude.ToString(CultureInfo.InvariantCulture);
                EventAddressTextBox.Text = course.Address;
                CourseDaysTextBox.Text = course.Days;
                CourseStartTimeTextBox.Text = course.StartTime.ToString(@"hh\:mm");
                CourseEndTimeTextBox.Text = course.EndTime.ToString(@"hh\:mm");
                EventPriceTextBox.Text = course.Price.ToString(CultureInfo.InvariantCulture);
                EventMaxSpaceTextBox.Text = course.MaxSpace.ToString();
                CourseAgeRestrictionTextBox.Text = course.AgeRestriction;
                EventGalleryTextBox.Text = course.Gallery;
                SelectComboBoxItemByContent(EventStatusComboBox, course.Status.ToString());

                SaveEventButton.Content = "Update Course";
                CreateEventMessageTextBlock.Text = string.Empty;
                DashboardMessageTextBlock.Text = "Editing selected course.";
            }
        }

        private void ShowDeleteConfirmation(ProviderActivityListItem activityItem)
        {
            _pendingDeleteItem = activityItem;

            DeleteConfirmationTextBlock.Text =
                $"Are you sure you want to delete this {activityItem.ActivityType.ToLower()} \"{activityItem.Title}\"?";

            DeleteConfirmationPanel.IsVisible = true;
        }

        private void ConfirmDeleteButton_Click(object? sender, RoutedEventArgs e)
        {
            if (_pendingDeleteAnnouncement != null)
            {
                AppServices.AnnouncementRepository.DeleteAnnouncement(_pendingDeleteAnnouncement.Id);

                _pendingDeleteAnnouncement = null;
                DeleteConfirmationPanel.IsVisible = false;

                LoadProviderAnnouncements();
                LoadDashboardCounts();

                DashboardMessageTextBlock.Text = "The selected announcement was deleted.";
                return;
            }

            if (_pendingDeleteItem == null)
            {
                DeleteConfirmationPanel.IsVisible = false;
                return;
            }

            if (_pendingDeleteItem.ActivityType == "Event")
            {
                AppServices.ActivityEventRepository.DeleteEvent(_pendingDeleteItem.ItemId);
            }
            else if (_pendingDeleteItem.ActivityType == "Course")
            {
                AppServices.CourseRepository.DeleteCourse(_pendingDeleteItem.ItemId);
            }

            _pendingDeleteItem = null;

            DeleteConfirmationPanel.IsVisible = false;

            LoadProviderEvents();
            LoadDashboardCounts();

            DashboardMessageTextBlock.Text = "The selected item was deleted.";
        }

        private void CancelDeleteButton_Click(object? sender, RoutedEventArgs e)
        {
            _pendingDeleteItem = null;
            _pendingDeleteAnnouncement = null;

            DeleteConfirmationPanel.IsVisible = false;
        }

        private void SelectComboBoxItemByContent(ComboBox comboBox, string content)
        {
            foreach (object? item in comboBox.Items)
            {
                if (item is ComboBoxItem comboBoxItem &&
                    comboBoxItem.Content?.ToString() == content)
                {
                    comboBox.SelectedItem = comboBoxItem;
                    return;
                }
            }

            comboBox.SelectedItem = null;
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

        private void ProviderAnnouncementsItemsControl_ButtonClick(object? sender, RoutedEventArgs e)
        {
            if (e.Source is not Button button)
            {
                return;
            }

            if (button.Tag is not Announcement announcement)
            {
                return;
            }

            if (button.Classes.Contains("edit-activity-button"))
            {
                OpenEditAnnouncementForm(announcement);
                e.Handled = true;
                return;
            }

            if (button.Classes.Contains("delete-activity-button"))
            {
                ShowDeleteAnnouncementConfirmation(announcement);
                e.Handled = true;
            }
        }

        private void OpenEditAnnouncementForm(Announcement announcement)
        {
            HideDashboardPanels();

            CreateAnnouncementFormPanel.IsVisible = true;
            DeleteConfirmationPanel.IsVisible = false;

            _isAnnouncementEditMode = true;
            _editingAnnouncementId = announcement.Id;

            AnnouncementTitleTextBox.Text = announcement.Title;
            AnnouncementTextTextBox.Text = announcement.Text;
            AnnouncementGalleryTextBox.Text = announcement.Gallery;

            AnnouncementStatusComboBox.SelectedIndex = announcement.IsActive ? 0 : 1;

            AnnouncementTargetModeComboBox.SelectedIndex = 3;
            LoadAnnouncementTargetItems();

            List<AnnouncementTarget> existingTargets = AppServices.AnnouncementRepository
                .GetTargetsByAnnouncementId(announcement.Id);

            MarkSelectedAnnouncementTargets(existingTargets);

            PublishAnnouncementButton.Content = "Update Announcement";

            AnnouncementMessageTextBlock.Text = string.Empty;
            DashboardMessageTextBlock.Text = "Editing selected announcement.";
        }

        private void MarkSelectedAnnouncementTargets(List<AnnouncementTarget> existingTargets)
        {
            if (AnnouncementTargetItemsControl.ItemsSource is not IEnumerable<AnnouncementTargetSelectionItem> targetItems)
            {
                return;
            }

            foreach (AnnouncementTargetSelectionItem targetItem in targetItems)
            {
                targetItem.IsSelected = existingTargets.Any(existingTarget =>
                    existingTarget.ItemId == targetItem.ItemId &&
                    existingTarget.ItemType == targetItem.ItemType);
            }

            AnnouncementTargetItemsControl.ItemsSource = null;
            AnnouncementTargetItemsControl.ItemsSource = targetItems.ToList();
        }

        private void ShowDeleteAnnouncementConfirmation(Announcement announcement)
        {
            _pendingDeleteAnnouncement = announcement;
            _pendingDeleteItem = null;

            DeleteConfirmationTextBlock.Text =
                $"Are you sure you want to delete announcement \"{announcement.Title}\"?";

            DeleteConfirmationPanel.IsVisible = true;
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
                Id = _isAnnouncementEditMode ? _editingAnnouncementId : 0,
                Title = title,
                Text = text,
                AnnouncementDate = DateTime.Now,
                Gallery = gallery,
                IsActive = GetSelectedAnnouncementIsActive(),
                ServiceProviderId = serviceProvider.Id
            };

            if (_isAnnouncementEditMode)
            {
                AppServices.AnnouncementRepository.UpdateAnnouncement(announcement, targets);
            }
            else
            {
                AppServices.AnnouncementRepository.AddAnnouncement(announcement, targets);
            }

            ClearAnnouncementForm();
            LoadDashboardCounts();

            if (ProviderAnnouncementsPanel.IsVisible)
            {
                LoadProviderAnnouncements();
            }

            AnnouncementMessageTextBlock.Foreground = Avalonia.Media.Brushes.Green;
            AnnouncementMessageTextBlock.Text = _isAnnouncementEditMode
                ? "Announcement updated successfully."
                : "Announcement published successfully.";

            DashboardMessageTextBlock.Text = _isAnnouncementEditMode
                ? "The announcement was updated successfully."
                : "The announcement was published and linked to the selected events/courses.";
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
            _isAnnouncementEditMode = false;
            _editingAnnouncementId = 0;

            PublishAnnouncementButton.Content = "Publish";
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

            _isEditMode = false;
            _editingItemId = 0;
            _editingActivityType = string.Empty;

            CreateItemTypeComboBox.IsEnabled = true;
            SaveEventButton.Content = "Publish";

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