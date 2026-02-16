
using Otus.ToDoList.ConsoleBot;
using OtusTest3.Core.DataAccess;
using OtusTest3.Core.Exeptions;
using OtusTest3.Core.Infrastructure.DataAccess;
using OtusTest3.Core.Services;
using OtusTest3.Core.TelegramBot;
using System.Threading.Tasks;

namespace OtusTest3
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            ArgumentNullException.ThrowIfNull(args);
            //Задаю макс кол-во задач, считываю, идет преобразование.
            try
            {
                CancellationTokenSource source = new CancellationTokenSource();
                CancellationToken token = source.Token;
                ConsoleBotClient botClient = new();         
                InMemoryUserRepository inMemoryUserRepository = new();
                InMemoryToDoRepository inMemoryToDoRepository = new();
                UserService UserService = new UserService(inMemoryUserRepository);
                ToDoReportService toDoReportService = new ToDoReportService(inMemoryToDoRepository);
                ToDoService toDoService = new(inMemoryToDoRepository);
                botClient.StartReceiving(new UpdateHandler(UserService, toDoService, toDoReportService), token);
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
