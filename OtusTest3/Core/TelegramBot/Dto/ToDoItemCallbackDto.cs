using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OtusTest3.Core.TelegramBot.Dto
{
    internal class ToDoItemCallbackDto : CallbackDto
    {
        public string Action { get; set; } = string.Empty;
        public Guid ToDoItemId { get; set; }
        public static string ToString(ToDoItemCallbackDto dto)
        {
            // Формат: "action|todoListId"
            ArgumentNullException.ThrowIfNull(dto);
            return $"{dto.Action}|{dto.ToDoItemId}";
        }
        public static ToDoItemCallbackDto FromString(string input)
        {
            if (input is null)
                throw new ArgumentNullException(nameof(input));
            var separatorIndex = input.IndexOf('|');
            if (separatorIndex < 0)
            {
                return new ToDoItemCallbackDto
                {
                    Action = input
                };
            }
            var action = input[..separatorIndex];
            var idPart = input[(separatorIndex + 1)..];
            var ok = Guid.TryParse(idPart, out var id);
            return new ToDoItemCallbackDto
            {
                Action = action,
                ToDoItemId = ok ? id : Guid.Empty
            };
        }
    }
}
