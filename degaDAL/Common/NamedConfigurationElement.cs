﻿
using dega.Configuration;
using dega.Oracle.Configuration;
using System.Configuration;
using System.Xml;


namespace dega.Common.Configuration
{
    /// <summary>
    /// Represents a named <see cref="ConfigurationElement"/> where the name is the key to a collection.
    /// </summary>
    /// <remarks>
    /// This class is used in conjunction with a <see cref="Configuration.NamedElementCollection&lt;T&gt;"/>.
    /// </remarks>

    [NameProperty("Name")]
    public class NamedConfigurationElement : ConfigurationElement, IObjectWithName
    {
        /// <summary>
        /// Name of the property that holds the name of <see cref="NamedConfigurationElement"/>.
        /// </summary>
        public const string nameProperty = "name";

        /// <summary>
        /// Initialize a new instance of a <see cref="NamedConfigurationElement"/> class.
        /// </summary>
        public NamedConfigurationElement()
        { }

        /// <summary>
        /// Intialize a new instance of a <see cref="NamedConfigurationElement"/> class with a name.
        /// </summary>
        /// <param name="name">The name of the element.</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors", Justification = "As designed")]
        public NamedConfigurationElement(string name)
        {
            Name = name;
        }

        /// <summary>
        /// Gets or sets the name of the element.
        /// </summary>
        /// <value>
        /// The name of the element.
        /// </value>
        [ConfigurationProperty(nameProperty, IsKey = true, DefaultValue = "Name", IsRequired = true)]
        [StringValidator(MinLength = 1)]
        [ResourceDescription(typeof(dega.Configuration.DesignResources), "NamedConfigurationElementNameDescription")]
        [ResourceDisplayName(typeof(Oracle.Configuration.DesignResources), "NamedConfigurationElementNameDisplayName")]
        [ResourceCategory(typeof(ResourceCategoryAttribute), "CategoryName")]
        [EnvironmentalOverrides(false)]
        public virtual string Name
        {
            get { return (string)this[nameProperty]; }
            set { this[nameProperty] = value; }
        }

        /// <summary>
        /// This method supports the Enterprise Library infrastructure and is not intended to be used directly from your code.
        /// Updates the configuration properties of the receiver with the information in the current element in the <paramref name="reader"/>.
        /// </summary>
        /// <param name="reader">The reader over the configuration file.</param>
        public void DeserializeElement(XmlReader reader)
        {
            base.DeserializeElement(reader, false);
        }
    }
}
