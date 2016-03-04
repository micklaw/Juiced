using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

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
        /// <param name="handleError"></param>
        public static void HandleError(Locations location, Type type, Exception exception, Func<Type, Exception, bool> handleError)
        {
            if (handleError != null && !handleError(type, exception))
            {
                var message = $"Error in '{location.ToString()}' converting type '{type.Name}': see inner exception for details.";

                var juicedException = new JuicedException(message, exception);

                throw juicedException;
            }
        }
    }
}