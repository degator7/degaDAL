

using System;
using System.Collections.Generic;
using System.Text;
using dega.Common.Configuration;

namespace dega.Common.Configuration
{
    /// <summary>
    /// Represents the abstraction of an object with a name and a type.
    /// </summary>
    public interface IObjectWithNameAndType : IObjectWithName
    {
        /// <summary>
        /// Gets the type.
        /// </summary>
        Type Type { get; }
    }
}
