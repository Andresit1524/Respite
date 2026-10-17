using System.Windows;
using Respite.Services;

namespace Respite;

public partial class App : System.Windows.Application
{
    private TrayIconService? _trayIconService;
    private SettingsService? _settingsService;
    private TimerCoordinatorService? _timerCoordinator;
    private MainWindow? _mainWindow;
    private bool _isExplicitExit = false;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        _settingsService = new SettingsService();
        _timerCoordinator = new TimerCoordinatorService(_settingsService);

        // Sincronizar auto-arranque en inicio según configuración
        if (_settingsService.Current.StartWithWindows != StartupService.IsStartupEnabled())
        {
            StartupService.SetStartup(_settingsService.Current.StartWithWindows);
        }

        _mainWindow = new MainWindow();
        _mainWindow.Closing += OnMainWindowClosing;

        // Conectar límites duros: al dispararse el descanso forzoso
        _timerCoordinator.HardLimitBreakRequired += OnHardLimitBreakRequired;

        _trayIconService = new TrayIconService(
            onOpenRequested: ShowMainWindow,
            onExitRequested: ExitApplication,
            onTogglePomodoro: TogglePomodoro
        );

        // Si se inició directamente por el usuario, mostrar la ventana principal
        ShowMainWindow();
    }

    private void ShowMainWindow()
    {
        if (_mainWindow == null)
        {
            _mainWindow = new MainWindow();
            _mainWindow.Closing += OnMainWindowClosing;
        }

        if (_mainWindow.WindowState == WindowState.Minimized)
        {
            _mainWindow.WindowState = WindowState.Normal;
        }

        _mainWindow.Show();
        _mainWindow.Activate();
    }

    private void OnMainWindowClosing(object? sender, System.ComponentModel.CancelEventArgs e)
    {
        if (!_isExplicitExit && (_settingsService?.Current.MinimizeToTrayOnClose ?? true))
        {
            e.Cancel = true;
            _mainWindow?.Hide();
            _trayIconService?.ShowNotification("Respite sigue activo", "Puedes abrir la aplicación desde la bandeja del sistema.");
        }
    }

    private void OnHardLimitBreakRequired()
    {
        Dispatcher.Invoke(() =>
        {
            WindowActionService.MinimizeAllWindows();

            if (_timerCoordinator != null)
            {
                var breakWindow = new Views.BreakOverlayWindow(_timerCoordinator.HardLimit);
                breakWindow.Show();
            }
        });
    }

    private void TogglePomodoro()
    {
        if (_timerCoordinator == null) return;

        if (_timerCoordinator.Pomodoro.State == PomodoroState.Working ||
            _timerCoordinator.Pomodoro.State == PomodoroState.ShortBreak ||
            _timerCoordinator.Pomodoro.State == PomodoroState.LongBreak)
        {
            _timerCoordinator.Pomodoro.Stop();
            _trayIconService?.ShowNotification("Pomodoro detenido", "Los descansos de límite duro han sido reanudados.");
        }
        else
        {
            int workMins = _settingsService?.Current.PomodoroWorkMinutes ?? 25;
            _timerCoordinator.Pomodoro.StartWork(workMins);
            _trayIconService?.ShowNotification("Pomodoro iniciado", $"Sesión de trabajo de {workMins} minutos comenzada.");
        }
    }

    private void ExitApplication()
    {
        _isExplicitExit = true;
        _trayIconService?.Dispose();
        Shutdown();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _trayIconService?.Dispose();
        base.OnExit(e);
    }
}

