using System.Collections.Generic;
using System.Collections.ObjectModel;
using NUnit.Framework;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Threading;

namespace Juiced.Tests
{
    public class TestClass
    {
        public IList<string> Items { get; set; }

        public int IntA { get; set; }
    }

    [TestFixture]
    public class JuicedTests
    {
        [Test]
        public void Inject_ValueType()
        {
            var result = Juiced.Inject<int>();

            Assert.AreEqual(result, 1);
        }

        [Test]
        public async void Inject_Completes()
        {
            var settings = Mixer.Configure
                                .MapAbstract<IList<string>>(new[] { typeof(List<string>) })
                                .OnType<int>(() => 999);

            TestClass testClass = null;

            var result = Juiced.InjectAsync<TestClass>(settings);

            Thread.Sleep(200);
            Trace.WriteLine("Doing some Async stuff here !");
            Thread.Sleep(600);
            Trace.WriteLine("Wee bit more !");
            Thread.Sleep(200);

            await result.ContinueWith((continuation) =>
             {
                 if (!continuation.IsFaulted)
                 {
                     testClass = continuation.Result;
                 }

                 Trace.WriteLine("Juiced successfully !");
             });

            Assert.IsNotNull(testClass);
            Assert.IsNotNull(testClass.Items);
            Assert.AreEqual(testClass.IntA, 999);
        }
    }
}
