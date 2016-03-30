using System.Collections.Generic;
using Juiced.Tests.Models.Structs;

namespace Juiced.Tests.Models
{
    public class TestClassChild
    {
        public decimal Decimal { get; set; }

        public IList<string> Items { get; set; }

        public int IntA { get; set; }

        public TestClass Recursion { get; set; }

        public ChildStruct ChildStruct { get; set; }
    }
}