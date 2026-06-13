using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Threading;
using ActivityProjectApp.Models;
using ActivityProjectApp.Services;
using System;
using System.Collections.Generic;

namespace ActivityProjectApp.Views
{
    public partial class LoginView : UserControl
    {
        private readonly AuthService _authService;

        private readonly DispatcherTimer _carouselTimer;
        private readonly List<Border> _carouselCards = new List<Border>();
        private const double CarouselSpeed = 1.4;
        private const double CarouselResetLeft = -390;
        private const double CarouselRightLimit = 1600;
        private const double CarouselCardSpacing = 390;
        public LoginView()
        {
            InitializeComponent();

            _authService = AppServices.AuthService;

            LoginButton.Click += LoginButton_Click;
            SignUpButton.Click += SignUpButton_Click;

            ForgotPasswordButton.Click += ForgotPasswordButton_Click;
            SendPasswordResetButton.Click += SendPasswordResetButton_Click;
            CancelForgotPasswordButton.Click += CancelForgotPasswordButton_Click;

            _carouselCards.Add(CarouselCardOne);
            _carouselCards.Add(CarouselCardTwo);
            _carouselCards.Add(CarouselCardThree);
            _carouselCards.Add(CarouselCardFour);
            _carouselCards.Add(CarouselCardFive);

            _carouselTimer = new DispatcherTimer();
            _carouselTimer.Interval = TimeSpan.FromMilliseconds(25);
            _carouselTimer.Tick += CarouselTimer_Tick;
            _carouselTimer.Start();
        }

        private void CarouselTimer_Tick(object? sender, EventArgs e)
        {
            foreach (Border carouselCard in _carouselCards)
            {
                double currentLeft = Canvas.GetLeft(carouselCard);

                if (double.IsNaN(currentLeft))
                {
                    currentLeft = CarouselResetLeft;
                }

                double nextLeft = currentLeft + CarouselSpeed;

                if (nextLeft > CarouselRightLimit)
                {
                    nextLeft = GetLeftMostCardPosition() - CarouselCardSpacing;
                }

                Canvas.SetLeft(carouselCard, nextLeft);
            }
        }

        private double GetLeftMostCardPosition()
        {
            double leftMostPosition = double.MaxValue;

            foreach (Border carouselCard in _carouselCards)
            {
                double currentLeft = Canvas.GetLeft(carouselCard);

                if (double.IsNaN(currentLeft))
                {
                    continue;
                }

                if (currentLeft < leftMostPosition)
                {
                    leftMostPosition = currentLeft;
                }
            }

            if (leftMostPosition == double.MaxValue)
            {
                return CarouselResetLeft;
            }

            return leftMostPosition;
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
                Window? window = TopLevel.GetTopLevel(this) as Window;

                if (window != null)
                {
                    window.Content = new CustomerDashboardView();
                }

                return;
            }

            LoginMessageTextBlock.Foreground = Brushes.Green;
            LoginMessageTextBlock.Text = message;
        }

        private void ForgotPasswordButton_Click(object? sender, RoutedEventArgs e)
        {
            ForgotPasswordPanel.IsVisible = true;
            SignUpSectionPanel.IsVisible = false;

            LoginMessageTextBlock.Text = string.Empty;
            ResetPasswordMessageTextBlock.Text = string.Empty;

            ResetEmailTextBox.Text = LoginIdentifierTextBox.Text?.Trim() ?? string.Empty;
        }

        private void CancelForgotPasswordButton_Click(object? sender, RoutedEventArgs e)
        {
            ForgotPasswordPanel.IsVisible = false;
            SignUpSectionPanel.IsVisible = true;

            ResetEmailTextBox.Text = string.Empty;
            ResetPasswordMessageTextBlock.Text = string.Empty;
        }
        private void SendPasswordResetButton_Click(object? sender, RoutedEventArgs e)
        {
            string email = ResetEmailTextBox.Text?.Trim() ?? string.Empty;

            ResetPasswordMessageTextBlock.Foreground = Brushes.Red;

            if (string.IsNullOrWhiteSpace(email))
            {
                ResetPasswordMessageTextBlock.Text = "Please enter your email.";
                return;
            }

            if (!email.Contains("@") || !email.Contains("."))
            {
                ResetPasswordMessageTextBlock.Text = "Please enter a valid email address.";
                return;
            }

            ResetPasswordMessageTextBlock.Foreground = Brushes.Green;
            ResetPasswordMessageTextBlock.Text =
                "If this email exists, password reset instructions will be sent shortly.";
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