using Microsoft.UI.Xaml.Controls;
using Wrath.UI;

namespace Wrath.App.Pages;

public sealed partial class SessionHistoryPage : Page
{
    public SessionHistoryPage() => InitializeComponent();

    protected override async void OnNavigatedTo(Microsoft.UI.Xaml.Navigation.NavigationEventArgs e)
    {
        if (e.Parameter is not MainShellViewModel vm) return;
        await vm.LoadAsync();
        HistoryList.ItemsSource = vm.SessionHistory;
    }
}
