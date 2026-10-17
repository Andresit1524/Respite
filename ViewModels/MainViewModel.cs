using System.Windows.Input;
using Respite.Models;
using Respite.Services;

namespace Respite.ViewModels;

public class MainViewModel : ViewModelBase
{
    private readonly SettingsService _settingsService;
    private readonly TimerCoordinatorService _coordinator;

    // Reloj Pomodoro
    private string _pomodoroDisplay = "25:00";
    private string _pomodoroStatus = "Listo para comenzar";
    private bool _isPomodoroRunning;

    // Reloj Límite Duro
    private string _hardLimitDisplay = "60:00";
    private string _hardLimitStatus = "Monitoreando tiempo continuo";

    // Configuraciones
    private int _pomodoroWorkMinutes;
    private int _pomodoroShortBreakMinutes;
    private int _pomodoroLongBreakMinutes;
    private int _hardLimitIntervalMinutes;
    private int _hardLimitBreakMinutes;
    private bool _startWithWindows;
    private bool _minimizeToTrayOnClose;

    public string PomodoroDisplay
    {
        get => _pomodoroDisplay;
        set => SetProperty(ref _pomodoroDisplay, value);
    }

    public string PomodoroStatus
    {
        get => _pomodoroStatus;
        set => SetProperty(ref _pomodoroStatus, value);
    }

    public bool IsPomodoroRunning
    {
        get => _isPomodoroRunning;
        set => SetProperty(ref _isPomodoroRunning, value);
    }

    public string HardLimitDisplay
    {
        get => _hardLimitDisplay;
        set => SetProperty(ref _hardLimitDisplay, value);
    }

    public string HardLimitStatus
    {
        get => _hardLimitStatus;
        set => SetProperty(ref _hardLimitStatus, value);
    }

    public int PomodoroWorkMinutes
    {
        get => _pomodoroWorkMinutes;
        set => SetProperty(ref _pomodoroWorkMinutes, value);
    }

    public int PomodoroShortBreakMinutes
    {
        get => _pomodoroShortBreakMinutes;
        set => SetProperty(ref _pomodoroShortBreakMinutes, value);
    }

    public int PomodoroLongBreakMinutes
    {
        get => _pomodoroLongBreakMinutes;
        set => SetProperty(ref _pomodoroLongBreakMinutes, value);
    }

    public int HardLimitIntervalMinutes
    {
        get => _hardLimitIntervalMinutes;
        set => SetProperty(ref _hardLimitIntervalMinutes, value);
    }

    public int HardLimitBreakMinutes
    {
        get => _hardLimitBreakMinutes;
        set => SetProperty(ref _hardLimitBreakMinutes, value);
    }

    public bool StartWithWindows
    {
        get => _startWithWindows;
        set => SetProperty(ref _startWithWindows, value);
    }

    public bool MinimizeToTrayOnClose
    {
        get => _minimizeToTrayOnClose;
        set => SetProperty(ref _minimizeToTrayOnClose, value);
    }

    public ICommand StartPomodoroCommand { get; }
    public ICommand StopPomodoroCommand { get; }
    public ICommand ResetHardLimitCommand { get; }
    public ICommand SaveSettingsCommand { get; }

    public MainViewModel(SettingsService settingsService, TimerCoordinatorService coordinator)
    {
        _settingsService = settingsService;
        _coordinator = coordinator;

        // Cargar valores iniciales desde Settings
        LoadSettings();

        // Comandos
        StartPomodoroCommand = new RelayCommand(StartPomodoro);
        StopPomodoroCommand = new RelayCommand(StopPomodoro);
        ResetHardLimitCommand = new RelayCommand(ResetHardLimit);
        SaveSettingsCommand = new RelayCommand(SaveSettings);

        // Suscripción a eventos de temporizadores
        _coordinator.Pomodoro.Tick += OnPomodoroTick;
        _coordinator.Pomodoro.StateChanged += OnPomodoroStateChanged;
        _coordinator.HardLimit.Tick += OnHardLimitTick;
        _coordinator.HardLimit.BreakStarted += OnHardLimitBreakStarted;
        _coordinator.HardLimit.BreakEnded += OnHardLimitBreakEnded;

        UpdateHardLimitDisplay();
        UpdatePomodoroDisplay();
    }

    private void LoadSettings()
    {
        var s = _settingsService.Current;
        _pomodoroWorkMinutes = s.PomodoroWorkMinutes;
        _pomodoroShortBreakMinutes = s.PomodoroShortBreakMinutes;
        _pomodoroLongBreakMinutes = s.PomodoroLongBreakMinutes;
        _hardLimitIntervalMinutes = s.HardLimitIntervalMinutes;
        _hardLimitBreakMinutes = s.HardLimitBreakMinutes;
        _startWithWindows = s.StartWithWindows;
        _minimizeToTrayOnClose = s.MinimizeToTrayOnClose;
    }

    private void StartPomodoro()
    {
        _coordinator.Pomodoro.StartWork(PomodoroWorkMinutes);
        IsPomodoroRunning = true;
    }

    private void StopPomodoro()
    {
        _coordinator.Pomodoro.Stop();
        IsPomodoroRunning = false;
        UpdatePomodoroDisplay();
    }

    private void ResetHardLimit()
    {
        _coordinator.HardLimit.ResetWorkTimer();
        UpdateHardLimitDisplay();
    }

    private void SaveSettings()
    {
        var s = _settingsService.Current;
        s.PomodoroWorkMinutes = PomodoroWorkMinutes;
        s.PomodoroShortBreakMinutes = PomodoroShortBreakMinutes;
        s.PomodoroLongBreakMinutes = PomodoroLongBreakMinutes;
        s.HardLimitIntervalMinutes = Math.Clamp(HardLimitIntervalMinutes, 10, 180);
        s.HardLimitBreakMinutes = Math.Clamp(HardLimitBreakMinutes, 1, 60);
        s.StartWithWindows = StartWithWindows;
        s.MinimizeToTrayOnClose = MinimizeToTrayOnClose;

        _settingsService.Save(s);
        _coordinator.ApplySettings(s);

        StartupService.SetStartup(StartWithWindows);
    }

    private void OnPomodoroTick()
    {
        UpdatePomodoroDisplay();
    }

    private void OnPomodoroStateChanged(PomodoroState state)
    {
        IsPomodoroRunning = state != PomodoroState.Stopped;
        PomodoroStatus = state switch
        {
            PomodoroState.Working => "Enfoque / Trabajo",
            PomodoroState.ShortBreak => "Descanso Corto",
            PomodoroState.LongBreak => "Descanso Largo",
            _ => "Detenido"
        };
        UpdatePomodoroDisplay();
    }

    private void UpdatePomodoroDisplay()
    {
        var remaining = _coordinator.Pomodoro.RemainingTime;
        if (_coordinator.Pomodoro.State == PomodoroState.Stopped)
        {
            PomodoroDisplay = $"{PomodoroWorkMinutes:D2}:00";
        }
        else
        {
            PomodoroDisplay = $"{remaining.Minutes:D2}:{remaining.Seconds:D2}";
        }
    }

    private void OnHardLimitTick()
    {
        UpdateHardLimitDisplay();
    }

    private void OnHardLimitBreakStarted()
    {
        HardLimitStatus = "En descanso obligatorio";
        UpdateHardLimitDisplay();
    }

    private void OnHardLimitBreakEnded()
    {
        HardLimitStatus = "Monitoreando tiempo continuo";
        UpdateHardLimitDisplay();
    }

    private void UpdateHardLimitDisplay()
    {
        var remaining = _coordinator.HardLimit.RemainingTime;
        HardLimitDisplay = $"{remaining.Hours * 60 + remaining.Minutes:D2}:{remaining.Seconds:D2}";
    }
}

