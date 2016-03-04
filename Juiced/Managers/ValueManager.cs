using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;

using static Juiced.RecursionManager;
using static Juiced.ErrorManager;
using System.Collections;
using System.Collections.ObjectModel;

namespace Juiced
{
    /// <summary>
    /// Return default values for a given type
    /// </summary>
    internal class ValueManager
    {
        /// <summary>
        /// Get a default value for a type
        /// </summary>
        /// <param name="type"></param>
        /// <param name="identifier"></param>
        /// <param name="mixer"></param>
        /// <returns></returns>
        internal static object GetValue(Type type, Guid identifier, Mixer mixer)
        {
            object value = null;

            try
            {
                PushTo(identifier, type);

                // ML - Attempt to grab from our delegate creation list

                Func<object> getObject;

                if (mixer.TryGetOnTypeFunc(type, out getObject))
                {
                    value = getObject();
                }

                // ML - If not create, then tryour value types and .Net types etc

                if (value == null)
                {
                    if (type.IsEnum)
                    {
                        value = Enum.GetValues(type).GetValue(1);
                    }

                    if (type.IsArray)
                    {
                        value = Activator.CreateInstance(type, new object[] { 1 });
                    }

                    if (type.IsGenericType && typeof(IEnumerable).IsAssignableFrom(type))
                    {
                        var isGenericList = (type.GetGenericTypeDefinition() == typeof(IList<>) || type.GetGenericTypeDefinition() == typeof(List<>));
                        var isGenericCollection = (type.GetGenericTypeDefinition() == typeof(ICollection<>) || type.GetGenericTypeDefinition() == typeof(Collection<>));
                        var genericType = isGenericList || isGenericCollection ? type.GetGenericArguments().FirstOrDefault() : type;

                        var constructedListType = (isGenericList ? typeof(List<>) : typeof(Collection<>)).MakeGenericType(genericType);

                        value = Activator.CreateInstance(constructedListType);
                    }

                    if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
                    {
                        var underlyingType = Nullable.GetUnderlyingType(type);

                        value = GetValue(underlyingType, identifier, mixer);
                    }
                }

                // ML - If its an abstract type try to create from our mappings

                if (value == null && type.IsAbstract)
                {
                    Type[] concreteTypes;
                    if (!mixer.TryGetAbstract(type, out concreteTypes))
                    {
                        throw new Exception("Must be a non abstract type if not a list.");
                    }

                    value = Construct(concreteTypes.GetRandom(), identifier, mixer);
                }

                // ML - Or just try

                if (value == null)
                {
                    value = type.IsValueType ? Activator.CreateInstance(type) : Construct(type, identifier, mixer);
                }
            }
            catch (Exception exception)
            {
                HandleError(Locations.GetValue, type, exception, mixer.OnError);
            }
            finally
            {
                PopTo(identifier);
            }

            // ML - Value type would never error, so this must be a reference type, so give it null

            return value;
        }

        /// <summary>
        /// Set the child properties of an object
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="identifier"></param>
        /// <param name="mixer"></param>
        private static void AndProperties(object instance, Guid identifier, Mixer mixer)
        {
            if (instance != null)
            {
                var type = instance.GetType();

                var properties = type.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

                if (properties.Any())
                {
                    foreach (var property in properties.Where(i => i.GetSetMethod(false) != null))
                    {
                        var recursionLimit = CountStackType(identifier, property.PropertyType);

                        if (recursionLimit <= mixer.RecursionLimit)
                        {
                            var value = GetValue(property.PropertyType, identifier, mixer);

                            property.SetValue(instance, value, null);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Create an instance from a constructor
        /// </summary>
        /// <param name="type"></param>
        /// <param name="identifier"></param>
        /// <param name="mixer"></param>
        /// <returns></returns>
        private static object Construct(Type type, Guid identifier, Mixer mixer)
        {
            object instance = null;

            var ctors = type.GetConstructors(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

            if (ctors.Any())
            {
                var ctor = ctors.OrderBy(i => i.GetParameters().Count()).FirstOrDefault();

                if (ctor != null)
                {
                    var parameters = ctor.GetParameters();

                    if (!parameters.Any())
                    {
                        instance = Activator.CreateInstance(type);
                    }
                    else
                    {
                        var paramaterList = parameters.Select(parameter => GetValue(parameter.ParameterType, identifier, mixer)).ToArray();

                        instance = Activator.CreateInstance(type, paramaterList);
                    }

                    AndProperties(instance, identifier, mixer);
                }
            }

            return instance;
        }

        /// <summary>
        /// Create a default instance of a type
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static object In(Type type, Guid identifier, Mixer mixer = null)
        {
            mixer = mixer ?? new Mixer();

            return GetValue(type, identifier, mixer);
        }

        /// <summary>
        /// Create a default instance of a type
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static object In(Type type, Mixer mixer = null)
        {
            mixer = mixer ?? new Mixer();

            return GetValue(type, Guid.NewGuid(), mixer);
        }

        /// <summary>
        /// Create a default instance of a type
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T In<T>(Guid identifier, Mixer mixer = null)
        {
            mixer = mixer ?? new Mixer();

            return (T)GetValue(typeof(T), identifier, mixer);
        }

        /// <summary>
        /// Create a default instance of a type
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T In<T>(Mixer mixer = null)
        {
            mixer = mixer ?? new Mixer();

            return (T)GetValue(typeof(T), Guid.NewGuid(), mixer);
        }
    }
}