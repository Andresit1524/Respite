using System.Windows;
using Respite.ViewModels;

namespace Respite;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    public MainWindow(MainViewModel viewModel) : this()
    {
        DataContext = viewModel;
    }

    public void PositionNearTray()
    {
        // Obtener el área de trabajo utilizable de la pantalla principal (excluye la barra de tareas)
        var workArea = SystemParameters.WorkArea;

        // Situar la ventana en la esquina inferior derecha justo encima de la barra de tareas / bandeja
        Left = workArea.Right - Width - 12;

        var targetHeight = ActualHeight > 0 ? ActualHeight : 240;
        var newTop = workArea.Bottom - targetHeight - 12;

        if (newTop < workArea.Top + 12)
        {
            newTop = workArea.Top + 12;
        }

        Top = newTop;
    }

    private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
    {
        if (IsVisible)
        {
            PositionNearTray();
        }
    }

    private void Window_Deactivated(object sender, EventArgs e)
    {
        // Comportamiento típico de popup de bandeja: al hacer clic fuera, se oculta suavemente
        Hide();
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        Hide();
    }
}