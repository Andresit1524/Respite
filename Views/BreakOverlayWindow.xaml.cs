using System.Windows;
using Respite.Services;

namespace Respite.Views;

public partial class BreakOverlayWindow : Window
{
    private readonly HardLimitEngine _hardLimitEngine;

    public BreakOverlayWindow(HardLimitEngine hardLimitEngine)
    {
        InitializeComponent();
        _hardLimitEngine = hardLimitEngine;

        UpdateTimerText();
        _hardLimitEngine.Tick += OnTick;
        _hardLimitEngine.BreakEnded += OnBreakEnded;
    }

    private void OnTick()
    {
        Dispatcher.Invoke(UpdateTimerText);
    }

    private void UpdateTimerText()
    {
        var remaining = _hardLimitEngine.RemainingTime;
        TxtRemainingTime.Text = $"{remaining.Minutes:D2}:{remaining.Seconds:D2}";
    }

    private void OnBreakEnded()
    {
        Dispatcher.Invoke(() =>
        {
            _hardLimitEngine.Tick -= OnTick;
            _hardLimitEngine.BreakEnded -= OnBreakEnded;
            Close();
        });
    }

    private void BtnPostpone_Click(object sender, RoutedEventArgs e)
    {
        // Aplazar temporalmente: pausar descanso y otorgar 5 minutos adicionales
        // TODO: ofrecer configuración para este tiempo
        _hardLimitEngine.Tick -= OnTick;
        _hardLimitEngine.BreakEnded -= OnBreakEnded;
        _hardLimitEngine.Configure(5, 10);
        _hardLimitEngine.Start();
        Close();
    }

    protected override void OnClosed(EventArgs e)
    {
        _hardLimitEngine.Tick -= OnTick;
        _hardLimitEngine.BreakEnded -= OnBreakEnded;
        base.OnClosed(e);
    }
}

