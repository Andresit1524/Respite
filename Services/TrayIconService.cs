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
            Icon = LoadOrGenerateTrayIcon(),
            Visible = true
        };

        _notifyIcon.DoubleClick += (s, e) => _onOpenRequested();
        SetupContextMenu();
    }

    private static Icon LoadOrGenerateTrayIcon()
    {
        try
        {
            // Si el usuario provee un icon.ico directamente en la carpeta
            var baseDir = AppDomain.CurrentDomain.BaseDirectory;
            var icoPath = Path.Combine(baseDir, "icon.ico");
            if (File.Exists(icoPath))
            {
                return new Icon(icoPath);
            }

            // O generar el Icono Win32 desde el diseño oficial de icon.svg (esmeralda + rosa)
            using var bmp = new Bitmap(32, 32);
            using (var g = Graphics.FromImage(bmp))
            {
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                g.Clear(Color.Transparent);

                // Círculo principal verde esmeralda (#059669)
                using var brushGreen = new SolidBrush(Color.FromArgb(5, 150, 105));
                g.FillEllipse(brushGreen, 3, 3, 26, 26);

                // Acento superior rosa (#F43F5E)
                using var brushPink = new SolidBrush(Color.FromArgb(244, 63, 94));
                g.FillEllipse(brushPink, 11, 1, 10, 10);

                // Centro ahuecado para dar la silueta de Respite
                using var brushHole = new SolidBrush(Color.FromArgb(15, 23, 42));
                g.FillEllipse(brushHole, 9, 9, 14, 14);
            }

            var hIcon = bmp.GetHicon();
            return Icon.FromHandle(hIcon);
        }
        catch
        {
            return SystemIcons.Application;
        }
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

