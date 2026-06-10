using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using System;
using System.Collections.Generic;
using ActivityProjectApp.Services;

namespace ActivityProjectApp.Views
{
    public partial class SignUpView : UserControl
    {
        private readonly AuthService _authService;

        public SignUpView()
        {
            InitializeComponent();

            _authService = AppServices.AuthService;

            AccountTypeComboBox.SelectionChanged += AccountTypeComboBox_SelectionChanged;
            CreateAccountButton.Click += CreateAccountButton_Click;
            BackToLoginButton.Click += BackToLoginButton_Click;
        }

        private void AccountTypeComboBox_SelectionChanged(object? sender, SelectionChangedEventArgs e)
        {
            string selectedAccountType = GetSelectedAccountType();

            if (selectedAccountType == "Customer")
            {
                CustomerFieldsPanel.IsVisible = true;
                ServiceProviderFieldsPanel.IsVisible = false;

                ClearSelectedTypesOfService();
            }
            else if (selectedAccountType == "Service Provider")
            {
                CustomerFieldsPanel.IsVisible = false;
                ServiceProviderFieldsPanel.IsVisible = true;

                GenderComboBox.SelectedItem = null;
                DateOfBirthPicker.SelectedDate = null;
            }
            else
            {
                CustomerFieldsPanel.IsVisible = false;
                ServiceProviderFieldsPanel.IsVisible = false;

                ClearSelectedTypesOfService();
                GenderComboBox.SelectedItem = null;
                DateOfBirthPicker.SelectedDate = null;
            }

            SignUpMessageTextBlock.Text = string.Empty;
        }

        private void CreateAccountButton_Click(object? sender, RoutedEventArgs e)
        {
            string selectedAccountType = GetSelectedAccountType();

            string name = NameTextBox.Text?.Trim() ?? string.Empty;
            string lastName = LastNameTextBox.Text?.Trim() ?? string.Empty;
            string username = UsernameTextBox.Text?.Trim() ?? string.Empty;
            string email = EmailTextBox.Text?.Trim() ?? string.Empty;
            string phone = PhoneTextBox.Text?.Trim() ?? string.Empty;
            string password = PasswordTextBox.Text ?? string.Empty;
            string confirmPassword = ConfirmPasswordTextBox.Text ?? string.Empty;

            string gender = GetSelectedGender();
            DateTimeOffset? dateOfBirth = DateOfBirthPicker.SelectedDate;

            List<string> selectedTypesOfService = GetSelectedTypesOfService();

            SignUpMessageTextBlock.Foreground = Brushes.Red;

            if (string.IsNullOrWhiteSpace(selectedAccountType))
            {
                SignUpMessageTextBlock.Text = "Please select an account type.";
                return;
            }

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

            if (selectedAccountType == "Customer")
            {
                if (string.IsNullOrWhiteSpace(gender))
                {
                    SignUpMessageTextBlock.Text = "Please select your gender.";
                    return;
                }

                if (dateOfBirth == null)
                {
                    SignUpMessageTextBlock.Text = "Please select your date of birth.";
                    return;
                }

                bool customerCreated = _authService.RegisterCustomer(
                    name,
                    lastName,
                    username,
                    email,
                    phone,
                    password,
                    gender,
                    dateOfBirth.Value.DateTime,
                    out string message);

                if (!customerCreated)
                {
                    SignUpMessageTextBlock.Foreground = Brushes.Red;
                    SignUpMessageTextBlock.Text = message;
                    return;
                }

                ClearForm();

                SignUpMessageTextBlock.Foreground = Brushes.Green;
                SignUpMessageTextBlock.Text = message;

                return;
            }

            if (selectedAccountType == "Service Provider")
            {
                if (selectedTypesOfService.Count == 0)
                {
                    SignUpMessageTextBlock.Text = "Please select at least one type of service.";
                    return;
                }

                bool serviceProviderCreated = _authService.RegisterServiceProvider(
                    name,
                    lastName,
                    username,
                    email,
                    phone,
                    password,
                    selectedTypesOfService,
                    out string message);

                if (!serviceProviderCreated)
                {
                    SignUpMessageTextBlock.Foreground = Brushes.Red;
                    SignUpMessageTextBlock.Text = message;
                    return;
                }

                ClearForm();

                SignUpMessageTextBlock.Foreground = Brushes.Green;
                SignUpMessageTextBlock.Text = message;

                return;
            }
        }

        private string GetSelectedAccountType()
        {
            if (AccountTypeComboBox.SelectedItem is ComboBoxItem selectedItem)
            {
                return selectedItem.Content?.ToString() ?? string.Empty;
            }

            return string.Empty;
        }

        private string GetSelectedGender()
        {
            if (GenderComboBox.SelectedItem is ComboBoxItem selectedItem)
            {
                return selectedItem.Content?.ToString() ?? string.Empty;
            }

            return string.Empty;
        }

        private List<string> GetSelectedTypesOfService()
        {
            List<string> selectedTypes = new List<string>();

            foreach (Control control in TypeOfServiceCheckBoxPanel.Children)
            {
                if (control is CheckBox checkBox && checkBox.IsChecked == true)
                {
                    string value = checkBox.Content?.ToString() ?? string.Empty;

                    if (!string.IsNullOrWhiteSpace(value))
                    {
                        selectedTypes.Add(value);
                    }
                }
            }

            return selectedTypes;
        }

        private void ClearSelectedTypesOfService()
        {
            foreach (Control control in TypeOfServiceCheckBoxPanel.Children)
            {
                if (control is CheckBox checkBox)
                {
                    checkBox.IsChecked = false;
                }
            }
        }

        private void ClearForm()
        {
            AccountTypeComboBox.SelectedItem = null;

            NameTextBox.Text = string.Empty;
            LastNameTextBox.Text = string.Empty;
            UsernameTextBox.Text = string.Empty;
            EmailTextBox.Text = string.Empty;
            PhoneTextBox.Text = string.Empty;
            PasswordTextBox.Text = string.Empty;
            ConfirmPasswordTextBox.Text = string.Empty;

            GenderComboBox.SelectedItem = null;
            DateOfBirthPicker.SelectedDate = null;

            ClearSelectedTypesOfService();

            CustomerFieldsPanel.IsVisible = false;
            ServiceProviderFieldsPanel.IsVisible = false;
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