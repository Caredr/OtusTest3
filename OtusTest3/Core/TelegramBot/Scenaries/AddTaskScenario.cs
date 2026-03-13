using OtusTest3.Core.DataAccess;
using OtusTest3.Core.Entities;
using OtusTest3.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace OtusTest3.Core.TelegramBot.Scenaries
{
    internal class AddTaskScenario : IScenario
    {
        IUserService _iUserService;
        IToDoService _iTodoService;
        public AddTaskScenario(IUserService iuserService, IToDoService iTodoService)
        {
            _iUserService = iuserService ?? throw new ArgumentNullException(nameof(_iUserService)); 
            _iTodoService = iTodoService ?? throw new ArgumentNullException(nameof(_iTodoService)); 
        }

        public bool CanHandle(ScenarioType scenario)
        {
            return scenario == ScenarioType.AddTask;
        }
        public async Task<ScenarioResult> HandleMessageAsync(ITelegramBotClient bot, ScenarioContext context, Message message, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();
            var user = context.Context!;
            var inputText = message.Text?.Trim();
            
            switch (context.CurrentStep)
            {
                case null:
                    user = await _iUserService.RegisterUser(message.Chat.Id, message?.From?.Username, ct);
                    await bot.SendMessage(message.Chat.Id, "Введите название задачи", cancellationToken: ct);
                    context.CurrentStep = "Name";
                    return ScenarioResult.Transition;
                case "Name":
                    await bot.SendMessage(message.Chat.Id, "Задача добавлена!", cancellationToken: ct);
                    context.CurrentStep = "Deadline";
                    return ScenarioResult.Transition;
                case "Deadline":
                    // Пытаемся распарсить дату
                    if (DateTime.TryParseExact(inputText, "dd.MM.yyyy",
                        null, System.Globalization.DateTimeStyles.None, out DateTime deadline))
                    {
                        // Дата корректна - добавляем задачу
                        var taskName = inputText;
                        await _iTodoService.AddAsync(user, taskName, deadline, ct);
                        await bot.SendMessage(message.Chat.Id,
                            $"Задача {taskName} добавлена, дедлайн {deadline:dd.MM.yyyy}!",
                            cancellationToken: ct);
                        return ScenarioResult.Completed;
                    }
                    else
                    {
                        // Неверный формат даты - запрашиваем снова
                        await bot.SendMessage(message.Chat.Id,
                            "Неверный формат даты. Введите дату в формате ДД.ММ.ГГГГ",
                            cancellationToken: ct);
                        return ScenarioResult.Transition;
                    }

                default:
                    await bot.SendMessage(message.Chat.Id, "Неизвестный шаг", cancellationToken: ct);
                    return ScenarioResult.Completed;
            }
        }
    }
}
