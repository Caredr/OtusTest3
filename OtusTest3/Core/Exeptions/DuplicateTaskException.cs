using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OtusTest3.Core.Exeptions
{
    public class DuplicateTaskException(string task) : Exception($"Такая {task} уже существует");
}
