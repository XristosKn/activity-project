using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;

namespace ActivityProjectApp.Views
{
    public partial class SignUpView : UserControl
    {
        public SignUpView()
        {
            InitializeComponent();

            CreateAccountButton.Click += CreateAccountButton_Click;
            BackToLoginButton.Click += BackToLoginButton_Click;
        }

        private void CreateAccountButton_Click(object? sender, RoutedEventArgs e)
        {
            string name = NameTextBox.Text?.Trim() ?? string.Empty;
            string lastName = LastNameTextBox.Text?.Trim() ?? string.Empty;
            string username = UsernameTextBox.Text?.Trim() ?? string.Empty;
            string email = EmailTextBox.Text?.Trim() ?? string.Empty;
            string phone = PhoneTextBox.Text?.Trim() ?? string.Empty;
            string password = PasswordTextBox.Text ?? string.Empty;
            string confirmPassword = ConfirmPasswordTextBox.Text ?? string.Empty;

            SignUpMessageTextBlock.Foreground = Brushes.Red;

            if (string.IsNullOrWhiteSpace(name))
            {
                SignUpMessageTextBlock.Text = "Please enter your name.";
                return;
            }

            if (string.IsNullOrWhiteSpace(lastName))
            {
                SignUpMessageTextBlock.Text = "Please enter your last name.";
                return;
            }

            if (string.IsNullOrWhiteSpace(username))
            {
                SignUpMessageTextBlock.Text = "Please enter a username.";
                return;
            }

            if (string.IsNullOrWhiteSpace(email))
            {
                SignUpMessageTextBlock.Text = "Please enter your email.";
                return;
            }

            if (string.IsNullOrWhiteSpace(phone))
            {
                SignUpMessageTextBlock.Text = "Please enter your phone number.";
                return;
            }

            if (string.IsNullOrWhiteSpace(password))
            {
                SignUpMessageTextBlock.Text = "Please enter a password.";
                return;
            }

            if (password != confirmPassword)
            {
                SignUpMessageTextBlock.Text = "Passwords do not match.";
                return;
            }

            SignUpMessageTextBlock.Foreground = Brushes.Green;
            SignUpMessageTextBlock.Text = "Test account created successfully.";
        }

        private void BackToLoginButton_Click(object? sender, RoutedEventArgs e)
        {
            Window? window = TopLevel.GetTopLevel(this) as Window;

            if (window != null)
            {
                window.Content = new LoginView();
            }
        }
    }
}