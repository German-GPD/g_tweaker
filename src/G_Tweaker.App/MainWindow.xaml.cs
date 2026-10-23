using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Security.Principal;
using G_Tweaker.Core.Models;
using G_Tweaker.Core.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.Win32;

namespace G_Tweaker.App;

public sealed class TweakViewModel
{
    public string Id { get; init; } = "";
    public string Title { get; init; } = "";
    public string Description { get; init; } = "";
    public string Category { get; init; } = "";
    public string State { get; init; } = "";
    public string Warning { get; init; } = "";
    public bool CanRestore { get; init; }
}

public sealed class HistoryViewModel
{
    public string Title { get; init; } = "";
    public string Details { get; init; } = "";
}

public sealed partial class MainWindow : Window
{
    private readonly TweakManager _manager = new();
    private readonly ObservableCollection<TweakViewModel> _visibleTweaks = [];
    private readonly ObservableCollection<HistoryViewModel> _historyRows = [];
    private string _selectedCategory = "Home";

    public MainWindow()
    {
        InitializeComponent();
        TweakRepeater.ItemsSource = _visibleTweaks;
        HistoryList.ItemsSource = _historyRows;
        Navigation.SelectedItem = Navigation.MenuItems[0];
        ShowPage("Home");
        BuildProfiles();
    }

    private void Navigation_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
    {
        if (args.SelectedItem is NavigationViewItem item && item.Tag is string tag)
            ShowPage(tag);
    }

    private void ShowPage(string tag)
    {
        _selectedCategory = tag;
        var isTweakPage = tag is "All" or "Privacy" or "Performance" or "Input" or "Explorer" or "Gaming";
        TweakRepeater.Visibility = isTweakPage ? Visibility.Visible : Visibility.Collapsed;
        ProfilesPanel.Visibility = tag == "Profiles" ? Visibility.Visible : Visibility.Collapsed;
        ActivationPanel.Visibility = tag == "Activation" ? Visibility.Visible : Visibility.Collapsed;
        HistoryPanel.Visibility = tag == "History" ? Visibility.Visible : Visibility.Collapsed;

        PageTitle.Text = tag switch
        {
            "Home" => "Welcome to G_Tweaker",
            "All" => "All tweaks",
            "Privacy" => "Privacy",
            "Performance" => "Performance",
            "Input" => "Input & accessibility",
            "Explorer" => "File Explorer",
            "Gaming" => "Gaming",
            "Profiles" => "Profiles",
            "Activation" => "Windows activation",
            "History" => "Change history",
            _ => "G_Tweaker"
        };
        PageSubtitle.Text = tag switch
        {
            "Home" => "Tune Windows with clear explanations and reversible changes.",
            "Profiles" => "Preview a set of settings before applying a profile.",
            "Activation" => "Check the current licence state or open Windows settings.",
            "History" => "Review changes and roll back the latest operation.",
            _ => "Review the current state before applying a change."
        };

        if (tag == "Home")
        {
            _visibleTweaks.Clear();
            foreach (var tweak in TweakCatalog.All.Take(3))
                _visibleTweaks.Add(ToViewModel(tweak));
            TweakRepeater.Visibility = Visibility.Visible;
            PageSubtitle.Text = "A small, transparent set of reversible Windows settings. Choose a category to explore all available tweaks.";
        }
        else if (isTweakPage)
        {
            RefreshTweaks();
        }
        else if (tag == "History")
        {
            RefreshHistory();
        }
        else if (tag == "Activation")
        {
            ActivationStatus.Text = "Select Check status to query Windows.";
        }
    }

    private void SearchBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
    {
        if (_selectedCategory is "All" or "Privacy" or "Performance" or "Input" or "Explorer" or "Gaming")
            RefreshTweaks();
    }

    private void RefreshTweaks()
    {
        _visibleTweaks.Clear();
        var query = SearchBox.Text?.Trim() ?? "";
        foreach (var tweak in TweakCatalog.All)
        {
            if (_selectedCategory != "All" &&
                !string.Equals(tweak.Category.DisplayName(), _selectedCategory, StringComparison.OrdinalIgnoreCase))
                continue;

            if (!string.IsNullOrWhiteSpace(query) &&
                !($"{tweak.Title} {tweak.Description} {tweak.Category.DisplayName()}").Contains(query, StringComparison.OrdinalIgnoreCase))
                continue;

            _visibleTweaks.Add(ToViewModel(tweak));
        }
    }

    private TweakViewModel ToViewModel(TweakDefinition tweak)
    {
        string state;
        try { state = _manager.GetState(tweak); }
        catch (Exception ex) { state = $"Unavailable: {ex.Message}"; }

        return new TweakViewModel
        {
            Id = tweak.Id,
            Title = tweak.Title,
            Description = tweak.Description,
            Category = tweak.Category.DisplayName(),
            State = state,
            Warning = tweak.Warning ?? "",
            CanRestore = tweak.DisabledValue is not null
        };
    }

    private async void Apply_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not Button { Tag: string id }) return;
        await ApplyTweakAsync(id, true);
    }

    private async void Restore_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not Button { Tag: string id }) return;
        await ApplyTweakAsync(id, false);
    }

    private async Task ApplyTweakAsync(string id, bool enabled)
    {
        var tweak = TweakCatalog.All.FirstOrDefault(t => t.Id == id);
        if (tweak is null) return;
        if (!enabled && tweak.DisabledValue is null)
        {
            await ShowMessageAsync("Not available", "This setting has no safe predefined restore value. Use History to roll back a recorded change.");
            return;
        }

        var dialog = new ContentDialog
        {
            XamlRoot = ContentPanel.XamlRoot,
            Title = "Confirm change",
            Content = $"{(enabled ? "Apply" : "Restore the configured value for")} “{tweak.Title}”?\\n\\nCurrent state: {_manager.GetState(tweak)}\\n\\nThe previous registry value will be saved for rollback.",
            PrimaryButtonText = enabled ? "Apply" : "Restore",
            CloseButtonText = "Cancel",
            DefaultButton = ContentDialogButton.Close
        };
        if (await dialog.ShowAsync() != ContentDialogResult.Primary) return;

        try
        {
            _manager.Apply(tweak, enabled);
            StatusText.Text = $"Updated: {tweak.Title}";
        }
        catch (Exception ex)
        {
            await ShowMessageAsync("Could not apply setting", ex.Message);
        }
        RefreshTweaks();
        RefreshHistory();
    }

    private void BuildProfiles()
    {
        ProfilesPanel.Children.Clear();
        AddProfile("Privacy starter", "Request the required diagnostic-data level where supported and disable the advertising ID.",
            ["privacy.diagnostic-data", "privacy.advertising-id"]);
        AddProfile("Gaming starter", "Enable Windows Game Mode.",
            ["gaming.game-mode"]);
        AddProfile("Everyday usability", "Show file extensions and disable the Sticky Keys shortcut.",
            ["explorer.file-extensions", "input.sticky-keys"]);
    }

    private void AddProfile(string name, string description, IReadOnlyList<string> ids)
    {
        var card = new Border
        {
            Background = (Microsoft.UI.Xaml.Media.Brush)Application.Current.Resources["CardBackgroundFillColorDefaultBrush"],
            BorderBrush = (Microsoft.UI.Xaml.Media.Brush)Application.Current.Resources["CardStrokeColorDefaultBrush"],
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(12),
            Padding = new Thickness(18),
            Margin = new Thickness(0, 0, 0, 10)
        };
        var stack = new StackPanel { Spacing = 10 };
        stack.Children.Add(new TextBlock { Text = name, FontSize = 17, FontWeight = Microsoft.UI.Text.FontWeights.SemiBold });
        stack.Children.Add(new TextBlock { Text = description, TextWrapping = Microsoft.UI.Xaml.TextWrapping.Wrap });
        var button = new Button { Content = "Preview and apply", Tag = new ProfileRequest(name, description, ids) };
        button.Click += ProfileApply_Click;
        stack.Children.Add(button);
        card.Child = stack;
        ProfilesPanel.Children.Add(card);
    }

    private sealed record ProfileRequest(string Name, string Description, IReadOnlyList<string> Ids);

    private async void ProfileApply_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not Button { Tag: ProfileRequest profile }) return;
        var tweaks = TweakCatalog.All.Where(t => profile.Ids.Contains(t.Id)).ToList();
        if (tweaks.Any(t => t.Hive == RegistryHive.LocalMachine) && !IsAdministrator())
        {
            await ShowMessageAsync("Administrator rights required",
                "This profile contains system-wide settings. Close G_Tweaker, launch it with Run as administrator, then apply the profile again. No changes have been made.");
            return;
        }
        var preview = string.Join(Environment.NewLine, tweaks.Select(t => $"• {t.Title} — current: {_manager.GetState(t)}"));
        var dialog = new ContentDialog
        {
            XamlRoot = ContentPanel.XamlRoot,
            Title = $"Apply {profile.Name}?",
            Content = $"{profile.Description}\\n\\n{preview}\\n\\nEach change is saved separately for rollback.",
            PrimaryButtonText = "Apply profile",
            CloseButtonText = "Cancel",
            DefaultButton = ContentDialogButton.Close
        };
        if (await dialog.ShowAsync() != ContentDialogResult.Primary) return;

        var failures = new List<string>();
        foreach (var tweak in tweaks)
        {
            try { _manager.Apply(tweak, true); }
            catch (Exception ex) { failures.Add($"{tweak.Title}: {ex.Message}"); break; }
        }
        if (failures.Count > 0)
            await ShowMessageAsync("Profile partially applied", "Some changes may already have been applied. Check History.\\n\\n" + string.Join(Environment.NewLine, failures));
        else
            await ShowMessageAsync("Profile applied", $"{profile.Name} was applied. You can roll back each recorded operation from History.");
        RefreshHistory();
    }

    private void RefreshHistory()
    {
        _historyRows.Clear();
        try
        {
            foreach (var item in _manager.History)
                _historyRows.Add(new HistoryViewModel
                {
                    Title = item.Title,
                    Details = $"{item.Timestamp.LocalDateTime:g} • {item.Hive}\\{item.SubKey}\\{item.ValueName}"
                });
        }
        catch (Exception ex)
        {
            _historyRows.Add(new HistoryViewModel { Title = "History could not be read", Details = ex.Message });
        }
    }

    private async void Rollback_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new ContentDialog
        {
            XamlRoot = ContentPanel.XamlRoot,
            Title = "Roll back latest change?",
            Content = "The previous registry value for the latest recorded operation will be restored.",
            PrimaryButtonText = "Roll back",
            CloseButtonText = "Cancel",
            DefaultButton = ContentDialogButton.Close
        };
        if (await dialog.ShowAsync() != ContentDialogResult.Primary) return;
        try
        {
            var title = _manager.RollbackLatest();
            await ShowMessageAsync("Rollback complete", $"Restored the previous value for: {title}");
        }
        catch (Exception ex)
        {
            await ShowMessageAsync("Rollback failed", ex.Message);
        }
        RefreshHistory();
        RefreshTweaks();
    }

    private async void CheckActivation_Click(object sender, RoutedEventArgs e)
    {
        ActivationStatus.Text = "Checking Windows activation status…";
        try
        {
            ActivationStatus.Text = await Task.Run(ReadActivationStatus);
        }
        catch (Exception ex)
        {
            ActivationStatus.Text = $"Could not query activation status: {ex.Message}";
        }
    }

    private static string ReadActivationStatus()
    {
        var windir = Environment.GetFolderPath(Environment.SpecialFolder.Windows);
        var script = Path.Combine(windir, "System32", "slmgr.vbs");
        var start = new ProcessStartInfo("cscript.exe")
        {
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true
        };
        start.ArgumentList.Add("//Nologo");
        start.ArgumentList.Add(script);
        start.ArgumentList.Add("/xpr");
        using var process = Process.Start(start) ?? throw new InvalidOperationException("Could not start Windows Script Host.");
        var output = process.StandardOutput.ReadToEnd();
        var error = process.StandardError.ReadToEnd();
        if (!process.WaitForExit(20000))
        {
            process.Kill(entireProcessTree: true);
            throw new TimeoutException("The activation query timed out.");
        }
        return string.IsNullOrWhiteSpace(output) ? error : output;
    }

    private void OpenActivation_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            Process.Start(new ProcessStartInfo("ms-settings:activation") { UseShellExecute = true });
        }
        catch (Exception ex)
        {
            _ = ShowMessageAsync("Could not open Settings", ex.Message);
        }
    }

    private static bool IsAdministrator()
    {
        using var identity = WindowsIdentity.GetCurrent();
        return new WindowsPrincipal(identity).IsInRole(WindowsBuiltInRole.Administrator);
    }

    private async Task ShowMessageAsync(string title, string message)
    {
        var dialog = new ContentDialog
        {
            XamlRoot = ContentPanel.XamlRoot,
            Title = title,
            Content = message,
            CloseButtonText = "OK",
            DefaultButton = ContentDialogButton.Close
        };
        await dialog.ShowAsync();
    }
}
