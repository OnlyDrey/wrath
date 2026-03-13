using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Wrath.UI;

namespace Wrath.App;

public sealed partial class MainWindow : Window
{
    private readonly MainShellViewModel _viewModel;
    private bool _shellInitialized;

    public MainWindow(MainShellViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        UpdateMessages();
    }

    public void InitializeShell()
    {
        if (_shellInitialized)
        {
            return;
        }

        _shellInitialized = true;
        RootFrame.Navigate(typeof(Pages.ConnectionsPage), _viewModel);
        UpdateMessages();
    }

    private void RootNav_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
    {
        if (args.SelectedItemContainer?.Tag is not string tag) return;
        var pageType = tag switch
        {
            "connections" => typeof(Pages.ConnectionsPage),
            "details" => typeof(Pages.ConnectionDetailsPage),
            "edit" => typeof(Pages.EditConnectionPage),
            "history" => typeof(Pages.SessionHistoryPage),
            _ => typeof(Pages.ConnectionsPage)
        };

        RootFrame.Navigate(pageType, _viewModel);
    }

    private async void Search_Click(object sender, RoutedEventArgs e)
    {
        _viewModel.SearchText = SearchBox.Text;
        _viewModel.SearchCommand.Execute(null);
        await Task.Delay(30);
        UpdateMessages();
    }

    private void New_Click(object sender, RoutedEventArgs e) => RootFrame.Navigate(typeof(Pages.EditConnectionPage), _viewModel);

    private void History_Click(object sender, RoutedEventArgs e) => RootFrame.Navigate(typeof(Pages.SessionHistoryPage), _viewModel);

    public void UpdateMessages()
    {
        StatusText.Text = string.IsNullOrWhiteSpace(_viewModel.StatusMessage) ? "Ready" : _viewModel.StatusMessage;
        ErrorText.Text = _viewModel.ErrorMessage ?? string.Empty;
    }
}
