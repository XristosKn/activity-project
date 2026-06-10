using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using ActivityProjectApp.Models;
using ActivityProjectApp.Services;

namespace ActivityProjectApp.Views
{
    public partial class LoginView : UserControl
    {
        private readonly AuthService _authService;

        public LoginView()
        {
            InitializeComponent();

            _authService = AppServices.AuthService;

            LoginButton.Click += LoginButton_Click;
            SignUpButton.Click += SignUpButton_Click;
        }

        private void LoginButton_Click(object? sender, RoutedEventArgs e)
        {
            string identifier = LoginIdentifierTextBox.Text?.Trim() ?? string.Empty;
            string password = LoginPasswordTextBox.Text ?? string.Empty;

            LoginMessageTextBlock.Foreground = Brushes.Red;

            if (string.IsNullOrWhiteSpace(identifier))
            {
                LoginMessageTextBlock.Text = "Please enter your username or email.";
                return;
            }

            if (string.IsNullOrWhiteSpace(password))
            {
                LoginMessageTextBlock.Text = "Please enter your password.";
                return;
            }

            bool loginSuccess = _authService.Login(identifier, password, out string message);

            if (!loginSuccess)
            {
                LoginMessageTextBlock.Foreground = Brushes.Red;
                LoginMessageTextBlock.Text = message;
                return;
            }

            User? currentUser = _authService.GetCurrentUser();

            if (currentUser is ServiceProvider)
            {
                Window? window = TopLevel.GetTopLevel(this) as Window;

                if (window != null)
                {
                    window.Content = new ServiceProviderDashboardView();
                }

                return;
            }

            if (currentUser is Customer)
            {
                LoginMessageTextBlock.Foreground = Brushes.Green;
                LoginMessageTextBlock.Text = "Customer login successful. Customer dashboard will be added later.";
                return;
            }

            LoginMessageTextBlock.Foreground = Brushes.Green;
            LoginMessageTextBlock.Text = message;
        }

        private void SignUpButton_Click(object? sender, RoutedEventArgs e)
        {
            Window? window = TopLevel.GetTopLevel(this) as Window;

            if (window != null)
            {
                window.Content = new SignUpView();
            }
        }
    }
}