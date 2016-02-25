using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace Juiced
{
    public class Mixer
    {
        /// <summary>
        /// Delegate used to catch / suppress and handle errors
        /// </summary>
        public Func<Type, Exception, bool> OnError { get; set; }

        /// <summary>
        /// Creates a mapping between abstract types and their concrete implementation to use. By default uses smaller constructor
        /// </summary>
        private ConcurrentDictionary<Type, Type[]> _abstractMapping { get; } = new ConcurrentDictionary<Type, Type[]>();

        /// <summary>
        /// Creates a mapping between a type and a default object
        /// </summary>
        private ConcurrentDictionary<Type, Func<object>> _onTypeFunc { get; set; } = new ConcurrentDictionary<Type, Func<object>>();


        /// <summary>
        /// Create a new instance of our settings
        /// </summary>
        /// <returns></returns>
        public static Mixer Configure => new Mixer();


        /// <summary>
        /// Adds a type mapping to an abstract type, of which on creation a random selection will be used
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="type"></param>
        /// <returns></returns>
        public Mixer AddAbstract<T>(Type type) => MapAbstract<T>(new [] { type });

        /// <summary>
        /// Adds a collection of assignable type mappings to an abstract type, of which on creation a random selection will be used
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="types"></param>
        /// <returns></returns>
        public Mixer MapAbstract<T>(Type[] types)
        {
            var abstractType = typeof (T);

            if (!abstractType.IsAbstract)
            {
                throw new InvalidCastException($"Type {abstractType.Name} is not an abstract type.");    
            }

            if (_abstractMapping.ContainsKey(abstractType))
            {
                throw new InvalidOperationException($"Key of type {abstractType.Namespace} already exists in the configuration.");
            }

            if (types == null || types.Length == 0)
            {
                throw new InvalidOperationException($"Your not adding any types to map to your abstract type {abstractType.Name}.");
            }

            foreach (var type in types.Where(type => !abstractType.IsAssignableFrom(type)))
            {
                throw new InvalidCastException($"Type {type.Name} is not assignable to type {abstractType.Name}.");
            }

            _abstractMapping.TryAdd(abstractType, types);

            return this;
        }

        /// <summary>
        /// Attempt to get a value from the internal collection
        /// </summary>
        /// <param name="type"></param>
        /// <param name="types"></param>
        /// <returns></returns>
        internal bool TryGetAbstract(Type type, out Type[] types) => _abstractMapping.TryGetValue(type, out types);


        /// <summary>
        /// Adds a collection of assignable type mappings to an abstract type, of which on creation a random selection will be used
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="func"></param>
        /// <returns></returns>
        public Mixer OnType<T>(Func<object> func)
        {
            var type = typeof(T);

            if (_abstractMapping.ContainsKey(type))
            {
                throw new InvalidOperationException($"Key of type {type.Namespace} already exists in the configuration.");
            }

            if (func == null)
            {
                throw new InvalidOperationException($"Your not adding any func to map to your type {type.Name}.");
            }

            _onTypeFunc.TryAdd(type, func);

            return this;
        }

        /// <summary>
        /// Attempt to get a value from the internal collection
        /// </summary>
        /// <param name="type"></param>
        /// <param name="func"></param>
        /// <returns></returns>
        internal bool TryGetOnTypeFunc(Type type, out Func<object> func) => _onTypeFunc.TryGetValue(type, out func);
    }
}