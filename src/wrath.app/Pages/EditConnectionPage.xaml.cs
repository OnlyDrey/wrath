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

    protected override async void OnNavigatedTo(Microsoft.UI.Xaml.Navigation.NavigationEventArgs e)
    {
        if (e.Parameter is Tuple<MainShellViewModel, Guid> edit)
        {
            _vm = edit.Item1;
            _editingId = edit.Item2;

            await _vm.LoadAsync();
            var profile = _vm.Connections.FirstOrDefault(x => x.Id == _editingId);
            if (profile is not null)
            {
                NameBox.Text = profile.Name;
                HostBox.Text = profile.Host;
                PortBox.Text = profile.Port.ToString();
                UsernameBox.Text = profile.Username ?? string.Empty;
                GroupBox.Text = profile.GroupPath ?? string.Empty;
                TagsBox.Text = string.Join(", ", profile.Tags.Select(x => x.Value));
                ProtocolBox.SelectedIndex = profile.Protocol == ProtocolType.Ssh ? 1 : 0;
                return;
            }
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

        var protocol = ProtocolBox.SelectedIndex == 1 ? ProtocolType.Ssh : ProtocolType.Rdp;
        var parsedTags = TagsBox.Text
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(x => x.Trim())
            .Where(x => x.Length > 0)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        var request = new ConnectionProfileRequest(
            NameBox.Text,
            protocol,
            HostBox.Text,
            int.TryParse(PortBox.Text, out var port) ? port : protocol == ProtocolType.Rdp ? 3389 : 22,
            UsernameBox.Text,
            GroupBox.Text,
            parsedTags);

        await _vm.CreateOrUpdateAsync(_editingId, request);

        if (_vm.ErrorMessage is null)
        {
            Frame.Navigate(typeof(ConnectionsPage), _vm);
        }
    }
}
