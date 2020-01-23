using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using System.Globalization;
using System.Xml;
using dega.Configuration;
using System.ComponentModel;
using dega.Properties;
using System.Resources;

namespace dega.Common.Configuration
{
    /// <summary>
    /// Represents a collection of <see cref="NameTypeConfigurationElement"/> objects.
    /// </summary>
    /// <typeparam name="T">The type of <see cref="NameTypeConfigurationElement"/> object this collection contains.</typeparam>
    /// <typeparam name="TCustomElementData">The type used for Custom configuration elements in this collection.</typeparam>
    public class CustomConfigurationElementCollection<T, TCustomElementData> : NameTypeConfigurationElementCollection<ConfigurationSourceElement, ConfigurationSourceElement>
        where T : NameTypeConfigurationElement, new()
        where TCustomElementData : T, new()
    {
        /// <summary>
        /// Get the configuration object for each <see cref="NameTypeConfigurationElement"/> object in the collection.
        /// </summary>
        /// <param name="reader">The <see cref="XmlReader"/> that is deserializing the element.</param>
        protected override Type RetrieveConfigurationElementType(XmlReader reader)
        {
            const string TypeAttributeName = "type";
            Type configurationElementType = null;
            if (reader.AttributeCount > 0)
            {
                // expect the first attribute to be the name
                for (bool go = reader.MoveToFirstAttribute(); go; go = reader.MoveToNextAttribute())
                {
                    if (TypeAttributeName.Equals(reader.Name))
                    {
                        Type providerType = Type.GetType(reader.Value, false);
                        if (providerType == null)
                        {
                            throw new ConfigurationErrorsException(string.Format(CultureInfo.CurrentCulture, Resources.ConfigurationSourceInvalidTypeErrorMessage, reader.Value, reader[0]));
                        }

                        Attribute attribute = Attribute.GetCustomAttribute(providerType, typeof(ConfigurationElementTypeAttribute));
                        if (attribute == null)
                        {
                            throw new ConfigurationErrorsException(string.Format(CultureInfo.CurrentCulture, Resources.ExceptionNoConfigurationElementAttribute, providerType.Name));
                        }

                        configurationElementType = ((ConfigurationElementTypeAttribute)attribute).ConfigurationType;
                        break;
                    }
                }

                if (configurationElementType == null)
                {
                    throw new ConfigurationErrorsException(string.Format(CultureInfo.CurrentCulture, Resources.ExceptionNoTypeAttribute, reader.Name));
                }

                // cover the traces ;)
                reader.MoveToElement();
            }
            return configurationElementType;
        }
    }





    /// <summary>
    /// Represents a collection of <see cref="NameTypeConfigurationElement"/> objects.
    /// </summary>
    /// <typeparam name="T">The type of <see cref="NameTypeConfigurationElement"/> object this collection contains.</typeparam>
    /// <typeparam name="TCustomElementData">The type used for Custom configuration elements in this collection.</typeparam>
    public class NameTypeConfigurationElementCollection<T, TCustomElementData> : PolymorphicConfigurationElementCollection<T>
        where T : NameTypeConfigurationElement, new()
        where TCustomElementData : T, new()
    {
        private const string typeAttribute = "type";

        /// <summary>
        /// Get the configuration object for each <see cref="NameTypeConfigurationElement"/> object in the collection.
        /// </summary>
        /// <param name="reader">The <see cref="XmlReader"/> that is deserializing the element.</param>
        protected override Type RetrieveConfigurationElementType(XmlReader reader)
        {
            Type configurationElementType = null;
            if (reader.AttributeCount > 0)
            {
                // expect the first attribute to be the name
                for (bool go = reader.MoveToFirstAttribute(); go; go = reader.MoveToNextAttribute())
                {
                    if (typeAttribute.Equals(reader.Name))
                    {
                        Type providerType = Type.GetType(reader.Value, false);
                        if (providerType == null)
                        {
                            configurationElementType = typeof(TCustomElementData);
                            break;
                        }

                        Attribute attribute = Attribute.GetCustomAttribute(providerType, typeof(ConfigurationElementTypeAttribute));
                        if (attribute == null)
                        {
                            throw new ConfigurationErrorsException(string.Format(CultureInfo.CurrentCulture, Resources.ExceptionNoConfigurationElementAttribute, providerType.Name));
                        }

                        configurationElementType = ((ConfigurationElementTypeAttribute)attribute).ConfigurationType;
                        break;
                    }
                }

                if (configurationElementType == null)
                {
                    throw new ConfigurationErrorsException(string.Format(CultureInfo.CurrentCulture, Resources.ExceptionNoTypeAttribute, reader.Name));
                }

                // cover the traces ;)
                reader.MoveToElement();
            }
            return configurationElementType;
        }
    }



    public class NameTypeConfigurationElement : NamedConfigurationElement, IObjectWithNameAndType
    {
        private AssemblyQualifiedTypeNameConverter typeConverter = new AssemblyQualifiedTypeNameConverter();

        /// <summary>
        /// Name of the property that holds the type of <see cref="EnterpriseLibrary.Common.Configuration.NameTypeConfigurationElement"/>.
        /// </summary>
        public const string typeProperty = "type";

        /// <summary>
        /// Intialzie an instance of the <see cref="NameTypeConfigurationElement"/> class.
        /// </summary>
        public NameTypeConfigurationElement()
        {
        }

        /// <summary>
        /// Initialize an instance of the <see cref="NameTypeConfigurationElement"/> class
        /// </summary>
        /// <param name="name">The name of the element.</param>
        /// <param name="type">The <see cref="Type"/> that this element is the configuration for.</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors", Justification = "As designed")]
        public NameTypeConfigurationElement(string name, Type type)
            : base(name)
        {
            this.Type = type;
        }

        /// <summary>
        /// Gets or sets the <see cref="Type"/> the element is the configuration for.
        /// </summary>
        /// <value>
        /// The <see cref="Type"/> the element is the configuration for.
        /// </value>
        public virtual Type Type
        {
            get { return (Type)typeConverter.ConvertFrom(TypeName); }
            set { TypeName = typeConverter.ConvertToString(value); }
        }

        /// <summary>
        /// Gets or sets the fully qualified name of the <see cref="Type"/> the element is the configuration for.
        /// </summary>
        /// <value>
        /// the fully qualified name of the <see cref="Type"/> the element is the configuration for.
        /// </value>
        [ConfigurationProperty(typeProperty, IsRequired = true)]
        [Browsable(true)]
        [DesignTimeReadOnly(true)]
        [ResourceDescription(typeof(DesignResources), "NameTypeConfigurationElementTypeNameDescription")]
        [ResourceDisplayName(typeof(DesignResources), "NameTypeConfigurationElementTypeNameDisplayName")]
        [Validation(CommonDesignTime.ValidationTypeNames.TypeValidator)]
        [ViewModel(CommonDesignTime.ViewModelTypeNames.TypeNameProperty)]
        public virtual string TypeName
        {
            get { return (string)this[typeProperty]; }
            set { this[typeProperty] = value; }
        }

        internal ConfigurationPropertyCollection MetadataProperties
        {
            get { return base.Properties; }
        }
    }


    public class AssemblyQualifiedTypeNameConverter : ConfigurationConverterBase
    {
        /// <summary>
        /// Returns the assembly qualified name for the passed in Type.
        /// </summary>
        /// <param name="context">The container representing this System.ComponentModel.TypeDescriptor.</param>
        /// <param name="culture">Culture info for assembly</param>
        /// <param name="value">Value to convert.</param>
        /// <param name="destinationType">Type to convert to.</param>
        /// <returns>Assembly Qualified Name as a string</returns>
        public override object ConvertTo(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value, Type destinationType)
        {
            if (value != null)
            {
                Type typeValue = value as Type;
                if (typeValue == null)
                {
                    throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, Resources.ExceptionCanNotConvertType, typeof(Type).Name));
                }

                if (typeValue != null) return (typeValue).AssemblyQualifiedName;
            }
            return null;
        }

        /// <summary>
        /// Returns a type based on the assembly qualified name passed in as data.
        /// </summary>
        /// <param name="context">The container representing this System.ComponentModel.TypeDescriptor.</param>
        /// <param name="culture">Culture info for assembly.</param>
        /// <param name="value">Data to convert.</param>
        /// <returns>Type of the data</returns>
        public override object ConvertFrom(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value)
        {
            string stringValue = (string)value;
            if (!string.IsNullOrEmpty(stringValue))
            {
                Type result = Type.GetType(stringValue, false);
                if (result == null)
                {
                    throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, Resources.ExceptionInvalidType, stringValue));
                }

                return result;
            }
            return null;
        }
    }





    /// <summary>
    /// Attribute class used to specify a specific View Model derivement or visual representation to be used on the target element.
    /// </summary>
    /// <remarks>
    /// 
    /// <para>The View Model Type should derive from the ElementViewModel or Property class in the Configuration.Design assembly. <br/>
    /// As this attribute can be applied to the configuration directly and we dont want to force a dependency on the Configuration.Design assembly <br/>
    /// You can specify the View Model Type in a loosy coupled fashion, passing a qualified name of the type.</para>
    ///
    /// </remarks>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Property, AllowMultiple = false)]
    public sealed class ViewModelAttribute : Attribute
    {
        private readonly string modelTypeName;

        ///<summary>
        /// Initializes a new instance of the <see cref="ViewModelAttribute"/> class.
        ///</summary>
        ///<param name="modelType">The type of the View Model that should be used for the annotated element.</param>
        public ViewModelAttribute(Type modelType)
            : this(modelType != null ? modelType.AssemblyQualifiedName : null)
        { }


        ///<summary>
        /// Initializes a new instance of the <see cref="ViewModelAttribute"/> class.
        ///</summary>
        ///<param name="modelTypeName">The type name of the View Model that should be used for the annotated element.</param>
        public ViewModelAttribute(string modelTypeName)
        {
            if (String.IsNullOrEmpty(modelTypeName)) throw new ArgumentException(Resources.ExceptionStringNullOrEmpty, "modelTypeName");

            this.modelTypeName = modelTypeName;
        }

        ///<summary>
        /// Gets the View Model Type that should be used to bind the annotated element to its view.
        ///</summary>
        public Type ModelType
        {
            get { return Type.GetType(modelTypeName, true, true); }
        }


    }


    /// <summary>
    /// Class that contains common type names and metadata used by the designtime.
    /// </summary>
    public static class CommonDesignTime
    {
        /// <summary>
        /// Class that contains common command types used by the designtime.
        /// </summary>
        public static class CommandTypeNames
        {
            /// <summary>
            /// Type name of the WizardCommand class, declared in the Configuration.DesignTime Assembly.
            /// </summary>
            public static string WizardCommand = "dega.Configuration.Design.ViewModel.Commands.WizardCommand, dega.Configuration.DesignTime";

            /// <summary>
            /// Type name of the AddSatelliteProviderCommand class, declared in the Configuration.DesignTime Assembly.
            /// </summary>
            public const string AddSatelliteProviderCommand = "dega.Configuration.Design.ViewModel.Commands.AddSatelliteProviderCommand, dega.Configuration.DesignTime";

            /// <summary>
            /// Type name of the AddApplicationBlockCommand class, declared in the Configuration.DesignTime Assembly.
            /// </summary>
            public const string AddApplicationBlockCommand = "dega.Configuration.Design.ViewModel.Commands.AddApplicationBlockCommand, dega.Configuration.DesignTime";

            /// <summary>
            /// Type name of the TypePickingCollectionElementAddCommand class, declared in the Configuration.DesignTime Assembly.
            /// </summary>
            public const string AddProviderUsingTypePickerCommand = "dega.Configuration.Design.ViewModel.TypePickingCollectionElementAddCommand, dega.Configuration.DesignTime";

            /// <summary>
            /// Type name of the ExportAdmTemplateCommand class, declared in the Configuration.DesignTime Assembly.
            /// </summary>
            public const string ExportAdmTemplateCommand = "dega.Configuration.Design.ViewModel.BlockSpecifics.ExportAdmTemplateCommand, dega.Configuration.DesignTime";

            /// <summary>
            /// Type name of the HiddenCommand class, declared in the Configuration.DesignTime Assembly.
            /// </summary>
            public const string HiddenCommand = "dega.Configuration.Design.ViewModel.BlockSpecifics.Commands.HiddenCommand, dega.Configuration.DesignTime";

            /// <summary>
            /// Type name of the AddInstrumentationBlockCommand class, declared in the Configuration.DesignTime Assembly.
            /// </summary>
            public const string AddInstrumentationApplicationBlockCommand = "dega.Configuration.Design.ViewModel.BlockSpecifics.AddInstrumentationBlockCommand, dega.Configuration.DesignTime";
        }

        /// <summary>
        /// Class that contains common editor types used by the designtime.
        /// </summary>
        public static class EditorTypes
        {
            /// <summary>
            /// Type name of the DatePickerEditor class, declared in the Configuration.DesignTime Assembly.
            /// </summary>
            public const string DatePickerEditor = "dega.Configuration.Design.ComponentModel.Editors.DatePickerEditor, dega.Configuration.DesignTime";

            /// <summary>
            /// Type name of the ElementCollectionEditor, declared class in the Configuration.DesignTime Assembly.
            /// </summary>
            public const string CollectionEditor = "dega.Configuration.Design.ComponentModel.Editors.ElementCollectionEditor, dega.Configuration.DesignTime";

            /// <summary>
            /// Type name of the UITypeEditor class, declared in the System.Drawing Assembly.
            /// </summary>
            public const string UITypeEditor = "System.Drawing.Design.UITypeEditor, System.Drawing, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a";

            /// <summary>
            /// Type name of the TypeSelectionEditor, declared class in the Configuration.DesignTime Assembly.
            /// </summary>
            public const string TypeSelector = "dega.Configuration.Design.ComponentModel.Editors.TypeSelectionEditor, dega.Configuration.DesignTime";

            /// <summary>
            /// Type name of the FilteredFileNameEditor, declared class in the Configuration.DesignTime Assembly.
            /// </summary>
            public const string FilteredFilePath = "dega.Configuration.Design.ComponentModel.Editors.FilteredFileNameEditor, dega.Configuration.DesignTime";

            /// <summary>
            /// Type name of the FrameworkElement, declared class in the PresentationFramework Assembly.
            /// </summary>
            public const string FrameworkElement = "System.Windows.FrameworkElement, PresentationFramework, Version=3.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35";

            /// <summary>
            /// Type name of the MultilineTextEditor class, declared in the Configuration.DesignTime Assembly.
            /// </summary>
            public const string MultilineText = "dega.Configuration.Design.ComponentModel.Editors.MultilineTextEditor, dega.Configuration.DesignTime";

            /// <summary>
            /// Type name of the PopupTextEditor class, declared in the Configuration.DesignTime Assembly.
            /// </summary>
            public const string PopupTextEditor = "dega.Configuration.Design.ComponentModel.Editors.PopupTextEditor, dega.Configuration.DesignTime";

            /// <summary>
            /// Type name of the FlagsEditor class, declared in the Configuration.DesignTime Assembly.
            /// </summary>
            public const string Flags = "dega.Configuration.Design.ComponentModel.Editors.FlagsEditor, dega.Configuration.DesignTime";

            /// <summary>
            /// Type name of the RegexTypeEditor class, declared in the System.Design Assembly.
            /// </summary>
            public const string RegexTypeEditor = "System.Web.UI.Design.WebControls.RegexTypeEditor, System.Design, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a";

            /// <summary>
            /// Type name of the ConnectionStringEditor class, declared in the System.Design Assembly.
            /// </summary>
            public const string ConnectionStringEditor = "System.Web.UI.Design.ConnectionStringEditor, System.Design, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a";

            /// <summary>
            /// Type name of the TemplateEditor class, declared in the Configuration.DesignTime Assembly.
            /// </summary>
            public const string TemplateEditor = "dega.Logging.Configuration.Design.Formatters.TemplateEditor, dega.Configuration.DesignTime";

            /// <summary>
            /// Type name of the IEnvironmentalOverridesEditor interface, declared in the Configuration.DesignTime Assembly.
            /// </summary>
            public const string OverridesEditor = "dega.Configuration.Design.ViewModel.BlockSpecifics.IEnvironmentalOverridesEditor, dega.Configuration.DesignTime";
        }

        /// <summary>
        /// Class that contains common view model types used by the designtime.
        /// </summary>
        public static class ViewModelTypeNames
        {
            /// <summary>
            /// Type name of the TypeNameProperty class, declared in the Configuration.DesignTime Assembly.
            /// </summary>
            public const string TypeNameProperty = "dega.Configuration.Design.ViewModel.TypeNameProperty, dega.Configuration.DesignTime";

            /// <summary>
            /// Type name of the ConfigurationProperty class, declared in the Configuration.DesignTime Assembly.
            /// </summary>
            public const string ConfigurationPropertyViewModel =
                "dega.Configuration.Design.ViewModel.ConfigurationProperty, dega.Configuration.DesignTime";

            /// <summary>
            /// Type name of the SectionViewModel class, declared in the Configuration.DesignTime Assembly.
            /// </summary>
            public const string SectionViewModel = "dega.Configuration.Design.ViewModel.SectionViewModel, dega.Configuration.DesignTime";

            /// <summary>
            /// Type name of the CollectionEditorContainedElementProperty class, declared in the Configuration.DesignTime Assembly.
            /// </summary>
            public const string CollectionEditorContainedElementProperty =
                "dega.Configuration.Design.ViewModel.CollectionEditorContainedElementProperty, dega.Configuration.DesignTime";

            /// <summary>
            /// Type name of the CollectionEditorContainedElementReferenceProperty class, declared in the Configuration.DesignTime Assembly.
            /// </summary>
            public const string CollectionEditorContainedElementReferenceProperty =
                "dega.Configuration.Design.ViewModel.CollectionEditorContainedElementReferenceProperty, dega.Configuration.DesignTime";

            /// <summary>
            /// Type name of the RedirectedSectionSourceProperty class, declared in the Configuration.DesignTime Assembly.
            /// </summary>
            public const string RedirectedSectionSourceProperty =
                "dega.Configuration.Design.ViewModel.BlockSpecifics.RedirectedSectionSourceProperty, dega.Configuration.DesignTime";
        }

        /// <summary>
        /// Class that contains common converter types used by the designtime runtime.
        /// </summary>
        public static class ConverterTypeNames
        {
            /// <summary>
            /// Type name of the RedirectedSectionNameConverter class, declared in the Configuration.DesignTime Assembly.
            /// </summary>
            public const string RedirectedSectionNameConverter =
                "dega.Configuration.Design.ComponentModel.Converters.RedirectedSectionNameConverter, dega.Configuration.DesignTime";

        }

        /// <summary>
        /// Class that contains common metadata classes used by the designtime.<br/>
        /// This class supports the Enterprise Library infrastructure and is not intended to be used directly from your code.
        /// </summary>
        public static class MetadataTypes
        {
            /// <summary>This class supports the Enterprise Library infrastructure and is not intended to be used directly from your code.</summary>
            [RegisterAsMetadataType(typeof(RedirectedSectionElement))]
            public abstract class RedirectedSectionElementMetadata
            {

                /// <summary>This property supports the Enterprise Library infrastructure and is not intended to be used directly from your code.</summary>
                [TypeConverter(ConverterTypeNames.RedirectedSectionNameConverter)]
                public string Name
                {
                    get;
                    set;
                }
            }
        }

        /// <summary>
        /// Class that contains common validation types used by the designtime.
        /// </summary>
        public static class ValidationTypeNames
        {
            /// <summary>
            /// Type name of the FileWritableValidator class, declared in the Configuration.DesignTime Assembly.
            /// </summary>
            public const string FileWritableValidator = "dega.Configuration.Design.Validation.FileWritableValidator, dega.Configuration.DesignTime";

            /// <summary>
            /// Type name of the FilePathValidator class, declared in the Configuration.DesignTime Assembly.
            /// </summary>
            public const string FileValidator = "dega.Configuration.Design.Validation.FilePathValidator, dega.Configuration.DesignTime";

            /// <summary>
            /// Type name of the FilePathExistsValidator class, declared in the Configuration.DesignTime Assembly.
            /// </summary>
            public const string PathExistsValidator = "dega.Configuration.Design.Validation.FilePathExistsValidator, dega.Configuration.DesignTime";

            /// <summary>
            /// Type name of the RequiredFieldValidator class, declared in the Configuration.DesignTime Assembly.
            /// </summary>
            public const string RequiredFieldValidator = "dega.Configuration.Design.Validation.RequiredFieldValidator, dega.Configuration.DesignTime";

            /// <summary>
            /// Type name of the TypeValidator class, declared in the Configuration.DesignTime Assembly.
            /// </summary>
            public const string TypeValidator =
                "dega.Configuration.Design.Validation.TypeValidator, dega.Configuration.DesignTime";

            /// <summary>
            /// Type name of the SelectedSourceValidator class, declared in the Configuration.DesignTime Assembly.
            /// </summary>
            public const string SelectedSourceValidator =
                "dega.Configuration.Design.ViewModel.BlockSpecifics.SelectedSourceValidator, dega.Configuration.DesignTime";

            /// <summary>
            /// Type name of the NameValueCollectionValidator class, declared in the Configuration.DesignTime Assembly.
            /// </summary>
            public const string NameValueCollectionValidator = "dega.Configuration.Design.Validation.NameValueCollectionValidator, dega.Configuration.DesignTime";
        }

        /// <summary>
        /// Type names for well known Enterprise Library <see cref="System.Configuration.ConfigurationSection"/> elements.
        /// </summary>
        public static class SectionType
        {
            /// <summary>
            /// Type name for the LoggingSettings section.
            /// </summary>
            public const string LoggingSettings = "dega.Logging.Configuration.LoggingSettings, dega.Logging";

            /// <summary>
            /// Type name for the DatabaseSettings section.
            /// </summary>
            public const string DatabaseSettings = "dega.Data.Configuration.DatabaseSettings, dega.Data";

            /// <summary>
            /// Type name for the ExceptionHandlingSettings section.
            /// </summary>
            public const string ExceptionHandlingSettings = "dega.ExceptionHandling.Configuration.ExceptionHandlingSettings, dega.ExceptionHandling";

        }
    }





    /// <summary>
    /// Represents a localized <see cref="CategoryAttribute"/>.
    /// </summary>
    [AttributeUsage(AttributeTargets.All)]
    public sealed class ResourceCategoryAttribute : CategoryAttribute
    {
        private readonly Type resourceType;
        private static ResourceCategoryAttribute general;

        static ResourceCategoryAttribute()
        {
            general = new ResourceCategoryAttribute(typeof(ResourceCategoryAttribute), "CategoryGeneral");
        }

        /// <summary>
        /// Initialize a new instance of the <see cref="SRCategoryAttribute"/> class with the <see cref="Type"/> containing the resources and the resource name.
        /// </summary>
        /// <param name="category">The resources string name.</param>
        /// <param name="resourceType">The <see cref="Type"/> containing the resource strings.</param>
        public ResourceCategoryAttribute(Type resourceType, string category)
            : base(category)
        {
            this.resourceType = resourceType;
        }

        /// <summary>
        /// Gets the type that contains the resources.
        /// </summary>
        /// <value>
        /// The type that contains the resources.
        /// </value>
        public Type ResourceType
        {
            get { return resourceType; }
        }

        /// <summary>
        /// Gets the localized string based on the key.
        /// </summary>
        /// <param name="value">The key to the string resources.</param>
        /// <returns>The localized string.</returns>
        protected override string GetLocalizedString(string value)
        {
            return ResourceStringLoader.LoadString(resourceType.FullName, value, resourceType.Assembly);
        }

        /// <summary>
        /// Returns a localized <see cref="ResourceCategoryAttribute"/> for the General category.
        /// </summary>
        public static ResourceCategoryAttribute General
        {
            get { return general; }
        }

    }





    /// <summary>
    /// Attribute class used to indicate whether a property can be overwritten per environment.<br/>
    /// The default behavior is that any property can be overwritten.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Class, AllowMultiple = false)]
    public sealed class EnvironmentalOverridesAttribute : Attribute
    {
        private readonly bool canOverride;

        /// <summary>
        /// Initializes a new instance of the <see cref="EnvironmentalOverridesAttribute"/> class.
        /// </summary>
        /// <param name="canOverride"><see langword="true"/> to specify the property can be overwritten per environment. Otherwise <see langword="false"/>.</param>
        public EnvironmentalOverridesAttribute(bool canOverride)
        {
            this.canOverride = canOverride;
        }

        /// <summary>
        /// <see langword="true"/> if the property can be overwritten per environment. Otherwise <see langword="false"/>.
        /// </summary>
        public bool CanOverride
        {
            get { return canOverride; }
        }

        /// <summary>
        /// Specifies a custom property type for the overrides property.<br/>
        /// </summary>
        public Type CustomOverridesPropertyType
        {
            get;
            set;
        }

        /// <summary>
        /// Specifies a <see cref="TypeConverter"/> that should be used to serialize the overriden value to the delta configuration file. <br/>
        /// This can be used to overwrite a property that doesnt implement <see cref="IConvertible"/>.  <br/>
        /// </summary>
        public Type StorageConverterType
        {
            get;
            set;
        }
    }




    [AttributeUsage(AttributeTargets.Class)]
    public class RegisterAsMetadataTypeAttribute : Attribute
    {
        private readonly Type targetType;

        /// <summary>
        /// Creates a new instance of <see cref="RegisterAsMetadataTypeAttribute"/>.
        /// </summary>
        /// <param name="targetType">The type for which this class should contain metadata attributes.</param>
        public RegisterAsMetadataTypeAttribute(Type targetType)
        {
            this.targetType = targetType;
        }

        /// <summary>
        /// Gets the type for which this class should contain metadata attributes.
        /// </summary>
        /// <value>
        /// The type for which this class should contain metadata attributes.
        /// </value>
        public Type TargetType
        {
            get { return targetType; }
        }
    }



    /// <summary>
    /// Configuration element for a redirected section.<br/>
    /// The <see cref="NamedConfigurationElement.Name"/> property is used to identify the redireced section, based on its section name.<br/>
    /// </summary>
    /// <seealso cref="ConfigurationSourceSection"/>
    [ResourceDescription(typeof(DesignResources), "RedirectedSectionElementDescription")]
    [ResourceDisplayName(typeof(DesignResources), "RedirectedSectionElementDisplayName")]
    public class RedirectedSectionElement : NamedConfigurationElement
    {
        private const string sourceNameProperty = "sourceName";

        /// <summary>
        /// Gets the name of the <see cref="ConfigurationSourceElement"/> which contains the configuration section.
        /// </summary>
        /// <value>
        /// The name of the <see cref="ConfigurationSourceElement"/> which contains the configuration section.
        /// </value>
        [ConfigurationProperty(sourceNameProperty, IsRequired = true)]
        [ResourceDescription(typeof(DesignResources), "RedirectedSectionElementSourceNameDescription")]
        [ResourceDisplayName(typeof(DesignResources), "RedirectedSectionElementSourceNameDisplayName")]
        //[Reference(typeof(CustomConfigurationElementCollection<ConfigurationSourceElement, ConfigurationSourceElement>), typeof(ConfigurationSourceElement))]
        [ViewModel(CommonDesignTime.ViewModelTypeNames.RedirectedSectionSourceProperty)]
        [EnvironmentalOverrides(false)]
        public string SourceName
        {
            get { return (string)this[sourceNameProperty]; }
            set { this[sourceNameProperty] = value; }
        }

    }






    /// <summary>
	/// Represents the configuration settings that describe an <see cref="IConfigurationSource"/>.
	/// </summary>
    [Browsable(false)]
    [Command(ConfigurationSourcesDesignTime.CommandTypeNames.ConfigurationSourceElementDeleteCommand,
        CommandPlacement = CommandPlacement.ContextDelete,
        Replace = CommandReplacement.DefaultDeleteCommandReplacement)]
    public class ConfigurationSourceElement : NameTypeConfigurationElement
    {
        /// <summary>
		/// Initializes a new instance of the <see cref="ConfigurationSourceElement"/> class with default values.
		/// </summary>
        public ConfigurationSourceElement()
        {
        }

        /// <summary>
		/// Initializes a new instance of the <see cref="ConfigurationSourceElement"/> class with a name and an type.
		/// </summary>
        /// <param name="name">The instance name.</param>
		/// <param name="type">The type for the represented <see cref="IConfigurationSource"/>.</param>
        public ConfigurationSourceElement(string name, Type type)
            : base(name, type)
        {
        }

        /// <summary>
        /// Returns a new <see cref="IConfigurationSource"/> configured with the receiver's settings.
        /// </summary>
        /// <returns>A new configuration source.</returns>
        public virtual IConfigurationSource CreateSource()
        {
            throw new ConfigurationErrorsException(Resources.ExceptionBaseConfigurationSourceElementIsInvalid);
        }

        ///<summary>
        /// Returns a new <see cref="IDesignConfigurationSource"/> configured based on this configuration element.
        ///</summary>
        ///<returns>Returns a new <see cref="IDesignConfigurationSource"/> or null if this source does not have design-time support.</returns>
        public virtual IDesignConfigurationSource CreateDesignSource(IDesignConfigurationSource rootSource)
        {

            return null;
        }
    }



    public interface IDesignConfigurationSource : IProtectedConfigurationSource
    {
        ///<summary>
        /// Retrieves a local section from the configuration source.
        ///</summary>
        ///<param name="sectionName"></param>
        ///<returns>The configuration section or null if it does not contain the section.</returns>
        ConfigurationSection GetLocalSection(string sectionName);

        /// <summary>
        /// Adds a local section to the configuration source.
        /// </summary>
        /// <param name="sectionName"></param>
        /// <param name="section"></param>
        void AddLocalSection(string sectionName, ConfigurationSection section);

        ///<summary>
        /// Removes a local section from the configuration source.
        ///</summary>
        ///<param name="sectionName"></param>
        void RemoveLocalSection(string sectionName);
    }




    /// <summary>
    /// Attribute used to decorate a designtime View Model element with an executable command. E.g. a context menu item that allows
    /// the user to perform an action in the elements context.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Property | AttributeTargets.Assembly, AllowMultiple = true)]
    public class CommandAttribute : Attribute
    {
        private Guid typeId;
        private string title;
        private bool resourceLoaded;

        /// <summary>
        /// Initializes a new instance of the <see cref="CommandAttribute"/> class, specifying the Command Model Type.
        /// </summary>
        /// <remarks>
        /// The Command Model Type should derive from the CommandModel class in the Configuration.Design assembly. <br/>
        /// As this attribute can be applied to the configuration directly and we dont want to force a dependency on the Configuration.Design assembly <br/>
        /// You can specify the Command Model Type in a loosy coupled fashion.
        /// </remarks>
        /// <param name="commandModelTypeName">The fully qualified name of the Command Model Type.</param>
        public CommandAttribute(string commandModelTypeName)
        {
            if (string.IsNullOrEmpty(commandModelTypeName)) throw new ArgumentException(Resources.ExceptionStringNullOrEmpty, "commandModelTypeName");

            this.CommandModelTypeName = commandModelTypeName;
            this.Replace = CommandReplacement.NoCommand;
            this.CommandPlacement = CommandPlacement.ContextCustom;

            this.typeId = Guid.NewGuid();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CommandAttribute"/> class, specifying the Command Model Type.
        /// </summary>
        /// <remarks>
        /// The Command Model Type should derive from the CommandModel class in the Configuration.Design assmbly. <br/>
        /// As this attribute can be applied to the configuration directly and we dont want to force a dependency on the Configuration.Design assembly <br/>
        /// You can specify the Command Model Type in a loosy coupled fashion.
        /// </remarks>
        /// <param name="commandModelType">The Command Model Type.</param>
        public CommandAttribute(Type commandModelType)
            : this(commandModelType != null ? commandModelType.AssemblyQualifiedName : string.Empty)
        {
        }

        /// <summary>
        /// Gets or sets the name of the resource, used to return a localized title that will be shown for this command in the UI (User Interface).
        /// </summary>
        public string TitleResourceName { get; set; }

        /// <summary>
        /// Gets or sets the type of the resource, used to return a localized title that will be shown for this command in the UI (User Interface).
        /// </summary>
        public Type TitleResourceType { get; set; }

        /// <summary>
        /// Gets the title that will be shown for this command in the UI (User Interface).
        /// </summary>
        public virtual string Title
        {
            get
            {
                if (TitleResourceName != null && TitleResourceType != null)
                {
                    EnsureTitleLoaded();
                }
                return title;
            }
            set
            {
                title = value;
            }
        }

        private void EnsureTitleLoaded()
        {
            if (resourceLoaded) return;

            var rm = new ResourceManager(TitleResourceType);

            try
            {
                title = rm.GetString(TitleResourceName);
            }
            catch (MissingManifestResourceException)
            {
                title = TitleResourceName;
            }

            resourceLoaded = true;
        }

        /// <summary>
        /// Gets or sets the <see cref="CommandReplacement"/> options for this command.
        /// </summary>
        public CommandReplacement Replace { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="CommandPlacement"/> options for this command.
        /// </summary>
        public CommandPlacement CommandPlacement { get; set; }

        /// <summary>
        /// Gets or Sets the Command Model Type Name for this command. <br/>
        /// The Command Model Type will be used at runtime to display and execute the command.<br/>
        /// Command Model Types should derive from the CommandModel class in the Configuration.Design assembly. 
        /// </summary>
        public string CommandModelTypeName { get; set; }

        /// <summary>
        /// Gets the Command Model Type for this command. <br/>
        /// The Command Model Type will be used at runtime to display and execute the command.<br/>
        /// Command Model Types should derive from the CommandModel class in the Configuration.Design assembly. 
        /// </summary>
        public Type CommandModelType
        {
            get { return Type.GetType(CommandModelTypeName, true); }
        }

        /// <summary>
        /// Defines the keyboard gesture for this command.
        /// </summary>
        /// <example>
        ///     command.KeyGesture = "Ctrl+1";
        /// </example>
        public string KeyGesture { get; set; }

        /// <summary>
        /// When implemented in a derived class, gets a unique identifier for this <see cref="T:System.Attribute"/>.
        /// </summary>
        /// <returns>
        /// An <see cref="T:System.Object"/> that is a unique identifier for the attribute.
        /// </returns>
        /// <filterpriority>2</filterpriority>
        public override object TypeId
        {
            get
            {
                //the identity of a CommandAttribute is either the command it replaces or unique value per instance.
                if (Replace == CommandReplacement.NoCommand)
                {
                    return typeId;
                }
                return Replace;
            }
        }

    }

    /// <summary>
    /// Specifies whether a command replaces a default command.
    /// </summary>
    public enum CommandReplacement
    {
        /// <summary>
        /// Specifies that the command should be used to replace the default add command.
        /// </summary>
        DefaultAddCommandReplacement,

        /// <summary>
        /// Specifies that the command should be used to replace the default delete command.
        /// </summary>
        DefaultDeleteCommandReplacement,

        /// <summary>
        /// Specifies that the command should not be used to replace any default command.
        /// </summary>
        NoCommand
    }

    /// <summary>
    /// Specifies the placement of a command. This can be either a top level menu, e.g.: <see cref="CommandPlacement.FileMenu"/> or <see cref="CommandPlacement.BlocksMenu"/> or
    /// a context menu, e.g.: <see cref="CommandPlacement.ContextAdd"/>,  <see cref="CommandPlacement.ContextCustom"/>.
    /// </summary>
    public enum CommandPlacement
    {
        /// <summary>
        /// Specifies placement of the command in the top level file menu.
        /// </summary>
        FileMenu,

        /// <summary>
        /// Specifies placement of the command in the top level blocks menu.
        /// </summary>
        BlocksMenu,

        /// <summary>
        /// Specifies placement of the command in the top level wizards menu.
        /// </summary>
        WizardMenu,

        /// <summary>
        /// Specifies placement of the command in the contextual add menu for an configuration element.
        /// </summary>
        ContextAdd,

        /// <summary>
        /// Specifies placement of the command in the custom commands menu for an configuration element.
        /// </summary>
        ContextCustom,

        /// <summary>
        /// Specifies placement of the command in the delete commands menu for an configuration element.
        /// </summary>
        ContextDelete,

    }



    /// <summary>
    /// This class supports the Enterprise Library infrastructure and is not intended to be used directly from your code.
    /// </summary>
    internal static class ConfigurationSourcesDesignTime
    {
        /// <summary>
        /// This class supports the Enterprise Library infrastructure and is not intended to be used directly from your code.
        /// </summary>
        public static class ViewModelTypeNames
        {
            /// <summary>
            /// This field supports the Enterprise Library infrastructure and is not intended to be used directly from your code.
            /// </summary>
            public const string ConfigurationSourcesSectionViewModel = "EnterpriseLibrary.Configuration.Design.ViewModel.BlockSpecifics.ConfigurationSourceSectionViewModel, EnterpriseLibrary.Configuration.DesignTime";

            /// <summary>
            /// This field supports the Enterprise Library infrastructure and is not intended to be used directly from your code.
            /// </summary>
            public const string ConfigurationSourceSectionViewModel =
                "EnterpriseLibrary.Configuration.Design.ViewModel.BlockSpecifics.ConfigurationSourceSectionViewModel, EnterpriseLibrary.Configuration.DesignTime";
        }

        /// <summary>
        /// This class supports the Enterprise Library infrastructure and is not intended to be used directly from your code.
        /// </summary>
        public static class CommandTypeNames
        {
            /// <summary>
            /// This field supports the Enterprise Library infrastructure and is not intended to be used directly from your code.
            /// </summary>
            public const string AddConfigurationSourcesBlockCommand = "EnterpriseLibrary.Configuration.Design.ViewModel.BlockSpecifics.AddConfigurationSourcesBlockCommand, EnterpriseLibrary.Configuration.DesignTime";

            /// <summary>
            /// This field supports the Enterprise Library infrastructure and is not intended to be used directly from your code.
            /// </summary>
            public const string ConfigurationSourceElementDeleteCommand = "EnterpriseLibrary.Configuration.Design.ViewModel.BlockSpecifics.ConfigurationSourceElementDeleteCommand, EnterpriseLibrary.Configuration.DesignTime";
        }
    }



    ///<summary>
    /// Determines if the corresponding property is read-only at designtime.
    ///</summary>
    ///<remarks>
    /// This attribute is used to mark properties that should be presented as read-only, but underlying code may change the value on.
    /// <seealso cref="ReadOnlyAttribute"/></remarks>
    [AttributeUsage(AttributeTargets.Property)]
    public class DesignTimeReadOnlyAttribute : Attribute
    {
        ///<summary>
        /// Initializes a new instance of the <see cref="DesignTimeReadOnlyAttribute"/> class.
        ///</summary>
        ///<param name="readOnly"><see langword="true"/> if the property should be read-only at designtime.</param>
        public DesignTimeReadOnlyAttribute(bool readOnly)
        {
            ReadOnly = readOnly;
        }

        ///<summary>
        /// Determines if the property is read-only by design-time.
        /// Returns <see langword="true" /> if the property is read-only at design-time
        /// and <see langword="false" /> otherwise.
        ///</summary>
        public bool ReadOnly { get; private set; }

    }



    ///<summary>
    /// Defines the type of attribute to apply this configuration property or field.
    ///</summary>
    /// <remarks>
    /// This attribute is applied to create validators for use in the configuration design-time.
    /// </remarks>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = true)]
    public class ValidationAttribute : Attribute
    {
        private string validatorType;

        ///<summary>
        /// Creates an instance of ValidationAttribute with the validator type specified by <see cref="string"/>.
        ///</summary>
        public ValidationAttribute(string validatorType)
        {
            if (string.IsNullOrEmpty(validatorType)) throw new ArgumentException(Resources.ExceptionStringNullOrEmpty, "validatorType");

            this.validatorType = validatorType;
        }


        ///<summary>
        /// Creates an instance of the ValidationAttribute with the validator type specified by <see cref="Type"/>
        ///</summary>
        public ValidationAttribute(Type validatorType)
            : this(validatorType != null ? validatorType.AssemblyQualifiedName : null)
        {
        }

        ///<summary>
        /// Retrieves the validator <see cref="Type"/>.
        ///</summary>
        public Type ValidatorType
        {
            get { return Type.GetType(validatorType, true, true); }
        }

        ///<summary>
        /// Creates a validator objects.   This is expected to return a Validator type from
        /// the EnterpriseLibrary.Configuration.Design namespace.  
        ///</summary>
        ///<returns></returns>
        public object CreateValidator()
        {
            var validatorType = ValidatorType;
            return Activator.CreateInstance(validatorType);
        }

        /// <summary>
        /// When implemented in a derived class, gets a unique identifier for this <see cref="T:System.Attribute"/>.
        /// </summary>
        /// <returns>
        /// An <see cref="T:System.Object"/> that is a unique identifier for the attribute.
        /// </returns>
        /// <filterpriority>2</filterpriority>
        public override object TypeId
        {
            get
            {
                return this.validatorType;
            }
        }
    }

    /// <summary>
    /// Indicates an element level validator.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Property, AllowMultiple = true)]
    public class ElementValidationAttribute : Attribute
    {
        private readonly string validatorTypeName;

        ///<summary>
        /// Creates an instance of ElementValidationAttribute with the validator type specified by <see cref="string"/>.
        ///</summary>
        ///<param name="validatorTypeName"></param>
        public ElementValidationAttribute(string validatorTypeName)
        {
            if (String.IsNullOrEmpty(validatorTypeName)) throw new ArgumentException(Resources.ExceptionStringNullOrEmpty, "validatorTypeName");

            this.validatorTypeName = validatorTypeName;
        }


        ///<summary>
        /// Creates an instance of the ElementValidationAttribute with the validator type specified by <see cref="Type"/>
        ///</summary>
        ///<param name="validatorType"></param>
        public ElementValidationAttribute(Type validatorType)
            : this(validatorType == null ? null : validatorType.AssemblyQualifiedName)
        {
        }

        ///<summary>
        /// Retrieves the validator <see cref="Type"/>.
        ///</summary>
        public Type ValidatorType
        {
            get { return Type.GetType(validatorTypeName, true, true); }
        }

        ///<summary>
        /// Creates a validator objects.   This is expected to return a Validator type from
        /// the EnterpriseLibrary.Configuration.Design namespace.  
        ///</summary>
        ///<returns></returns>
        public object CreateValidator()
        {
            var validatorType = ValidatorType;
            return Activator.CreateInstance(validatorType);
        }

        /// <summary>
        /// When implemented in a derived class, gets a unique identifier for this <see cref="T:System.Attribute"/>.
        /// </summary>
        /// <returns>
        /// An <see cref="T:System.Object"/> that is a unique identifier for the attribute.
        /// </returns>
        /// <filterpriority>2</filterpriority>
        public override object TypeId
        {
            get
            {
                return this.validatorTypeName;
            }
        }
    }





    /// <summary>
    /// Indicates the base class or interface that must be assignable from the type specified in the property that this attribute decorates.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public sealed class BaseTypeAttribute : Attribute
    {
        private readonly Type configurationType;
        private readonly Type baseType;
        private readonly TypeSelectorIncludes typeSelectorIncludes;

        /// <summary>
        /// Initializes a new instance of the  <see cref="BaseTypeAttribute"/> class with the specified <see cref="Type"/> object.
        /// </summary>
        /// <param name="baseType">
        /// The <see cref="Type"/> to filter selections.
        /// </param>
        public BaseTypeAttribute(Type baseType)
            : this(baseType, TypeSelectorIncludes.None)
        {
        }

        /// <summary>
        /// Initializes a new instance of the  <see cref="BaseTypeAttribute"/> class with the specified base <see cref="Type"/> object and configuration <see cref="Type"/>.
        /// </summary>
        /// <param name="baseType">The base <see cref="Type"/> to filter.</param>
        /// <param name="configurationType">The configuration object <see cref="Type"/>.</param>
        public BaseTypeAttribute(Type baseType, Type configurationType)
            : this(baseType, TypeSelectorIncludes.None, configurationType)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BaseTypeAttribute"/> class with the specified <see cref="Type"/> object and <see cref="TypeSelectorIncludes"/>.
        /// </summary>
        /// <param name="baseType">
        /// The <see cref="Type"/> to filter selections.
        /// </param>
        /// <param name="typeSelectorIncludes">
        /// One of the <see cref="TypeSelectorIncludes"/> values.
        /// </param>
        public BaseTypeAttribute(Type baseType, TypeSelectorIncludes typeSelectorIncludes)
            : this(baseType, typeSelectorIncludes, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the  <see cref="BaseTypeAttribute"/> class with the specified base <see cref="Type"/> object and configuration <see cref="Type"/>.
        /// </summary>
        /// <param name="typeSelectorIncludes">
        /// One of the <see cref="typeSelectorIncludes"/> values.
        /// </param>
        /// <param name="baseType">The base <see cref="Type"/> to filter.</param>
        /// <param name="configurationType">The configuration object <see cref="Type"/>.</param>
        public BaseTypeAttribute(Type baseType, TypeSelectorIncludes typeSelectorIncludes, Type configurationType)
            : base()
        {
            if (null == baseType) throw new ArgumentNullException("baseType");
            this.configurationType = configurationType;
            this.baseType = baseType;
            this.typeSelectorIncludes = typeSelectorIncludes;
        }

        /// <summary>
        /// Gets the includes for the type selector.
        /// </summary>
        /// <value>
        /// The includes for the type selector.
        /// </value>
        public TypeSelectorIncludes TypeSelectorIncludes
        {
            get { return typeSelectorIncludes; }
        }

        /// <summary>
        /// Gets the <see cref="Type"/> to filter selections.
        /// </summary>
        /// <value>
        /// The <see cref="Type"/> to filter selections.
        /// </value>
        public Type BaseType
        {
            get { return baseType; }
        }

        /// <summary>
        /// Gets the configuration object <see cref="Type"/>.
        /// </summary>
        /// <value>
        /// The configuration object <see cref="Type"/>.
        /// </value>
        public Type ConfigurationType
        {
            get { return configurationType; }
        }
    }




    /// <summary>
    /// Provides attributes for the filter of types.
    /// </summary>
    [Flags]
    public enum TypeSelectorIncludes
    {
        /// <summary>
        /// No filter are applied to types.
        /// </summary>
        None = 0x00,
        /// <summary>
        /// Inclue abstract types in the filter.
        /// </summary>
        AbstractTypes = 0x01,
        /// <summary>
        /// Inclue interfaces in the filter.
        /// </summary>
        Interfaces = 0x02,
        /// <summary>
        /// Inclue base types in the filter.
        /// </summary>
        BaseType = 0x04,
        /// <summary>
        /// Inclue non public types in the filter.
        /// </summary>
        NonpublicTypes = 0x08,
        /// <summary>
        /// Include all types in the filter.
        /// </summary>
        All = 0x0F
    }





}
