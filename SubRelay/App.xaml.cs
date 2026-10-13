using System;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Interop;
using SubRelay.Audio;
using SubRelay.Models;
using SubRelay.Services;
using Localization = SubRelay.Services.Localization;
using Forms = System.Windows.Forms;

namespace SubRelay;

public partial class App : Application
{
    private Mutex? _mutex;
    private EventWaitHandle? _activateEvent;
    private RegisteredWaitHandle? _activationWait;
    private EventWaitHandle? _exitEvent;
    private RegisteredWaitHandle? _exitWait;
    private bool _ownsMutex;
    private bool _exiting;
    private bool _trayHintShown;
    private Forms.NotifyIcon? _tray;
    private Forms.ToolStripMenuItem? _trayOpenItem;
    private Forms.ToolStripMenuItem? _trayPauseItem;
    private Forms.ToolStripMenuItem? _trayExitItem;
    private Icon? _icon;
    private IAudioEngine? _audio;
    private AppSettings _settings = new();
    private readonly SettingsStore _store = new();

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        Localization.Apply("system");
        if (e.Args.Contains("--exit", StringComparer.OrdinalIgnoreCase))
        {
            Shutdown(RequestExistingInstanceExit());
            return;
        }
        DispatcherUnhandledException += (_, args) =>
        {
            AppLog.Write("UI exception", args.Exception);
            MessageBox.Show(Localization.Text("UiError", args.Exception.Message), "Sub Relay", MessageBoxButton.OK, MessageBoxImage.Error);
            args.Handled = true;
            ExitApplication();
        };
        if (!AcquireSingleInstance()) { Shutdown(); return; }
        try
        {
            _settings = _store.Load();
            Localization.Apply(_settings.Language);
            _audio = new AudioEngine(Localization.Text, AppLog.Write);
            var window = new MainWindow(_settings, _audio, SaveSettings);
            MainWindow = window;
            window.ExitRequested += ExitApplication;
            window.PauseStateChanged += UpdateTrayPause;
            window.Closing += WindowClosing;
            CreateTray();
            window.SourceInitialized += (_, _) =>
            {
                if (_icon is not null) window.Icon = Imaging.CreateBitmapSourceFromHIcon(_icon.Handle, Int32Rect.Empty, System.Windows.Media.Imaging.BitmapSizeOptions.FromEmptyOptions());
            };
            _audio.Configure(_settings.Subwoofers, _settings.IsPaused);
            SaveSettings();
            bool hiddenStartup = e.Args.Contains("--startup", StringComparer.OrdinalIgnoreCase) && _settings.CloseToTray;
            if (!hiddenStartup) window.Show();
            if (_store.LoadWarning is not null) ShowNotification(_store.LoadWarning);
        }
        catch (Exception ex)
        {
            AppLog.Write("Startup exception", ex);
            MessageBox.Show(ex.Message, Localization.Text("StartupError"), MessageBoxButton.OK, MessageBoxImage.Error);
            Shutdown(1);
        }
    }

    private bool AcquireSingleInstance()
    {
        _mutex = new Mutex(true, @"Local\SubRelay.Application", out _ownsMutex);
        _activateEvent = new EventWaitHandle(false, EventResetMode.AutoReset, @"Local\SubRelay.Activate");
        if (!_ownsMutex) { _activateEvent.Set(); return false; }
        _exitEvent = new EventWaitHandle(false, EventResetMode.AutoReset, @"Local\SubRelay.Exit");
        _exitWait = ThreadPool.RegisterWaitForSingleObject(_exitEvent,
            (_, _) => { if (!_exiting) Dispatcher.BeginInvoke(ExitApplication); }, null, Timeout.Infinite, false);
        _activationWait = ThreadPool.RegisterWaitForSingleObject(_activateEvent,
            (_, _) => { if (!_exiting) Dispatcher.BeginInvoke(ShowWindow); }, null, Timeout.Infinite, false);
        return true;
    }

    // Setup uses this command to stop the tray application before updating or
    // removing files. The helper never loads settings or changes startup entries.
    private static int RequestExistingInstanceExit()
    {
        try
        {
            if (!Mutex.TryOpenExisting(@"Local\SubRelay.Application", out var instance)) return 0;
            using (instance)
            {
                bool acquired = false;
                try
                {
                    try { acquired = instance.WaitOne(0); }
                    catch (AbandonedMutexException) { acquired = true; }
                    if (acquired) return 0;
                    if (!EventWaitHandle.TryOpenExisting(@"Local\SubRelay.Exit", out var exitEvent)) return 1;
                    using (exitEvent) exitEvent.Set();
                    try { acquired = instance.WaitOne(TimeSpan.FromSeconds(10)); }
                    catch (AbandonedMutexException) { acquired = true; }
                    return acquired ? 0 : 1;
                }
                finally { if (acquired) instance.ReleaseMutex(); }
            }
        }
        catch (UnauthorizedAccessException) { return 1; }
        catch (System.Threading.WaitHandleCannotBeOpenedException) { return 1; }
    }

    private void CreateTray()
    {
        var iconResource = GetResourceStream(new Uri("pack://application:,,,/Assets/SubRelay.ico"))
            ?? throw new InvalidOperationException(Localization.Text("MissingAppIcon"));
        using (iconResource.Stream) _icon = new Icon(iconResource.Stream, 32, 32);
        var menu = new Forms.ContextMenuStrip();
        _trayOpenItem = new Forms.ToolStripMenuItem(Localization.Text("OpenApp"), null, (_, _) => Dispatcher.Invoke(ShowWindow));
        _trayPauseItem = new Forms.ToolStripMenuItem(Localization.Text(_settings.IsPaused ? "Resume" : "Pause"), null,
            (_, _) => Dispatcher.Invoke(() => (MainWindow as SubRelay.MainWindow)?.TogglePause()));
        _trayExitItem = new Forms.ToolStripMenuItem(Localization.Text("Exit"), null, (_, _) => Dispatcher.Invoke(ExitApplication));
        menu.Items.Add(_trayOpenItem);
        menu.Items.Add(_trayPauseItem);
        menu.Items.Add(new Forms.ToolStripSeparator());
        menu.Items.Add(_trayExitItem);
        _tray = new Forms.NotifyIcon { Icon = _icon, Text = Localization.Text("TrayTitle"), ContextMenuStrip = menu, Visible = true };
        _tray.DoubleClick += (_, _) => Dispatcher.Invoke(ShowWindow);
        Localization.Changed += UpdateTrayLanguage;
    }

    private void UpdateTrayLanguage()
    {
        if (_tray is null) return;
        if (_trayOpenItem is not null) _trayOpenItem.Text = Localization.Text("OpenApp");
        if (_trayExitItem is not null) _trayExitItem.Text = Localization.Text("Exit");
        UpdateTrayPause();
        _tray.Text = Localization.Text("TrayTitle");
    }

    private void UpdateTrayPause()
    {
        if (_trayPauseItem is not null) _trayPauseItem.Text = Localization.Text(_settings.IsPaused ? "Resume" : "Pause");
    }

    private void SaveSettings()
    {
        try
        {
            StartupService.Apply(_settings.StartWithWindows);
            _store.Save(_settings);
        }
        catch (Exception ex)
        {
            AppLog.Write("Save settings/startup", ex);
            MessageBox.Show(Localization.Text("SaveSettingsError", ex.Message), "Sub Relay", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    private void WindowClosing(object? sender, CancelEventArgs e)
    {
        if (_exiting) return;
        if (_settings.CloseToTray)
        {
            e.Cancel = true;
            MainWindow.Hide();
            if (!_trayHintShown)
            {
                _trayHintShown = true;
                ShowNotification(Localization.Text("TrayHint"));
            }
        }
        else ExitApplication();
    }

    private void ShowWindow()
    {
        if (_exiting || MainWindow is null) return;
        MainWindow.Show();
        if (MainWindow.WindowState == WindowState.Minimized) MainWindow.WindowState = WindowState.Normal;
        MainWindow.Activate();
    }

    private void ShowNotification(string message)
    {
        if (_tray is null) return;
        _tray.ShowBalloonTip(3500, "Sub Relay", message, Forms.ToolTipIcon.Info);
    }

    private void ExitApplication()
    {
        if (_exiting) return;
        _exiting = true;
        Shutdown();
    }

    protected override void OnSessionEnding(SessionEndingCancelEventArgs e)
    {
        _exiting = true;
        base.OnSessionEnding(e);
        Shutdown();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _exiting = true;
        _activationWait?.Unregister(null);
        Localization.Changed -= UpdateTrayLanguage;
        _exitWait?.Unregister(null);
        _audio?.Dispose();
        if (_tray is not null)
        {
            _tray.Visible = false;
            _tray.ContextMenuStrip?.Dispose();
            _tray.Dispose();
        }
        _icon?.Dispose();
        _activateEvent?.Dispose();
        _exitEvent?.Dispose();
        if (_ownsMutex) _mutex?.ReleaseMutex();
        _mutex?.Dispose();
        base.OnExit(e);
    }

}
