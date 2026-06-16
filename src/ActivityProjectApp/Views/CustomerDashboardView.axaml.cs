using Avalonia.Controls;
using Avalonia.Interactivity;
using ActivityProjectApp.Models;
using ActivityProjectApp.Services;
using ActivityProjectApp.Helpers;
using Mapsui;
using Mapsui.Layers;
using Mapsui.Projections;
using Mapsui.Styles;
using Mapsui.Tiling;
using Mapsui.UI.Avalonia;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ActivityProjectApp.Views
{
    public partial class CustomerDashboardView : UserControl
    {
        private readonly AuthService _authService;
        private readonly MapControl _mapControl;

        private List<CustomerActivityListItem> _currentActivities = new List<CustomerActivityListItem>();
        private CustomerActivityListItem? _selectedActivity;

        private bool _isViewLoaded = false;
        private int _currentZoomLevel = 13;

        private const double DefaultLatitude = 37.9838;
        private const double DefaultLongitude = 23.7275;
        private const string ActivityEventsLayerName = "Activity Events Layer";

        public CustomerDashboardView()
        {
            InitializeComponent();

            _authService = AppServices.AuthService;

            _mapControl = new MapControl();

            if (_mapControl.Map == null)
            {
                _mapControl.Map = new Mapsui.Map();
            }

            _mapControl.Map.Layers.Add(OpenStreetMap.CreateTileLayer());
            _mapControl.Info += MapControl_Info;

            MapHost.Content = _mapControl;

            LoadCurrentUser();
            LoadAvailableEvents();

            SearchButton.Click += SearchButton_Click;
            ClearSearchButton.Click += ClearSearchButton_Click;
            LogoutButton.Click += LogoutButton_Click;
            ProfileButton.Click += ProfileButton_Click;
            ZoomInButton.Click += ZoomInButton_Click;
            ZoomOutButton.Click += ZoomOutButton_Click;
            CenterMapButton.Click += CenterMapButton_Click;
            EnrollButton.Click += EnrollButton_Click;
            SaveButton.Click += SaveButton_Click;

            AvailableEventsItemsControl.SelectionChanged += AvailableEventsItemsControl_SelectionChanged;
            AvailableEventsItemsControl.AddHandler(
                Button.ClickEvent,
                AvailableEventsItemsControl_ButtonClick,
                RoutingStrategies.Bubble);

            Loaded += CustomerDashboardView_Loaded;
        }

        private void CustomerDashboardView_Loaded(object? sender, RoutedEventArgs e)
        {
            _isViewLoaded = true;
            RefreshMapView();
        }

        private void LoadCurrentUser()
        {
            User? currentUser = _authService.GetCurrentUser();

            if (currentUser != null)
            {
                WelcomeTextBlock.Text = $"Welcome, {currentUser.Name} {currentUser.LastName}";
                return;
            }

            WelcomeTextBlock.Text = "Welcome, Customer";
        }

        private void ShowSuccessMessage(string message)
        {
            DashboardMessageTextBlock.Foreground = Avalonia.Media.Brushes.Green;
            DashboardMessageTextBlock.Text = message;
        }

        private void ShowErrorMessage(string message)
        {
            DashboardMessageTextBlock.Foreground = Avalonia.Media.Brushes.Red;
            DashboardMessageTextBlock.Text = message;
        }

        private void ClearDashboardMessage()
        {
            DashboardMessageTextBlock.Text = string.Empty;
        }

        private void LoadAvailableEvents()
        {
            _currentActivities = GetAvailableActivities(string.Empty);

            AvailableEventsItemsControl.ItemsSource = _currentActivities;
            EventsCountTextBlock.Text = $"{_currentActivities.Count} activities found";

            if (_currentActivities.Count == 0)
            {
                ShowErrorMessage("No active events or courses are available yet.");
            }
            else
            {
                ClearDashboardMessage();
            }

            RefreshMapView();
        }

        private List<CustomerActivityListItem> GetAvailableActivities(string searchText)
        {
            List<CustomerActivityListItem> activities = new List<CustomerActivityListItem>();

            Customer? currentCustomer = _authService.GetCurrentUser() as Customer;

            List<SavedItem> customerSavedItems = currentCustomer == null
                ? new List<SavedItem>()
                : AppServices.SavedItemRepository.GetSavedItemsByCustomerId(currentCustomer.Id);

            List<Enrollment> customerEnrollments = currentCustomer == null
                ? new List<Enrollment>()
                : AppServices.EnrollmentRepository.GetEnrollmentsByCustomerId(currentCustomer.Id);

            List<Enrollment> allActiveEnrollments = AppServices.EnrollmentRepository
                .GetAllEnrollments()
                .Where(enrollment => enrollment.Status != EnrollmentStatus.Cancelled)
                .ToList();

            List<ActivityEvent> activeEvents = string.IsNullOrWhiteSpace(searchText)
                ? AppServices.ActivityEventRepository.GetActiveEvents()
                : AppServices.ActivityEventRepository.SearchActiveEvents(searchText);

            List<Course> activeCourses = string.IsNullOrWhiteSpace(searchText)
                ? AppServices.CourseRepository.GetActiveCourses()
                : AppServices.CourseRepository.SearchActiveCourses(searchText);

            foreach (ActivityEvent activityEvent in activeEvents)
            {
                int currentEnrollmentsCount = allActiveEnrollments.Count(enrollment =>
                    enrollment.ItemId == activityEvent.Id &&
                    enrollment.ItemType == EnrollmentItemType.Event);

                if (currentEnrollmentsCount >= activityEvent.MaxSpace)
                {
                    continue;
                }

                bool isSaved = customerSavedItems.Any(savedItem =>
                    savedItem.ItemId == activityEvent.Id &&
                    savedItem.ItemType == SavedItemType.Event);

                bool hasConfirmedEnrollment = customerEnrollments.Any(enrollment =>
                    enrollment.ItemId == activityEvent.Id &&
                    enrollment.ItemType == EnrollmentItemType.Event &&
                    enrollment.Status == EnrollmentStatus.Confirmed);

                if (hasConfirmedEnrollment)
                {
                    continue;
                }

                bool hasPendingPayment = customerEnrollments.Any(enrollment =>
                    enrollment.ItemId == activityEvent.Id &&
                    enrollment.ItemType == EnrollmentItemType.Event &&
                    enrollment.Status == EnrollmentStatus.PendingPayment);

                activities.Add(new CustomerActivityListItem
                {
                    ItemId = activityEvent.Id,
                    ItemType = EnrollmentItemType.Event,
                    ActivityType = "Event",
                    Title = activityEvent.Title,
                    Category = activityEvent.Category,
                    Description = activityEvent.Description,
                    Latitude = activityEvent.Latitude,
                    Longitude = activityEvent.Longitude,
                    Address = activityEvent.Address,
                    ScheduleText = $"{activityEvent.Date:dd/MM/yyyy} at {activityEvent.Time:hh\\:mm}",
                    Price = activityEvent.Price,
                    PriceText = $"€{activityEvent.Price}",
                    MaxSpace = activityEvent.MaxSpace,
                    ExtraInfoText = hasPendingPayment
                        ? $"Pending payment | Event | {activityEvent.Date:dd/MM/yyyy} at {activityEvent.Time:hh\\:mm} | €{activityEvent.Price}"
                        : $"Event | {activityEvent.Date:dd/MM/yyyy} at {activityEvent.Time:hh\\:mm} | €{activityEvent.Price} | Max spaces: {activityEvent.MaxSpace}",
                    IsSaved = isSaved,
                    IsAlreadyEnrolled = hasConfirmedEnrollment,
                    HasPendingPayment = hasPendingPayment
                });
            }

            foreach (Course course in activeCourses)
            {
                int currentEnrollmentsCount = allActiveEnrollments.Count(enrollment =>
                    enrollment.ItemId == course.Id &&
                    enrollment.ItemType == EnrollmentItemType.Course);

                if (currentEnrollmentsCount >= course.MaxSpace)
                {
                    continue;
                }

                bool isSaved = customerSavedItems.Any(savedItem =>
                    savedItem.ItemId == course.Id &&
                    savedItem.ItemType == SavedItemType.Course);

                bool hasConfirmedEnrollment = customerEnrollments.Any(enrollment =>
                    enrollment.ItemId == course.Id &&
                    enrollment.ItemType == EnrollmentItemType.Course &&
                    enrollment.Status == EnrollmentStatus.Confirmed);

                if (hasConfirmedEnrollment)
                {
                    continue;
                }

                bool hasPendingPayment = customerEnrollments.Any(enrollment =>
                    enrollment.ItemId == course.Id &&
                    enrollment.ItemType == EnrollmentItemType.Course &&
                    enrollment.Status == EnrollmentStatus.PendingPayment);

                activities.Add(new CustomerActivityListItem
                {
                    ItemId = course.Id,
                    ItemType = EnrollmentItemType.Course,
                    ActivityType = "Course",
                    Title = course.Title,
                    Category = course.Category,
                    Description = course.Description,
                    Latitude = course.Latitude,
                    Longitude = course.Longitude,
                    Address = course.Address,
                    ScheduleText = $"{course.Days}, {course.StartTime:hh\\:mm} - {course.EndTime:hh\\:mm}",
                    Price = course.Price,
                    PriceText = $"€{course.Price}",
                    MaxSpace = course.MaxSpace,
                    ExtraInfoText = $"Course | {course.Days}, {course.StartTime:hh\\:mm} - {course.EndTime:hh\\:mm} | €{course.Price} | Max spaces: {course.MaxSpace} | Age: {course.AgeRestriction}",
                    IsSaved = isSaved,
                    IsAlreadyEnrolled = hasConfirmedEnrollment,
                    HasPendingPayment = hasPendingPayment
                });
            }

            return activities;
        }

        private void SearchButton_Click(object? sender, RoutedEventArgs e)
        {
            string searchText = SearchTextBox.Text?.Trim() ?? string.Empty;

            _selectedActivity = null;
            SelectedEventPanel.IsVisible = false;
            AvailableEventsItemsControl.SelectedItem = null;

            _currentActivities = GetAvailableActivities(searchText);

            AvailableEventsItemsControl.ItemsSource = _currentActivities;
            EventsCountTextBlock.Text = $"{_currentActivities.Count} activities found";

            if (_currentActivities.Count == 0)
            {
                ShowErrorMessage("No events or courses found for your search.");
            }
            else
            {
                ClearDashboardMessage();
            }

            UpdateSaveButtonState();
            UpdateEnrollButtonState();
            RefreshMapView();
        }

        private void ClearSearchButton_Click(object? sender, RoutedEventArgs e)
        {
            SearchTextBox.Text = string.Empty;

            _selectedActivity = null;
            SelectedEventPanel.IsVisible = false;
            AvailableEventsItemsControl.SelectedItem = null;

            UpdateSaveButtonState();
            UpdateEnrollButtonState();

            LoadAvailableEvents();
        }

        private void CenterMapButton_Click(object? sender, RoutedEventArgs e)
        {
            RefreshMapView();
        }

        private SavedItemType ConvertToSavedItemType(EnrollmentItemType itemType)
        {
            if (itemType == EnrollmentItemType.Course)
            {
                return SavedItemType.Course;
            }

            return SavedItemType.Event;
        }

        private void EnrollButton_Click(object? sender, RoutedEventArgs e)
        {
            if (_selectedActivity == null)
            {
                ShowErrorMessage("Please select an event or course before enrolling.");
                return;
            }

            if (_authService.GetCurrentUser() is not Customer customer)
            {
                ShowErrorMessage("Only customers can enroll.");
                return;
            }

            Enrollment? existingEnrollment = AppServices.EnrollmentRepository.GetEnrollment(
                customer.Id,
                _selectedActivity.ItemId,
                _selectedActivity.ItemType);

            if (existingEnrollment != null &&
                existingEnrollment.Status == EnrollmentStatus.PendingPayment)
            {
                MainWindow? paymentWindow = this.VisualRoot as MainWindow;

                if (paymentWindow == null)
                {
                    return;
                }

                paymentWindow.Content = new EventCourseDetailsView(
                    _selectedActivity.ItemId,
                    _selectedActivity.ItemType,
                    existingEnrollment);

                return;
            }

            if (existingEnrollment != null &&
                existingEnrollment.Status == EnrollmentStatus.Confirmed)
            {
                ShowErrorMessage("You are already enrolled in this item.");
                UpdateEnrollButtonState();
                return;
            }

            int currentEnrollmentsCount = AppServices.EnrollmentRepository.GetActiveEnrollmentCount(
                _selectedActivity.ItemId,
                _selectedActivity.ItemType);

            if (currentEnrollmentsCount >= _selectedActivity.MaxSpace)
            {
                ShowErrorMessage("This event/course is full. Enrollment is not available.");

                _selectedActivity = null;
                SelectedEventPanel.IsVisible = false;
                AvailableEventsItemsControl.SelectedItem = null;

                ReloadAvailableActivitiesPreservingMap(false);

                UpdateSaveButtonState();
                UpdateEnrollButtonState();

                return;
            }

            Enrollment enrollment = AppServices.EnrollmentRepository.CreateEnrollment(
                customer.Id,
                _selectedActivity.ItemId,
                _selectedActivity.ItemType);

            MainWindow? mainWindow = this.VisualRoot as MainWindow;

            if (mainWindow == null)
            {
                return;
            }

            mainWindow.Content = new EventCourseDetailsView(
                _selectedActivity.ItemId,
                _selectedActivity.ItemType,
                enrollment);
        }

        private void RefreshMapView()
        {
            if (!_isViewLoaded)
            {
                return;
            }

            try
            {
                UpdateActivityEventLayer();
                CenterMapOnCurrentEvents();
            }
            catch (Exception ex)
            {
                ShowErrorMessage($"Map loading error: {ex.Message}");
            }
        }

        private void UpdateActivityEventLayer()
        {
            if (_mapControl.Map == null)
            {
                return;
            }

            RemoveExistingActivityEventLayer();

            MemoryLayer activityEventLayer = new MemoryLayer
            {
                Name = ActivityEventsLayerName,
                IsMapInfoLayer = true,
                Features = CreateActivityEventFeatures(),
                Style = null
            };

            _mapControl.Map.Layers.Add(activityEventLayer);
            _mapControl.Map.Refresh();
        }

        private IEnumerable<IFeature> CreateActivityEventFeatures()
        {
            List<IFeature> features = new List<IFeature>();

            foreach (CustomerActivityListItem activity in _currentActivities)
            {
                var mercatorPoint = SphericalMercator.FromLonLat(
                    activity.Longitude,
                    activity.Latitude);

                PointFeature feature = new PointFeature(
                    new MPoint(mercatorPoint.x, mercatorPoint.y));

                feature["ItemId"] = activity.ItemId;
                feature["ItemType"] = activity.ItemType.ToString();
                feature["Title"] = activity.Title;
                feature["Category"] = activity.Category;

                feature.Styles.Add(CreateMapPinStyle(activity));

                features.Add(feature);
            }

            return features;
        }

        private SymbolStyle CreateMapPinStyle(CustomerActivityListItem activity)
        {
            bool isSelected =
                _selectedActivity != null &&
                _selectedActivity.ItemId == activity.ItemId &&
                _selectedActivity.ItemType == activity.ItemType;

            Mapsui.Styles.Color categoryColor = CategoryStyleHelper.GetMapColor(activity.Category);

            return new SymbolStyle
            {
                SymbolScale = isSelected ? 0.85 : 0.58,
                Fill = new Mapsui.Styles.Brush
                {
                    Color = categoryColor
                },
                Outline = new Mapsui.Styles.Pen
                {
                    Color = isSelected
                        ? Mapsui.Styles.Color.FromString("#FACC15")
                        : Mapsui.Styles.Color.FromString("#FFFFFF"),
                    Width = isSelected ? 3 : 2
                },
                RotateWithMap = false
            };
        }

        private void RemoveExistingActivityEventLayer()
        {
            if (_mapControl.Map == null)
            {
                return;
            }

            ILayer? existingLayer = _mapControl.Map.Layers
                .FirstOrDefault(layer => layer.Name == ActivityEventsLayerName);

            if (existingLayer != null)
            {
                _mapControl.Map.Layers.Remove(existingLayer);
            }
        }

        private void MapControl_Info(object? sender, MapInfoEventArgs e)
        {
            CustomerActivityListItem? selectedActivity = TryGetSelectedActivityFromMap(e);

            if (selectedActivity == null)
            {
                return;
            }

            SelectActivity(selectedActivity);

            e.Handled = true;
        }

        private CustomerActivityListItem? TryGetSelectedActivityFromMap(MapInfoEventArgs e)
        {
            object? itemIdValue = e.MapInfo?.Feature?["ItemId"];
            object? itemTypeValue = e.MapInfo?.Feature?["ItemType"];

            if (itemIdValue == null || itemTypeValue == null)
            {
                return null;
            }

            if (!int.TryParse(itemIdValue.ToString(), out int itemId))
            {
                return null;
            }

            if (!Enum.TryParse(itemTypeValue.ToString(), out EnrollmentItemType itemType))
            {
                return null;
            }

            return _currentActivities.FirstOrDefault(activity =>
                activity.ItemId == itemId &&
                activity.ItemType == itemType);
        }

        private void CenterMapOnCurrentEvents()
        {
            double centerLatitude = DefaultLatitude;
            double centerLongitude = DefaultLongitude;

            if (_currentActivities.Count > 0)
            {
                centerLatitude = _currentActivities.Average(activity => activity.Latitude);
                centerLongitude = _currentActivities.Average(activity => activity.Longitude);
            }

            SetMapView(centerLatitude, centerLongitude, _currentZoomLevel);
        }

        private void SetMapView(double latitude, double longitude, int zoomLevel)
        {
            if (_mapControl.Map == null)
            {
                return;
            }

            if (_mapControl.Map.Navigator.Resolutions.Count == 0)
            {
                return;
            }

            int safeZoomLevel = zoomLevel;

            if (safeZoomLevel < 0)
            {
                safeZoomLevel = 0;
            }

            if (safeZoomLevel >= _mapControl.Map.Navigator.Resolutions.Count)
            {
                safeZoomLevel = _mapControl.Map.Navigator.Resolutions.Count - 1;
            }

            var mercatorPoint = SphericalMercator.FromLonLat(longitude, latitude);

            MPoint centerPoint = new MPoint(
                mercatorPoint.x,
                mercatorPoint.y);

            _mapControl.Map.Navigator.CenterOnAndZoomTo(
                centerPoint,
                _mapControl.Map.Navigator.Resolutions[safeZoomLevel]);
        }

        private void ZoomInButton_Click(object? sender, RoutedEventArgs e)
        {
            _currentZoomLevel++;
            CenterMapOnCurrentEvents();
        }

        private void ZoomOutButton_Click(object? sender, RoutedEventArgs e)
        {
            _currentZoomLevel--;
            CenterMapOnCurrentEvents();
        }

        private void AvailableEventsItemsControl_SelectionChanged(object? sender, SelectionChangedEventArgs e)
        {
            if (AvailableEventsItemsControl.SelectedItem is not CustomerActivityListItem selectedActivity)
            {
                return;
            }

            SelectActivity(selectedActivity);
        }

        private void SelectActivity(CustomerActivityListItem activity)
        {
            _selectedActivity = activity;

            if (AvailableEventsItemsControl.SelectedItem != activity)
            {
                AvailableEventsItemsControl.SelectedItem = activity;
            }

            SelectedEventPanel.IsVisible = true;

            SelectedEventTitleTextBlock.Text = activity.Title;

            SelectedEventCategoryTextBlock.Text = $"{activity.ActivityType} • {activity.Category}";
            SelectedEventCategoryTextBlock.Foreground =
                CategoryStyleHelper.GetAvaloniaBrush(activity.Category);

            SelectedEventAddressTextBlock.Text = activity.Address;
            SelectedEventInfoTextBlock.Text = activity.ExtraInfoText;

            ShowSuccessMessage($"Selected {activity.ActivityType.ToLower()}: {activity.Title}");

            SetMapView(activity.Latitude, activity.Longitude, _currentZoomLevel);

            UpdateSaveButtonState();
            UpdateEnrollButtonState();
        }

        private void AvailableEventsItemsControl_ButtonClick(object? sender, RoutedEventArgs e)
        {
            if (e.Source is not Button button)
            {
                return;
            }

            if (!button.Classes.Contains("activity-card-unsave-button"))
            {
                return;
            }

            if (button.Tag is not CustomerActivityListItem activityItem)
            {
                return;
            }

            UnsaveActivityFromCard(activityItem);

            e.Handled = true;
        }

        private void UnsaveActivityFromCard(CustomerActivityListItem activityItem)
        {
            if (_authService.GetCurrentUser() is not Customer customer)
            {
                ShowErrorMessage("Only customers can remove saved items.");
                return;
            }

            SavedItemType savedItemType = ConvertToSavedItemType(activityItem.ItemType);

            AppServices.SavedItemRepository.RemoveSavedItem(
                customer.Id,
                activityItem.ItemId,
                savedItemType);

            ShowSuccessMessage($"{activityItem.Title} removed from saved items.");

            activityItem.IsSaved = false;

            ReloadAvailableActivitiesPreservingMap(false);
            UpdateSelectedActivityAfterListReload(activityItem.ItemId, activityItem.ItemType);
        }

        private void ReloadAvailableActivitiesPreservingMap(bool refreshMap)
        {
            string searchText = SearchTextBox.Text?.Trim() ?? string.Empty;

            _currentActivities = GetAvailableActivities(searchText);

            AvailableEventsItemsControl.ItemsSource = null;
            AvailableEventsItemsControl.ItemsSource = _currentActivities;

            EventsCountTextBlock.Text = $"{_currentActivities.Count} activities found";

            if (refreshMap)
            {
                RefreshMapView();
            }
        }

        private void UpdateSelectedActivityAfterListReload(int itemId, EnrollmentItemType itemType)
        {
            CustomerActivityListItem? refreshedSelectedActivity = _currentActivities
                .FirstOrDefault(activity =>
                    activity.ItemId == itemId &&
                    activity.ItemType == itemType);

            if (refreshedSelectedActivity == null)
            {
                _selectedActivity = null;
                SelectedEventPanel.IsVisible = false;
                AvailableEventsItemsControl.SelectedItem = null;

                UpdateSaveButtonState();
                UpdateEnrollButtonState();
                return;
            }

            _selectedActivity = refreshedSelectedActivity;
            AvailableEventsItemsControl.SelectedItem = refreshedSelectedActivity;

            UpdateSaveButtonState();
            UpdateEnrollButtonState();
        }

        private void SaveButton_Click(object? sender, RoutedEventArgs e)
        {
            if (_selectedActivity == null)
            {
                ShowErrorMessage("Please select an event or course before saving.");
                return;
            }

            if (_authService.GetCurrentUser() is not Customer customer)
            {
                ShowErrorMessage("Only customers can save items.");
                return;
            }

            SavedItemType savedItemType = ConvertToSavedItemType(_selectedActivity.ItemType);

            bool alreadySaved = AppServices.SavedItemRepository.IsItemSaved(
                customer.Id,
                _selectedActivity.ItemId,
                savedItemType);

            int selectedItemId = _selectedActivity.ItemId;
            EnrollmentItemType selectedItemType = _selectedActivity.ItemType;

            if (alreadySaved)
            {
                AppServices.SavedItemRepository.RemoveSavedItem(
                    customer.Id,
                    _selectedActivity.ItemId,
                    savedItemType);

                ShowSuccessMessage($"{_selectedActivity.Title} removed from saved items.");
            }
            else
            {
                AppServices.SavedItemRepository.SaveItem(
                    customer.Id,
                    _selectedActivity.ItemId,
                    savedItemType);

                ShowSuccessMessage($"{_selectedActivity.Title} saved successfully.");
            }

            ReloadAvailableActivitiesPreservingMap(false);
            UpdateSelectedActivityAfterListReload(selectedItemId, selectedItemType);
        }

        private void UpdateSaveButtonState()
        {
            if (_selectedActivity == null)
            {
                SaveButton.Content = "Save";
                SaveButton.Classes.Remove("saved");
                return;
            }

            if (_authService.GetCurrentUser() is not Customer customer)
            {
                SaveButton.Content = "Save";
                SaveButton.Classes.Remove("saved");
                return;
            }

            SavedItemType savedItemType = ConvertToSavedItemType(_selectedActivity.ItemType);

            bool isSaved = AppServices.SavedItemRepository.IsItemSaved(
                customer.Id,
                _selectedActivity.ItemId,
                savedItemType);

            if (isSaved)
            {
                SaveButton.Content = "Saved";
                SaveButton.Classes.Add("saved");
            }
            else
            {
                SaveButton.Content = "Save";
                SaveButton.Classes.Remove("saved");
            }
        }

        private void UpdateEnrollButtonState()
        {
            if (_selectedActivity == null)
            {
                EnrollButton.Content = "Enroll";
                EnrollButton.IsEnabled = false;
                return;
            }

            if (_selectedActivity.HasPendingPayment)
            {
                EnrollButton.Content = "View / Enroll";
                EnrollButton.IsEnabled = true;
                return;
            }

            if (_authService.GetCurrentUser() is not Customer customer)
            {
                EnrollButton.Content = "Enroll";
                EnrollButton.IsEnabled = false;
                return;
            }

            bool alreadyConfirmed = AppServices.EnrollmentRepository
                .GetEnrollmentsByCustomerId(customer.Id)
                .Any(enrollment =>
                    enrollment.ItemId == _selectedActivity.ItemId &&
                    enrollment.ItemType == _selectedActivity.ItemType &&
                    enrollment.Status == EnrollmentStatus.Confirmed);

            if (alreadyConfirmed)
            {
                EnrollButton.Content = "Enrolled";
                EnrollButton.IsEnabled = false;
                return;
            }

            EnrollButton.Content = "Enroll";
            EnrollButton.IsEnabled = true;
        }

        private void ProfileButton_Click(object? sender, RoutedEventArgs e)
        {
            MainWindow? mainWindow = this.VisualRoot as MainWindow;

            if (mainWindow == null)
            {
                return;
            }

            mainWindow.Content = new CustomerProfileView();
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