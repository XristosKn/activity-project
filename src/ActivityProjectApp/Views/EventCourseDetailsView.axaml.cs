using Avalonia.Controls;
using Avalonia.Interactivity;
using ActivityProjectApp.Models;
using ActivityProjectApp.Services;
using System.Diagnostics;
using AppActivityEvent = ActivityProjectApp.Models.ActivityEvent;
using System.Linq;

namespace ActivityProjectApp.Views
{
    public partial class EventCourseDetailsView : UserControl
    {
        private readonly int _itemId;
        private readonly EnrollmentItemType _itemType;
        private readonly Enrollment _enrollment;

        private AppActivityEvent? _activityEvent;

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
        }

        private void LoadItemDetails()
        {
            if (_itemType == EnrollmentItemType.Event)
            {
                LoadEventDetails();
                return;
            }

            LoadCourseDetailsPlaceholder();
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
        }

        private void LoadCourseDetailsPlaceholder()
        {
            PageTitleTextBlock.Text = "Course Enrollment Details";
            MediaPlaceholderTextBlock.Text = "Course Image / Video Preview";

            ItemTitleTextBlock.Text = "Course details will be available later.";
            ItemCategoryTextBlock.Text = "Course";
            ItemAddressTextBlock.Text = "-";
            ItemDescriptionTextBlock.Text = "Course support will be added in a later step.";

            ItemTypeTextBlock.Text = "Course";
            ItemDateTextBlock.Text = "-";
            ItemTimeTextBlock.Text = "-";
            ItemSpacesTextBlock.Text = "-";
            ItemPriceTextBlock.Text = "-";

            EnrollmentStatusTextBlock.Text = _enrollment.Status.ToString();

            PayButton.IsEnabled = false;
        }

        private void BackButton_Click(object? sender, RoutedEventArgs e)
        {
            NavigateBackToCustomerDashboard();
        }

        private void PayButton_Click(object? sender, RoutedEventArgs e)
        {
            AppServices.EnrollmentRepository.MarkAsPaid(_enrollment.Id);

            _enrollment.Status = EnrollmentStatus.Paid;
            EnrollmentStatusTextBlock.Text = _enrollment.Status.ToString();

            string paymentUrl = CreateExternalPaymentUrl();

            ProcessStartInfo processStartInfo = new ProcessStartInfo
            {
                FileName = paymentUrl,
                UseShellExecute = true
            };

            Process.Start(processStartInfo);
        }

        private string CreateExternalPaymentUrl()
        {
            return $"https://example-payment-provider.com/pay?enrollmentId={_enrollment.Id}&itemId={_itemId}&itemType={_itemType}";
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

        private void LogoutButton_Click(object? sender, RoutedEventArgs e)
        {
            AppServices.AuthService.Logout();
            NavigateToLogin();
        }
    }
}