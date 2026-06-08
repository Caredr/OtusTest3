using OtusTest3.Core.DataAccess;
using OtusTest3.Core.Exeptions;
using OtusTest3.Core.Infrastructure.DataAccess;
using OtusTest3.Core.Services;
using OtusTest3.Core.TelegramBot;
using OtusTest3.Core.TelegramBot.Scenaries;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace OtusTest3
{
    internal class Program
    {
        private static async Task Main(string[] args)
        {
            ArgumentNullException.ThrowIfNull(args);
            try
            {
                CancellationTokenSource sourceToken = new CancellationTokenSource();
                CancellationToken token = sourceToken.Token;

                var botClient = new TelegramBotClient("Token");
                var userRepo = new FileUserRepository("data/users");
                var toDoRepo = new FileToDoRepository("data/todos");

                UserService userService = new UserService(userRepo);
                ToDoReportService toDoReportService = new ToDoReportService(toDoRepo);
                ToDoService toDoService = new(toDoRepo);
                ToDoListService toDoListService = new ToDoListService(toDoService);

                var scenarios = new List<IScenario>
                {
                    new AddTaskScenario(userService, toDoService, toDoListService),
                    new AddListScenario(userService, toDoListService),
                    new DeleteListScenario(userService, toDoListService),
                    new ShowTasksScenario(toDoService, userService),
                };

                InMemoryScenarioContextRepository contextRepo = new();
                var updateHandler = new UpdateHandler(
                            userService,
                            toDoService,
                            toDoReportService,
                            scenarios,
                            contextRepo,
                            toDoListService);

                var receiverOptions = new ReceiverOptions
                {
                    AllowedUpdates = [UpdateType.Message, UpdateType.CallbackQuery],
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
                await Task.Delay(-1);
            }
            catch (TypeInitializationException ex)
            {
                Console.WriteLine($"Произошла непредвиденная ошибка {ex.Message}");
                Console.WriteLine($"StackTrace:\n{ex.StackTrace}");
                if (ex.InnerException != null)
                    Console.WriteLine("Inner exception: {0}", ex.InnerException);
            }
            catch (ArgumentNullException ex)
            {
                Console.WriteLine($"Произошла непредвиденная ошибка {ex.Message}");
                Console.WriteLine($"StackTrace:\n{ex.StackTrace}");
                if (ex.InnerException != null)
                    Console.WriteLine("Inner exception: {0}", ex.InnerException);
            }
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
                    Console.WriteLine("Inner exception: {0}", ex.InnerException);
            }
        }
    }
}
