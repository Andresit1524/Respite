using System.Windows.Threading;

namespace Respite.Services;

public enum HardLimitState
{
    Tracking,   // Acumulando tiempo de uso de PC continuo
    BreakTime,  // Período de descanso obligatorio activo
    Paused      // En pausa (por ejemplo, cuando Pomodoro está activo o usuario solicitó excepción temporal)
}

public class HardLimitEngine
{
    private readonly DispatcherTimer _timer;
    private int _workSecondsRemaining;
    private int _breakSecondsRemaining;
    private int _intervalMinutes = 60;
    private int _breakMinutes = 10;

    public HardLimitState State { get; private set; } = HardLimitState.Tracking;
    public TimeSpan RemainingTime => State == HardLimitState.BreakTime
        ? TimeSpan.FromSeconds(Math.Max(0, _breakSecondsRemaining))
        : TimeSpan.FromSeconds(Math.Max(0, _workSecondsRemaining));

    public event Action? Tick;
    public event Action? BreakStarted;
    public event Action? BreakEnded;

    public HardLimitEngine()
    {
        _timer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(1)
        };
        _timer.Tick += OnTimerTick;
    }

    public void Configure(int intervalMinutes, int breakMinutes, int maxMinutes = 180)
    {
        _intervalMinutes = Math.Min(intervalMinutes, maxMinutes);
        _breakMinutes = breakMinutes;
        ResetWorkTimer();
    }

    public void Start()
    {
        ResetWorkTimer();
        State = HardLimitState.Tracking;
        _timer.Start();
    }

    public void Pause()
    {
        _timer.Stop();
        State = HardLimitState.Paused;
    }

    public void Resume()
    {
        if (State == HardLimitState.Paused)
        {
            State = HardLimitState.Tracking;
            _timer.Start();
        }
    }

    public void ResetWorkTimer()
    {
        _workSecondsRemaining = _intervalMinutes * 60;
    }

    private void OnTimerTick(object? sender, EventArgs e)
    {
        if (State == HardLimitState.Tracking)
        {
            if (_workSecondsRemaining > 0)
            {
                _workSecondsRemaining--;
                Tick?.Invoke();
            }

            if (_workSecondsRemaining <= 0)
            {
                // Iniciar descanso obligatorio de límite duro
                State = HardLimitState.BreakTime;
                _breakSecondsRemaining = _breakMinutes * 60;
                BreakStarted?.Invoke();
            }
        }
        else if (State == HardLimitState.BreakTime)
        {
            if (_breakSecondsRemaining > 0)
            {
                _breakSecondsRemaining--;
                Tick?.Invoke();
            }

            if (_breakSecondsRemaining <= 0)
            {
                // Finalizó el descanso
                ResetWorkTimer();
                State = HardLimitState.Tracking;
                BreakEnded?.Invoke();
            }
        }
    }
}

