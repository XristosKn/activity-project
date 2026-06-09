using Avalonia.Controls;
using ActivityProjectApp.Views;

namespace ActivityProjectApp;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();

        MainContent.Content = new LoginView();
    }
}