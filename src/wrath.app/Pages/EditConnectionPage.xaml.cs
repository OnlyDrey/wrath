using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Wrath.Application;
using Wrath.Domain;
using Wrath.UI;

namespace Wrath.App.Pages;

public sealed partial class EditConnectionPage : Page
{
    private MainShellViewModel? _vm;
    private Guid? _editingId;

    public EditConnectionPage() => InitializeComponent();

    protected override void OnNavigatedTo(Microsoft.UI.Xaml.Navigation.NavigationEventArgs e)
    {
        if (e.Parameter is Tuple<MainShellViewModel, Guid> edit)
        {
            _vm = edit.Item1;
            _editingId = edit.Item2;
        }
        else
        {
            _vm = e.Parameter as MainShellViewModel;
        }

        ProtocolBox.SelectedIndex = 0;
    }

    private async void Save_Click(object sender, RoutedEventArgs e)
    {
        if (_vm is null) return;

        var protocol = Enum.TryParse<ProtocolType>(ProtocolBox.SelectedItem?.ToString(), true, out var p)
            ? p
            : ProtocolType.Rdp;
        var request = new ConnectionProfileRequest(
            NameBox.Text,
            protocol,
            HostBox.Text,
            int.TryParse(PortBox.Text, out var port) ? port : protocol == ProtocolType.Rdp ? 3389 : 22,
            UsernameBox.Text,
            GroupBox.Text,
            TagsBox.Text.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries));

        await _vm.CreateOrUpdateAsync(_editingId, request);
        Frame.Navigate(typeof(ConnectionsPage), _vm);
    }
}
