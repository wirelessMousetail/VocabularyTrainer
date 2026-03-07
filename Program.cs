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




// todo
//  Далее:
//  + сделать размеры автоподстраиваемыми
//  + Закрывать форму через 5 секунд после правильного ответа
//  + Таймер стартовать только после того, как предыдущее закрыто
//  + Сделать без хедера, сплошным бирюзовым цветом, кнопки - подстраиваемые (крестик должен быть)
//  + Сделать тайметр настраиваемым (добавить поле для насттройки сколько раз в день спрашивать?)
//  + Добавить возможность определять часть речи автоматически и выдавать только одинаковые
//  + Сохранять опции в конфиг файл (если еще не)
//  + Добавить в опции выбор количества вариантов
//  + Добавить веса, прописывать в цсвхе. Если ошибаешься - увеличивать вес, при достаточном количестве правильных ответов - уменьшать (Но не сразу до нормы)
//  + При обновлении цсв приложением сохранять ее отдельно. При старте программы мержить изначальную с ней
//  + Добавление новых слов
//  + Группировать по части речи при загрузке новых слов, читать ее из списка. Если пустое поле - вычислять
//  + Добавить возможность изменить "направление" опроса eng-dutch and vice versa
//  + Мигрировать с вин форм на что-нибудь кросс платформенное, сделать мак совместимым
//      + Иконку приложения в заголовке окна вместо дефолтной
//      + Уточнить назначения RelayCommand и ViewModelBase
//      + Добавить команды для релиза под разные платформ
//      + Протестить на маке
//      * Открыть PR и дать заревьювить кодексу
//  * Открывать нвоый вопрос двойным щелчком по трею
//  * Выбирать в варианты "похожие" слова
//  * Повышать вес не только слова которое не угадал, но и варианта который выбрал вместо него
//  ** Возможность просить напечатать ответ (требуется нормализация списка)
//  * Add contributions.md file
//  * Improve ARCHITECTURE.md: Make sure that infrastructure layer is on the diagram and replace the text diagram with graphical diagram
//  * Transform WordGrouping to service with interface, so it can be easily replaced with another implementation
//  * Application context class should not manage timer and handle pause/resume reactions
//  * Custom tray icon for paused state?
//  * Придумать что делать с синонимами: beslissen and besluiten have similar meaning, so both should be correct answers to "to decide"
//  * Новым загруженным словам ставить автоматически рейтинг повыше (максимум?) и сразу 5 страйков, чтобы далее уменьшалось экспоционально
//  * Написать скрипт, чтобы смержить words и appdata, привести финальный лист в нормальный вид, убрать дубликаты если есть
//  - Логику таймера в отдельный интерфейс (чтобы можно было свичнуться в начало отсчета после окончания предыдущего вместо ответа)




//todo chatgpt next steps proposal:
// When you’re ready, we can:
//   + Prevent multiple windows
//   + Configure interval
//   + Add tray icon
//   + Remember wrong answers 
//   Track stats
//   live preview of “next quiz in X minutes”
//   Just tell me what you want next 😄