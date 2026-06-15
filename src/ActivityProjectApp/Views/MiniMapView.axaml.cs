using Avalonia;
using Avalonia.Controls;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Mapsui;
using Mapsui.Layers;
using Mapsui.Projections;
using Mapsui.Styles;
using Mapsui.Tiling;
using Mapsui.UI.Avalonia;
using System.Collections.Generic;
using System.Linq;

namespace ActivityProjectApp.Views
{
    public partial class MiniMapView : UserControl
    {
        public static readonly StyledProperty<double> LatitudeProperty =
            AvaloniaProperty.Register<MiniMapView, double>(nameof(Latitude));

        public static readonly StyledProperty<double> LongitudeProperty =
            AvaloniaProperty.Register<MiniMapView, double>(nameof(Longitude));

        public static readonly StyledProperty<string> AddressProperty =
            AvaloniaProperty.Register<MiniMapView, string>(nameof(Address), string.Empty);

        private const string MiniMapLayerName = "Mini Map Marker Layer";

        private MapControl? _mapControl;
        private bool _isAttachedToVisualTree = false;
        private bool _mapRefreshScheduled = false;

        public double Latitude
        {
            get => GetValue(LatitudeProperty);
            set => SetValue(LatitudeProperty, value);
        }

        public double Longitude
        {
            get => GetValue(LongitudeProperty);
            set => SetValue(LongitudeProperty, value);
        }

        public string Address
        {
            get => GetValue(AddressProperty);
            set => SetValue(AddressProperty, value);
        }

        public MiniMapView()
        {
            InitializeComponent();

            Loaded += MiniMapView_Loaded;
        }

        private void MiniMapView_Loaded(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            EnsureMapCreated();
            ScheduleMiniMapRefresh();
        }

        protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
        {
            base.OnAttachedToVisualTree(e);

            _isAttachedToVisualTree = true;

            EnsureMapCreated();
            ScheduleMiniMapRefresh();
        }

        protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
        {
            base.OnDetachedFromVisualTree(e);

            _isAttachedToVisualTree = false;
        }

        protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
        {
            base.OnPropertyChanged(change);

            if (change.Property == LatitudeProperty ||
                change.Property == LongitudeProperty ||
                change.Property == AddressProperty ||
                change.Property == IsVisibleProperty)
            {
                ScheduleMiniMapRefresh();
            }
        }

        private void EnsureMapCreated()
        {
            if (_mapControl != null)
            {
                return;
            }

            _mapControl = new MapControl();

            if (_mapControl.Map == null)
            {
                _mapControl.Map = new Mapsui.Map();
            }

            _mapControl.Map.Layers.Add(OpenStreetMap.CreateTileLayer());

            MiniMapHost.Content = _mapControl;
        }

        private void ScheduleMiniMapRefresh()
        {
            if (_mapRefreshScheduled)
            {
                return;
            }

            _mapRefreshScheduled = true;

            Dispatcher.UIThread.Post(() =>
            {
                _mapRefreshScheduled = false;

                if (!_isAttachedToVisualTree)
                {
                    return;
                }

                EnsureMapCreated();
                RefreshMiniMap();

            }, DispatcherPriority.Background);
        }

        private void RefreshMiniMap()
        {
            AddressTextBlock.Text = Address;

            if (_mapControl?.Map == null)
            {
                return;
            }

            if (!HasValidCoordinates())
            {
                return;
            }

            RemoveExistingMarkerLayer();

            MemoryLayer markerLayer = new MemoryLayer
            {
                Name = MiniMapLayerName,
                IsMapInfoLayer = false,
                Features = CreateMarkerFeature(),
                Style = null
            };

            _mapControl.Map.Layers.Add(markerLayer);

            CenterMapOnLocation();

            _mapControl.Map.Refresh();
        }

        private bool HasValidCoordinates()
        {
            double mapLatitude = GetCorrectedLatitude();
            double mapLongitude = GetCorrectedLongitude();

            if (mapLatitude == 0 && mapLongitude == 0)
            {
                return false;
            }

            if (mapLatitude < -90 || mapLatitude > 90)
            {
                return false;
            }

            if (mapLongitude < -180 || mapLongitude > 180)
            {
                return false;
            }

            return true;
        }

        private IEnumerable<IFeature> CreateMarkerFeature()
        {
            List<IFeature> features = new List<IFeature>();

            double mapLatitude = GetCorrectedLatitude();
            double mapLongitude = GetCorrectedLongitude();

            var mercatorPoint = SphericalMercator.FromLonLat(mapLongitude, mapLatitude);

            PointFeature feature = new PointFeature(
                new MPoint(mercatorPoint.x, mercatorPoint.y));

            feature.Styles.Add(new SymbolStyle
            {
                SymbolScale = 0.9,
                Fill = new Mapsui.Styles.Brush
                {
                    Color = Mapsui.Styles.Color.FromString("#DC2626")
                },
                Outline = new Mapsui.Styles.Pen
                {
                    Color = Mapsui.Styles.Color.FromString("#FFFFFF"),
                    Width = 3
                },
                RotateWithMap = false
            });

            features.Add(feature);

            return features;
        }

        private void RemoveExistingMarkerLayer()
        {
            if (_mapControl?.Map == null)
            {
                return;
            }

            ILayer? existingLayer = _mapControl.Map.Layers
                .FirstOrDefault(layer => layer.Name == MiniMapLayerName);

            if (existingLayer != null)
            {
                _mapControl.Map.Layers.Remove(existingLayer);
            }
        }

        private void CenterMapOnLocation()
        {
            if (_mapControl?.Map == null)
            {
                return;
            }

            if (_mapControl.Map.Navigator.Resolutions.Count == 0)
            {
                return;
            }

            double mapLatitude = GetCorrectedLatitude();
            double mapLongitude = GetCorrectedLongitude();

            var mercatorPoint = SphericalMercator.FromLonLat(mapLongitude, mapLatitude);

            MPoint centerPoint = new MPoint(
                mercatorPoint.x,
                mercatorPoint.y);

            int zoomLevel = 15;

            if (zoomLevel >= _mapControl.Map.Navigator.Resolutions.Count)
            {
                zoomLevel = _mapControl.Map.Navigator.Resolutions.Count - 1;
            }

            if (zoomLevel < 0)
            {
                zoomLevel = 0;
            }

            _mapControl.Map.Navigator.CenterOn(centerPoint);
            _mapControl.Map.Navigator.ZoomTo(_mapControl.Map.Navigator.Resolutions[zoomLevel]);
        }

        private double GetCorrectedLatitude()
        {
            if (LooksLikeGreekCoordinatesAreSwapped())
            {
                return Longitude;
            }

            return Latitude;
        }

        private double GetCorrectedLongitude()
        {
            if (LooksLikeGreekCoordinatesAreSwapped())
            {
                return Latitude;
            }

            return Longitude;
        }

        private bool LooksLikeGreekCoordinatesAreSwapped()
        {
            bool latitudeLooksLikeGreekLongitude = Latitude >= 19 && Latitude <= 30;
            bool longitudeLooksLikeGreekLatitude = Longitude >= 34 && Longitude <= 42;

            return latitudeLooksLikeGreekLongitude && longitudeLooksLikeGreekLatitude;
        }
    }
}