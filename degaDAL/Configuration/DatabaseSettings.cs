

using System;
using System.ComponentModel;
using System.Configuration;
using System.Resources;
using dega;
using dega.Common.Configuration;
using dega.Common.Configuration.Design;
//using dega.Common.Configuration.Design.Validation;
using dega.Configuration;
using dega.Properties;

namespace dega.Configuration
{
    /// <summary>
    /// <para>Represents the root configuration for data.</para>
    /// </summary>
    /// <remarks>
    /// <para>The class maps to the <c>databaseSettings</c> element in configuration.</para>
    /// </remarks>
    [ResourceDescription(typeof(DesignResources), "DatabaseSettingsDescription")]
    [ResourceDisplayName(typeof(DesignResources), "DatabaseSettingsDisplayName")]
    public class DatabaseSettings : SerializableConfigurationSection
    {
        private const string defaultDatabaseProperty = "defaultDatabase";
        private const string dbProviderMappingsProperty = "providerMappings";

        /// <summary>
        /// The name of the data configuration section.
        /// </summary>
        public const string SectionName = "dataConfiguration";

        /// <summary>
        /// <para>Initializes a new instance of the <see cref="DatabaseSettings"/> class.</para>
        /// </summary>
        public DatabaseSettings()
            : base()
        {
        }

        /// <summary>
        /// Retrieves the <see cref="DatabaseSettings"/> from a configuration source.
        /// </summary>
        /// <param name="configurationSource">The <see cref="IConfigurationSource"/> to query for the database settings.</param>
        /// <returns>The database settings from the configuration source, or <see langword="null"/> (<b>Nothing</b> in Visual Basic) if the 
        /// configuration source does not contain database settings.</returns>
        public static DatabaseSettings GetDatabaseSettings(IConfigurationSource configurationSource)
        {
            if (configurationSource == null) throw new ArgumentNullException("configurationSource");

            return (DatabaseSettings)configurationSource.GetSection(SectionName);
        }

        /// <summary>
        /// <para>Gets or sets the default database instance name.</para>
        /// </summary>
        /// <value>
        /// <para>The default database instance name.</para>
        /// </value>
        /// <remarks>
        /// <para>This property maps to the <c>defaultInstance</c> element in configuration.</para>
        /// </remarks>
        [ConfigurationProperty(defaultDatabaseProperty, IsRequired = false)]
        [Reference(typeof(ConnectionStringSettingsCollection), typeof(ConnectionStringSettings))]
        [ResourceDescription(typeof(DesignResources), "DatabaseSettingsDefaultDatabaseDescription")]
        [ResourceDisplayName(typeof(DesignResources), "DatabaseSettingsDefaultDatabaseDisplayName")]
        public string DefaultDatabase
        {
            get
            {
                return (string)this[defaultDatabaseProperty];
            }
            set
            {
                this[defaultDatabaseProperty] = value;
            }
        }

        /// <summary>
        /// Holds the optional mappings from ADO.NET's database providers to Enterprise Library's database types.
        /// </summary>
        /// <seealso cref="DbProviderMapping"/>
        [ConfigurationProperty(dbProviderMappingsProperty, IsRequired = false)]
        [ConfigurationCollection(typeof(DbProviderMapping))]
        [ResourceDescription(typeof(DesignResources), "DatabaseSettingsProviderMappingsDescription")]
        [ResourceDisplayName(typeof(DesignResources), "DatabaseSettingsProviderMappingsDisplayName")]
        public NamedElementCollection<DbProviderMapping> ProviderMappings
        {
            get
            {
                return (NamedElementCollection<DbProviderMapping>)base[dbProviderMappingsProperty];
            }
        }
    }






    /// <summary>
    /// A customized version of <see cref="DescriptionAttribute"/> that can
    /// load the string from assembly resources instead of just a hard-wired
    /// string.
    /// </summary>
    public class ResourceDescriptionAttribute : DescriptionAttribute
    {
        private bool resourceLoaded;

        /// <summary>
        /// Create a new instance of <see cref="ResourceDescriptionAttribute"/> where
        /// the type and name of the resource is set via properties.
        /// </summary>
        public ResourceDescriptionAttribute()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ResourceDescriptionAttribute"/> class.
        /// </summary>
        /// <param name="resourceType">Type used to locate the assembly containing the resources.</param>
        /// <param name="resourceName">Name of the entry in the resource table.</param>
        public ResourceDescriptionAttribute(Type resourceType, string resourceName)
        {
            ResourceType = resourceType;
            ResourceName = resourceName;
        }

        /// <summary>
        /// A type contained in the assembly we want to get our display name from.
        /// </summary>
        public Type ResourceType { get; set; }

        /// <summary>
        /// Name of the string resource containing our display name.
        /// </summary>
        public string ResourceName { get; set; }

        /// <summary>
        /// Gets the description for a property, event, or public void method that takes no arguments stored in this attribute.
        /// </summary>
        /// <returns>
        /// The display name.
        /// </returns>
        public override string Description
        {
            get
            {
                EnsureDescriptionLoaded();
                return DescriptionValue;
            }
        }

        private void EnsureDescriptionLoaded()
        {
            if (resourceLoaded) return;

            var rm = new ResourceManager(ResourceType);

            try
            {
                DescriptionValue = rm.GetString(ResourceName);
            }
            catch (MissingManifestResourceException)
            {
                DescriptionValue = ResourceName;
            }
            DescriptionValue = DescriptionValue == null ? String.Empty : DescriptionValue;

            resourceLoaded = true;
        }

    }


    /// <summary>
    /// A customized version of <see cref="DisplayNameAttribute"/> that can
    /// load the string from assembly resources instead of just a hard-wired
    /// string.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Property)]
    public class ResourceDisplayNameAttribute : DisplayNameAttribute
    {
        private bool resourceLoaded;

        /// <summary>
        /// Create a new instance of <see cref="ResourceDisplayNameAttribute"/> where
        /// the type and name of the resource is set via properties.
        /// </summary>
        public ResourceDisplayNameAttribute()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ResourceDisplayNameAttribute"/> class.
        /// </summary>
        /// <param name="resourceType">Type used to locate the assembly containing the resources.</param>
        /// <param name="resourceName">Name of the entry in the resource table.</param>
        public ResourceDisplayNameAttribute(Type resourceType, string resourceName)
        {
            ResourceType = resourceType;
            ResourceName = resourceName;
        }

        /// <summary>
        /// A type contained in the assembly we want to get our display name from.
        /// </summary>
        public Type ResourceType { get; set; }

        /// <summary>
        /// Name of the string resource containing our display name.
        /// </summary>
        public string ResourceName { get; set; }

        /// <summary>
        /// Gets the display name for a property, event, or public void method that takes no arguments stored in this attribute.
        /// </summary>
        /// <returns>
        /// The display name.
        /// </returns>
        public override string DisplayName
        {
            get
            {
                EnsureDisplayNameLoaded();
                return DisplayNameValue;
            }
        }

        private void EnsureDisplayNameLoaded()
        {
            if (resourceLoaded) return;

            var rm = new ResourceManager(ResourceType);
            try
            {
                DisplayNameValue = rm.GetString(ResourceName);
            }
            catch (MissingManifestResourceException)
            {
                DisplayNameValue = ResourceName;
            }
            if (String.IsNullOrEmpty(DisplayNameValue)) DisplayNameValue = ResourceName;
            resourceLoaded = true;
        }
    }





    /// <summary>
    /// Attribute class used to indicate that the property is a reference to provider. <br/>
    /// Reference properties will show an editable dropdown that allows the referred element to be selected.<br/>
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public sealed class ReferenceAttribute : Attribute
    {
        private readonly string scopeTypeName;
        private readonly string targetTypeName;
        private Type cachedType;
        private Type cachedScopeType;
        private bool scopeTypeCached = false;


        /// <summary>
        /// Initializes a new instance of the <see cref="ReferenceAttribute"/> class.
        /// </summary>
        /// <param name="targetTypeName">The configuration type name of the provider that used as a reference.</param>
        public ReferenceAttribute(string targetTypeName)
        {
            if (string.IsNullOrEmpty(targetTypeName)) throw new ArgumentException(Resources.ExceptionStringNullOrEmpty, "targetTypeName");
            this.targetTypeName = targetTypeName;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ReferenceAttribute"/> class.
        /// </summary>
        /// <param name="scopeTypeName">The name of a configuration type that contains the references.</param>
        /// <param name="targetTypeName">The configuration type name of the provider that used as a reference.</param>
        public ReferenceAttribute(string scopeTypeName, string targetTypeName)
        {
            if (string.IsNullOrEmpty(scopeTypeName)) throw new ArgumentException(Resources.ExceptionStringNullOrEmpty, "scopeTypeName");
            if (string.IsNullOrEmpty(targetTypeName)) throw new ArgumentException(Resources.ExceptionStringNullOrEmpty, "targetTypeName");

            this.scopeTypeName = scopeTypeName;
            this.targetTypeName = targetTypeName;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ReferenceAttribute"/> class.
        /// </summary>
        /// <param name="targetType">The configuration type of the provider that used as a reference.</param>
        public ReferenceAttribute(Type targetType)
        {
            if (targetType == null) throw new ArgumentNullException("targetType");

            this.targetTypeName = targetType.AssemblyQualifiedName;
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="ReferenceAttribute"/> class.
        /// </summary>
        /// <param name="scopeType">The configuration type that contains the references.</param>
        /// <param name="targetType">The configuration type of the provider that used as a reference.</param>
        public ReferenceAttribute(Type scopeType, Type targetType)
        {
            if (targetType == null) throw new ArgumentNullException("targetType");
            if (scopeType == null) throw new ArgumentNullException("scopeType");

            this.scopeTypeName = scopeType.AssemblyQualifiedName;
            this.targetTypeName = targetType.AssemblyQualifiedName;
        }

        /// <summary>
        /// Gets the configuration type that contains the references.
        /// </summary>
        public Type ScopeType
        {
            get
            {
                if (!scopeTypeCached)
                {
                    cachedScopeType = string.IsNullOrEmpty(scopeTypeName) ? null : Type.GetType(scopeTypeName);
                    scopeTypeCached = true;
                }

                return cachedScopeType;
            }
        }

        /// <summary>
        /// Gets or sets a boolean indicating whether only providers can be used that are contained in the current Element View Model.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if only providers can be used that are contained in the current Element View Model. Otherwise <see langword="false"/>.
        /// </value>
        public bool ScopeIsDeclaringElement
        {
            get;
            set;
        }

        /// <summary>
        /// Gets the configuration type of the provider that used as a reference.
        /// </summary>
        public Type TargetType
        {
            get
            {
                if (cachedType == null)
                {
                    cachedType = Type.GetType(targetTypeName);
                }

                return cachedType;
            }
        }
    }




}
