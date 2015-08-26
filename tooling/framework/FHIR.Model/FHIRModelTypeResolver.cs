/*
	HeD Schema Framework
	Copyright (c) 2012 - 2014 Office of the National Coordinator for Health Information Technology (ONC)
	This file is licensed under a modified BSD-license which can be found in the HED_License.txt file included with this distribution.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HeD.Engine.Model;
using HeD.Engine.Verification;
using HL7.FHIR.Model;

namespace FHIR.Model
{
    public class FHIRModelTypeResolver : ITypeResolver
    {
		public HeD.Engine.Model.DataType Resolve(string typeName)
		{
            var dataTypeName = String.Format("HL7.FHIR.Model.{0}", typeName.GetLocalName());
			var dataType = Type.GetType(dataTypeName);
            if (dataType == null)
            {
                throw new ArgumentException(String.Format("Could not resolve model type for type name {0}.", typeName));
            }

			var resultType = DataTypes.ResolveType(dataType);
			return resultType;
		}
    }

    public class FHIRDateTimeTypeResolver : ITypeResolver
    {
        public HeD.Engine.Model.DataType Resolve(string typeName)
        {
            // The FHIRDateTime type uses an XML union for the datetime-primitive, which resolves to a String in C#
            // The actual implementation will need to represent CQL datetimes using a custom structure, because the semantics of
            // the type can't actually be represented in a native C# datetime (same for Java). In fact, the string representation
            // may be the most compact form, but that's not relevant for semantic verification, only implementation.
            // For semantic verification, all we need to do is make sure that the "value" property of the FHIR.dateTime type
            // resolves to the DateTime type.
            var dataType = typeof(dateTime);

            var resultType = (ObjectType)DataTypes.ResolveType(dataType);
            resultType.Properties.Remove(resultType.Properties.Find(p => p.Name == "value"));
            resultType.GetHashCode(); // resets the sorted properties due to the way caching is done within the ObjectType (see ObjectType.SortedProperties)
            resultType.Properties.Add(new PropertyDef("value", DataTypes.DateTime));

            return resultType;
        }
    }
}
