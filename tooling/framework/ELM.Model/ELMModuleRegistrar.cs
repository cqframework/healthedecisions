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

using HeD.Engine.Model;
using HeD.Engine.Verification;

namespace ELM.Model
{
	public class ELMModuleRegistrar : IModuleRegistrar
	{
		#region IModuleRegistrar Members

		public IEnumerable<Operator> Register()
		{
			return
				new Operator[]
				{
                    new Operator("InValueSet", new Signature(new[] { (DataType)DataTypes.Code, DataTypes.CodeList }), DataTypes.Boolean),
                    new Operator("CalculateAge", new Signature(new[] { DataTypes.Timestamp }), DataTypes.Integer),
                    new Operator("CalculateAgeAt", new Signature(new[] { DataTypes.Timestamp, DataTypes.Timestamp }), DataTypes.Integer),

                    new Operator("DurationBetween", new Signature(new[] { DataTypes.Timestamp, DataTypes.Timestamp }), DataTypes.Integer),
                    new Operator("DateFrom", new Signature(new[] { DataTypes.Timestamp }), DataTypes.Timestamp),
                    new Operator("TimeFrom", new Signature(new[] { DataTypes.Timestamp }), DataTypes.Timestamp),
                    new Operator("TimezoneFrom", new Signature(new[] { DataTypes.Timestamp }), DataTypes.Real),
                    new Operator("DateTimeComponentFrom", new Signature(new[] { DataTypes.Timestamp }), DataTypes.Integer),
                    new Operator("DateTime", new Signature(new[] { DataTypes.Integer, DataTypes.Integer, DataTypes.Integer, DataTypes.Integer, DataTypes.Integer, DataTypes.Integer, DataTypes.Integer, DataTypes.Real }), DataTypes.Timestamp),
                    new Operator("DateTime", new Signature(new[] { DataTypes.Integer, DataTypes.Integer, DataTypes.Integer, DataTypes.Integer, DataTypes.Integer, DataTypes.Integer, DataTypes.Integer }), DataTypes.Timestamp),
                    new Operator("DateTime", new Signature(new[] { DataTypes.Integer, DataTypes.Integer, DataTypes.Integer, DataTypes.Integer, DataTypes.Integer, DataTypes.Integer }), DataTypes.Timestamp),
                    new Operator("DateTime", new Signature(new[] { DataTypes.Integer, DataTypes.Integer, DataTypes.Integer, DataTypes.Integer, DataTypes.Integer }), DataTypes.Timestamp),
                    new Operator("DateTime", new Signature(new[] { DataTypes.Integer, DataTypes.Integer, DataTypes.Integer, DataTypes.Integer }), DataTypes.Timestamp),
                    new Operator("DateTime", new Signature(new[] { DataTypes.Integer, DataTypes.Integer, DataTypes.Integer }), DataTypes.Timestamp),
                    new Operator("DateTime", new Signature(new[] { DataTypes.Integer, DataTypes.Integer }), DataTypes.Timestamp),
                    new Operator("DateTime", new Signature(new[] { DataTypes.Integer }), DataTypes.Timestamp),
                    new Operator("SameAs", new Signature(new[] { DataTypes.Timestamp, DataTypes.Timestamp }), DataTypes.Boolean),
                    new Operator("SameOrBefore", new Signature(new[] { DataTypes.Timestamp, DataTypes.Timestamp }), DataTypes.Boolean),
                    new Operator("SameOrAfter", new Signature(new[] { DataTypes.Timestamp, DataTypes.Timestamp }), DataTypes.Boolean),
                    new Operator("Before", new Signature(new[] { DataTypes.Timestamp, DataTypes.Timestamp }), DataTypes.Boolean),
                    new Operator("After", new Signature(new[] { DataTypes.Timestamp, DataTypes.Timestamp }), DataTypes.Boolean),
				};
		}

		#endregion
	}
}
