using System.Windows.Threading;

namespace Respite.Services;

public enum PomodoroState { Stopped, Working, ShortBreak, LongBreak }

public class PomodoroEngine
{
    private readonly DispatcherTimer _timer;
    private int _remainingSeconds;
    private int _completedWorkCycles = 0;

    public PomodoroState State { get; private set; } = PomodoroState.Stopped;
    public TimeSpan RemainingTime => TimeSpan.FromSeconds(Math.Max(0, _remainingSeconds));
    public int CompletedWorkCycles => _completedWorkCycles;

    public event Action? Tick;
    public event Action<PomodoroState>? StateChanged;

    public PomodoroEngine()
    {
        _timer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(1)
        };
        _timer.Tick += OnTimerTick;
    }

    public void StartWork(int workMinutes)
    {
        State = PomodoroState.Working;
        _remainingSeconds = workMinutes * 60;
        _timer.Start();
        StateChanged?.Invoke(State);
    }

    public void StartBreak(int breakMinutes, bool isLong = false)
    {
        State = isLong ? PomodoroState.LongBreak : PomodoroState.ShortBreak;
        _remainingSeconds = breakMinutes * 60;
        _timer.Start();
        StateChanged?.Invoke(State);
    }

    public void Pause()
    {
        _timer.Stop();
    }

    public void Resume()
    {
        if (State != PomodoroState.Stopped && !_timer.IsEnabled)
        {
            _timer.Start();
        }
    }

    public void Stop()
    {
        _timer.Stop();
        State = PomodoroState.Stopped;
        _remainingSeconds = 0;
        StateChanged?.Invoke(State);
    }

    private void OnTimerTick(object? sender, EventArgs e)
    {
        if (_remainingSeconds > 0)
        {
            _remainingSeconds--;
            Tick?.Invoke();
        }

        if (_remainingSeconds <= 0)
        {
            _timer.Stop();
            var finishedState = State;

            if (finishedState == PomodoroState.Working)
            {
                _completedWorkCycles++;
            }

            State = PomodoroState.Stopped;
            StateChanged?.Invoke(finishedState);
        }
    }
}

