using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskFlow.Domain.Enums
{
    public enum Status
    {
        New = 0,
        Open = 1,
        InProgress = 2,
        Completed = 3,
        Closed = 4
    }
}
