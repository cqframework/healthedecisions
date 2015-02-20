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

namespace HeD.Engine.Verification
{
	public class BaseModuleRegistrar : IModuleRegistrar
	{
		#region IModuleRegistrar Members

		public IEnumerable<Operator> Register()
		{
			return
				new[]
				{
					// And - Nary operator, not registered
					// Or - Nary operator, not registered

					// Not
					new Operator("Not", new Signature(new[] { DataTypes.Boolean }), DataTypes.Boolean),

					// Equal
					new Operator("Equal", new Signature(new[] { DataTypes.Boolean, DataTypes.Boolean }), DataTypes.Boolean),
					new Operator("Equal", new Signature(new[] { DataTypes.Integer, DataTypes.Integer }), DataTypes.Boolean),
					new Operator("Equal", new Signature(new[] { DataTypes.Real, DataTypes.Real }), DataTypes.Boolean),
					new Operator("Equal", new Signature(new[] { DataTypes.String, DataTypes.String }), DataTypes.Boolean),
					new Operator("Equal", new Signature(new[] { DataTypes.Timestamp, DataTypes.Timestamp }), DataTypes.Boolean),

					// NotEqual
					new Operator("NotEqual", new Signature(new[] { DataTypes.Boolean, DataTypes.Boolean }), DataTypes.Boolean),
					new Operator("NotEqual", new Signature(new[] { DataTypes.Integer, DataTypes.Integer }), DataTypes.Boolean),
					new Operator("NotEqual", new Signature(new[] { DataTypes.Real, DataTypes.Real }), DataTypes.Boolean),
					new Operator("NotEqual", new Signature(new[] { DataTypes.String, DataTypes.String }), DataTypes.Boolean),
					new Operator("NotEqual", new Signature(new[] { DataTypes.Timestamp, DataTypes.Timestamp }), DataTypes.Boolean),

					// Less
					new Operator("Less", new Signature(new[] { DataTypes.Integer, DataTypes.Integer }), DataTypes.Boolean),
					new Operator("Less", new Signature(new[] { DataTypes.Real, DataTypes.Real }), DataTypes.Boolean),
					new Operator("Less", new Signature(new[] { DataTypes.String, DataTypes.String }), DataTypes.Boolean),
					new Operator("Less", new Signature(new[] { DataTypes.Timestamp, DataTypes.Timestamp }), DataTypes.Boolean),
					new Operator("Less", new Signature(new[] { DataTypes.PhysicalQuantity, DataTypes.PhysicalQuantity }), DataTypes.Boolean),
					//new Operator("Less", new Signature(new[] { DataTypes.IntegerRatio, DataTypes.IntegerRatio }), DataTypes.Boolean),
					//new Operator("Less", new Signature(new[] { DataTypes.PhysicalQuantityRatio, DataTypes.PhysicalQuantityRatio }), DataTypes.Boolean),

					// Greater
					new Operator("Greater", new Signature(new[] { DataTypes.Integer, DataTypes.Integer }), DataTypes.Boolean),
					new Operator("Greater", new Signature(new[] { DataTypes.Real, DataTypes.Real }), DataTypes.Boolean),
					new Operator("Greater", new Signature(new[] { DataTypes.String, DataTypes.String }), DataTypes.Boolean),
					new Operator("Greater", new Signature(new[] { DataTypes.Timestamp, DataTypes.Timestamp }), DataTypes.Boolean),
					new Operator("Greater", new Signature(new[] { DataTypes.PhysicalQuantity, DataTypes.PhysicalQuantity }), DataTypes.Boolean),
					//new Operator("Greater", new Signature(new[] { DataTypes.IntegerRatio, DataTypes.IntegerRatio }), DataTypes.Boolean),
					//new Operator("Greater", new Signature(new[] { DataTypes.PhysicalQuantityRatio, DataTypes.PhysicalQuantityRatio }), DataTypes.Boolean),

					// LessOrEqual
					new Operator("LessOrEqual", new Signature(new[] { DataTypes.Integer, DataTypes.Integer }), DataTypes.Boolean),
					new Operator("LessOrEqual", new Signature(new[] { DataTypes.Real, DataTypes.Real }), DataTypes.Boolean),
					new Operator("LessOrEqual", new Signature(new[] { DataTypes.String, DataTypes.String }), DataTypes.Boolean),
					new Operator("LessOrEqual", new Signature(new[] { DataTypes.Timestamp, DataTypes.Timestamp }), DataTypes.Boolean),
					new Operator("LessOrEqual", new Signature(new[] { DataTypes.PhysicalQuantity, DataTypes.PhysicalQuantity }), DataTypes.Boolean),
					//new Operator("LessOrEqual", new Signature(new[] { DataTypes.IntegerRatio, DataTypes.IntegerRatio }), DataTypes.Boolean),
					//new Operator("LessOrEqual", new Signature(new[] { DataTypes.PhysicalQuantityRatio, DataTypes.PhysicalQuantityRatio }), DataTypes.Boolean),

					// GreaterOrEqual
					new Operator("GreaterOrEqual", new Signature(new[] { DataTypes.Integer, DataTypes.Integer }), DataTypes.Boolean),
					new Operator("GreaterOrEqual", new Signature(new[] { DataTypes.Real, DataTypes.Real }), DataTypes.Boolean),
					new Operator("GreaterOrEqual", new Signature(new[] { DataTypes.String, DataTypes.String }), DataTypes.Boolean),
					new Operator("GreaterOrEqual", new Signature(new[] { DataTypes.Timestamp, DataTypes.Timestamp }), DataTypes.Boolean),
					new Operator("GreaterOrEqual", new Signature(new[] { DataTypes.PhysicalQuantity, DataTypes.PhysicalQuantity }), DataTypes.Boolean),
					//new Operator("GreaterOrEqual", new Signature(new[] { DataTypes.IntegerRatio, DataTypes.IntegerRatio }), DataTypes.Boolean),
					//new Operator("GreaterOrEqual", new Signature(new[] { DataTypes.PhysicalQuantityRatio, DataTypes.PhysicalQuantityRatio }), DataTypes.Boolean),

					// Add
					new Operator("Add", new Signature(new[] { DataTypes.Integer, DataTypes.Integer }), DataTypes.Integer),
					new Operator("Add", new Signature(new[] { DataTypes.Real, DataTypes.Real}), DataTypes.Real),

					// Subtract
					new Operator("Subtract", new Signature(new[] { DataTypes.Integer, DataTypes.Integer }), DataTypes.Integer),
					new Operator("Subtract", new Signature(new[] { DataTypes.Real, DataTypes.Real}), DataTypes.Real),

					// Multiply
					new Operator("Multiply", new Signature(new[] { DataTypes.Integer, DataTypes.Integer }), DataTypes.Integer),
					new Operator("Multiply", new Signature(new[] { DataTypes.Real, DataTypes.Real}), DataTypes.Real),

					// Divide
					new Operator("Divide", new Signature(new[] { DataTypes.Integer, DataTypes.Integer }), DataTypes.Real),
					new Operator("Divide", new Signature(new[] { DataTypes.Real, DataTypes.Real}), DataTypes.Real),

					// TruncatedDivide
					new Operator("TruncatedDivide", new Signature(new[] { DataTypes.Integer, DataTypes.Integer }), DataTypes.Integer),

					// Modulo
					new Operator("Modulo", new Signature(new[] { DataTypes.Integer, DataTypes.Integer }), DataTypes.Integer),
					new Operator("Modulo", new Signature(new[] { DataTypes.Real, DataTypes.Real}), DataTypes.Real),

					// Ceiling
					new Operator("Ceiling", new Signature(new[] { DataTypes.Real }), DataTypes.Integer),

					// Floor
					new Operator("Floor", new Signature(new[] { DataTypes.Real }), DataTypes.Integer),

                    // Truncate
                    new Operator("Truncate", new Signature(new[] { DataTypes.Real }), DataTypes.Integer),

					// Abs
					new Operator("Abs", new Signature(new[] { DataTypes.Integer }), DataTypes.Integer),
					new Operator("Abs", new Signature(new[] { DataTypes.Real }), DataTypes.Real),

					// Negate
					new Operator("Negate", new Signature(new[] { DataTypes.Integer }), DataTypes.Integer),
					new Operator("Negate", new Signature(new[] { DataTypes.Real }), DataTypes.Real),

					// Round
					new Operator("Round", new Signature(new[] { DataTypes.Real }), DataTypes.Real),
					new Operator("Round", new Signature(new[] { DataTypes.Real, DataTypes.Integer }), DataTypes.Real),

					// Ln
					new Operator("Ln", new Signature(new[] { DataTypes.Real }), DataTypes.Real),

					// Log
					new Operator("Log", new Signature(new[] { DataTypes.Real, DataTypes.Real }), DataTypes.Real),

					// Power
					new Operator("Power", new Signature(new[] { DataTypes.Real, DataTypes.Real }), DataTypes.Real),

					// Succ
					new Operator("Succ", new Signature(new[] { DataTypes.Integer }), DataTypes.Integer),
					new Operator("Succ", new Signature(new[] { DataTypes.Timestamp }), DataTypes.Timestamp),

					// Pred
					new Operator("Pred", new Signature(new[] { DataTypes.Integer }), DataTypes.Integer),
					new Operator("Pred", new Signature(new[] { DataTypes.Timestamp }), DataTypes.Timestamp),

					// Concat - Nary, not registered

					// Combine
					new Operator("Combine", new Signature(new[] { new ListType(DataTypes.String) }), DataTypes.String),
					new Operator("Combine", new Signature(new DataType[] { new ListType(DataTypes.String), DataTypes.String }), DataTypes.String),

					// Split
					new Operator("Split", new Signature(new[] { DataTypes.String }), new ListType(DataTypes.String)),
					new Operator("Split", new Signature(new[] { DataTypes.String, DataTypes.String }), new ListType(DataTypes.String)),

					// Length - Interval polymorphic, not registered

					// Upper
					new Operator("Upper", new Signature(new[] { DataTypes.String }), DataTypes.String),

					// Lower
					new Operator("Lower", new Signature(new[] { DataTypes.String }), DataTypes.String),

					// Indexer - List polymorphic, not registered

					// Pos
					new Operator("Pos", new Signature(new[] { DataTypes.String, DataTypes.String }), DataTypes.Integer),

					// Substring
					new Operator("Substring", new Signature(new[] { DataTypes.String, DataTypes.Integer }), DataTypes.String),
					new Operator("Substring", new Signature(new[] { DataTypes.String, DataTypes.Integer, DataTypes.Integer }), DataTypes.String),

					// DateAdd
					new Operator("DateAdd", new Signature(new[] { DataTypes.Timestamp, DataTypes.DateGranularity, DataTypes.Integer }), DataTypes.Timestamp),
					new Operator("DateAdd", new Signature(new[] { DataTypes.Timestamp, DataTypes.DateGranularity, DataTypes.Real }), DataTypes.Timestamp),

					// DateDiff
					new Operator("DateDiff", new Signature(new[] { DataTypes.Timestamp, DataTypes.Timestamp, DataTypes.DateGranularity }), DataTypes.Real),

					// DatePart
					new Operator("DatePart", new Signature(new[] { DataTypes.Timestamp, DataTypes.DateGranularity }), DataTypes.Real),

					// Today
					new Operator("Today", new Signature(new DataType[] { }), DataTypes.Timestamp),

					// Now
					new Operator("Now", new Signature(new DataType[] { }), DataTypes.Timestamp),

                    // DateOf
                    new Operator("DateOf", new Signature(new[] { DataTypes.Timestamp }), DataTypes.Timestamp),

                    // TimeOf
                    new Operator("TimeOf", new Signature(new[] { DataTypes.Timestamp }), DataTypes.Timestamp),

					// Date
					new Operator("Date", new Signature(new[] { DataTypes.Integer, DataTypes.Integer, DataTypes.Integer }), DataTypes.Timestamp),
					new Operator("Date", new Signature(new[] { DataTypes.Integer, DataTypes.Integer, DataTypes.Integer, DataTypes.Integer, DataTypes.Integer, DataTypes.Integer }), DataTypes.Timestamp),
					new Operator("Date", new Signature(new[] { DataTypes.Integer, DataTypes.Integer, DataTypes.Integer, DataTypes.Integer, DataTypes.Integer, DataTypes.Integer, DataTypes.Real }), DataTypes.Timestamp),

					// InValueSet
					new Operator("InValueSet", new Signature(new[] { DataTypes.Code }), DataTypes.Boolean),
					new Operator("InValueSet", new Signature(new[] { DataTypes.CodeList }), DataTypes.Boolean),

					// Subsumes
					new Operator("Subsumes", new Signature(new[] { DataTypes.Code, DataTypes.Code }), DataTypes.Boolean),

					// SetSubsumes
					new Operator("SetSubsumes", new Signature(new[] { DataTypes.CodeList, DataTypes.CodeList }), DataTypes.CodeList),
				};
		}

		#endregion
	}
}
