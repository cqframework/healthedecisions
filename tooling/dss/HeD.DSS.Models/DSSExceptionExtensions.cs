/*
	HeD CDS Service Example Implementation
	Copyright (c) 2012 - 2013 Office of the National Coordinator for Health Information Technology (ONC)
	This file is licensed under a modified BSD-license which can be found in the HED_License.txt file included with this distribution.
*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace HeD.DSS.Models
{
    /// <summary>
    /// Extension methods for dealing with DSSExceptions.
    /// </summary>
    public static class DSSExceptionExtensions
    {
        /// <summary>
        /// Converts the errorMessage property of a DSSException to a single string for use as the message of a C# exception.
        /// </summary>
        /// <param name="exception">The DSSException containing the message.</param>
        /// <returns>A single string containing the concatenated list of messages in the given exception, separated by carriage-return;line-feed characters.</returns>
        public static string GetMessage(this DSSException exception)
        {
            if (exception.errorMessage == null)
            {
                return null;
            }

            return String.Join("\r\n", exception.errorMessage);
        }

        public static T DeserializeFromString<T>(string content) where T : DSSException
        {
            var serializer = new XmlSerializer(typeof(T));

            using (var reader = new StringReader(content))
            {
                return (T)serializer.Deserialize(reader);
            }
        }
    }
}
