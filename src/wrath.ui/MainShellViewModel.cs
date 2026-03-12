using Wrath.Application;
using Wrath.Domain;

namespace Wrath.UI;

public sealed class MainShellViewModel : ObservableObject
{
    private readonly ConnectionProfileService _profiles;
    private readonly SessionService _sessions;

    private string _searchText = string.Empty;
    public string SearchText
    {
        get => _searchText;
        set => SetProperty(ref _searchText, value);
    }

    public List<ConnectionProfile> Connections { get; } = [];
    public List<SessionRecord> SessionHistory { get; } = [];

    public AsyncRelayCommand SearchCommand { get; }

    public MainShellViewModel(ConnectionProfileService profiles, SessionService sessions)
    {
        _profiles = profiles;
        _sessions = sessions;
        SearchCommand = new AsyncRelayCommand(SearchAsync);
    }

    public async Task LoadAsync(CancellationToken ct = default)
    {
        Connections.Clear();
        Connections.AddRange(await _profiles.SearchConnectionsAsync(null, ct));

        SessionHistory.Clear();
        SessionHistory.AddRange(await _sessions.GetRecentAsync(50, ct));
    }

    public async Task<Guid> CreateOrUpdateAsync(Guid? id, ConnectionProfileRequest request, CancellationToken ct = default)
    {
        if (id is null)
        {
            var createdId = await _profiles.CreateConnectionProfileAsync(request, ct);
            await LoadAsync(ct);
            return createdId;
        }

        await _profiles.UpdateConnectionProfileAsync(id.Value, request, ct);
        await LoadAsync(ct);
        return id.Value;
    }

    public async Task<LaunchResult> LaunchAsync(Guid profileId, CancellationToken ct = default)
    {
        var result = await _sessions.LaunchSessionAsync(profileId, ct);
        await LoadAsync(ct);
        return result;
    }

    private async Task SearchAsync()
    {
        Connections.Clear();
        Connections.AddRange(await _profiles.SearchConnectionsAsync(SearchText, CancellationToken.None));
    }
}
