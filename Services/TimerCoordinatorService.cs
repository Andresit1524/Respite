using Respite.Models;

namespace Respite.Services;

public class TimerCoordinatorService
{
    private readonly SettingsService _settingsService;
    public PomodoroEngine Pomodoro { get; }
    public HardLimitEngine HardLimit { get; }

    public event Action? PomodoroCycleCompleted;
    public event Action? HardLimitBreakRequired;
    public event Action? HardLimitBreakFinished;

    public TimerCoordinatorService(SettingsService settingsService)
    {
        _settingsService = settingsService;
        Pomodoro = new PomodoroEngine();
        HardLimit = new HardLimitEngine();

        // Configuración inicial de límites duros
        ApplySettings(_settingsService.Current);

        // Suscripción a eventos
        Pomodoro.StateChanged += OnPomodoroStateChanged;
        HardLimit.BreakStarted += () => HardLimitBreakRequired?.Invoke();
        HardLimit.BreakEnded += () => HardLimitBreakFinished?.Invoke();

        // Iniciar seguimiento de límite duro automáticamente
        HardLimit.Start();
    }

    public void ApplySettings(AppSettings settings)
    {
        HardLimit.Configure(
            settings.HardLimitIntervalMinutes,
            settings.HardLimitBreakMinutes,
            settings.MaxAllowedSessionMinutes
        );
    }

    private void OnPomodoroStateChanged(PomodoroState state)
    {
        if (state == PomodoroState.Working || state == PomodoroState.ShortBreak || state == PomodoroState.LongBreak)
        {
            // El pomodoro omite los límites duros
            HardLimit.Pause();
        }
        else if (state == PomodoroState.Stopped)
        {
            // Al detener o terminar el pomodoro, los límites duros reanudan su seguimiento
            HardLimit.Resume();
            PomodoroCycleCompleted?.Invoke();
        }
    }
}

