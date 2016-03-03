using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

using static Juiced.RecursionManager;

namespace Juiced
{
    public class Juiced
    {
        /// <summary>
        /// Handle errors DRYly
        /// </summary>
        /// <param name="type"></param>
        /// <param name="exception"></param>
        /// <param name="handleError"></param>
        private static void HandleError(Locations location, Type type, Exception exception, Func<Type, Exception, bool> handleError)
        {
            if (handleError != null && !handleError(type, exception))
            {
                var message = $"Error in '{location.ToString()}' converting type '{type.Name}': see inner exception for details.";

                var juicedException = new JuicedException(message, exception);

                throw juicedException;
            }
        }

        /// <summary>
        /// Create an instance from a constructor
        /// </summary>
        /// <param name="type"></param>
        /// <param name="mixer"></param>
        /// <returns></returns>
        private static object Construct(Type type, Guid identifier, Mixer mixer)
        {
            var ctors = type.GetConstructors(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

            if (ctors.Any())
            {
                var ctor = ctors.OrderBy(i => i.GetParameters().Count()).FirstOrDefault();

                if (ctor != null)
                {
                    var parameters = ctor.GetParameters();

                    if (!parameters.Any())
                    {
                        return Activator.CreateInstance(type);
                    }
                    else
                    {
                        var paramaterList = parameters.Select(parameter => GetValue(parameter.ParameterType, identifier, mixer)).ToArray();

                        return Activator.CreateInstance(type, paramaterList);
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Get a default value for a type
        /// </summary>
        /// <param name="type"></param>
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

                    if (type == typeof(int))
                    {
                        value = 1;
                    }

                    if (type == (typeof(decimal)))
                    {
                        value = 1.0m;
                    }

                    if (type == typeof(string))
                    {
                        value = string.Empty;
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
                    value = type.IsValueType ? Activator.CreateInstance(type) : Hydrate(type, identifier, mixer);
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
        /// Hydrate an object given its type
        /// </summary>
        /// <param name="type"></param>
        /// <param name="mixer"></param>
        /// <returns></returns>
        private static object Hydrate(Type type, Guid identifier, Mixer mixer)
        {
            type = type.NotNull("type");
                 
            var instance = Construct(type, identifier, mixer);

            if (instance != null)
            {
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

            return instance;
        }

        /// <summary>
        /// Run the job async to get on with other stuff while you're waiting
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="mixer"></param>
        /// <returns></returns>
        public static Task<T> HydrateAsync<T>(Mixer mixer = null)
        {
            var identifier = Guid.NewGuid();

            mixer = mixer ?? Mixer.Configure;

            return Task.Run<T>(() => (T)Hydrate(typeof(T), identifier, mixer));
        }
    }
}