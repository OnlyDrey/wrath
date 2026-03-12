using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Wrath.UI;

namespace Wrath.App;

public sealed partial class MainWindow : Window
{
    private readonly MainShellViewModel _viewModel;

    public MainWindow(MainShellViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        RootFrame.Navigate(typeof(Pages.ConnectionsPage), _viewModel);
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

    private void Search_Click(object sender, RoutedEventArgs e)
    {
        _viewModel.SearchText = SearchBox.Text;
        _viewModel.SearchCommand.Execute(null);
    }

    private void New_Click(object sender, RoutedEventArgs e) => RootFrame.Navigate(typeof(Pages.EditConnectionPage), _viewModel);

    private void History_Click(object sender, RoutedEventArgs e) => RootFrame.Navigate(typeof(Pages.SessionHistoryPage), _viewModel);
}
