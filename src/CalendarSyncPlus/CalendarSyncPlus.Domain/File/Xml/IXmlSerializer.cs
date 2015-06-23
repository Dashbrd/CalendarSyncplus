using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace CalendarSyncPlus.Domain.File.Xml
{
    public interface IXmlSerializer<T> where T : class, new()
    {
        /// <summary>
        ///     Deserializes a XML string into an object Default encoding:
        ///     <c>UTF8</c>
        /// </summary>
        /// <param name="xml">The XML string to deserialize</param>
        /// <returns>
        ///     An object of type <c>T</c>
        /// </returns>
        T Deserialize(string xml);

        /// <summary>
        ///     Deserializes a XML string into an object Default encoding:
        ///     <c>UTF8</c>
        /// </summary>
        /// <param name="xml">The XML string to deserialize</param>
        /// <param name="encoding">The encoding</param>
        /// <returns>
        ///     An object of type <c>T</c>
        /// </returns>
        T Deserialize(string xml, Encoding encoding);

        /// <summary>
        ///     Deserializes a XML string into an object
        /// </summary>
        /// <param name="xml">The XML string to deserialize</param>
        /// <param name="settings">
        ///     XML serialization settings. <see cref="XmlReaderSettings" />
        /// </param>
        /// <returns>
        ///     An object of type <c>T</c>
        /// </returns>
        T Deserialize(string xml, XmlReaderSettings settings);

        /// <summary>
        ///     Deserializes a XML string into an object
        /// </summary>
        /// <param name="xml">The XML string to deserialize</param>
        /// <param name="encoding">The encoding</param>
        /// <param name="settings">
        ///     XML serialization settings. <see cref="XmlReaderSettings" />
        /// </param>
        /// <returns>
        ///     An object of type <c>T</c>
        /// </returns>
        T Deserialize(string xml, Encoding encoding, XmlReaderSettings settings);

        /// <summary>
        ///     Deserializes a XML file.
        /// </summary>
        /// <param name="filename">
        ///     The filename of the XML file to deserialize
        /// </param>
        /// <returns>
        ///     An object of type <c>T</c>
        /// </returns>
        T DeserializeFromFile(string filename);

        /// <summary>
        ///     Deserializes a XML file.
        /// </summary>
        /// <param name="filename">
        ///     The filename of the XML file to deserialize
        /// </param>
        /// <param name="settings">
        ///     XML serialization settings. <see cref="XmlReaderSettings" />
        /// </param>
        /// <returns>
        ///     An object of type <c>T</c>
        /// </returns>
        T DeserializeFromFile(string filename, XmlReaderSettings settings);

        /// <summary>
        ///     <see cref="Serialize" /> an object
        /// </summary>
        /// <param name="source">The object to serialize</param>
        /// <returns>
        ///     A XML string that represents the object to be serialized
        /// </returns>
        string Serialize(T source);

        /// <summary>
        ///     <see cref="Serialize" /> an object
        /// </summary>
        /// <param name="source">The object to serialize</param>
        /// <param name="namespaces">
        ///     Namespaces to include in serialization
        /// </param>
        /// <returns>
        ///     A XML string that represents the object to be serialized
        /// </returns>
        string Serialize(T source, XmlSerializerNamespaces namespaces);

        /// <summary>
        ///     <see cref="Serialize" /> an object
        /// </summary>
        /// <param name="source">The object to serialize</param>
        /// <param name="settings">
        ///     XML serialization settings. <see cref="XmlWriterSettings" />
        /// </param>
        /// <returns>
        ///     A XML string that represents the object to be serialized
        /// </returns>
        string Serialize(T source, XmlWriterSettings settings);

        /// <summary>
        ///     <see cref="Serialize" /> an object
        /// </summary>
        /// <param name="source">The object to serialize</param>
        /// <param name="namespaces">
        ///     Namespaces to include in serialization
        /// </param>
        /// <param name="settings">
        ///     XML serialization settings. <see cref="XmlWriterSettings" />
        /// </param>
        /// <returns>
        ///     A XML string that represents the object to be serialized
        /// </returns>
        string Serialize(T source, XmlSerializerNamespaces namespaces, XmlWriterSettings settings);

        /// <summary>
        ///     <see cref="Serialize" /> an object to a XML file
        /// </summary>
        /// <param name="source">The object to serialize</param>
        /// <param name="filename">The file to generate</param>
        void SerializeToFile(T source, string filename);

        /// <summary>
        ///     <see cref="Serialize" /> an object to a XML file
        /// </summary>
        /// <param name="source">The object to serialize</param>
        /// <param name="filename">The file to generate</param>
        /// <param name="namespaces">
        ///     Namespaces to include in serialization
        /// </param>
        void SerializeToFile(T source, string filename, XmlSerializerNamespaces namespaces);

        /// <summary>
        ///     <see cref="Serialize" /> an object to a XML file
        /// </summary>
        /// <param name="source">The object to serialize</param>
        /// <param name="filename">The file to generate</param>
        /// <param name="settings">
        ///     XML serialization settings. <see cref="XmlWriterSettings" />
        /// </param>
        void SerializeToFile(T source, string filename, XmlWriterSettings settings);

        /// <summary>
        ///     <see cref="Serialize" /> an object to a XML file
        /// </summary>
        /// <param name="source">The object to serialize</param>
        /// <param name="filename">The file to generate</param>
        /// <param name="namespaces">
        ///     Namespaces to include in serialization
        /// </param>
        /// <param name="settings">
        ///     XML serialization settings. <see cref="XmlWriterSettings" />
        /// </param>
        void SerializeToFile(T source, string filename, XmlSerializerNamespaces namespaces,
            XmlWriterSettings settings);
    }
}