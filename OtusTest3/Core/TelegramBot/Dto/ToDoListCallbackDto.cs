using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OtusTest3.Core.TelegramBot.Dto
{
    public class ToDoListCallbackDto
    {
        public string Action { get; set; } = string.Empty;
        public Guid ToDoListId { get; set; }

        public static string ToString(ToDoListCallbackDto dto)
        {
            // Формат: "action|todoListId"
            return $"{dto.Action}|{dto.ToDoListId}";
        }

        public static ToDoListCallbackDto FromString(string input)
        {
            if (input is null)
                throw new ArgumentNullException(nameof(input));

            var separatorIndex = input.IndexOf('|');
            if (separatorIndex < 0)
            {
                return new ToDoListCallbackDto
                {
                    Action = input
                };
            }

            var action = input.Substring(0, separatorIndex);
            var idPart = input.Substring(separatorIndex + 1);

            var ok = Guid.TryParse(idPart, out var id);
            return new ToDoListCallbackDto
            {
                Action = action,
                ToDoListId = ok ? id : Guid.Empty
            };
        }



    }
}
