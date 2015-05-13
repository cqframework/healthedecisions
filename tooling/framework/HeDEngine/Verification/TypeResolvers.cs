/*
	HeD Schema Framework
	Copyright (c) 2012 - 2013 Office of the National Coordinator for Health Information Technology (ONC)
	This file is licensed under a modified BSD-license which can be found in the HED_License.txt file included with this distribution.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using HeD.Engine.Model;
using HeD.Model;

namespace HeD.Engine.Verification
{
	public class ScalarTypeResolver : ITypeResolver
	{
		#region ITypeResolver Members

		public DataType Resolve(string typeName)
		{
			return new ScalarType(typeName);
		}

		#endregion
	}

	public class BooleanTypeResolver : ITypeResolver
	{
		#region ITypeResolver Members

		public DataType Resolve(string typeName)
		{
			return DataTypes.Boolean;
		}

		#endregion
	}

	public class CodeTypeResolver : ITypeResolver
	{
		#region ITypeResolver Members

		public DataType Resolve(string typeName)
		{
			return DataTypes.Code;
		}

		#endregion
	}

	public class ConceptTypeResolver : ITypeResolver
	{
		#region ITypeResolver Members

		public DataType Resolve(string typeName)
		{
			return DataTypes.Concept;
		}

		#endregion
	}

	public class CodedOrdinalTypeResolver : ITypeResolver
	{
		#region ITypeResolver Members

		public DataType Resolve(string typeName)
		{
			return DataTypes.ResolveType(typeof(CO));
		}

		#endregion
	}

	public class EntityNameTypeResolver : ITypeResolver
	{
		#region ITypeResolver Members

		public DataType Resolve(string typeName)
		{
            return DataTypes.ResolveType(typeof(EN));
		}

		#endregion
	}

	public class IdentifierTypeResolver : ITypeResolver
	{
		#region ITypeResolver Members

		public DataType Resolve(string typeName)
		{
			return DataTypes.ResolveType(typeof(II));
		}

		#endregion
	}

	public class IntegerTypeResolver : ITypeResolver
	{
		#region ITypeResolver Members

		public DataType Resolve(string typeName)
		{
			return DataTypes.Integer;
		}

		#endregion
	}

	public class IntegerIntervalTypeResolver : ITypeResolver
	{
		#region ITypeResolver Members

		public DataType Resolve(string typeName)
		{
			return DataTypes.IntegerInterval;
		}

		#endregion
	}

	public class PeriodTypeResolver : ITypeResolver
	{
		#region ITypeResolver Members

		public DataType Resolve(string typeName)
		{
			return DataTypes.ResolveType(typeof(PIVL_TS));
		}

		#endregion
	}

	public class PhysicalQuantityTypeResolver : ITypeResolver
	{
		#region ITypeResolver Members

		public DataType Resolve(string typeName)
		{
			return DataTypes.Quantity;
		}

		#endregion
	}

	public class PhysicalQuantityIntervalTypeResolver : ITypeResolver
	{
		#region ITypeResolver Members

		public DataType Resolve(string typeName)
		{
			return DataTypes.QuantityInterval;
		}

		#endregion
	}

	public class QuantityIntervalTypeResolver : ITypeResolver
	{
		#region ITypeResolver Members

		public DataType Resolve(string typeName)
		{
			return DataTypes.QuantityInterval;
		}

		#endregion
	}

	public class RatioTypeResolver : ITypeResolver
	{
		#region ITypeResolver Members

		public DataType Resolve(string typeName)
		{
			return DataTypes.ResolveType(typeof(RTO));
		}

		#endregion
	}

	public class RealTypeResolver : ITypeResolver
	{
		#region ITypeResolver Members

		public DataType Resolve(string typeName)
		{
			return DataTypes.Decimal;
		}

		#endregion
	}

	public class RealIntervalTypeResolver : ITypeResolver
	{
		#region ITypeResolver Members

		public DataType Resolve(string typeName)
		{
			return DataTypes.DecimalInterval;
		}

		#endregion
	}

	public class SimpleCodeTypeResolver : ITypeResolver
	{
		#region ITypeResolver Members

		public DataType Resolve(string typeName)
		{
			return DataTypes.String;
		}

		#endregion
	}

	public class StringTypeResolver : ITypeResolver
	{
		#region ITypeResolver Members

		public DataType Resolve(string typeName)
		{
			return DataTypes.String;
		}

		#endregion
	}

	public class TimestampTypeResolver : ITypeResolver
	{
		#region ITypeResolver Members

		public DataType Resolve(string typeName)
		{
			return DataTypes.DateTime;
		}

		#endregion
	}

	public class TimeTypeResolver : ITypeResolver
	{
		#region ITypeResolver Members

		public DataType Resolve(string typeName)
		{
			return DataTypes.Time;
		}

		#endregion
	}

	public class TimestampIntervalTypeResolver : ITypeResolver
	{
		#region ITypeResolver Members

		public DataType Resolve(string typeName)
		{
			return DataTypes.DateTimeInterval;
		}

		#endregion
	}

	public class URLTypeResolver : ITypeResolver
	{
		#region ITypeResolver Members

		public DataType Resolve(string typeName)
		{
			return DataTypes.String;
		}

		#endregion
	}

	public abstract class GenericModelTypeResolver : ITypeResolver
	{
		#region ITypeResolver Members

		public DataType Resolve(string typeName)
		{
			var dataType = Type.GetType(String.Format("{0}.{1}, {2}", this.GetType().Namespace, typeName.GetLocalName(), this.GetType().Assembly.GetName().Name));
            if (dataType == null)
            {
                throw new ArgumentException(String.Format("Could not resolve model type for type name {0}.", typeName));
            }

			var resultType = DataTypes.ResolveType(dataType);
			return resultType;
		}

		#endregion
	}
}
