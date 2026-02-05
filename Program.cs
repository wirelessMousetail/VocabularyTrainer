namespace VocabularyTrainer;

using System;
using System.IO;
using System.Windows.Forms;
using VocabularyTrainer.Services;

static class Program
{
    [STAThread]
    static void Main()
    {
        ApplicationConfiguration.Initialize();

        var settingsPath = Path.Combine(AppContext.BaseDirectory, "appsettings.json");
        var settingsService = new SettingsService(settingsPath);
        
        Application.Run(
            new VocabApplicationContext(settingsService)
        );
    }
}

