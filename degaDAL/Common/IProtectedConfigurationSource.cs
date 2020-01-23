using System;
using System.Collections.Generic;
using System.Text;
using System.Configuration;

namespace dega.Common.Configuration
{
    /// <summary>
    /// </summary>
    public interface IProtectedConfigurationSource
    {
        /// <summary>
        /// </summary>
        /// <param name="sectionName"></param>
        /// <param name="configurationSection"></param>
        /// <param name="protectionProviderName"></param>
        void Add(string sectionName, ConfigurationSection configurationSection, string protectionProviderName);


    }
}
