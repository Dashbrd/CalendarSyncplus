using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CalendarSyncPlus.Domain.File.Json
{
    public interface IJsonSerializer<T> where T : class, new()
    {
        /// <summary>
        ///     Deserializes a XML string into an object Default encoding:
        ///     <c>UTF8</c>
        /// </summary>
        /// <param name="xml">The XML string to deserialize</param>
        /// <returns>
        ///     An object of type <c>T</c>
        /// </returns>
        T Deserialize(string json);

        /// <summary>
        ///     Deserializes a XML string into an object Default encoding:
        ///     <c>UTF8</c>
        /// </summary>
        /// <param name="xml">The XML string to deserialize</param>
        /// <param name="encoding">The encoding</param>
        /// <returns>
        ///     An object of type <c>T</c>
        /// </returns>
        T Deserialize(string json, Encoding encoding);

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
        ///     <see cref="Serialize" /> an object
        /// </summary>
        /// <param name="source">The object to serialize</param>
        /// <returns>
        ///     A XML string that represents the object to be serialized
        /// </returns>
        string Serialize(T source);

        /// <summary>
        ///     <see cref="Serialize" /> an object to a XML file
        /// </summary>
        /// <param name="source">The object to serialize</param>
        /// <param name="filename">The file to generate</param>
        void SerializeToFile(T source, string filename);
    }
}
