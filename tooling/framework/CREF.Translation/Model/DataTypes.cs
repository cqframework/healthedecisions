/*
	HeD Schema Framework
	Copyright (c) 2012 - 2013 Office of the National Coordinator for Health Information Technology (ONC)
	This file is licensed under a modified BSD-license which can be found in the HED_License.txt file included with this distribution.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using HeD.Engine.Model;

namespace CREF.Model
{
	public static class DataTypes
	{
		private static readonly Dictionary<string, DataType> _resolvedTypes = new Dictionary<string, DataType>();

		public static readonly ScalarType Boolean = new ScalarType("Boolean");
		public static readonly ScalarType Integer = new ScalarType("Integer");
		public static readonly ScalarType Decimal = new ScalarType("Decimal");
		public static readonly ScalarType String = new ScalarType("String");
		public static readonly ScalarType DateTime = new ScalarType("DateTime");

		public static DataType ResolveType(Type type)
		{
			var listInterface = type.GetInterface("IList`1");
			if (listInterface != null)
			{
				var elementType = listInterface.GetGenericArguments().First();
				return new ListType(ResolveType(elementType));
			}

			switch (type.Name)
			{
				case "Boolean" : return DataTypes.Boolean;
				case "Byte" :
				case "Int16" :
				case "Int32" :
				case "Int64" : return DataTypes.Integer;
				case "String" : return DataTypes.String;
				case "DateTime" : return DataTypes.DateTime;
				case "Single" :
				case "Double" :
				case "Decimal" : return DataTypes.Decimal;

				// Everything else is an object type
				default:
				{
					DataType resultType;
					if (!_resolvedTypes.TryGetValue(type.Name, out resultType))
					{
						var objectType = new ObjectType(type.Name, new PropertyDef[] { });
						_resolvedTypes.Add(type.Name, objectType);
						objectType.Properties.AddRange(ResolveProperties(type));

						resultType = objectType;
					}

					return resultType;
				}
			}
		}

		private static IEnumerable<PropertyDef> ResolveProperties(Type type)
		{
			var properties = new List<PropertyDef>();

			foreach (var property in type.GetProperties())
			{
				// For choice elements, the element names should appear traversible as properties, otherwise there would be no way to access the value
				// (HeD does not have a Cast, and the xsd doesn't actually have a property named Item, which is how choices deserialize)
				var propertyAdded = false;
				foreach (var attribute in property.GetCustomAttributes(typeof(XmlElementAttribute), false).OfType<XmlElementAttribute>())
				{
					if (!string.IsNullOrEmpty(attribute.ElementName) && attribute.Type != null && attribute.Type != type)
					{
						properties.Add(new PropertyDef(attribute.ElementName, ResolveType(attribute.Type)));
						propertyAdded = true;
					}
				}

				if (!propertyAdded)
				{
					properties.Add(new PropertyDef(property.Name, ResolveType(property.PropertyType)));
				}
			}

			return properties;
		}
	}
}
