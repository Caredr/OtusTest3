
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using OtusTest3.Core.DataAccess;
using OtusTest3.Core.Exeptions;
using OtusTest3.Core.Infrastructure.DataAccess;
using OtusTest3.Core.Services;
using OtusTest3.Core.TelegramBot;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;

namespace OtusTest3
{
    internal class Program
    {
        private static async Task Main(string[] args)
        {
            ArgumentNullException.ThrowIfNull(args);
            //Задаю макс кол-во задач, считываю, идет преобразование.
            try
            {

                CancellationTokenSource sourceToken = new CancellationTokenSource();
                CancellationToken token = sourceToken.Token;

                var botClient = new TelegramBotClient("8531549139:AAGbr5w3jVvce4Bj0FvTXItzOXStzKbJn6c");
             
                InMemoryUserRepository inMemoryUserRepository = new();
                InMemoryToDoRepository inMemoryToDoRepository = new();

                UserService userService = new UserService(inMemoryUserRepository);
                ToDoReportService toDoReportService = new ToDoReportService(inMemoryToDoRepository);
                ToDoService toDoService = new(inMemoryToDoRepository);

                var updateHandler = new UpdateHandler(userService, toDoService, toDoReportService);

                var receiverOptions = new ReceiverOptions
                {
                    AllowedUpdates = [UpdateType.Message],
                    DropPendingUpdates = true
                };

                botClient.StartReceiving(updateHandler.HandleUpdateAsync, updateHandler.HandleErrorAsync, receiverOptions, token);
                 var me = await botClient.GetMe();
                Console.WriteLine($"{me.FirstName} запущен!");
                Console.WriteLine("Нажмите А чтобы остановиться");
                if (Console.ReadLine() == "A")
                {
                    sourceToken.Cancel();
                    Environment.Exit(0);
                }
                await Task.Delay(-1); // Устанавливаем бесконечную задержку

            }
            // Когда статический конструктор класса падает с ошибкой
            catch (TypeInitializationException ex)
            {
                Console.WriteLine($"Произошла непредвиденная ошибка {ex.Message}");
                Console.WriteLine($"StackTrace:\n{ex.StackTrace}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine("Inner exception: {0}", ex.InnerException);
                }
            }
            //Когда экземпляр не инициализирован, когда не написал new() в MyApp myapp = new();
            catch (ArgumentNullException ex)
            {
                Console.WriteLine($"Произошла непредвиденная ошибка {ex.Message}");
                Console.WriteLine($"StackTrace:\n{ex.StackTrace}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine("Inner exception: {0}", ex.InnerException);
                }
            }
            //Когда экземпляр написан не верно(?)
            catch (ArgumentException ex)
            {
                Console.WriteLine($"Произошла непредвиденная ошибка {ex.Message}");
            }
            catch (TaskCountLimitException ex)
            {
                Console.WriteLine($"Превышен лимит карт  {ex.Message}");
            }
            catch (TaskLengthLimitException ex)
            {
                Console.WriteLine($"Превышен лимит длины названия карты {ex.Message}");
            }
            catch (DuplicateTaskException ex)
            {
                Console.WriteLine($"Такое название карты уже есть {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Произошла непредвиденная ошибка {ex.Message}");
                Console.WriteLine($"Произошла непредвиденная ошибка:{ex.GetType().Name}");
                Console.WriteLine($"StackTrace:\n{ex.StackTrace}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine("Inner exception: {0}", ex.InnerException);
                }
            }
        }
       
    }   
}
