using Avalonia.Controls;
using Avalonia.Interactivity;

namespace ActivityProjectApp.Views;

public partial class LoginView : UserControl
{
    public LoginView()
    {
        InitializeComponent();
    }

    private void LoginButton_Click(object? sender, RoutedEventArgs e)
    {
        MessageTextBlock.Text = "Login button clicked!";
    }
}