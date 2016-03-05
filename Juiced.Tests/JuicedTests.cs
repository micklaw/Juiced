using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using NUnit.Framework;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Threading;
using Juiced.Tests.Models;
using Juiced.Tests.Models.Abstract;

namespace Juiced.Tests
{
    [TestFixture]
    public class JuicedTests
    {
        [TestCase(0)]
        [TestCase(1)]
        [TestCase(2)]
        public void Inject_RecursionLimit(int recursion)
        {
            var settings = Mixer.Configure.SetRecursion(recursion);

            var result = Juiced.HydrateAsync<TestClass>(settings).Result;

            TestClass testClass = result.Recursion;

            for (var i = 0; i <= recursion; i++)
            {
                if (i== recursion)
                {
                    Assert.IsNull(testClass);
                    continue;
                }

                Assert.IsNotNull(testClass);

                testClass = testClass.Recursion;
            }
        }

        [Test]
        public void HydrateAsync_UnhandlesExceptionsThrows()
        {
            var settings = Mixer.Configure.OnType<double>(() =>
                {
                    throw new InvalidCastException("This is an invalid cast exception");
                });

            var exception = Assert.Throws<AggregateException>(() => { var i = Juiced.HydrateAsync<double>(settings).Result; });

            Assert.AreEqual(exception.InnerExceptions[0].GetType(), typeof(InvalidCastException));
        }

        [TestCase(false, TestName = "Returns Juiced Exception when handled and advised to throw instead of suppress")]
        [TestCase(true, TestName = "Does not throw exceptions when handled and advised to suppress errors")]
        public void HydrateAsync_HandledExceptions(bool suppressExceptions)
        {
            var settings = Mixer.Configure.OnType<double>(() =>
            {
                throw new InvalidCastException("This is an invalid cast exception");
            });

            settings.OnError = (type, handledException) => suppressExceptions;

            double instance = 0;

            if (!suppressExceptions)
            {
                var exception = Assert.Throws<AggregateException>(() => { instance = Juiced.HydrateAsync<double>(settings).Result; });
                var firstInnerException = exception.InnerExceptions[0];
                var secondInner = firstInnerException.InnerException;

                Assert.AreEqual(firstInnerException.GetType(), typeof(JuicedException));
                Assert.AreEqual(secondInner.GetType(), typeof(InvalidCastException));

                return;
            }

            Assert.AreEqual(instance, 0);
        }

        [Test]
        public void HydrateAsync_AsValueType()
        {
            var settings = Mixer.Configure.OnType<double>(() => 999.0d);

            var result = Juiced.HydrateAsync<double>(settings).Result;

            Assert.AreEqual(result, 999.0d);
        }

        [Test]
        public async void HydrateAsync_AbstractRegistrationCompletes()
        {
            var settings = Mixer.Configure
                .MapAbstract<ITestClass>(new[] {typeof (TestClass)})
                .SetRecursion(1);

            TestClass testClass = null;

            var result = Juiced.HydrateAsync<TestClass>(settings);

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
            Assert.IsNotNull(testClass.RecursionAsInterface);
        }

        [Test]
        public async void HydrateAsync_WorksAsync()
        {
            var settings = Mixer.Configure
                .OnType<int>(() => 999);

            TestClass testClass = null;

            var result = Juiced.HydrateAsync<TestClass>(settings);

            Trace.WriteLine("Doing some Async stuff here !");
            Trace.WriteLine("Wee bit more !");

            await result.ContinueWith((continuation) =>
             {
                 if (!continuation.IsFaulted)
                 {
                     testClass = continuation.Result;
                 }

                 Trace.WriteLine("Juiced successfully !");
             });

            Assert.IsNotNull(testClass);
            Assert.AreEqual(testClass.BoolValue, false);
            Assert.AreEqual(testClass.ByteValue, new byte());
            Assert.IsNotNull(testClass.ChildStruct);
            Assert.AreEqual(testClass.ChildStruct.IntB, 999);
            Assert.AreEqual(testClass.DecimalValue, 1.1M);
            Assert.AreEqual(testClass.DoubleValue, 1.1D);
            Assert.AreEqual(testClass.UIntValue, 1U);
            Assert.AreEqual(testClass.IntA, 999);
            Assert.IsNotNull(testClass.Items);
            Assert.AreEqual(testClass.LongValue, 1L);
            Assert.IsNull(testClass.Recursion);
            Assert.AreEqual(testClass.ShortValue, (short)1);
            Assert.AreEqual(testClass.ULongValue, 1UL);
            Assert.AreEqual(testClass.FloatValue, 1.1F);
            Assert.AreEqual(testClass.CharValue, new char());
            Assert.IsNotNull(testClass.StringArrayValue);
            Assert.IsNotNullOrEmpty(testClass.StringValue);
            Assert.DoesNotThrow(() => new Guid(testClass.StringValue));
        }
    }
}
