using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NUnit.Framework;

namespace Mercurial.Tests
{
    [TestFixture]
    [Category("Internal")]
    public class TestTests
    {
        public IEnumerable<object[]> Test_HasCategory_TestCases()
        {
            return
                from type in typeof(TestTests).Assembly.GetTypes()
                where type.IsDefined(typeof(TestFixtureAttribute), true)
                from method in type.GetMethods()
                where method.IsDefined(typeof(TestAttribute), true)
                      || method.IsDefined(typeof(TestCaseAttribute), true)
                      || method.IsDefined(typeof(TestCaseSourceAttribute), true)
                select new object[] { type, method };
        }

        [TestCaseSource("Test_HasCategory_TestCases")]
        [Test]
        public void Test_HasCategory(Type testFixture, MethodInfo testMethod)
        {
            Assert.That(testFixture.IsDefined(typeof(CategoryAttribute), true) || testMethod.IsDefined(typeof(CategoryAttribute), true), Is.True);
        }

        [TestCaseSource("Test_HasCategory_TestCases")]
        [Test]
        public void Test_HasTestAttribute(Type testFixture, MethodInfo testMethod)
        {
            Assert.That(testMethod.IsDefined(typeof(TestAttribute), true), Is.True);
        }
    }
}