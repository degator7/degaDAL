

using dega.Common.Configuration;
using dega.Configuration;
using dega.Properties;
using System;
using System.ComponentModel;
using System.Configuration;

namespace dega.Oracle.Configuration
{
    /// <summary>
    /// Oracle-specific connection information.
    /// </summary>
    [ResourceDescription(typeof(DesignResources), "OracleConnectionDataDescription")]
    [ResourceDisplayName(typeof(DesignResources), "OracleConnectionDataDisplayName")]
    [NameProperty("Name", NamePropertyDisplayFormat = "Oracle Packages for {0}")]
    public class OracleConnectionData : NamedConfigurationElement
    {
        private const string packagesProperty = "packages";

        /// <summary>
        /// Initializes a new instance of the <see cref="OracleConnectionData"/> class with default values.
        /// </summary>
        public OracleConnectionData()
        {
        }

        /// <summary>
        /// 
        /// </summary>
        [Reference(typeof(ConnectionStringSettingsCollection), typeof(ConnectionStringSettings))]
        [ResourceDescription(typeof(DesignResources), "OracleConnectionDataNameDescription")]
        [ResourceDisplayName(typeof(DesignResources), "OracleConnectionDataNameDisplayName")]
        public override string Name
        {
            get { return base.Name; }
            set { base.Name = value; }
        }

        /// <summary>
        /// Gets a collection of <see cref="OraclePackageData"/> objects.
        /// </summary>
        /// <value>
        /// A collection of <see cref="OraclePackageData"/> objects.
        /// </value>
        [ConfigurationProperty(packagesProperty, IsRequired = true)]
        [ConfigurationCollection(typeof(OraclePackageData))]
        [ResourceDescription(typeof(DesignResources), "OracleConnectionDataPackagesDescription")]
        [ResourceDisplayName(typeof(DesignResources), "OracleConnectionDataPackagesDisplayName")]
        [Editor(CommonDesignTime.EditorTypes.CollectionEditor, CommonDesignTime.EditorTypes.FrameworkElement)]
        [EnvironmentalOverrides(false)]
        [DesignTimeReadOnly(false)]
        public NamedElementCollection<OraclePackageData> Packages
        {
            get
            {
                return (NamedElementCollection<OraclePackageData>)base[packagesProperty];
            }
        }





    }




    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class NamePropertyAttribute : Attribute
    {
        private readonly string propertyName;

        /// <summary>
        /// Initializes a new instance of the <see cref="NamePropertyAttribute"/> class.
        /// </summary>
        /// <param name="propertyName">The reflection name of the property that will be used as the Element View Model's name.</param>
        public NamePropertyAttribute(string propertyName)
        {
            if (string.IsNullOrEmpty(propertyName)) throw new ArgumentException(Resources.ExceptionStringNullOrEmpty, "propertyName");

            this.propertyName = propertyName;
            this.NamePropertyDisplayFormat = "{0}";
        }

        /// <summary>
        /// Gets the reflection name of the property that will be used as the Element View Model's name.
        /// </summary>
        public string PropertyName
        {
            get { return propertyName; }
        }

        /// <summary>
        /// Gets the Display Format that will be used to display the name property.<br/>
        /// The Display Format should be a Format-string with 1 argument:<Br/>
        /// The token '{0}' will be replaced with the Name Properties value.
        /// </summary>
        public string NamePropertyDisplayFormat { get; set; }
    }





}
