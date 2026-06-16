using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using ActivityProjectApp.Models;
using ActivityProjectApp.Services;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using AppActivityEvent = ActivityProjectApp.Models.ActivityEvent;

namespace ActivityProjectApp.Views
{
    public partial class EventCourseDetailsView : UserControl
    {
        private readonly int _itemId;
        private readonly EnrollmentItemType _itemType;
        private readonly Enrollment _enrollment;

        private AppActivityEvent? _activityEvent;
        private Course? _course;

        public EventCourseDetailsView()
        {
            InitializeComponent();
        }

        public EventCourseDetailsView(
            int itemId,
            EnrollmentItemType itemType,
            Enrollment enrollment)
        {
            InitializeComponent();

            _itemId = itemId;
            _itemType = itemType;
            _enrollment = enrollment;

            if (AppServices.AuthService.GetCurrentUser() is not Customer)
            {
                NavigateToLogin();
                return;
            }

            LoadItemDetails();

            BackButton.Click += BackButton_Click;
            PayButton.Click += PayButton_Click;
            LogoutButton.Click += LogoutButton_Click;
            SaveButton.Click += SaveButton_Click;

            GalleryImagesItemsControl.AddHandler(
                Button.ClickEvent,
                GalleryImagesItemsControl_ButtonClick,
                RoutingStrategies.Bubble);

            GalleryPreviewImageCard.PointerPressed += GalleryPreviewImageCard_PointerPressed;
            GalleryPreviewCloseArea.PointerPressed += GalleryPreviewCloseArea_PointerPressed;
            CloseGalleryPreviewButton.Click += CloseGalleryPreviewButton_Click;

            AppServices.PaymentSimulationService.PaymentStatusChanged += PaymentSimulationService_PaymentStatusChanged;
        }

        private void LoadItemDetails()
        {
            if (_itemType == EnrollmentItemType.Event)
            {
                LoadEventDetails();
                return;
            }

            LoadCourseDetails();
        }

        private void LoadEventDetails()
        {
            _activityEvent = AppServices.ActivityEventRepository
                .GetAllEvents()
                .FirstOrDefault(activityEvent => activityEvent.Id == _itemId);

            if (_activityEvent == null)
            {
                PageTitleTextBlock.Text = "Item not found";
                ItemTitleTextBlock.Text = "The selected event could not be found.";
                PayButton.IsEnabled = false;
                return;
            }

            PageTitleTextBlock.Text = "Event Enrollment Details";

            ItemTitleTextBlock.Text = _activityEvent.Title;
            ItemCategoryTextBlock.Text = _activityEvent.Category;
            ItemAddressTextBlock.Text = _activityEvent.Address;
            ItemDescriptionTextBlock.Text = _activityEvent.Description;

            ItemTypeTextBlock.Text = "Event";
            ItemDateTextBlock.Text = _activityEvent.Date.ToString("dd/MM/yyyy");
            ItemTimeTextBlock.Text = _activityEvent.Time.ToString(@"hh\:mm");
            ItemSpacesTextBlock.Text = _activityEvent.MaxSpace.ToString();
            ItemPriceTextBlock.Text = $"€{_activityEvent.Price}";

            EnrollmentStatusTextBlock.Text = _enrollment.Status.ToString();

            LoadImagesForItem(
                _activityEvent.Id,
                ActivityImageItemType.Event,
                _activityEvent.MainImagePath,
                _activityEvent.Gallery);

            UpdateSaveButtonState();
            UpdatePaymentUiByStatus();
        }

        private void LoadCourseDetails()
        {
            _course = AppServices.CourseRepository
                .GetAllCourses()
                .FirstOrDefault(course => course.Id == _itemId);

            if (_course == null)
            {
                PageTitleTextBlock.Text = "Item not found";
                ItemTitleTextBlock.Text = "The selected course could not be found.";
                ItemCategoryTextBlock.Text = "Course";
                ItemAddressTextBlock.Text = "-";
                ItemDescriptionTextBlock.Text = "No course information is available.";

                ItemTypeTextBlock.Text = "Course";
                ItemDateTextBlock.Text = "-";
                ItemTimeTextBlock.Text = "-";
                ItemSpacesTextBlock.Text = "-";
                ItemPriceTextBlock.Text = "-";
                EnrollmentStatusTextBlock.Text = "-";

                PayButton.IsEnabled = false;
                return;
            }

            PageTitleTextBlock.Text = "Course Enrollment Details";

            ItemTitleTextBlock.Text = _course.Title;
            ItemCategoryTextBlock.Text = _course.Category;
            ItemAddressTextBlock.Text = _course.Address;
            ItemDescriptionTextBlock.Text = _course.Description;

            ItemTypeTextBlock.Text = "Course";
            ItemDateTextBlock.Text = _course.Days;
            ItemTimeTextBlock.Text = $"{_course.StartTime:hh\\:mm} - {_course.EndTime:hh\\:mm}";
            ItemSpacesTextBlock.Text = _course.MaxSpace.ToString();
            ItemPriceTextBlock.Text = $"€{_course.Price}";

            EnrollmentStatusTextBlock.Text = _enrollment.Status.ToString();

            LoadImagesForItem(
                _course.Id,
                ActivityImageItemType.Course,
                _course.MainImagePath,
                _course.Gallery);

            UpdateSaveButtonState();
            UpdatePaymentUiByStatus();
        }

        private void LoadImagesForItem(
            int itemId,
            ActivityImageItemType itemType,
            string fallbackMainImagePath,
            string fallbackGallery)
        {
            ActivityImage? mainImage = AppServices.ActivityImageRepository
                .GetMainImage(itemId, itemType);

            List<ActivityImage> galleryImages = AppServices.ActivityImageRepository
                .GetGalleryImages(itemId, itemType);

            string mainImagePath = string.Empty;

            if (mainImage != null && !string.IsNullOrWhiteSpace(mainImage.ImagePath))
            {
                mainImagePath = mainImage.ImagePath;
            }
            else if (!string.IsNullOrWhiteSpace(fallbackMainImagePath))
            {
                mainImagePath = fallbackMainImagePath;
            }

            Bitmap? mainBitmap = LoadBitmapOrNull(mainImagePath);

            if (mainBitmap != null)
            {
                MainActivityImage.Source = mainBitmap;
                MainActivityImage.IsVisible = true;
                MediaPlaceholderTextBlock.IsVisible = false;
            }
            else
            {
                MainActivityImage.Source = null;
                MainActivityImage.IsVisible = false;
                MediaPlaceholderTextBlock.IsVisible = true;
            }

            List<EventCourseGalleryImageItem> galleryItems = new List<EventCourseGalleryImageItem>();

            if (galleryImages.Count > 0)
            {
                galleryItems = galleryImages
                    .Select(image => new EventCourseGalleryImageItem
                    {
                        ImagePath = image.ImagePath,
                        Caption = image.Caption,
                        PreviewImage = LoadBitmapOrNull(image.ImagePath)
                    })
                    .Where(item => item.PreviewImage != null)
                    .ToList();
            }
            else if (!string.IsNullOrWhiteSpace(fallbackGallery))
            {
                galleryItems = fallbackGallery
                    .Split(';', ',', System.StringSplitOptions.RemoveEmptyEntries)
                    .Select(path => path.Trim())
                    .Where(path => !string.IsNullOrWhiteSpace(path))
                    .Select(path => new EventCourseGalleryImageItem
                    {
                        ImagePath = path,
                        Caption = string.Empty,
                        PreviewImage = LoadBitmapOrNull(path)
                    })
                    .Where(item => item.PreviewImage != null)
                    .ToList();
            }

            GalleryImagesItemsControl.ItemsSource = galleryItems;
            GallerySectionPanel.IsVisible = galleryItems.Count > 0;
        }

        private Bitmap? LoadBitmapOrNull(string imagePath)
        {
            if (string.IsNullOrWhiteSpace(imagePath))
            {
                return null;
            }

            if (!File.Exists(imagePath))
            {
                return null;
            }

            try
            {
                return new Bitmap(imagePath);
            }
            catch
            {
                return null;
            }
        }

        private void GalleryImagesItemsControl_ButtonClick(object? sender, RoutedEventArgs e)
        {
            if (e.Source is not Button button)
            {
                return;
            }

            if (button.Tag is not EventCourseGalleryImageItem galleryImageItem)
            {
                return;
            }

            if (galleryImageItem.PreviewImage == null)
            {
                return;
            }

            ExpandedGalleryImage.Source = galleryImageItem.PreviewImage;

            ExpandedGalleryImageCaptionTextBlock.Text = string.IsNullOrWhiteSpace(galleryImageItem.Caption)
                ? "Selected gallery image"
                : galleryImageItem.Caption;

            GalleryImageOverlay.IsVisible = true;

            e.Handled = true;
        }

        private void GalleryPreviewImageCard_PointerPressed(object? sender, PointerPressedEventArgs e)
        {
            // Keep the preview open when the user clicks on the image area.
            e.Handled = true;
        }

        private void GalleryPreviewCloseArea_PointerPressed(object? sender, PointerPressedEventArgs e)
        {
            CloseGalleryImagePreview();

            e.Handled = true;
        }

        private void CloseGalleryPreviewButton_Click(object? sender, RoutedEventArgs e)
        {
            CloseGalleryImagePreview();

            e.Handled = true;
        }

        private void CloseGalleryImagePreview()
        {
            GalleryImageOverlay.IsVisible = false;
            ExpandedGalleryImage.Source = null;
            ExpandedGalleryImageCaptionTextBlock.Text = string.Empty;
        }

        private void PaymentSimulationService_PaymentStatusChanged(int enrollmentId, EnrollmentStatus status)
        {
            if (enrollmentId != _enrollment.Id)
            {
                return;
            }

            Dispatcher.UIThread.Post(() =>
            {
                _enrollment.Status = status;
                EnrollmentStatusTextBlock.Text = status.ToString();

                UpdatePaymentUiByStatus();
            });
        }

        private void UpdatePaymentUiByStatus()
        {
            if (_enrollment.Status == EnrollmentStatus.Confirmed)
            {
                EnrollmentStatusTextBlock.Foreground = Avalonia.Media.Brushes.Green;
                PayButton.IsEnabled = false;
                return;
            }

            if (_enrollment.Status == EnrollmentStatus.Cancelled)
            {
                EnrollmentStatusTextBlock.Foreground = Avalonia.Media.Brushes.Red;
                PayButton.IsEnabled = false;
                return;
            }

            EnrollmentStatusTextBlock.Foreground = Avalonia.Media.Brushes.Orange;
            PayButton.IsEnabled = true;
        }

        private void BackButton_Click(object? sender, RoutedEventArgs e)
        {
            NavigateBackToCustomerDashboard();
        }

        private void PayButton_Click(object? sender, RoutedEventArgs e)
        {
            AppServices.PaymentSimulationService.OpenPaymentPage(_enrollment.Id);
        }

        private void SaveButton_Click(object? sender, RoutedEventArgs e)
        {
            if (AppServices.AuthService.GetCurrentUser() is not Customer customer)
            {
                return;
            }

            SavedItemType savedItemType = ConvertToSavedItemType(_itemType);

            bool alreadySaved = AppServices.SavedItemRepository.IsItemSaved(
                customer.Id,
                _itemId,
                savedItemType);

            if (alreadySaved)
            {
                AppServices.SavedItemRepository.RemoveSavedItem(
                    customer.Id,
                    _itemId,
                    savedItemType);
            }
            else
            {
                AppServices.SavedItemRepository.SaveItem(
                    customer.Id,
                    _itemId,
                    savedItemType);
            }

            UpdateSaveButtonState();
        }

        private void UpdateSaveButtonState()
        {
            if (AppServices.AuthService.GetCurrentUser() is not Customer customer)
            {
                SaveButton.Content = "Save";
                SaveButton.Classes.Remove("saved");
                return;
            }

            SavedItemType savedItemType = ConvertToSavedItemType(_itemType);

            bool isSaved = AppServices.SavedItemRepository.IsItemSaved(
                customer.Id,
                _itemId,
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

        private SavedItemType ConvertToSavedItemType(EnrollmentItemType itemType)
        {
            if (itemType == EnrollmentItemType.Course)
            {
                return SavedItemType.Course;
            }

            return SavedItemType.Event;
        }

        private void LogoutButton_Click(object? sender, RoutedEventArgs e)
        {
            AppServices.AuthService.Logout();
            NavigateToLogin();
        }

        private void NavigateBackToCustomerDashboard()
        {
            MainWindow? mainWindow = this.VisualRoot as MainWindow;

            if (mainWindow == null)
            {
                return;
            }

            mainWindow.Content = new CustomerDashboardView();
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