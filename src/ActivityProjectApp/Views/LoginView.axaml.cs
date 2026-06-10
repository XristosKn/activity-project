using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;

namespace ActivityProjectApp.Views
{
    public partial class LoginView : UserControl
    {
        public LoginView()
        {
            InitializeComponent();

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

            if (identifier == "customer@test.com" && password == "1234")
            {
                LoginMessageTextBlock.Foreground = Brushes.Green;
                LoginMessageTextBlock.Text = "Login successful.";
            }
            else
            {
                LoginMessageTextBlock.Text = "Invalid username/email or password.";
            }
        }

        private void SignUpButton_Click(object? sender, RoutedEventArgs e)
        {
            LoginMessageTextBlock.Foreground = Brushes.Gray;
            LoginMessageTextBlock.Text = "Sign up form will be available later.";
        }
    }
}