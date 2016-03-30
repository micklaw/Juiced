using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using static Juiced.RecursionManager;
using static Juiced.ErrorManager;
using System.Collections.ObjectModel;

namespace Juiced
{
    /// <summary>
    /// Return default values for a given type
    /// </summary>
    internal class ValueManager
    {
        /// <summary>
        /// Lazy loading of the random as a singleton
        /// </summary>
        private static readonly Lazy<Random> _random = new Lazy<Random>(() => new Random());

        /// <summary>
        /// The instance of the random
        /// </summary>
        private static Random RandomInstance => _random.Value;
         
        /// <summary>
        /// Get a default value for a type
        /// </summary>
        /// <param name="type"></param>
        /// <param name="identifier"></param>
        /// <param name="mixer"></param>
        /// <returns></returns>
        internal static object GetValue(Type type, Guid identifier, Mixer mixer)
        {
            var typeInfo = type.GetTypeInfo();

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

                // ML - If not create, then try our value types and .Net types etc

                if (value == null)
                {
                    if (typeInfo.IsEnum)
                    {
                        var enums = Enum.GetValues(type);

                        value = enums.GetValue(RandomInstance.Next(0, enums.Length - 1));
                    }

                    if (type.IsArray)
                    {
                        value = Activator.CreateInstance(type, 1);
                    }

                    if (typeInfo.IsGenericType && typeof(IEnumerable).GetTypeInfo().IsAssignableFrom(typeInfo))
                    {
                        var isGenericList = (type.GetGenericTypeDefinition() == typeof(IList<>) || type.GetGenericTypeDefinition() == typeof(List<>));
                        var isGenericCollection = (type.GetGenericTypeDefinition() == typeof(ICollection<>) || type.GetGenericTypeDefinition() == typeof(Collection<>));
                        var genericType = isGenericList || isGenericCollection ? type.GetGenericArguments().FirstOrDefault() : type;

                        var constructedListType = (isGenericList ? typeof(List<>) : typeof(Collection<>)).MakeGenericType(genericType);

                        value = Activator.CreateInstance(constructedListType);
                    }

                    if (typeInfo.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
                    {
                        var underlyingType = Nullable.GetUnderlyingType(type);

                        value = GetValue(underlyingType, identifier, mixer);
                    }
                }

                if (value == null)
                {
                    // ML - If its an abstract type try to create from our mappings

                    if (typeInfo.IsAbstract)
                    {
                        Type[] concreteTypes;
                        if (mixer.TryGetAbstract(type, out concreteTypes))
                        {
                            value = Construct(concreteTypes.GetRandom(), identifier, mixer);
                        }
                    }
                    else
                    {
                        // ML - Else last change construct or fire a value type off

                        value = Construct(type, identifier, mixer) ?? Activator.CreateInstance(type);
                    }
                }


                // ML - Or just try and construct
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
    }
}