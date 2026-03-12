using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Wrath.UI;

namespace Wrath.App.Pages;

public sealed partial class ConnectionDetailsPage : Page
{
    private MainShellViewModel? _vm;
    private Guid _id;

    public ConnectionDetailsPage() => InitializeComponent();

    protected override async void OnNavigatedTo(Microsoft.UI.Xaml.Navigation.NavigationEventArgs e)
    {
        if (e.Parameter is not Tuple<MainShellViewModel, Guid> payload) return;
        _vm = payload.Item1;
        _id = payload.Item2;

        await _vm.LoadAsync();
        var profile = _vm.Connections.FirstOrDefault(x => x.Id == _id);
        if (profile is null) return;

        NameText.Text = profile.Name;
        HostText.Text = $"{profile.Host}:{profile.Port}";
        ProtocolText.Text = profile.Protocol.ToString();
    }

    private void Edit_Click(object sender, RoutedEventArgs e)
    {
        if (_vm is null) return;
        Frame.Navigate(typeof(EditConnectionPage), new Tuple<MainShellViewModel, Guid>(_vm, _id));
    }
}
