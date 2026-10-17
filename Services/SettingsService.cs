using System.IO;
using System.Text.Json;
using Respite.Models;

namespace Respite.Services;

public class SettingsService
{
    private static readonly string AppFolder = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "Respite"
    );
    private static readonly string FilePath = Path.Combine(AppFolder, "settings.json");

    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        WriteIndented = true
    };

    public AppSettings Current { get; private set; }

    public SettingsService()
    {
        Current = Load();
    }

    public AppSettings Load()
    {
        try
        {
            if (File.Exists(FilePath))
            {
                var json = File.ReadAllText(FilePath);
                var settings = JsonSerializer.Deserialize<AppSettings>(json);
                if (settings != null)
                {
                    // Aplica el límite de tiempo del límite duro
                    if (settings.HardLimitIntervalMinutes > settings.MaxAllowedSessionMinutes)
                    {
                        settings.HardLimitIntervalMinutes = settings.MaxAllowedSessionMinutes;
                    }
                    return settings;
                }
            }
        }
        catch
        {
            // TODO: logs aquí
            // Si hay error de lectura o corrupción, se inicia con valores por defecto
        }

        return new AppSettings();
    }

    public void Save(AppSettings? settings = null)
    {
        if (settings != null)
        {
            Current = settings;
        }

        if (Current.HardLimitIntervalMinutes > Current.MaxAllowedSessionMinutes)
        {
            Current.HardLimitIntervalMinutes = Current.MaxAllowedSessionMinutes;
        }

        try
        {
            if (!Directory.Exists(AppFolder))
            {
                Directory.CreateDirectory(AppFolder);
            }

            var json = JsonSerializer.Serialize(Current, _jsonOptions);
            File.WriteAllText(FilePath, json);
        }
        catch
        {
            // TODO: logs aquí
        }
    }
}

