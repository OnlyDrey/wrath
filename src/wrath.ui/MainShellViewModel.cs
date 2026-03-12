using Wrath.Application;
using Wrath.Domain;
using Wrath.Protocols.Abstractions;

namespace Wrath.UI;

public sealed class MainShellViewModel : ObservableObject
{
    private readonly ConnectionProfileService _profiles;
    private readonly SessionService _sessions;

    private string _searchText = string.Empty;
    private string _statusMessage = string.Empty;
    private string? _errorMessage;

    public string SearchText
    {
        get => _searchText;
        set => SetProperty(ref _searchText, value);
    }

    public string StatusMessage
    {
        get => _statusMessage;
        private set => SetProperty(ref _statusMessage, value);
    }

    public string? ErrorMessage
    {
        get => _errorMessage;
        private set => SetProperty(ref _errorMessage, value);
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

    public async Task<OperationResult<Guid>> CreateOrUpdateAsync(Guid? id, ConnectionProfileRequest request, CancellationToken ct = default)
    {
        if (id is null)
        {
            var createResult = await _profiles.CreateConnectionProfileAsync(request, ct);
            SetMessages(createResult.Message, createResult.Error);
            if (createResult.Succeeded)
            {
                await LoadAsync(ct);
            }

            return createResult;
        }

        var updateResult = await _profiles.UpdateConnectionProfileAsync(id.Value, request, ct);
        SetMessages(updateResult.Message, updateResult.Error);
        if (updateResult.Succeeded)
        {
            await LoadAsync(ct);
            return OperationResult<Guid>.Success(id.Value, updateResult.Message);
        }

        return OperationResult<Guid>.Failure(updateResult.Message, updateResult.Error);
    }

    public async Task<OperationResult> LaunchAsync(Guid profileId, CancellationToken ct = default)
    {
        var result = await _sessions.LaunchSessionAsync(profileId, ct);
        SetMessages(result.Message, result.Error);
        await LoadAsync(ct);
        return result;
    }

    private async Task SearchAsync()
    {
        Connections.Clear();
        Connections.AddRange(await _profiles.SearchConnectionsAsync(SearchText, CancellationToken.None));
    }

    private void SetMessages(string status, string? error)
    {
        StatusMessage = status;
        ErrorMessage = error;
    }
}
