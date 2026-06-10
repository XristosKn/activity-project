using Avalonia.Controls;
using Avalonia.Interactivity;
using ActivityProjectApp.Models;
using ActivityProjectApp.Services;

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

            LogoutButton.Click += LogoutButton_Click;
            CreateActivityButton.Click += CreateActivityButton_Click;
            MyActivitiesButton.Click += MyActivitiesButton_Click;
            BookingsButton.Click += BookingsButton_Click;
            ProfileButton.Click += ProfileButton_Click;
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

        private void CreateActivityButton_Click(object? sender, RoutedEventArgs e)
        {
            DashboardMessageTextBlock.Text = "Create Activity page will be added later.";
        }

        private void MyActivitiesButton_Click(object? sender, RoutedEventArgs e)
        {
            DashboardMessageTextBlock.Text = "My Activities page will be added later.";
        }

        private void BookingsButton_Click(object? sender, RoutedEventArgs e)
        {
            DashboardMessageTextBlock.Text = "Bookings page will be added later.";
        }

        private void ProfileButton_Click(object? sender, RoutedEventArgs e)
        {
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