using System;

namespace Juiced
{
    /// <summary>
    /// For all errors specific to Juiced
    /// </summary>
    public class JuicedException : Exception
    {
        public JuicedException(string message) : base(message) { }

        public JuicedException(string message, Exception exception) : base(message, exception) { }
    }
}