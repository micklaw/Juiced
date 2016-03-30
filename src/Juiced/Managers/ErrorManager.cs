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
        /// <param name="handleAndSuppress"></param>
        public static void HandleError(Locations location, Type type, Exception exception, Func<Type, Exception, bool> handleAndSuppress)
        {
            if (handleAndSuppress != null)
            {
                if (!handleAndSuppress(type, exception))
                {
                    var message = $"Error in '{location}' converting type '{type.Name}': see inner exception for details.";

                    var juicedException = new JuicedException(message, exception);

                    throw juicedException;
                }
            }
            else
            {
                throw exception;
            }
        }
    }
}