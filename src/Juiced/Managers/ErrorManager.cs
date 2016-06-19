using System;

namespace Juiced
{
    internal class ErrorManager
    {
        /// <summary>
        /// Handle errors DRYly
        /// </summary>
        /// <param name="location"></param>
        /// <param name="type"></param>
        /// <param name="exception"></param>
        /// <param name="settings"></param>
        public static void HandleError(Locations location, Type type, Exception exception, Mixer settings)
        {
            if (settings != null)
            {
                Func<Type, Exception, bool> typeExceptionHandler;
                settings.OnTypeError.TryGetValue(type, out typeExceptionHandler);

                typeExceptionHandler = typeExceptionHandler ?? settings.OnError;

                if (typeExceptionHandler != null && typeExceptionHandler(type, exception))
                {
                    return;
                }
            }

            var message = $"Error in '{location}' converting type '{type.Name}': see inner exception for details.";

            var up = new JuicedException(message, exception);

            throw up;
        }
    }
}