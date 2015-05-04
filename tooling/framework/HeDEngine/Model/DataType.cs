/*
	HeD Schema Framework
	Copyright (c) 2012 - 2013 Office of the National Coordinator for Health Information Technology (ONC)
	This file is licensed under a modified BSD-license which can be found in the HED_License.txt file included with this distribution.
*/

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using HeD.Model;

namespace HeD.Engine.Model
{
	/// <summary>
	/// Provides data type equality, equivalence, and resolution services.
	/// </summary>
	public static class DataTypes
	{
		private static readonly Dictionary<string, DataType> _resolvedTypes = new Dictionary<string, DataType>();

		public static readonly ObjectType Any = (ObjectType)ResolveType(typeof(ANY));
        public static readonly ObjectType Quantity = (ObjectType)ResolveType(typeof(QTY));
		public static readonly ScalarType Boolean = new ScalarType("Boolean", Any);
		public static readonly ScalarType Integer = new ScalarType("Integer", Quantity);
		public static readonly ScalarType Real = new ScalarType("Real", Quantity);
		public static readonly ScalarType String = new ScalarType("String", Any);
		public static readonly ScalarType Timestamp = new ScalarType("Timestamp", Quantity);
		public static readonly ObjectType PhysicalQuantity = (ObjectType)ResolveType(typeof(PQ));
		public static readonly ObjectType Ratio = (ObjectType)ResolveType(typeof(RTO));
		//public static readonly ObjectType IntegerRatio = (ObjectType)ResolveType(typeof(RTO_INT));
		//public static readonly ObjectType PhysicalQuantityRatio = (ObjectType)ResolveType(typeof(RTO_PQ));
		public static readonly IntervalType TimestampInterval = new IntervalType(DataTypes.Timestamp);
		public static readonly IntervalType RealInterval = new IntervalType(DataTypes.Real);
		//public static readonly IntervalType IntegerRatioInterval = new IntervalType(DataTypes.IntegerRatio);
		//public static readonly IntervalType PhysicalQuantityRatioInterval = new IntervalType(DataTypes.PhysicalQuantityRatio);
        public static readonly IntervalType QuantityInterval = new IntervalType(DataTypes.Quantity);
		public static readonly IntervalType PhysicalQuantityInterval = new IntervalType(DataTypes.PhysicalQuantity);
		public static readonly IntervalType IntegerInterval = new IntervalType(DataTypes.Integer);
		public static readonly ScalarType DateGranularity = new ScalarType("DateGranularity");
		public static readonly ObjectType Identifier = (ObjectType)ResolveType(typeof(II));
		public static readonly ObjectType Code = (ObjectType)ResolveType(typeof(CD));
        public static readonly ObjectType CodedOrdinal = (ObjectType)ResolveType(typeof(CO));
        public static readonly ObjectType SimpleCode = (ObjectType)ResolveType(typeof(CS));
        public static readonly ObjectType EntityName = (ObjectType)ResolveType(typeof(EN));
        public static readonly ObjectType Period = (ObjectType)ResolveType(typeof(HeD.Model.PIVL_TS));
        public static readonly ObjectType URL = (ObjectType)ResolveType(typeof(HeD.Model.TEL));
		public static readonly ListType CodeList = new ListType(Code);

		/// <summary>
		/// Compares two DataType instances for equality.
		/// </summary>
		/// <returns>True if the DataTypes are equal, false otherwise.</returns>
		/// <remarks>
		/// <para>
		/// Data type equality is based on the notion of names, not instances, meaning
		/// that two different instances of the ScalarType named Boolean will be considered 
		/// equal.
		/// </para>
		/// <para>
		/// Two scalar types are equal if they have the same name.
		/// </para>
		/// <para>
		/// Two list types are equal if their element types are equal.
		/// </para>
		/// <para>
		/// Two interval types are equal if their point types are equal.
		/// </para>
		/// <para>
		/// Two object types are equal if they have the same name.
		/// </para>
		/// </remarks>
		public static bool Equal(DataType a, DataType b)
		{
			return a != null && b != null && a.Equals(b);
		}

		/// <summary>
		/// Compares two DataType instances for equivalence.
		/// </summary>
		/// <returns>True if the DataTypes are equivalent, false otherwise.</returns>
		/// <remarks>
		/// <para>
		/// Data type equivalence is looser than data type equality. So two data types
		/// are equivalent if they are equal, or, in the case of object types, if they
		/// have the same set of properties, by name and property type equivalence.
		/// </para>
		/// </remarks>
		public static bool Equivalent(DataType a, DataType b)
		{
			return a != null && b != null && a.EquivalentTo(b);
		}

        /// <summary>
        /// Compares two DataType instances for sub-type.
        /// </summary>
        /// <returns>True if the first type is a subtype (or the same type) as the second type, false otherwise.</returns>
        public static bool SubTypeOf(DataType a, DataType b)
        {
            return a != null && b != null && a.IsSubType(b);
        }

        /// <summary>
        /// Compares two data type instances for super-type.
        /// </summary>
        /// <returns>True if the first type is a supertype (or the same type) as the second type, false otherwise.</returns>
        public static bool SuperTypeOf(DataType a, DataType b)
        {
            return a != null && b != null && a.IsSuperType(b);
        }

		/// <summary>
		/// Resolves C# native types to their logical DataType representation.
		/// </summary>
		/// <param name="type">The C# native type to resolve.</param>
		/// <returns>The logical DataType representation of the type.</returns>
		public static DataType ResolveType(Type type)
		{
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }

			var listInterface = type.GetInterface("IList`1");
			if (listInterface != null)
			{
				var elementType = listInterface.GetGenericArguments().First();
				return new ListType(ResolveType(elementType));
			}

            if (type.IsEnum)
            {
                return DataTypes.Code; // TODO: This may not be the best mapping, but it should work for what we're doing right now...
            }

			switch (type.Name)
			{
				case "Boolean" : return DataTypes.Boolean;
				case "Byte" :
				case "Int16" :
				case "Int32" :
				case "Int64" : return DataTypes.Integer;
				case "String" : return DataTypes.String;
				case "DateTime" : return DataTypes.Timestamp;
				case "Single" :
				case "Double" :
				case "Decimal" : return DataTypes.Real;

				// cdsdt data types
				case "BL" : return DataTypes.Boolean;
				case "INT" : return DataTypes.Integer;
				case "REAL" : return DataTypes.Real;
				case "ST" : return DataTypes.String;
				case "TS" : return DataTypes.Timestamp;
				case "IVL_TS" : return DataTypes.TimestampInterval;
				case "IVL_REAL" : return DataTypes.RealInterval;
				//case "IVL_RTO" : return DataTypes.IntegerRatioInterval;
				//case "IVL_RTO_INT" : return DataTypes.IntegerRatioInterval;
				//case "IVL_RTO_PQ" : return DataTypes.PhysicalQuantityRatioInterval;
				case "IVL_PQ" : return DataTypes.PhysicalQuantityInterval;
				case "IVL_INT" : return DataTypes.IntegerInterval;

				// Everything else is either an object type, or a base type (if it's represented as an enum)
				default:
				{
					DataType resultType;
					if (!_resolvedTypes.TryGetValue(type.Name, out resultType))
					{
                        if (type.IsEnum)
                        {
                            resultType = DataTypes.String;
                            _resolvedTypes.Add(type.Name, resultType);
                        }
                        else
                        {
                            var baseType = type.BaseType != null && !type.BaseType.Equals(typeof(object)) ? ResolveType(type.BaseType) as ObjectType : null;
							// The possibility exists for the type to be resolved at this point due to the resolution of properties on an ancestor type.
							if (!_resolvedTypes.TryGetValue(type.Name, out resultType))
							{
								var objectType = new ObjectType(type.Name, baseType, new PropertyDef[] { });
								_resolvedTypes.Add(type.Name, objectType);
								objectType.Properties.AddRange(ResolveProperties(type));

								resultType = objectType;
							}
                        }
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

	/// <summary>
	/// Abstract base class for the representation of all types in the HeD expression language.
	/// </summary>
	public abstract class DataType
	{
        public DataType() { }
        public DataType(DataType baseType)
        {
            _baseType = baseType;
        }

        private DataType _baseType;
        public DataType BaseType { get { return _baseType; } }

		public string Name { get { return GetName(); } }

		protected abstract string GetName();

		public override string ToString()
		{
			return Name;
		}

		public virtual bool EquivalentTo(DataType other)
		{
			return other != null && Name == other.Name;
		}

        public virtual bool IsSubType(DataType other)
        {
            var currentType = this;
            while (currentType != null)
            {
                if (currentType.EquivalentTo(other))
                {
                    return true;
                }
                currentType = currentType.BaseType;
            }

            return false;
        }

        public virtual bool IsSuperType(DataType other)
        {
            while (other != null)
            {
                if (EquivalentTo(other))
                {
                    return true;
                }
                other = other.BaseType;
            }

            return false;
        }
	}

	/// <summary>
	/// Provides the representation for all scalar types, such as Integer, String, and Real.
	/// </summary>
	public class ScalarType : DataType
	{
        public ScalarType(string name) : this(name, null) { }
		public ScalarType(string name, DataType baseType) : base(baseType)
		{
			if (String.IsNullOrEmpty(name))
			{
				throw new ArgumentNullException("name");
			}

			_name = name;
		}

		private string _name;

		protected override string GetName()
		{
			return _name;
		}

		public override bool Equals(object obj)
		{
			var other = obj as ScalarType;
			return (other != null) && (_name == other.Name);
		}

		public override int GetHashCode()
		{
			return _name.GetHashCode();
		}
	}

	/// <summary>
	/// Provides the representation for a list type.
	/// </summary>
	public class ListType : DataType
	{
		public ListType(DataType elementType) : base(null)
		{
			if (elementType == null)
			{
				throw new ArgumentNullException("elementType");
			}

			_elementType = elementType;
		}

		private DataType _elementType;
		public DataType ElementType { get { return _elementType; } }

		protected override string GetName()
		{
			return String.Format("List<{0}>", _elementType.Name);
		}

		public override bool Equals(object obj)
		{
			var other = obj as ListType;
			return other != null && _elementType.Equals(other.ElementType);
		}

		public override bool EquivalentTo(DataType other)
		{
			var otherList = other as ListType;
			return otherList != null && _elementType.EquivalentTo(otherList.ElementType);
		}

        public override bool IsSubType(DataType other)
        {
            // List covariance works because lists are immutable
            var otherList = other as ListType;
            return otherList != null && _elementType.IsSubType(otherList.ElementType);
        }

		public override int GetHashCode()
		{
			return  59 * _elementType.GetHashCode();
		}
	}

	/// <summary>
	/// Provides the representation for an interval type.
	/// </summary>
	public class IntervalType : DataType
	{
		public IntervalType(DataType pointType) : base(DataTypes.Any)
		{
			if (pointType == null)
			{
				throw new ArgumentNullException("pointType");
			}

			_pointType = pointType;

			// All interval types have these properties
			_properties = 
				new List<PropertyDef> 
				{ 
					new PropertyDef("begin", pointType), 
					new PropertyDef("end", pointType), 
					new PropertyDef("beginOpen", DataTypes.Boolean), 
					new PropertyDef("endOpen", DataTypes.Boolean),

                    // Added to support access to the properties using both begin/end as well as the cdsdt-style low/high
                    // NOTE: This was not possible before due to the inconsistent naming of interval types in cdsdt;
                    // those issues have been corrected as part of vMR-R2.
                    new PropertyDef("low", pointType),
                    new PropertyDef("high", pointType),
                    new PropertyDef("lowClosed", DataTypes.Boolean),
                    new PropertyDef("highClosed", DataTypes.Boolean)
				};
		}

		private DataType _pointType;
		public DataType PointType { get { return _pointType; } }

		private List<PropertyDef> _properties;
		public List<PropertyDef> Properties { get { return _properties; } }

		protected override string GetName()
		{
			return String.Format("Interval<{0}>", _pointType.Name);
		}

		public override bool Equals(object obj)
		{
			var other = obj as IntervalType;
			return other != null && PointType.Equals(other.PointType);
		}

		public override bool EquivalentTo(DataType other)
		{
			var otherIntervalType = other as IntervalType;
			return otherIntervalType != null && PointType.EquivalentTo(otherIntervalType.PointType);
		}

/*
        public override bool IsSubType(DataType other)
        {
            var otherIntervalType = other as IntervalType;
            return otherIntervalType != null && _pointType.IsSubType(otherIntervalType.PointType);
        }

        public override bool IsSuperType(DataType other)
        {
            var otherIntervalType = other as IntervalType;
            return otherIntervalType != null && _pointType.IsSuperType(otherIntervalType.PointType);
        }
*/

		public override int GetHashCode()
		{
			return 67 * _pointType.GetHashCode();
		}
	}

	public class PropertyDef
	{
		public PropertyDef(string name, DataType propertyType)
		{
			if (String.IsNullOrEmpty(name))
			{
				throw new ArgumentNullException("name");
			}

			if (propertyType == null)
			{
				throw new ArgumentNullException("propertyType");
			}

			_name = name;
			_propertyType = propertyType;
		}

		private string _name;
		public string Name { get { return _name; } }

		private DataType _propertyType;
		public DataType PropertyType { get { return _propertyType; } }

		public override string ToString()
		{
			return String.Format("{0} : {1}", _name, _propertyType.Name);
		}

		public override bool Equals(object obj)
		{
			var other = obj as PropertyDef;
			return other != null && _name == other.Name && _propertyType.Equals(other.PropertyType);
		}

		public bool EquivalentTo(PropertyDef other)
		{
			return other != null && _name == other.Name && _propertyType.EquivalentTo(other.PropertyType);
		}

		public override int GetHashCode()
		{
			return 13 + (17 * _name.GetHashCode()) + (23 * + _propertyType.GetHashCode());
		}
	}

	/// <summary>
	/// Provides the representation for an object type.
	/// </summary>
	public class ObjectType : DataType
	{
        public ObjectType(string name, IEnumerable<PropertyDef> properties) : this(name, null, properties) { }
		public ObjectType(string name, DataType baseType, IEnumerable<PropertyDef> properties) : base(baseType)
		{
			if (String.IsNullOrEmpty(name))
			{
				throw new ArgumentNullException("name");
			}

			if (properties == null)
			{
				throw new ArgumentNullException("properties");
			}

			_name = name;
			_properties = properties.ToList();
		}

		private string _name;
		protected override string GetName()
		{
			return _name;
		}

		private List<PropertyDef> _properties;
		public List<PropertyDef> Properties { get { return _properties; } }

		private List<PropertyDef> _sortedProperties;
		private List<PropertyDef> GetSortedProperties()
		{
			// Poor man's cache refresh, assumes PropertyDefs are immutable, and property lists would only ever be changed by adding xor removing properties,
			// resulting in a different number of properties in the cached sorted property list.
			if (_sortedProperties != null && (_properties.Count != _sortedProperties.Count))
			{
				_sortedProperties = null;
			}

			if (_sortedProperties == null)
			{
				_sortedProperties = new List<PropertyDef>(_properties);
				_sortedProperties.Sort((a, b) => String.Compare(a.Name, b.Name, false, CultureInfo.InvariantCulture));
			}

			return _sortedProperties;
		}

		public override bool Equals(object obj)
		{
			var other = obj as ObjectType;
			return other != null && String.Compare(_name, other.Name, false, CultureInfo.InvariantCulture) == 0;
		}

		public override bool EquivalentTo(DataType other)
		{
			var otherObjectType = other as ObjectType;

			if (otherObjectType != null)
			{
				if (String.Compare(_name, otherObjectType.Name, false, CultureInfo.InvariantCulture) == 0)
				{
					return true;
				}

				if (otherObjectType.Properties.Count == _properties.Count)
				{
					var properties = GetSortedProperties();
					var otherProperties = otherObjectType.GetSortedProperties();

					for (int i = 0; i < properties.Count; i++)
					{
						if (!properties[i].EquivalentTo(otherProperties[i]))
						{
							return false;
						}
					}

					return true;
				}
			}

			return false;
		}

		public override int GetHashCode()
		{
			return _name.GetHashCode();
		}
	}
}
