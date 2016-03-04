using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Juiced
{
    /// <summary>
    /// Stack used to manage recursion in each iteration
    /// </summary>
    internal class RecursionManager
    {
        private static ConcurrentDictionary<Guid, ConcurrentStack<Type>> _stacks = new ConcurrentDictionary<Guid, ConcurrentStack<Type>>();

        /// <summary>
        /// Get a stack by its identifier
        /// </summary>
        /// <param name="identifier"></param>
        /// <returns></returns>
        private static ConcurrentStack<Type> GetStack(Guid identifier)
        {
            ConcurrentStack<Type> stack;

            _stacks.TryGetValue(identifier, out stack);

            if (stack == null)
            {
                throw new JuicedException($"No stack found for key {identifier.ToString()}.");
            }

            return stack;
        }

        /// <summary>
        /// Creates a newstack or pushes to an existing one
        /// </summary>
        /// <param name="identifier"></param>
        /// <param name="type"></param>
        public static void PushTo(Guid identifier, Type type)
        {
            ConcurrentStack<Type> stack;

            _stacks.TryGetValue(identifier, out stack);

            if (stack == null)
            {
                stack = new ConcurrentStack<Type>();

                _stacks.TryAdd(identifier, stack);
            }

            stack.Push(type);
        }

        /// <summary>
        /// Creates a newstack or pushes to an existing one
        /// </summary>
        /// <param name="identifier"></param>
        /// <param name="type"></param>
        public static int CountStackType(Guid identifier, Type type)
        {
            var stack = GetStack(identifier);

            return stack.Count(i => i == type);
        }

        /// <summary>
        /// Pops the latest from a stack
        /// </summary>
        /// <param name="identifier"></param>
        /// <param name="type"></param>
        public static Type PopTo(Guid identifier)
        {
            var stack = GetStack(identifier);

            Type popped;
            stack.TryPop(out popped);

            return popped;
        }
    }
}