using System.Collections.Generic;
using System.Collections.ObjectModel;
using NUnit.Framework;

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
        public void Inject_Completes()
        {
            var settings = Mixer.Configure
                .AddAbstract<IList<string>>(new[] {typeof (List<string>)})
                .AddOnTypeFunc<int>(() => 999);

            var testClass = Juiced.Inject<TestClass>(settings);

            Assert.IsNotNull(testClass);
        }
    }
}
