using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Juiced.Tests.Models
{
    public struct ChildStruct
    {
        public readonly int IntB;

        public ChildStruct(int intB)
        {
            IntB = intB;
        }
    }
}
