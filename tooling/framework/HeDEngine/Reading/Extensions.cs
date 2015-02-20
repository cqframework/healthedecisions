/*
	HeD Schema Framework
	Copyright (c) 2012 - 2013 Office of the National Coordinator for Health Information Technology (ONC)
	This file is licensed under a modified BSD-license which can be found in the HED_License.txt file included with this distribution.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.Xml.Schema;

namespace HeD.Engine.Reading
{
	public static class SchemaExtensions
	{
		public static XmlSchema GetSchema(this IXmlSchemaInfo schemaInfo)
		{
			var schemaType = schemaInfo.SchemaType;

			if (schemaType == null)
			{
				throw new InvalidOperationException("Schema info does not have a schema type.");
			}

			return schemaType.Parent.GetSchema();
		}

		public static XmlSchema GetSchema(this XmlSchemaObject schemaObject)
		{
			if (schemaObject == null)
			{
				throw new InvalidOperationException("Schema object is null.");
			}

			var schema = schemaObject as XmlSchema;
			if (schema != null)
			{
				return schema;
			}

			return schemaObject.Parent.GetSchema();
		}

		public static string GetNamespace(this IXmlSchemaInfo schemaInfo, string namespacePrefix)
		{
			var ns = schemaInfo.GetSchema().Namespaces.ToArray().FirstOrDefault(n => n.Name == namespacePrefix);
			if (ns == null)
			{
				throw new InvalidOperationException(String.Format("Could not resolve namespace prefix {0}.", namespacePrefix));
			}
			return ns.Namespace;
		}

		public static string ExpandName(this IXmlSchemaInfo schemaInfo, string qualifiedName)
		{
			var names = qualifiedName.Split(':');
			if (names.Length == 2)
			{
				var ns = schemaInfo.GetNamespace(names[0]);
				return String.Format("{0}:{1}", ns, names[1]);
			}

			return qualifiedName;
		}
	}

	public static class XExtensions
	{
		public static string ExpandName(this XElement element, string qualifiedName)
		{
			var names = qualifiedName.Split(':');
			if (names.Length == 2)
			{
				return String.Format("{0}:{1}", element.GetNamespaceOfPrefix(names[0]).NamespaceName, names[1]);
			}

			return qualifiedName;
		}
	}
}
