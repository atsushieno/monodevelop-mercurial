using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using NUnit.Framework;

namespace Mercurial.Tests
{
    [TestFixture]
    public class FluentInterfaceTests
    {
        public static IEnumerable<object[]> AllFluentInterfaceMethodsWithMatchingProperties()
        {
            IEnumerable<Type> types =
                from type in typeof(AddCommand).Assembly.GetTypes()
                where
                    !type.IsAbstract && !type.IsGenericType && type.BaseType.IsGenericType &&
                    (type.BaseType.GetGenericTypeDefinition() == typeof(MercurialCommandBase<>) ||
                     type.BaseType.GetGenericTypeDefinition() == typeof(IncludeExcludeCommandBase<>))
                select type;

            var methods =
                from type in types
                from method in type.GetMethods(BindingFlags.Instance | BindingFlags.Public)
                where method.Name.StartsWith("With") && method.Name != "WithConfigurationOverride"
                      && !method.IsDefined(typeof(ObsoleteAttribute), true)
                select new
                {
                    type,
                    method
                };

            IEnumerable<object[]> methodsWithMatchingProperties =
                from entry in methods
                let property1 = entry.type.GetProperty(entry.method.Name.Substring(4))
                let property2 =
                    entry.method.DeclaringType.GetProperty(entry.method.Name.Substring(4) + "s")
                let property3 =
                    entry.method.DeclaringType.GetProperty(entry.method.Name.Substring(4) + "es")
                select new object[]
                {
                    entry.type, entry.method, property1 ?? property2 ?? property3
                };

            return methodsWithMatchingProperties;
        }

        [TestCaseSource("AllFluentInterfaceMethodsWithMatchingProperties")]
        [Test]
        [Category("API")]
        public void AllFluentInterfaceMethods_MatchAProperty(Type type, MethodInfo method, PropertyInfo property)
        {
            Assert.That(property, Is.Not.Null);
        }

        [TestCaseSource("AllFluentInterfaceMethodsWithMatchingProperties")]
        [Test]
        [Category("API")]
        public void AllFluentInterfaceMethods_SetsThePropertyValue(Type type, MethodInfo method, PropertyInfo property)
        {
            try
            {
                try
                {
                    if (property == null)
                        return;

                    if (property.PropertyType == typeof(string))
                        CheckStringMethod(property, type, method);
                    else if (property.PropertyType == typeof(bool))
                        CheckBooleanMethod(property, type, method);
                    else if (property.PropertyType == typeof(int))
                        CheckInt32Method(property, type, method);
                    else if (property.PropertyType == typeof(int?))
                        CheckNullableInt32Method(property, type, method);
                    else if (property.PropertyType == typeof(DateTime?))
                        CheckNullableDateTimeMethod(property, type, method);
                    else if (property.PropertyType.IsEnum)
                        CheckEnumMethod(property, type, method);
                    else if (property.PropertyType == typeof(RevSpec))
                        CheckRevSpecMethod(property, type, method);
                    else if (property.PropertyType == typeof(Collection<string>))
                        CheckStringCollectionMethod(property, type, method);
                    else if (property.PropertyType == typeof(Collection<RevSpec>))
                        CheckRevSpecCollectionMethod(property, type, method);
                    else if (property.PropertyType == typeof(IMercurialCommandObserver))
                        CheckObserverMethod(property, type, method);
                    else
                        Assert.Fail("Don't know how to verify fluent interface for property type " + property.PropertyType.Name);
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
        }

        private static void CheckObserverMethod(PropertyInfo property, Type type, MethodInfo method)
        {
            var input = new[]
            {
                new DebugObserver(), null
            };

            foreach (DebugObserver observer in input)
            {
                object instance = CreateInstance(type);
                method.Invoke(
                    instance, new object[]
                    {
                        observer
                    });
                object output = property.GetValue(instance, null);

                Assert.That(output, Is.SameAs(observer));
            }
        }

        private static void CheckRevSpecCollectionMethod(PropertyInfo property, Type type, MethodInfo method)
        {
            var input = new[]
            {
                RevSpec.Single(42), RevSpec.Range(RevSpec.Single(17), RevSpec.Single(42))
            };

            foreach (RevSpec value in input)
            {
                object instance = CreateInstance(type);
                method.Invoke(
                    instance, new object[]
                    {
                        value
                    });
                var output = (Collection<RevSpec>)property.GetValue(instance, null);

                CollectionAssert.AreEqual(
                    output, new[]
                    {
                        value
                    });
            }
        }

        private static object CreateInstance(Type type)
        {
            if (type == typeof(CustomCommand))
                return new CustomCommand("command");

            return Activator.CreateInstance(type);
        }

        private static void CheckStringCollectionMethod(PropertyInfo property, Type type, MethodInfo method)
        {
            var input = new[]
            {
                "a", "b", "c"
            };

            foreach (string value in input)
            {
                object instance = CreateInstance(type);
                if (method.GetParameters()[0].ParameterType == typeof(string[]))
                {
                    method.Invoke(
                        instance, new object[]
                        {
                            new[]
                            {
                                value
                            }
                        });
                }
                else
                {
                    method.Invoke(
                        instance, new object[]
                        {
                            value
                        });
                }
                var output = (Collection<string>)property.GetValue(instance, null);

                CollectionAssert.AreEqual(
                    output, new[]
                    {
                        value
                    });
            }
        }

        private static void CheckRevSpecMethod(PropertyInfo property, Type type, MethodInfo method)
        {
            var input = new[]
            {
                RevSpec.Single(42), RevSpec.Range(RevSpec.Single(17), RevSpec.Single(42))
            };

            foreach (RevSpec value in input)
            {
                object instance = CreateInstance(type);
                method.Invoke(
                    instance, new object[]
                    {
                        value
                    });
                object output = property.GetValue(instance, null);

                Assert.That(output, Is.EqualTo(value));
            }
        }

        private static void CheckEnumMethod(PropertyInfo property, Type type, MethodInfo method)
        {
            var input = Enum.GetValues(property.PropertyType).Cast<object>().ToList();

            Assert.That(input.Count, Is.GreaterThan(0));

            foreach (object value in input)
            {
                object instance = CreateInstance(type);
                method.Invoke(
                    instance, new[]
                    {
                        value
                    });
                object output = property.GetValue(instance, null);

                Assert.That(output, Is.EqualTo(value));
            }
        }

        private static void CheckNullableDateTimeMethod(PropertyInfo property, Type type, MethodInfo method)
        {
            var input = new[]
            {
                new DateTime(2010, 1, 1), new DateTime(2005, 7, 11, 18, 0, 37)
            };

            foreach (DateTime value in input)
            {
                object instance = CreateInstance(type);
                method.Invoke(
                    instance, new object[]
                    {
                        value
                    });
                object output = property.GetValue(instance, null);

                Assert.That(output, Is.EqualTo(value));
            }
        }

        private static void CheckNullableInt32Method(PropertyInfo property, Type type, MethodInfo method)
        {
            var input = new[]
            {
                10, 42, 99
            };

            foreach (int value in input)
            {
                object instance = CreateInstance(type);
                method.Invoke(
                    instance, new object[]
                    {
                        value
                    });
                object output = property.GetValue(instance, null);

                Assert.That(output, Is.EqualTo(value));
            }
        }

        private static void CheckInt32Method(PropertyInfo property, Type type, MethodInfo method)
        {
            var input = new[]
            {
                10, 42, 177
            };
            var expected = new[]
            {
                10, 42, 177
            };

            for (int index = 0; index < input.Length; index++)
            {
                object instance = CreateInstance(type);
                method.Invoke(
                    instance, new object[]
                    {
                        input[index]
                    });
                object output = property.GetValue(instance, null);

                Assert.That(output, Is.EqualTo(expected[index]));
            }
        }

        private static void CheckBooleanMethod(PropertyInfo property, Type type, MethodInfo method)
        {
            var input = new[]
            {
                false, true
            };
            var expected = new[]
            {
                false, true
            };

            for (int index = 0; index < input.Length; index++)
            {
                object instance = CreateInstance(type);
                if (!SetValue(method, instance, input[index]))
                    continue;
                object output = property.GetValue(instance, null);

                Assert.That(output, Is.EqualTo(expected[index]));
            }
        }

        private static void CheckStringMethod(PropertyInfo property, Type type, MethodInfo method)
        {
            var input = new[]
            {
                null, string.Empty, " \r \t \n", "dummy"
            };
            var expected = new[]
            {
                string.Empty, string.Empty, string.Empty, "dummy"
            };

            for (int index = 0; index < input.Length; index++)
            {
                object instance = CreateInstance(type);
                object value = input[index];
                if (!SetValue(method, instance, value))
                    continue;
                object output = property.GetValue(instance, null);

                Assert.That(output, Is.EqualTo(expected[index]));
            }
        }

        // ReSharper disable UndocumentedThrownException
        private static bool SetValue(MethodInfo method, object instance, object value)
        {
            try
            {
                try
                {
                    method.Invoke(
                        instance, new[]
                        {
                            value
                        });
                }
                catch (TargetInvocationException ex)
                {
                    if (ex.InnerException != null)
                        throw ex.InnerException;
                    throw;
                }
            }
            catch (NotSupportedException ex)
            {
                if (ex.Message.Contains(" requires version "))
                    return false;
                throw;
            }
            return true;
        }
        // ReSharper restore UndocumentedThrownException
    }
}