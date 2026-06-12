using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;
using ActivityProjectApp.Models;
using ActivityProjectApp.Services;
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
            MediaPlaceholderTextBlock.Text = "Event Image / Video Preview";

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
            MediaPlaceholderTextBlock.Text = "Course Image / Video Preview";

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

            UpdatePaymentUiByStatus();
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