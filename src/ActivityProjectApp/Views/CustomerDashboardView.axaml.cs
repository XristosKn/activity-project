using Avalonia.Controls;
using Avalonia.Interactivity;
using ActivityProjectApp.Models;
using ActivityProjectApp.Services;
using System.Collections.Generic;

namespace ActivityProjectApp.Views
{
    public partial class CustomerDashboardView : UserControl
    {
        private readonly AuthService _authService;

        public CustomerDashboardView()
        {
            InitializeComponent();

            _authService = AppServices.AuthService;

            LoadCurrentUser();
            LoadAvailableEvents();

            SearchButton.Click += SearchButton_Click;
            LogoutButton.Click += LogoutButton_Click;
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
            List<ActivityEvent> activeEvents = AppServices.ActivityEventRepository.GetActiveEvents();

            AvailableEventsItemsControl.ItemsSource = activeEvents;
            EventsCountTextBlock.Text = $"{activeEvents.Count} events found";

            if (activeEvents.Count == 0)
            {
                DashboardMessageTextBlock.Text = "No active events are available yet.";
            }
            else
            {
                DashboardMessageTextBlock.Text = string.Empty;
            }
        }

        private void SearchButton_Click(object? sender, RoutedEventArgs e)
        {
            string searchText = SearchTextBox.Text?.Trim() ?? string.Empty;

            List<ActivityEvent> filteredEvents = AppServices.ActivityEventRepository.SearchActiveEvents(searchText);

            AvailableEventsItemsControl.ItemsSource = filteredEvents;
            EventsCountTextBlock.Text = $"{filteredEvents.Count} events found";

            if (filteredEvents.Count == 0)
            {
                DashboardMessageTextBlock.Text = "No events found for your search.";
            }
            else
            {
                DashboardMessageTextBlock.Text = string.Empty;
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