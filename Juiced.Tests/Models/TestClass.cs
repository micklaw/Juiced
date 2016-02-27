using System.Collections.Generic;

namespace Juiced.Tests.Models
{
    public class TestClass
    {
        public IList<string> Items { get; set; }

        public int IntA { get; set; }

        public TestClass Recursion { get; set; }
    }
}