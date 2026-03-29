using Avalonia;
using System;

namespace VocabularyTrainer;

static class Program
{
    [STAThread]
    static void Main(string[] args)
    {
        BuildAvaloniaApp()
            .StartWithClassicDesktopLifetime(args);
    }

    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace();
}




// todo Next:
//  * Открывать нвоый вопрос двойным щелчком по трею
//  * Выбирать в варианты "похожие" слова - нужно определять "расстояние" между словами
//  * Повышать вес не только слова которое не угадал, но и варианта который выбрал вместо него
//  * Новым загруженным словам ставить автоматически рейтинг повыше-половину от максимального и сразу 5 страйков, чтобы далее уменьшалось экспоционально
//  ** Возможность просить напечатать ответ (требуется нормализация списка)
//  * Custom tray icon for paused state?
//  * Add live preview of “next quiz in X minutes” on hover on tray
//  * Add contributions.md file
//  * Review ARCHITECTURE.md and validate that it matches current implementation
//  * Improve ARCHITECTURE.md: Make sure that infrastructure layer is on the diagram


//todo Написать скрипт, чтобы смержить words и appdata, привести финальный лист в нормальный вид, убрать дубликаты если есть