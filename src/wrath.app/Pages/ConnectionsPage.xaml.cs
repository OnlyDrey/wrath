using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Wrath.Domain;
using Wrath.UI;

namespace Wrath.App.Pages;

public sealed partial class ConnectionsPage : Page
{
    private MainShellViewModel? _vm;

    public ConnectionsPage() => InitializeComponent();

    protected override async void OnNavigatedTo(Microsoft.UI.Xaml.Navigation.NavigationEventArgs e)
    {
        _vm = e.Parameter as MainShellViewModel;
        if (_vm is null) return;

        await _vm.LoadAsync();
        ConnectionsList.ItemsSource = _vm.Connections;
    }

    private async void Launch_Click(object sender, RoutedEventArgs e)
    {
        if (_vm is null) return;
        if (sender is Button { Tag: Guid id })
        {
            await _vm.LaunchAsync(id);
            ConnectionsList.ItemsSource = null;
            ConnectionsList.ItemsSource = _vm.Connections;
        }
    }

    private void ConnectionsList_ItemClick(object sender, ItemClickEventArgs e)
    {
        if (_vm is null) return;
        if (e.ClickedItem is ConnectionProfile profile)
        {
            Frame.Navigate(typeof(ConnectionDetailsPage), new Tuple<MainShellViewModel, Guid>(_vm, profile.Id));
        }
    }
}
