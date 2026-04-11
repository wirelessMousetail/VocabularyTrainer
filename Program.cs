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
//  * Review namespace and directory structure
//  * Refine words list and make sure that it is compliant
//    * Exclude words with non-latin letters or with more than 3 words in answer (including article and words in brackets)
//    * All multioption answers should be put correctly
//    * Questions should not contain commas or brackets
//  * Rewrite all testes: those which can be parametrized, should be parametrized
//  * improve to add other languages support


//todo Написать скрипт, чтобы смержить words и appdata, привести финальный лист в нормальный вид, убрать дубликаты если есть
