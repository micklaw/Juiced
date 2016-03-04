using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Juiced
{
    /// <summary>
    /// Locations where errors could throw in application
    /// </summary>
    internal enum Locations
    {
        GetValue,
        Hydrate
    }
}