using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using Mercurial.Attributes;
using NUnit.Framework;

namespace Mercurial.Tests
{
    [TestFixture]
    public class CollectionPropertiesTests
    {
        public IEnumerable<object[]> AllCollectionProperties()
        {
            return
                from type in typeof(CloneCommand).Assembly.GetTypes()
                where type.Name.EndsWith("Command")
                   && !type.Name.Contains("Gui")
                from property in type.GetProperties()
                where property.PropertyType.IsGenericType
                where property.PropertyType.GetGenericTypeDefinition() == typeof(Collection<>)
                   || property.PropertyType.GetGenericTypeDefinition() == typeof(List<>)
                select new object[] { type, property };
        }

        [Test]
        [TestCaseSource("AllCollectionProperties")]
        [Category("API")]
        public void ShouldNotHaveNullableArgumentAttribute(Type type, PropertyInfo property)
        {
            Assert.That(property.IsDefined(typeof(NullableArgumentAttribute), true), Is.False, type.FullName + "." + property.Name + " is collection, should not have NullableArgument attribute, but instead RepeatableArgument");
        }

        [Test]
        [TestCaseSource("AllCollectionProperties")]
        [Category("API")]
        public void ShouldBeGenericCollectionNotGenericList(Type type, PropertyInfo property)
        {
            Assert.That(property.PropertyType.GetGenericTypeDefinition(), Is.EqualTo(typeof(Collection<>)), type.FullName + "." + property.Name + " is List<>, should be Collection<>");
        }
    }
}
