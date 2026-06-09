using OtusTest3.Core.DataAccess;
using OtusTest3.Core.Entities;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace OtusTest3.Core.TelegramBot.Scenaries
{
    /// <summary>
    /// Сценарий просмотра задач выбранного списка (или "Без списка").
    /// Запускается через callback кнопки show|{listId} из /show.
    /// </summary>
    internal class ShowTasksScenario : IScenario
    {
        private readonly IToDoService _todoService;
        private readonly IUserService _userService;

        public ShowTasksScenario(IToDoService todoService, IUserService userService)
        {
            _todoService = todoService ?? throw new ArgumentNullException(nameof(todoService));
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
        }

        public bool CanHandle(ScenarioType scenario) => scenario == ScenarioType.ShowTasks;

        public async Task<ScenarioResult> HandleMessageAsync(
            ITelegramBotClient bot,
            ScenarioContext context,
            Update update,
            CancellationToken ct)
        {
            long chatId;
            long telegramUserId;

            if (update.CallbackQuery is { } cq)
            {
                chatId = cq.Message!.Chat.Id;
                telegramUserId = cq.From.Id;
            }
            else if (update.Message is { } msg)
            {
                chatId = msg.Chat.Id;
                telegramUserId = msg.From!.Id;
            }
            else
            {
                return ScenarioResult.Completed;
            }

            // Загружаем пользователя если нет в контексте
            if (context.Context is not ToDoUser user)
            {
                user = await _userService.GetUserAsync(telegramUserId, ct)
                    ?? await _userService.RegisterUser(telegramUserId, null, ct);
                context.Context = user;
            }

            // Достаём выбранный список из контекста
            Guid? listId = null;
            if (context.Data.TryGetValue("SelectedListId", out var listIdObj)
                && listIdObj is Guid lid && lid != Guid.Empty)
                listId = lid;

            string listName = context.Data.TryGetValue("SelectedListName", out var nameObj) && nameObj is string n
                ? n
                : "Без списка";

            // Загружаем задачи
            var tasks = await _todoService.GetByUserIdAndList(user.UserId, listId, ct);

            string message;

            if (tasks.Count == 0)
            {
                message = $"Список: {listName}\n\nЗадач пока нет.";
            }
            else
            {
                var sb = new System.Text.StringBuilder();
                sb.AppendLine($"Список: {listName} ({tasks.Count} задач(и))");
                sb.AppendLine();

                int idx = 1;
                foreach (var task in tasks)
                {
                    string state = task.State == ToDoItemState.Active ? "[ ]" : "[x]";

                    string deadline = task.DeadLine.HasValue
                        ? $"  до {task.DeadLine.Value:dd.MM.yyyy}"
                        : string.Empty;

                    sb.AppendLine($"{idx}. {state} {task.Name}{deadline}");
                    idx++;
                }

                message = sb.ToString();
            }

            await bot.SendMessage(chatId, message, cancellationToken: ct);

            return ScenarioResult.Completed;
        }
    }
}
