using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Configuration;
using System.Xml;

namespace NiL.WBE.Html
{
    [ConfigurationCollection(typeof(TemplateElement))]
    public class TemplateElementCollection : ConfigurationElementCollection, IConfigurationSectionHandler
    {
        internal const string PropertyName = "template";

        public override ConfigurationElementCollectionType CollectionType
        {
            get
            {
                return ConfigurationElementCollectionType.BasicMapAlternate;
            }
        }
        protected override string ElementName
        {
            get
            {
                return PropertyName;
            }
        }

        protected override bool IsElementName(string elementName)
        {
            return elementName.Equals(PropertyName, StringComparison.InvariantCultureIgnoreCase);
        }

        public override bool IsReadOnly()
        {
            return false;
        }

        protected override ConfigurationElement CreateNewElement()
        {
            return new TemplateElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((TemplateElement)(element)).Name;
        }

        public new TemplateElement this[string name]
        {
            get { return (TemplateElement)BaseGet(name); }
        }

        public object Create(object parent, object configContext, XmlNode section)
        {
            foreach(XmlElement child in section.ChildNodes)
                this.BaseAdd(new TemplateElement() { Name = child.GetAttribute("name"), Path = child.GetAttribute("path") });
            return this;
        }
    }

    public class TemplateElement : ConfigurationElement
    {
        private static ConfigurationPropertyCollection _Properties;

        private static readonly ConfigurationProperty _Name = new ConfigurationProperty("name", typeof(string), null, ConfigurationPropertyOptions.IsRequired);
        private static readonly ConfigurationProperty _Path = new ConfigurationProperty("path", typeof(string), null, ConfigurationPropertyOptions.IsRequired);

        static TemplateElement()
        {
            _Properties = new ConfigurationPropertyCollection();

            _Properties.Add(_Name);
            _Properties.Add(_Path);
        }

        public TemplateElement()
        {

        }

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                return _Properties;
            }
        }

        [StringValidator(InvalidCharacters = " ~!@#$%^&*()[]{}/;'\"|\\?", MinLength = 1, MaxLength = 60)]
        public string Name
        {
            get
            {
                return (string)this["name"];
            }
            set
            {
                this["name"] = value;
            }
        }

        [StringValidator(InvalidCharacters = " ~!@#$%^&*()[]{};'\"|?", MinLength = 1, MaxLength = 60)]
        public string Path
        {
            get
            {
                return (string)this["path"];
            }
            set
            {
                this["path"] = value;
            }
        }
    }
}