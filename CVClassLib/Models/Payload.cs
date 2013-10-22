namespace WebApiInterface.Models
{
    using System.Xml;
    using System.Xml.Schema;

    using System.Collections.Generic;
    using System.Xml.Serialization;

    public class Payload : Dictionary<string, string>, IXmlSerializable
    {
        public XmlSchema GetSchema()
        {
            return null;
        }

        public void ReadXml(XmlReader reader)
        {
            bool wasEmpty = reader.IsEmptyElement;
            reader.Read();

            if (wasEmpty)
                return;

            reader.ReadStartElement("Properties");
            while (reader.NodeType != XmlNodeType.EndElement)
            {
                reader.ReadStartElement("Property");

                reader.ReadStartElement("Name");
                var key = reader.ReadString();
                reader.ReadEndElement();

                reader.ReadStartElement("Value");
                var value = reader.ReadString();
                reader.ReadEndElement();

                if (!string.IsNullOrWhiteSpace(key) && !string.IsNullOrWhiteSpace(value))
                {
                    Add(key, value);
                }

                reader.ReadEndElement();
            }
            reader.ReadEndElement();
        }

        public void WriteXml(XmlWriter writer)
        {
            writer.WriteStartElement("Properties");
            foreach (string key in Keys)
            {
                writer.WriteStartElement("Property");

                writer.WriteStartElement("Name");
                writer.WriteValue(key);
                writer.WriteEndElement();

                writer.WriteStartElement("Value");
                var value = this[key];
                writer.WriteValue(value);
                writer.WriteEndElement();

                writer.WriteEndElement();
            }
            writer.WriteEndElement();
        }
    }
}