using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Juiced
{
    public class Juiced
    {
        /// <summary>
        /// Create an instance from a constructor
        /// </summary>
        /// <param name="type"></param>
        /// <param name="mixer"></param>
        /// <returns></returns>
        private static object Construct(Type type, Mixer mixer)
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
                        var paramaterList = parameters.Select(parameter => GetValue(parameter.ParameterType, mixer)).ToArray();

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
        private static object GetValue(Type type, Mixer mixer)
        {
            try
            {
                Func<object> getObject;

                if (mixer.TryGetOnTypeFunc(type, out getObject))
                {
                    return getObject();
                }

                if (type.IsEnum)
                {
                    return Enum.GetValues(type).GetValue(1);
                }

                if (type == typeof (int))
                {
                    return 1;
                }

                if (type == (typeof (decimal)))
                {
                    return 1.0m;
                }
                if (type == typeof (string))
                {
                    return string.Empty;
                }

                if (type.IsArray)
                {
                    return Activator.CreateInstance(type, new object[] {1});
                }

                if (type.IsGenericType && typeof (IEnumerable).IsAssignableFrom(type))
                {
                    var isGenericList = (type.GetGenericTypeDefinition() == typeof (IList<>) || type.GetGenericTypeDefinition() == typeof (List<>));
                    var isGenericCollection = (type.GetGenericTypeDefinition() == typeof (ICollection<>) || type.GetGenericTypeDefinition() == typeof (Collection<>));
                    var genericType = isGenericList || isGenericCollection ? type.GetGenericArguments().FirstOrDefault() : type;

                    var constructedListType = (isGenericList ? typeof (List<>) : typeof (Collection<>)).MakeGenericType(genericType);
                    return Activator.CreateInstance(constructedListType);
                }

                if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof (Nullable<>))
                {
                    var underlyingType = Nullable.GetUnderlyingType(type);

                    return GetValue(underlyingType, mixer);
                }

                if (type.IsAbstract)
                {
                    Type[] concreteTypes;
                    if (!mixer.TryGetAbstract(type, out concreteTypes))
                    {
                        throw new Exception("Must be a non abstract type if not a list.");
                    }

                    return Construct(concreteTypes.GetRandom(), mixer);
                }

                return type.IsValueType ? Activator.CreateInstance(type) : Construct(type, mixer);
            }
            catch (Exception exception)
            {
                if (mixer.OnError == null || mixer.OnError(type, exception))
                {
                    throw;
                }
            }

            // ML - Value type would never error, so this must be a reference type, so give it null

            return null;
        }

        /// <summary>
        /// Hydrate an object given its type
        /// </summary>
        /// <param name="type"></param>
        /// <param name="mixer"></param>
        /// <returns></returns>
        private static object Hydrate(Type type, Mixer mixer)
        {
            var stack = new ConcurrentStack<Type>();

            type = type.NotNull("type");

            var instance = GetValue(type, mixer);

            if (instance != null)
            {
                stack.Push(type);

                var properties = type.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

                if (properties.Any())
                {
                    foreach (var property in properties.Where(i => i.GetSetMethod(false) != null))
                    {
                        Type peeked;

                        stack.TryPeek(out peeked);

                        if (property.PropertyType != peeked)
                        {
                            var isDirty = false;

                            if (property.GetGetMethod(false) != null)
                            {
                                object current = null;

                                try
                                {
                                    current = property.GetValue(instance, null);
                                }
                                catch
                                {
                                    // ML - Suppress the error if property throws for some reason
                                }

                                var defaultValue = property.PropertyType.IsValueType
                                    ? Activator.CreateInstance(property.PropertyType)
                                    : null;

                                if (current != null && !current.Equals(defaultValue))
                                {
                                    isDirty = true;
                                }
                            }

                            if (!isDirty)
                            {
                                var value = GetValue(property.PropertyType, mixer);

                                property.SetValue(instance, value, null);
                            }
                            else
                            {
                                // TODO: ML - Handle dirty properties
                            }
                        }
                    }
                }

                Type popped;

                stack.TryPop(out popped);

                return instance;
            }

            return null;
        }

        /// <summary>
        /// Run the job async to get on with other stuff while you're waiting
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="mixer"></param>
        /// <returns></returns>
        public static Task<T> HydrateAsync<T>(Mixer mixer)
        {
            return Task.Run<T>(() => Hydrate<T>(mixer));
        }

        /// <summary>
        /// Hydrate an object given its type
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="mixer"></param>
        /// <returns></returns>
        public static T Hydrate<T>(Mixer mixer = null)
        {
            mixer = mixer ?? Mixer.Configure;

            return (T)Hydrate(typeof(T), mixer);
        }
    }
}