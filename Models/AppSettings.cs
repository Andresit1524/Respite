namespace Respite.Models;

public class AppSettings
{
    // Pomodoro
    public int PomodoroWorkMinutes { get; set; } = 25;
    public int PomodoroShortBreakMinutes { get; set; } = 5;
    public int PomodoroLongBreakMinutes { get; set; } = 30;
    public int PomodoroCyclesBeforeLongBreak { get; set; } = 4;

    // Límites Duros
    public int HardLimitIntervalMinutes { get; set; } = 120;
    public int HardLimitBreakMinutes { get; set; } = 15;
    public int MaxAllowedSessionMinutes { get; set; } = 360;

    // Comportamiento del Sistema
    public bool StartWithWindows { get; set; } = true;
    public bool MinimizeToTrayOnClose { get; set; } = true;
    public bool PlayNotificationSounds { get; set; } = true;
}

