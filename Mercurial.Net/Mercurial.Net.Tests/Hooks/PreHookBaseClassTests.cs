using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mercurial.Hooks;
using NUnit.Framework;

namespace Mercurial.Tests.Hooks
{
    [TestFixture]
    [Category("API")]
    public class PreHookBaseClassTests
    {
        public IEnumerable<Type> HookClasses()
        {
            return
                from type in typeof(MercurialHookBase).Assembly.GetTypes()
                where !type.IsAbstract
                   && !type.IsGenericType
                   && type.Name.EndsWith("Hook")
                select type;
        }

        public IEnumerable<Type> PreHookClasses()
        {
            return
                from type in HookClasses()
                where type.Name.Contains("Pre")
                select type;
        }

        [Test]
        [TestCaseSource("PreHookClasses")]
        public void AllPreHooksMustDescendFromMercurialControllingHookBase(Type hookType)
        {
            Assert.That(typeof(MercurialControllingHookBase).IsAssignableFrom(hookType), Is.True);
        }
    }
}
