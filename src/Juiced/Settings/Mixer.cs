using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Reflection;

namespace Juiced
{
    public class Mixer
    {
        /// <summary>
        /// 
        /// </summary>
        internal Mixer()
        {
            OnType<int>(() => 1);
            OnType<decimal>(() => 1.1M);
            OnType<double>(() => 1.1D);
            OnType<byte>(() => new byte());
            OnType<short>(() => (short)1);
            OnType<uint>(() => 1U);
            OnType<long>(() => 1L);
            OnType<float>(() => 1.1F);
            OnType<ulong>(() => 1UL);
            OnType<bool>(() => false);
            OnType<string>(() => Guid.NewGuid().ToString().ToLower());
        }

        /// <summary>
        /// The recursion limit for nested classes
        /// </summary>
        public int RecursionLimit { get; private set; } = 0;

        /// <summary>
        /// Update the recursion limit so we don't overflow
        /// </summary>
        /// <param name="limit"></param>
        /// <returns></returns>
        public Mixer SetRecursion(int limit = 0)
        {
            RecursionLimit = limit;

            return this;
        }

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
        public Mixer MapAbstract<T>(Type type) => MapAbstract<T>(new[] { type });

        /// <summary>
        /// Adds a collection of assignable type mappings to an abstract type, of which on creation a random selection will be used
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="types"></param>
        /// <returns></returns>
        public Mixer MapAbstract<T>(Type[] types)
        {
            types = types.NotNull("types");

            var abstractType = typeof(T);
            var abstractTypeInfo = abstractType.GetTypeInfo();

            if (!abstractTypeInfo.IsAbstract)
            {
                throw new InvalidCastException($"Type {abstractType.Name} is not an abstract type.");
            }

            if (types.Length == 0)
            {
                throw new InvalidOperationException($"Your not adding any types to map to your abstract type {abstractType.Name}.");
            }

            foreach (var type in types.Where(type => !abstractTypeInfo.IsAssignableFrom(type.GetTypeInfo())))
            {
                throw new InvalidCastException($"Type {type.Name} is not assignable to type {abstractType.Name}.");
            }

            if (_abstractMapping.ContainsKey(abstractType))
            {
                Type[] oldTypes;

                _abstractMapping.TryGetValue(abstractType, out oldTypes);
                _abstractMapping.TryUpdate(abstractType, types, oldTypes);
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
            func = func.NotNull("func");

            var type = typeof(T);

            if (_onTypeFunc.ContainsKey(type))
            {
                Func<object> oldFunc;

                _onTypeFunc.TryGetValue(type, out oldFunc);
                _onTypeFunc.TryUpdate(type, func, oldFunc);
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