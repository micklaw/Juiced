using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

using static Juiced.ValueManager;

namespace Juiced
{
    public class Juiced
    {
        /// <summary>
        /// Hydrate an object given its type
        /// </summary>
        /// <param name="type"></param>
        /// <param name="identifier"></param>
        /// <param name="mixer"></param>
        /// <returns></returns>
        private static object Hydrate(Type type, Guid identifier, Mixer mixer)
        {
            type = type.NotNull("type");
                 
            var instance = GetValue(type, identifier, mixer);

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

            return Task.Run(() =>
            {
                var item = Hydrate(typeof (T), identifier, mixer);

                if (item == null)
                    return default(T);

                return (T)item;
            });
        }
    }
}