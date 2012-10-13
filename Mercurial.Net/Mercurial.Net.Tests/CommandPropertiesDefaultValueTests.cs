using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using NUnit.Framework;

namespace Mercurial.Tests
{
    [TestFixture]
    public class CommandPropertiesDefaultValueTests
    {
        public IEnumerable<Type> CommandClassTypes()
        {
            return from type in typeof(Repository).Assembly.GetTypes()
                   where
                       !type.IsAbstract && type.BaseType != null && type.BaseType.IsGenericType &&
                       (type.BaseType.GetGenericTypeDefinition() == typeof(MercurialCommandBase<>) ||
                        type.BaseType.GetGenericTypeDefinition() == typeof(IncludeExcludeCommandBase<>)) && !type.IsGenericType
                   select type;
        }

        public IEnumerable<object[]> CommandClassTypesWithProperties()
        {
            IEnumerable<Type> types = from type in typeof(Repository).Assembly.GetTypes()
                                      where
                                          !type.IsAbstract && type.BaseType != null && type.BaseType.IsGenericType &&
                                          (type.BaseType.GetGenericTypeDefinition() == typeof(MercurialCommandBase<>) ||
                                           type.BaseType.GetGenericTypeDefinition() == typeof(IncludeExcludeCommandBase<>)) && !type.IsGenericType
                                      select type;

            return from type in types
                   from prop in type.GetProperties(BindingFlags.Instance | BindingFlags.Public)
                   where
                       prop.Name != "Result" && prop.CanRead && prop.CanWrite &&
                       (!prop.PropertyType.IsGenericType ||
                        (prop.PropertyType.IsGenericType && prop.PropertyType.GetGenericTypeDefinition() != typeof(Collection<>) &&
                         prop.PropertyType.GetGenericTypeDefinition() != typeof(IEnumerable<>)))
                   select new object[]
                   {
                       type, prop
                   };
        }

        [TestCaseSource("CommandClassTypesWithProperties")]
        [Test]
        [NUnit.Framework.Category("API")]
        public void PropertiesHasDefaultValuesAttribute(Type optionsClassType, PropertyInfo property)
        {
            Assert.That(property.IsDefined(typeof(DefaultValueAttribute), true), Is.True);
        }

        [TestCaseSource("CommandClassTypesWithProperties")]
        [Test]
        [NUnit.Framework.Category("API")]
        public void PropertyGetsCorrectDefaultValueUponConstruction(Type type, PropertyInfo property)
        {
            object original;
            try
            {
                try
                {
                    original = CreateInstance(type);
                }
                catch (TargetInvocationException ex)
                {
                    throw ex.InnerException;
                }
            }
            catch (NotSupportedException)
            {
                return;
            }
            object defaultPropertyValue = property.GetValue(original, null);

            var attr = property.GetCustomAttributes(typeof(DefaultValueAttribute), true).FirstOrDefault() as DefaultValueAttribute;
            if (attr == null)
                return; // another unit-test fails on this
            object expectedPropertyValue = attr.Value;
            Assert.That(defaultPropertyValue, Is.EqualTo(expectedPropertyValue));
        }

        private object CreateInstance(Type type)
        {
            if (type == typeof(CustomCommand))
                return new CustomCommand("command");

            return Activator.CreateInstance(type);
        }
    }
}