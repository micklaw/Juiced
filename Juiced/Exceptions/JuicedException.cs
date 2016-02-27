using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Juiced
{
    /// <summary>
    /// For all errors specific to Juiced
    /// </summary>
    public class JuicedException : Exception
    {
        public JuicedException(string message, Exception exception) : base(message, exception) { }
    }
}