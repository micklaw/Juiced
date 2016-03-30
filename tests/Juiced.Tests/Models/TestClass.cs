using System.Collections.Generic;
using Juiced.Tests.Models.Abstract;
using Juiced.Tests.Models.Structs;

namespace Juiced.Tests.Models
{
    public class TestClass : ITestClass
    {
        public IList<string> Items { get; set; }

        public int IntA { get; set; }

        public TestClass Recursion { get; set; }

        public ITestClass RecursionAsInterface { get; set; }

        public ChildStruct ChildStruct { get; set; }

        public decimal DecimalValue { get; set; }

        public double DoubleValue { get; set; }

        public byte ByteValue { get; set; }

        public short ShortValue { get; set; }

        public uint UIntValue { get; set; }

        public long LongValue { get; set; }

        public ulong ULongValue { get; set; }

        public bool BoolValue { get; set; }

        public float FloatValue { get; set; }

        public string StringValue { get; set; }

        public string[] StringArrayValue { get; set; }

        public char CharValue { get; set; }
    }
}