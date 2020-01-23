
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Common;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using dega.Common.Configuration;
using dega.Common.Utility;
using dega.Configuration;
using dega.Properties;
using dega.Sql;

namespace dega
{
    /// <summary>
    /// <para>Represents a factory for creating named instances of <see cref="Database"/> objects.</para>
    /// </summary>
    public class DatabaseProviderFactory
    {
        private DatabaseConfigurationBuilder builder;

        /// <summary>
        /// <para>Initializes a new instance of the <see cref="DatabaseProviderFactory"/> class 
        /// with the default configuration source.</para>
        /// </summary>
        public DatabaseProviderFactory()
            : this(s => (ConfigurationSection)ConfigurationManager.GetSection(s))
        {
        }

        /// <summary>
        /// <para>Initializes a new instance of the <see cref="DatabaseProviderFactory"/> class 
        /// with the given configuration source.</para>
        /// </summary>
        /// <param name="configurationSource">The source for configuration information.</param>
        public DatabaseProviderFactory(IConfigurationSource configurationSource)
            : this(configurationSource.GetSection)
        {
        }

        /// <summary>
        /// <para>Initializes a new instance of the <see cref="DatabaseProviderFactory"/> class 
        /// with the given configuration accessor.</para>
        /// </summary>
        /// <param name="configurationAccessor">The source for configuration information.</param>
        public DatabaseProviderFactory(Func<string, ConfigurationSection> configurationAccessor)
        {
            Guard.ArgumentNotNull(configurationAccessor, "configurationAccessor");

            this.builder = new DatabaseConfigurationBuilder(configurationAccessor);
        }

        /// <summary>
        /// Returns a new <see cref="Database"/> instance based on the default instance configuration.
        /// </summary>
        /// <returns>
        /// A new Database instance.
        /// </returns>
        public Database CreateDefault()
        {
            return this.builder.CreateDefault();
        }

        /// <summary>
        /// Returns a new <see cref="Database"/> instance based on the configuration for <paramref name="name"/>.
        /// </summary>
        /// <param name="name">The name of the required instance.</param>
        /// <returns>
        /// A new Database instance.
        /// </returns>
        public Database Create(string name)
        {
            Guard.ArgumentNotNullOrEmpty(name, "name");

            return this.builder.Create(name);
        }

        private class DatabaseConfigurationBuilder
        {
            private readonly DatabaseSyntheticConfigSettings settings;
            private readonly ConcurrentDictionary<string, DatabaseData> databases;

            public DatabaseConfigurationBuilder(Func<string, ConfigurationSection> configurationAccessor)
            {
                Guard.ArgumentNotNull(configurationAccessor, "configurationAccessor");

                this.settings = new DatabaseSyntheticConfigSettings(configurationAccessor);
                this.databases = new ConcurrentDictionary<string, DatabaseData>();
            }

            public Database CreateDefault()
            {
                var defaultDatabaseName = this.settings.DefaultDatabase;

                if (string.IsNullOrEmpty(defaultDatabaseName))
                {
                    throw new InvalidOperationException(Resources.ExceptionNoDefaultDatabaseDefined);
                }

                var data =
                    this.databases.GetOrAdd(
                        defaultDatabaseName,
                        n =>
                        {
                            try
                            {
                                return this.settings.GetDatabase(n);
                            }
                            catch (ConfigurationErrorsException e)
                            {
                                throw new InvalidOperationException(
                                    string.Format(CultureInfo.CurrentCulture, Resources.ExceptionDefaultDatabaseInvalid, n),
                                    e);
                            }
                        });

                return data.BuildDatabase();
            }

            public Database Create(string name)
            {
                Guard.ArgumentNotNull(name, "name");

                var data =
                    this.databases.GetOrAdd(
                        name,
                        n =>
                        {
                            try
                            {
                                return this.settings.GetDatabase(n);
                            }
                            catch (ConfigurationErrorsException e)
                            {
                                throw new InvalidOperationException(
                                    string.Format(CultureInfo.CurrentCulture, Resources.ExceptionDatabaseInvalid, n),
                                    e);
                            }
                        });

                return data.BuildDatabase();
            }

           


        }
    }








    //[ResourceDescription(typeof(DesignResources), "DatabaseSettingsDescription")]
    //[ResourceDisplayName(typeof(DesignResources), "DatabaseSettingsDisplayName")]
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
        //[Reference(typeof(ConnectionStringSettingsCollection), typeof(ConnectionStringSettings))]
        //[ResourceDescription(typeof(DesignResources), "DatabaseSettingsDefaultDatabaseDescription")]
        //[ResourceDisplayName(typeof(DesignResources), "DatabaseSettingsDefaultDatabaseDisplayName")]
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

        ///// <summary>
        ///// Holds the optional mappings from ADO.NET's database providers to Enterprise Library's database types.
        ///// </summary>
        ///// <seealso cref="DbProviderMapping"/>
        //[ConfigurationProperty(dbProviderMappingsProperty, IsRequired = false)]
        //[ConfigurationCollection(typeof(DbProviderMapping))]
        ////[ResourceDescription(typeof(DesignResources), "DatabaseSettingsProviderMappingsDescription")]
        ////[ResourceDisplayName(typeof(DesignResources), "DatabaseSettingsProviderMappingsDisplayName")]
        //public NamedElementCollection<DbProviderMapping> ProviderMappings
        //{
        //    get
        //    {
        //        return (NamedElementCollection<DbProviderMapping>)base[dbProviderMappingsProperty];
        //    }
        //}
    }





    public class SerializableConfigurationSection : ConfigurationSection, IXmlSerializable
    {
        private const string SourceProperty = "source";

        /// <summary>
        /// Returns the XML schema for the configuration section.
        /// </summary>
        /// <returns>A string with the XML schema, or <see langword="null"/> (<b>Nothing</b> 
        /// in Visual Basic) if there is no schema.</returns>
        public XmlSchema GetSchema()
        {
            return null;
        }

        /// <summary>
        /// Updates the configuration section with the values from an <see cref="XmlReader"/>.
        /// </summary>
        /// <param name="reader">The <see cref="XmlReader"/> that reads the configuration source located at the element that describes the configuration section.</param>
        public void ReadXml(XmlReader reader)
        {
            if (reader == null) throw new ArgumentNullException("reader");

            reader.Read();
            DeserializeSection(reader);

        }

        /// <summary>
        /// Writes the configuration section values as an XML element to an <see cref="XmlWriter"/>.
        /// </summary>
        /// <param name="writer">The <see cref="XmlWriter"/> that writes to the configuration store.</param>
        public void WriteXml(XmlWriter writer)
        {
            if (writer == null) throw new ArgumentNullException("writer");

            String serialized = SerializeSection(this, "SerializableConfigurationSection", ConfigurationSaveMode.Full);
            writer.WriteRaw(serialized);
        }
    }




    public class GenericDatabase : Database
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GenericDatabase"/> class with a connection string and 
        /// a provider factory.
        /// </summary>
        /// <param name="connectionString">The connection string.</param>
        /// <param name="dbProviderFactory">The provider factory.</param>
        public GenericDatabase(string connectionString, DbProviderFactory dbProviderFactory)
            : base(connectionString, dbProviderFactory)
        {
        }

        /// <summary>
        /// This operation is not supported in this class.
        /// </summary>
        /// <param name="discoveryCommand">The <see cref="DbCommand"/> to do the discovery.</param>
        /// <remarks>There is no generic way to do it, the operation is not implemented for <see cref="GenericDatabase"/>.</remarks>
        /// <exception cref="NotSupportedException">Thrown whenever this method is called.</exception>
        protected override void DeriveParameters(DbCommand discoveryCommand)
        {
            throw new NotSupportedException(Resources.ExceptionParameterDiscoveryNotSupportedOnGenericDatabase);
        }
    }








    public class NamedElementCollection<T> : ConfigurationElementCollection, IMergeableConfigurationElementCollection, IEnumerable<T>
        where T : Common.Configuration.NamedConfigurationElement, new()
    {

        /// <summary>
        /// Performs the specified action on each element of the collection.
        /// </summary>
        /// <param name="action">The action to perform.</param>
        public void ForEach(Action<T> action)
        {
            for (int index = 0; index < Count; index++)
            {
                action(Get(index));
            }
        }

        /// <summary>
        /// Gets the configuration element at the specified index location. 
        /// </summary>
        /// <param name="index">The index location of the <see name="T"/> to return. </param>
        /// <returns>The <see name="T"/> at the specified index. </returns>
        public T Get(int index)
        {
            return (T)base.BaseGet(index);
        }

        /// <summary>
        /// Add an instance of <typeparamref name="T"/> to the collection.
        /// </summary>
        /// <param name="element">An instance of <typeparamref name="T"/>.</param>
        public void Add(T element)
        {
            BaseAdd(element, true);
        }

        /// <summary>
        /// Gets the named instance of <typeparamref name="T"/> from the collection.
        /// </summary>
        /// <param name="name">The name of the <typeparamref name="T"/> instance to retrieve.</param>
        /// <returns>The instance of <typeparamref name="T"/> with the specified key; otherwise, <see langword="null"/>.</returns>
        public T Get(string name)
        {
            return BaseGet(name) as T;
        }

        /// <summary>
        /// Determines if the name exists in the collection.
        /// </summary>
        /// <param name="name">The name to search.</param>
        /// <returns><see langword="true"/> if the name is contained in the collection; otherwise, <see langword="false"/>.</returns>
        public bool Contains(string name)
        {
            return BaseGet(name) != null;
        }

        /// <summary>
        /// Remove the named element from the collection.
        /// </summary>
        /// <param name="name">The name of the element to remove.</param>
        public void Remove(string name)
        {
            BaseRemove(name);
        }

        /// <summary>
        /// Clear the collection.
        /// </summary>
        public void Clear()
        {
            BaseClear();
        }

        /// <summary>
        /// Returns an enumerator that iterates through the collection. 
        /// </summary>
        /// <returns>An enumerator that iterates through the collection.</returns>
        public new IEnumerator<T> GetEnumerator()
        {
            return new GenericEnumeratorWrapper<T>(base.GetEnumerator());
        }

        /// <summary>
        /// Creates a new instance of a <typeparamref name="T"/> object.
        /// </summary>
        /// <returns>A new <see cref="ConfigurationElement"/>.</returns>
        protected override ConfigurationElement CreateNewElement()
        {
            return new T();
        }

        /// <summary>
        /// Gets the element key for a specified configuration element when overridden in a derived class. 
        /// </summary>
        /// <param name="element">The <see cref="ConfigurationElement"/> to return the key for. </param>
        /// <returns>An <see cref="Object"/> that acts as the key for the specified <see cref="ConfigurationElement"/>.</returns>
        protected override object GetElementKey(ConfigurationElement element)
        {
            T namedElement = (T)element;
            return namedElement.Name;
        }

        void IMergeableConfigurationElementCollection.ResetCollection(IEnumerable<ConfigurationElement> configurationElements)
        {
            foreach (T element in this)
            {
                Remove(element.Name);
            }

            foreach (T element in configurationElements.Reverse())
            {
                base.BaseAdd(0, element);
            }
        }

        ConfigurationElement IMergeableConfigurationElementCollection.CreateNewElement(Type configurationType)
        {
            return (ConfigurationElement)Activator.CreateInstance(configurationType);
        }
    }






    public interface IMergeableConfigurationElementCollection
    {
        /// <summary>
        /// Resets the elements in the <see cref="ConfigurationElementCollection"/> to the <see cref="ConfigurationElement"/>s passed as <paramref name="configurationElements" />.
        /// </summary>
        /// <param name="configurationElements">The new contents of this <see cref="ConfigurationElementCollection"/>.</param>
        void ResetCollection(IEnumerable<ConfigurationElement> configurationElements);

        /// <summary>
        /// Creates a new <see cref="ConfigurationElement"/> for the specifies <paramref name="configurationType" />.
        /// </summary>
        /// <param name="configurationType">The type of <see cref="ConfigurationElement"/> that should be created.</param>
        ConfigurationElement CreateNewElement(Type configurationType);
    }







}
