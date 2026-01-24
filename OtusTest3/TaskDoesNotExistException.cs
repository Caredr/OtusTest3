using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OtusTest3
{
    internal class TaskDoesNotExistException(string description) : Exception(description);
}
