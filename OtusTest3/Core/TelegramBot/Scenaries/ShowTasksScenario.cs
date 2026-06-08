using OtusTest3.Core.DataAccess;
using OtusTest3.Core.Entities;
using OtusTest3.Core.TelegramBot.Dto;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

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
            // Получаем chatId и userId из любого типа обновления
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

            // Достаём ToDoUser из контекста или загружаем
            if (context.Context is not ToDoUser user)
            {
                user = await _userService.GetUserAsync(telegramUserId, ct)
                    ?? await _userService.RegisterUser(telegramUserId, null, ct);
                context.Context = user;
            }

            // Достаём выбранный список из контекста
            Guid? listId = null;
            if (context.Data.TryGetValue("SelectedListId", out var listIdObj) && listIdObj is Guid lid && lid != Guid.Empty)
                listId = lid;

            string listName = context.Data.TryGetValue("SelectedListName", out var nameObj) && nameObj is string n
                ? n
                : "Без списка";

            // Загружаем задачи
            var tasks = await _todoService.GetByUserIdAndList(user.UserId, listId, ct);

            if (tasks.Count == 0)
            {
                await bot.SendMessage(
                    chatId,
                    $"📋 *{EscapeMarkdown(listName)}*\n\nЗадач пока нет.",
                    parseMode: ParseMode.MarkdownV2,
                    cancellationToken: ct);
            }
            else
            {
                var lines = new System.Text.StringBuilder();
                lines.AppendLine($"📋 *{EscapeMarkdown(listName)}* — {tasks.Count} задач(и):\n");

                int idx = 1;
                foreach (var task in tasks)
                {
                    string stateIcon = task.State == ToDoItemState.Active ? "🔲" : "✅";
                    string deadlineStr = task.DeadLine.HasValue && task.DeadLine.Value != DateTime.MaxValue
                        ? $"📅 `{task.DeadLine.Value:dd.MM.yyyy}`"
                        : string.Empty;

                    lines.AppendLine($"{idx}\\. {stateIcon} *{EscapeMarkdown(task.Name)}* {deadlineStr}");
                    idx++;
                }

                await bot.SendMessage(
                    chatId,
                    lines.ToString(),
                    parseMode: ParseMode.MarkdownV2,
                    cancellationToken: ct);
            }

            // Сценарий завершён сразу — просмотр не требует шагов
            return ScenarioResult.Completed;
        }

        private static string EscapeMarkdown(string text)
        {
            // Экранируем спецсимволы MarkdownV2
            return text
                .Replace("_", "\\_")
                .Replace("*", "\\*")
                .Replace("[", "\\[")
                .Replace("]", "\\]")
                .Replace("(", "\\(")
                .Replace(")", "\\)")
                .Replace("~", "\\~")
                .Replace("`", "\\`")
                .Replace(">", "\\>")
                .Replace("#", "\\#")
                .Replace("+", "\\+")
                .Replace("-", "\\-")
                .Replace("=", "\\=")
                .Replace("|", "\\|")
                .Replace("{", "\\{")
                .Replace("}", "\\}")
                .Replace(".", "\\.")
                .Replace("!", "\\!");
        }
    }
}
