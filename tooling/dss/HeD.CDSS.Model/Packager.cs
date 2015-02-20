/*
	HeD CDS Service Example Implementation
	Copyright (c) 2012 - 2013 Office of the National Coordinator for Health Information Technology (ONC)
	This file is licensed under a modified BSD-license which can be found in the HED_License.txt file included with this distribution.
*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using vMR.Model;

namespace HeD.CDSS.Model
{
    /// <summary>
    /// Provides utility methods for encoding and decoding request and response payloads.
    /// </summary>
    public static class Packager
    {
        /// <summary>
        /// Base-64 encodes a vMR request payload.
        /// </summary>
        /// <param name="request">The vMR request to be encoded.</param>
        /// <returns>A base-64 string containing the request document.</returns>
        public static string EncodeRequestPayload(CDSInput request)
        {
            var sb = new StringBuilder();
            using (var writer = new StringWriter(sb))
            {
                requestSerializer.Serialize(writer, request);
            }

            var bytes = Encoding.UTF8.GetBytes(sb.ToString());
            return Convert.ToBase64String(bytes);
        }

        /// <summary>
        /// Decodes a base-64 encoded vMR request payload.
        /// </summary>
        /// <param name="encodedPayload">The base-64 encoded payload.</param>
        /// <returns>The vMR request.</returns>
        public static CDSInput DecodeRequestPayload(string encodedPayload)
        {
            var payloadBytes = Convert.FromBase64String(encodedPayload);
            using (var payloadStream = new MemoryStream(payloadBytes))
            {
                using (var reader = new StreamReader(payloadStream, Encoding.UTF8))
                {
                    return (CDSInput)requestSerializer.Deserialize(reader);
                }
            }
        }

        /// <summary>
        /// Base-64 encodes an action group response payload.
        /// </summary>
        /// <param name="response">The action group response to be encoded.</param>
        /// <returns>A base-64 string containing the response document.</returns>
        public static string EncodeActionGroupResponsePayload(CDSActionGroupResponse response)
        {
            var sb = new StringBuilder();
            using (var writer = new StringWriter(sb))
            {
                actionGroupResponseSerializer.Serialize(writer, response);
            }

            var bytes = Encoding.UTF8.GetBytes(sb.ToString());
            return Convert.ToBase64String(bytes);
        }

        /// <summary>
        /// Decodes a base-64 encoded action group response.
        /// </summary>
        /// <param name="encodedPayload">The base-64 encoded action group response.</param>
        /// <returns>The action group response.</returns>
        public static CDSActionGroupResponse DecodeActionGroupResponsePayload(string encodedPayload)
        {
            var payloadBytes = Convert.FromBase64String(encodedPayload);
            using (var payloadStream = new MemoryStream(payloadBytes))
            {
                using (var reader = new StreamReader(payloadStream, Encoding.UTF8))
                {
                    return (CDSActionGroupResponse)actionGroupResponseSerializer.Deserialize(reader);
                }
            }
        }

        /// <summary>
        /// Base-64 encodes an execution message payload.
        /// </summary>
        /// <param name="response">The execution message to be encoded.</param>
        /// <returns>A base-64 string containing the message document.</returns>
        public static string EncodeExecutionMessagePayload(CDSExecutionMessage response)
        {
            var sb = new StringBuilder();
            using (var writer = new StringWriter(sb))
            {
                executionMessageSerializer.Serialize(writer, response);
            }

            var bytes = Encoding.UTF8.GetBytes(sb.ToString());
            return Convert.ToBase64String(bytes);
        }

        /// <summary>
        /// Decodes a base-64 encoded execution message payload.
        /// </summary>
        /// <param name="encodedPayload">The base-64 encoded execution message.</param>
        /// <returns>The execution message.</returns>
        public static CDSExecutionMessage DecodeExecutionMessagePayload(string encodedPayload)
        {
            var payloadBytes = Convert.FromBase64String(encodedPayload);
            using (var payloadStream = new MemoryStream(payloadBytes))
            {
                using (var reader = new StreamReader(payloadStream, Encoding.UTF8))
                {
                    return (CDSExecutionMessage)executionMessageSerializer.Deserialize(reader);
                }
            }
        }

        private static Type[] GetvMRTypes()
        {
            return typeof(CDSOutput).Assembly.GetTypes();
        }

        private static Type[] vMRTypes = GetvMRTypes();
        private static XmlSerializer requestSerializer = new XmlSerializer(typeof(CDSInput));
        private static XmlSerializer actionGroupResponseSerializer = new XmlSerializer(typeof(CDSActionGroupResponse));
        private static XmlSerializer executionMessageSerializer = new XmlSerializer(typeof(CDSExecutionMessage));
    }
}
