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
using System.Text;

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

        private readonly Dictionary<string, int> _pinBitmapIdsByColor = new Dictionary<string, int>();
        

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

            AvailableEventsItemsControl.SelectionChanged += AvailableEventsItemsControl_SelectionChanged;

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
            }
            else
            {
                WelcomeTextBlock.Text = "Welcome, Customer";
            }
        }

        private void LoadAvailableEvents()
        {
            _currentActivities = GetAvailableActivities(string.Empty);

            AvailableEventsItemsControl.ItemsSource = _currentActivities;
            EventsCountTextBlock.Text = $"{_currentActivities.Count} activities found";

            if (_currentActivities.Count == 0)
            {
                DashboardMessageTextBlock.Text = "No active events or courses are available yet.";
            }
            else
            {
                DashboardMessageTextBlock.Text = string.Empty;
            }

            RefreshMapView();
        }

        private List<CustomerActivityListItem> GetAvailableActivities(string searchText)
        {
            List<CustomerActivityListItem> activities = new List<CustomerActivityListItem>();

            List<ActivityEvent> activeEvents = string.IsNullOrWhiteSpace(searchText)
                ? AppServices.ActivityEventRepository.GetActiveEvents()
                : AppServices.ActivityEventRepository.SearchActiveEvents(searchText);

            List<Course> activeCourses = string.IsNullOrWhiteSpace(searchText)
                ? AppServices.CourseRepository.GetActiveCourses()
                : AppServices.CourseRepository.SearchActiveCourses(searchText);

            activities.AddRange(activeEvents.Select(activityEvent => new CustomerActivityListItem
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
                ExtraInfoText = $"Event | {activityEvent.Date:dd/MM/yyyy} at {activityEvent.Time:hh\\:mm} | €{activityEvent.Price} | Max spaces: {activityEvent.MaxSpace}"
            }));

            activities.AddRange(activeCourses.Select(course => new CustomerActivityListItem
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
                ExtraInfoText = $"Course | {course.Days}, {course.StartTime:hh\\:mm} - {course.EndTime:hh\\:mm} | €{course.Price} | Max spaces: {course.MaxSpace} | Age: {course.AgeRestriction}"
            }));

            return activities;
        }

        private void SearchButton_Click(object? sender, RoutedEventArgs e)
        {
            string searchText = SearchTextBox.Text?.Trim() ?? string.Empty;

            _currentActivities = GetAvailableActivities(searchText);

            AvailableEventsItemsControl.ItemsSource = _currentActivities;
            EventsCountTextBlock.Text = $"{_currentActivities.Count} activities found";

            if (_currentActivities.Count == 0)
            {
                DashboardMessageTextBlock.Text = "No events or courses found for your search.";
                SelectedEventPanel.IsVisible = false;
                _selectedActivity = null;
                AvailableEventsItemsControl.SelectedItem = null;
            }
            else
            {
                DashboardMessageTextBlock.Text = string.Empty;
            }

            RefreshMapView();
        }

        private void ClearSearchButton_Click(object? sender, RoutedEventArgs e)
        {
            SearchTextBox.Text = string.Empty;
            _selectedActivity = null;
            SelectedEventPanel.IsVisible = false;
            AvailableEventsItemsControl.SelectedItem = null;

            LoadAvailableEvents();
        }

        private void CenterMapButton_Click(object? sender, RoutedEventArgs e)
        {
            RefreshMapView();
        }

        private void EnrollButton_Click(object? sender, RoutedEventArgs e)
        {
            if (_selectedActivity == null)
            {
                DashboardMessageTextBlock.Text = "Please select an event or course before enrolling.";
                return;
            }

            if (_authService.GetCurrentUser() is not Customer customer)
            {
                DashboardMessageTextBlock.Text = "Only customers can enroll.";
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
                DashboardMessageTextBlock.Text = $"Map loading error: {ex.Message}";
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

        private int GetOrCreatePinBitmapId(string fillColorHex, string outlineColorHex)
        {
            string key = $"pin-{fillColorHex}-{outlineColorHex}";

            if (_pinBitmapIdsByColor.TryGetValue(key, out int existingBitmapId))
            {
                return existingBitmapId;
            }

            string svg = CreatePinSvg(fillColorHex, outlineColorHex);

            byte[] svgBytes = Encoding.UTF8.GetBytes(svg);

            int bitmapId = BitmapRegistry.Instance.Register(svgBytes);

            _pinBitmapIdsByColor[key] = bitmapId;

            return bitmapId;
        }

        private string CreatePinSvg(string fillColor, string outlineColor)
        {
            return $@"
        <svg width=""48"" height=""64"" viewBox=""0 0 48 64"" xmlns=""http://www.w3.org/2000/svg"">
            <path d=""M24 2 C12 2 4 11 4 23 C4 40 24 62 24 62 C24 62 44 40 44 23 C44 11 36 2 24 2 Z""
                fill=""{fillColor}""
                stroke=""{outlineColor}""
                stroke-width=""4""/>
            <circle cx=""24"" cy=""23"" r=""8""
                    fill=""#FFFFFF""/>
        </svg>";
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

            DashboardMessageTextBlock.Text = $"Selected {activity.ActivityType.ToLower()}: {activity.Title}";

            SetMapView(activity.Latitude, activity.Longitude, _currentZoomLevel);

            UpdateActivityEventLayer();
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