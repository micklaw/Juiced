using System;
using Juiced.Tests.Models;
using Juiced.Tests.Models.Abstract;
using Xunit;

namespace Juiced.Tests
{
    public class JuicedTests
    {
        [Theory]
        [InlineData(3)]
        [InlineData(5)]
        [InlineData(6)]
        public void Inject_RecursionLimit(int recursion)
        {
            var settings = Mixer.Configure.SetRecursion(recursion);

            var result = Juiced.HydrateAsync<TestClass>(settings).Result;

            var testClass = result.Recursion;

            for (var i = 0; i <= recursion; i++)
            {
                if (i== recursion)
                {
                    Assert.Null(testClass);
                    continue;
                }

                Assert.NotNull(testClass);

                testClass = testClass.Recursion;
            }
        }

        [Fact]
        public void HydrateAsync_UnhandlesExceptionsThrows()
        {
            var settings = Mixer.Configure.OnType<double>(() =>
            {
                throw new InvalidCastException("This is an invalid cast exception");
            });

            var exception = Assert.Throws<AggregateException>(() => { var i = Juiced.HydrateAsync<double>(settings).Result; });

            Assert.Equal(exception.InnerExceptions[0].GetType(), typeof(InvalidCastException));
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
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

                Assert.Equal(firstInnerException.GetType(), typeof(JuicedException));
                Assert.Equal(secondInner.GetType(), typeof(InvalidCastException));

                return;
            }

            Assert.Equal(instance, 0);
        }

        [Fact]
        public void HydrateAsync_AsValueType()
        {
            var settings = Mixer.Configure.OnType<double>(() => 999.0d);

            var result = Juiced.HydrateAsync<double>(settings).Result;

            Assert.Equal(result, 999.0d);
        }

        [Fact]
        public async void HydrateAsync_AbstractRegistrationCompletes()
        {
            var settings = Mixer.Configure
                .MapAbstract<ITestClass>(new[] {typeof (TestClass)})
                .SetRecursion(1);

            TestClass testClass = null;

            var result = Juiced.HydrateAsync<TestClass>(settings);

            await result.ContinueWith((continuation) =>
            {
                if (!continuation.IsFaulted)
                {
                    testClass = continuation.Result;
                }
            });

            Assert.NotNull(testClass);
            Assert.NotNull(testClass.RecursionAsInterface);
        }

        [Fact]
        public async void HydrateAsync_WorksAsync()
        {
            var settings = Mixer.Configure.OnType<int>(() => 999);

            TestClass testClass = null;

            var result = Juiced.HydrateAsync<TestClass>(settings);

            await result.ContinueWith((continuation) =>
             {
                 if (!continuation.IsFaulted)
                 {
                     testClass = continuation.Result;
                 }
             });

            Assert.NotNull(testClass);
            Assert.Equal(testClass.BoolValue, false);
            Assert.Equal(testClass.ByteValue, new byte());
            Assert.NotNull(testClass.ChildStruct);
            Assert.Equal(testClass.ChildStruct.IntB, 999);
            Assert.Equal(testClass.DecimalValue, 1.1M);
            Assert.Equal(testClass.DoubleValue, 1.1D);
            Assert.Equal(testClass.UIntValue, 1U);
            Assert.Equal(testClass.IntA, 999);
            Assert.NotNull(testClass.Items);
            Assert.Equal(testClass.LongValue, 1L);
            Assert.Null(testClass.Recursion);
            Assert.Equal(testClass.ShortValue, (short)1);
            Assert.Equal(testClass.ULongValue, 1UL);
            Assert.Equal(testClass.FloatValue, 1.1F);
            Assert.Equal(testClass.CharValue, new char());
            Assert.NotNull(testClass.StringArrayValue);
            Assert.NotNull(testClass.StringValue);
            Assert.NotEmpty(testClass.StringValue);
        }
    }
}
