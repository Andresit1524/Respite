using System.Drawing;
using System.IO;
using System.Windows.Forms;
using Application = System.Windows.Application;

namespace Respite.Services;

public class TrayIconService : IDisposable
{
    private readonly NotifyIcon _notifyIcon;
    private readonly Action _onOpenRequested;
    private readonly Action _onExitRequested;
    private readonly Action? _onTogglePomodoro;

    public TrayIconService(
        Action onOpenRequested,
        Action onExitRequested,
        Action? onTogglePomodoro = null)
    {
        _onOpenRequested = onOpenRequested;
        _onExitRequested = onExitRequested;
        _onTogglePomodoro = onTogglePomodoro;

        _notifyIcon = new NotifyIcon
        {
            Text = "Respite",
            Icon = SystemIcons.Application, // Fallback nativo
            Visible = true
        };

        _notifyIcon.DoubleClick += (s, e) => _onOpenRequested();
        SetupContextMenu();
    }

    private void SetupContextMenu()
    {
        var contextMenu = new ContextMenuStrip();

        var openItem = new ToolStripMenuItem("Abrir Respite", null, (s, e) => _onOpenRequested());
        openItem.Font = new System.Drawing.Font(openItem.Font, System.Drawing.FontStyle.Bold);
        contextMenu.Items.Add(openItem);

        if (_onTogglePomodoro != null)
        {
            contextMenu.Items.Add(new ToolStripMenuItem("Iniciar / Detener Pomodoro", null, (s, e) => _onTogglePomodoro()));
        }

        contextMenu.Items.Add(new ToolStripSeparator());
        contextMenu.Items.Add(new ToolStripMenuItem("Salir", null, (s, e) => _onExitRequested()));

        _notifyIcon.ContextMenuStrip = contextMenu;
    }

    public void ShowNotification(string title, string message, ToolTipIcon icon = ToolTipIcon.Info)
    {
        _notifyIcon.ShowBalloonTip(3000, title, message, icon);
    }

    public void UpdateTooltip(string text)
    {
        // El texto de tooltip en NotifyIcon tiene un límite de 63 caracteres en Win32
        if (text.Length > 63)
        {
            text = text.Substring(0, 60) + "...";
        }
        _notifyIcon.Text = text;
    }

    public void Dispose()
    {
        _notifyIcon.Visible = false;
        _notifyIcon.Dispose();
    }
}

