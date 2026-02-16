using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OtusTest3.Core.Exeptions
{
    public class TaskCountLimitException(int count) : Exception($"Превышенно максимальное количество карт{count}");
}
