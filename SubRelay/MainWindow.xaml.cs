using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using SubRelay.Audio;
using SubRelay.Models;
using SubRelay.Services;
using Localization = SubRelay.Services.Localization;
using SubRelay.UI;

namespace SubRelay;

public partial class MainWindow : Window
{
    private readonly AppSettings _settings;
    private readonly IAudioEngine _audio;
    private readonly Action _saveSettings;
    private readonly List<SubwooferCard> _cards = [];
    private AudioEngineSnapshot _snapshot;
    private bool _ready;
    private bool _closed;

    public event Action? ExitRequested;
    public event Action? PauseStateChanged;

    public MainWindow(AppSettings settings, IAudioEngine audio, Action saveSettings)
    {
        _settings = settings;
        _audio = audio;
        _saveSettings = saveSettings;
        _snapshot = audio.Snapshot;
        InitializeComponent();
        SubwooferScroll.MaxHeight = Math.Max(120, SystemParameters.WorkArea.Height - 310);
        CloseToTrayBox.IsChecked = settings.CloseToTray;
        StartWithWindowsBox.IsChecked = settings.StartWithWindows;
        UpdateLanguageChecks();
        UpdatePauseButton();
        if (settings.Subwoofers.Count == 0) settings.Subwoofers.Add(new SubwooferSettings());
        foreach (var subwoofer in settings.Subwoofers)
            AddCard(subwoofer);
        UpdateCardNumbers();
        _audio.SnapshotChanged += AudioSnapshotChanged;
        // Read again after subscribing: discovery may finish while XAML is loading.
        ApplySnapshot(_audio.Snapshot);
        Closed += (_, _) =>
        {
            _closed = true;
            _audio.SnapshotChanged -= AudioSnapshotChanged;
        };
        _ready = true;
    }

    private void AddCard(SubwooferSettings subwoofer)
    {
        var card = new SubwooferCard(subwoofer, RoutesChanged, RemoveCard);
        _cards.Add(card);
        SubwooferCards.Children.Add(card);
        card.ApplySnapshot(_snapshot);
    }

    private void AddSubwooferClicked(object sender, RoutedEventArgs e)
    {
        var subwoofer = new SubwooferSettings();
        _settings.Subwoofers.Add(subwoofer);
        AddCard(subwoofer);
        UpdateCardNumbers();
        RoutesChanged();
        _cards[^1].FocusDevice();
    }

    private void RemoveCard(SubwooferCard card)
    {
        if (_cards.Count == 1)
        {
            card.ClearDevice();
            RoutesChanged();
            return;
        }
        _settings.Subwoofers.Remove(card.Settings);
        _cards.Remove(card);
        SubwooferCards.Children.Remove(card);
        UpdateCardNumbers();
        RoutesChanged();
    }

    private void UpdateCardNumbers()
    {
        for (int index = 0; index < _cards.Count; index++)
            _cards[index].SetNumber(index + 1);
    }

    private void RoutesChanged()
    {
        if (!_ready)
            return;
        _saveSettings();
        _audio.Configure(_settings.Subwoofers.Select(s => s.Copy()).ToArray(), _settings.IsPaused);
        foreach (var card in _cards)
            card.ApplySnapshot(_snapshot);
    }

    private void SettingsChanged(object sender, RoutedEventArgs e)
    {
        if (!_ready)
            return;
        _settings.CloseToTray = CloseToTrayBox.IsChecked == true;
        _settings.StartWithWindows = StartWithWindowsBox.IsChecked == true;
        _saveSettings();
        _audio.Configure(_settings.Subwoofers, _settings.IsPaused);
    }

    private void PauseClicked(object sender, RoutedEventArgs e) => TogglePause();

    public void TogglePause()
    {
        if (!_ready) return;
        _settings.IsPaused = !_settings.IsPaused;
        UpdatePauseButton();
        _audio.Configure(_settings.Subwoofers, _settings.IsPaused);
        _saveSettings();
        PauseStateChanged?.Invoke();
    }

    private void UpdatePauseButton()
    {
        PauseButton.SetResourceReference(ContentControl.ContentProperty, _settings.IsPaused ? "Resume" : "Pause");
        PauseButton.SetResourceReference(FrameworkElement.ToolTipProperty, _settings.IsPaused ? "ManuallyPaused" : "PauseHelp");
    }

    private void RefreshClicked(object sender, RoutedEventArgs e) => _audio.Refresh();

    private void LanguageChanged(object sender, RoutedEventArgs e)
    {
        if (!_ready || sender is not MenuItem { Tag: string language }) return;
        _settings.Language = language;
        Localization.Apply(language);
        UpdateLanguageChecks();
        foreach (var card in _cards) card.RefreshLanguage();
        _saveSettings();
        _audio.Refresh();
    }

    private void UpdateLanguageChecks()
    {
        foreach (var item in LanguageMenu.Items.OfType<MenuItem>())
            item.IsChecked = string.Equals(item.Tag as string, _settings.Language, StringComparison.OrdinalIgnoreCase);
    }

    private void OpenSettingsClicked(object sender, RoutedEventArgs e)
    {
        var button = (Button)sender;
        button.ContextMenu.PlacementTarget = button;
        button.ContextMenu.Placement = PlacementMode.Bottom;
        button.ContextMenu.IsOpen = true;
    }

    private void ExitClicked(object sender, RoutedEventArgs e) => ExitRequested?.Invoke();

    private void AudioSnapshotChanged(AudioEngineSnapshot snapshot)
    {
        if (_closed || Dispatcher.HasShutdownStarted)
            return;
        if (Dispatcher.CheckAccess())
            ApplySnapshot(snapshot);
        else
            Dispatcher.BeginInvoke(new Action(() => { if (!_closed) ApplySnapshot(snapshot); }));
    }

    private void ApplySnapshot(AudioEngineSnapshot snapshot)
    {
        _snapshot = snapshot;
        SourceLabel.Text = snapshot.SourceDeviceName;
        SourceLabel.ToolTip = snapshot.SourceDeviceName;
        ErrorLabel.Text = snapshot.Error ?? "";
        ErrorLabel.Visibility = string.IsNullOrWhiteSpace(snapshot.Error) ? Visibility.Collapsed : Visibility.Visible;
        foreach (var card in _cards)
            card.ApplySnapshot(snapshot);
    }
}
