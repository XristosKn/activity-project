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

        private List<ActivityEvent> _currentEvents = new List<ActivityEvent>();

        private ActivityEvent? _selectedEvent;

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
            ZoomInButton.Click += ZoomInButton_Click;
            ZoomOutButton.Click += ZoomOutButton_Click;
            CenterMapButton.Click += CenterMapButton_Click;

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
            _currentEvents = AppServices.ActivityEventRepository.GetActiveEvents();

            AvailableEventsItemsControl.ItemsSource = _currentEvents;
            EventsCountTextBlock.Text = $"{_currentEvents.Count} events found";

            if (_currentEvents.Count == 0)
            {
                DashboardMessageTextBlock.Text = "No active events are available yet.";
            }
            else
            {
                DashboardMessageTextBlock.Text = string.Empty;
            }

            RefreshMapView();
        }

        private void SearchButton_Click(object? sender, RoutedEventArgs e)
        {
            string searchText = SearchTextBox.Text?.Trim() ?? string.Empty;

            _currentEvents = AppServices.ActivityEventRepository.SearchActiveEvents(searchText);

            AvailableEventsItemsControl.ItemsSource = _currentEvents;
            EventsCountTextBlock.Text = $"{_currentEvents.Count} events found";

            if (_currentEvents.Count == 0)
            {
                DashboardMessageTextBlock.Text = "No events found for your search.";
                SelectedEventPanel.IsVisible = false;
                _selectedEvent = null;
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
            _selectedEvent = null;
            SelectedEventPanel.IsVisible = false;

            LoadAvailableEvents();
        }

        private void CenterMapButton_Click(object? sender, RoutedEventArgs e)
        {
            RefreshMapView();
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

            foreach (ActivityEvent activityEvent in _currentEvents)
            {
                var mercatorPoint = SphericalMercator.FromLonLat(
                    activityEvent.Longitude,
                    activityEvent.Latitude);

                PointFeature feature = new PointFeature(
                    new MPoint(mercatorPoint.x, mercatorPoint.y));

                feature["EventId"] = activityEvent.Id;
                feature["Title"] = activityEvent.Title;
                feature["Category"] = activityEvent.Category;

                feature.Styles.Add(CreateMapPinStyle(activityEvent));

                features.Add(feature);
            }

            return features;
        }

        private SymbolStyle CreateMapPinStyle(ActivityEvent activityEvent)
        {
            bool isSelected = _selectedEvent != null && _selectedEvent.Id == activityEvent.Id;

            Mapsui.Styles.Color categoryColor = CategoryStyleHelper.GetMapColor(activityEvent.Category);

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
            int? selectedEventId = TryGetSelectedEventId(e);

            if (selectedEventId == null)
            {
                return;
            }

            ActivityEvent? selectedEvent = _currentEvents
                .FirstOrDefault(activityEvent => activityEvent.Id == selectedEventId.Value);

            if (selectedEvent == null)
            {
                return;
            }

            SelectEvent(selectedEvent);

            e.Handled = true;
        }

        private int? TryGetSelectedEventId(MapInfoEventArgs e)
        {
            object? eventIdValue = e.MapInfo?.Feature?["EventId"];

            if (eventIdValue == null)
            {
                return null;
            }

            if (eventIdValue is int eventId)
            {
                return eventId;
            }

            if (int.TryParse(eventIdValue.ToString(), out int parsedEventId))
            {
                return parsedEventId;
            }

            return null;
        }

        private void CenterMapOnCurrentEvents()
        {
            double centerLatitude = DefaultLatitude;
            double centerLongitude = DefaultLongitude;

            if (_currentEvents.Count > 0)
            {
                centerLatitude = _currentEvents.Average(activityEvent => activityEvent.Latitude);
                centerLongitude = _currentEvents.Average(activityEvent => activityEvent.Longitude);
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
            if (AvailableEventsItemsControl.SelectedItem is not ActivityEvent selectedEvent)
            {
                return;
            }

            SelectEvent(selectedEvent);
        }
        private void SelectEvent(ActivityEvent activityEvent)
        {
            _selectedEvent = activityEvent;

            if (AvailableEventsItemsControl.SelectedItem != activityEvent)
            {
                AvailableEventsItemsControl.SelectedItem = activityEvent;
            }

            SelectedEventPanel.IsVisible = true;

            SelectedEventTitleTextBlock.Text = activityEvent.Title;

            SelectedEventCategoryTextBlock.Text = activityEvent.Category;
            SelectedEventCategoryTextBlock.Foreground =
                CategoryStyleHelper.GetAvaloniaBrush(activityEvent.Category);

            SelectedEventAddressTextBlock.Text = activityEvent.Address;

            SelectedEventInfoTextBlock.Text =
                $"{activityEvent.Date:dd/MM/yyyy} at {activityEvent.Time:hh\\:mm} | €{activityEvent.Price} | Max spaces: {activityEvent.MaxSpace}";

            DashboardMessageTextBlock.Text = $"Selected event: {activityEvent.Title}";

            SetMapView(activityEvent.Latitude, activityEvent.Longitude, _currentZoomLevel);

            UpdateActivityEventLayer();
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