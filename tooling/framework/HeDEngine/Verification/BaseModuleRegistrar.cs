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
					// And
					new Operator("And", new Signature(new[] { DataTypes.Boolean, DataTypes.Boolean }), DataTypes.Boolean),

					// Or
					new Operator("Or", new Signature(new[] { DataTypes.Boolean, DataTypes.Boolean }), DataTypes.Boolean),

					// Xor
					new Operator("Xor", new Signature(new[] { DataTypes.Boolean, DataTypes.Boolean }), DataTypes.Boolean),

					// Not
					new Operator("Not", new Signature(new[] { DataTypes.Boolean }), DataTypes.Boolean),

					// ToString
					new Operator("ToString", new Signature(new[] { DataTypes.Boolean }), DataTypes.String),
					new Operator("ToString", new Signature(new[] { DataTypes.Integer }), DataTypes.String),
					new Operator("ToString", new Signature(new[] { DataTypes.Decimal }), DataTypes.String),
					new Operator("ToString", new Signature(new[] { DataTypes.DateTime }), DataTypes.String),
					new Operator("ToString", new Signature(new[] { DataTypes.Time }), DataTypes.String),
					new Operator("ToString", new Signature(new[] { DataTypes.Quantity }), DataTypes.String),

					// ToBoolean
					new Operator("ToBoolean", new Signature(new[] { DataTypes.String }), DataTypes.Boolean),

					// ToInteger
					new Operator("ToInteger", new Signature(new[] { DataTypes.String }), DataTypes.Integer),

					// ToDecimal
					new Operator("ToDecimal", new Signature(new[] { DataTypes.String }), DataTypes.Decimal),
					new Operator("ToDecimal", new Signature(new[] { DataTypes.Integer }), DataTypes.Decimal),

					// ToDateTime
					new Operator("ToDateTime", new Signature(new[] { DataTypes.String }), DataTypes.DateTime),
					
					// ToTime
					new Operator("ToTime", new Signature(new[] { DataTypes.String }), DataTypes.Time),

					// ToQuantity
					new Operator("ToQuantity", new Signature(new[] { DataTypes.String }), DataTypes.Quantity),

					// Equal
					new Operator("Equal", new Signature(new[] { DataTypes.Boolean, DataTypes.Boolean }), DataTypes.Boolean),
					new Operator("Equal", new Signature(new[] { DataTypes.Integer, DataTypes.Integer }), DataTypes.Boolean),
					new Operator("Equal", new Signature(new[] { DataTypes.Decimal, DataTypes.Decimal }), DataTypes.Boolean),
					new Operator("Equal", new Signature(new[] { DataTypes.String, DataTypes.String }), DataTypes.Boolean),
					new Operator("Equal", new Signature(new[] { DataTypes.DateTime, DataTypes.DateTime }), DataTypes.Boolean),
					new Operator("Equal", new Signature(new[] { DataTypes.Time, DataTypes.Time }), DataTypes.Boolean),
					new Operator("Equal", new Signature(new[] { DataTypes.Quantity, DataTypes.Quantity }), DataTypes.Boolean),
					new Operator("Equal", new Signature(new[] { DataTypes.Code, DataTypes.Code }), DataTypes.Boolean),
					new Operator("Equal", new Signature(new[] { DataTypes.Concept, DataTypes.Concept }), DataTypes.Boolean),

					// Matches
					new Operator("Matches", new Signature(new[] { DataTypes.Boolean, DataTypes.Boolean }), DataTypes.Boolean),
					new Operator("Matches", new Signature(new[] { DataTypes.Integer, DataTypes.Integer }), DataTypes.Boolean),
					new Operator("Matches", new Signature(new[] { DataTypes.Decimal, DataTypes.Decimal }), DataTypes.Boolean),
					new Operator("Matches", new Signature(new[] { DataTypes.String, DataTypes.String }), DataTypes.Boolean),
					new Operator("Matches", new Signature(new[] { DataTypes.DateTime, DataTypes.DateTime }), DataTypes.Boolean),
					new Operator("Matches", new Signature(new[] { DataTypes.Time, DataTypes.Time }), DataTypes.Boolean),
					new Operator("Matches", new Signature(new[] { DataTypes.Quantity, DataTypes.Quantity }), DataTypes.Boolean),
					new Operator("Matches", new Signature(new[] { DataTypes.Code, DataTypes.Code }), DataTypes.Boolean),
					new Operator("Matches", new Signature(new[] { DataTypes.Concept, DataTypes.Concept }), DataTypes.Boolean),

					// NotEqual
					new Operator("NotEqual", new Signature(new[] { DataTypes.Boolean, DataTypes.Boolean }), DataTypes.Boolean),
					new Operator("NotEqual", new Signature(new[] { DataTypes.Integer, DataTypes.Integer }), DataTypes.Boolean),
					new Operator("NotEqual", new Signature(new[] { DataTypes.Decimal, DataTypes.Decimal }), DataTypes.Boolean),
					new Operator("NotEqual", new Signature(new[] { DataTypes.String, DataTypes.String }), DataTypes.Boolean),
					new Operator("NotEqual", new Signature(new[] { DataTypes.DateTime, DataTypes.DateTime }), DataTypes.Boolean),
					new Operator("NotEqual", new Signature(new[] { DataTypes.Time, DataTypes.Time }), DataTypes.Boolean),
					new Operator("NotEqual", new Signature(new[] { DataTypes.Quantity, DataTypes.Quantity }), DataTypes.Boolean),
					new Operator("NotEqual", new Signature(new[] { DataTypes.Code, DataTypes.Code }), DataTypes.Boolean),
					new Operator("NotEqual", new Signature(new[] { DataTypes.Concept, DataTypes.Concept }), DataTypes.Boolean),

					// Less
					new Operator("Less", new Signature(new[] { DataTypes.Integer, DataTypes.Integer }), DataTypes.Boolean),
					new Operator("Less", new Signature(new[] { DataTypes.Decimal, DataTypes.Decimal }), DataTypes.Boolean),
					new Operator("Less", new Signature(new[] { DataTypes.String, DataTypes.String }), DataTypes.Boolean),
					new Operator("Less", new Signature(new[] { DataTypes.DateTime, DataTypes.DateTime }), DataTypes.Boolean),
					new Operator("Less", new Signature(new[] { DataTypes.Time, DataTypes.Time }), DataTypes.Boolean),
					new Operator("Less", new Signature(new[] { DataTypes.Quantity, DataTypes.Quantity }), DataTypes.Boolean),
					//new Operator("Less", new Signature(new[] { DataTypes.IntegerRatio, DataTypes.IntegerRatio }), DataTypes.Boolean),
					//new Operator("Less", new Signature(new[] { DataTypes.QuantityRatio, DataTypes.QuantityRatio }), DataTypes.Boolean),

					// Greater
					new Operator("Greater", new Signature(new[] { DataTypes.Integer, DataTypes.Integer }), DataTypes.Boolean),
					new Operator("Greater", new Signature(new[] { DataTypes.Decimal, DataTypes.Decimal }), DataTypes.Boolean),
					new Operator("Greater", new Signature(new[] { DataTypes.String, DataTypes.String }), DataTypes.Boolean),
					new Operator("Greater", new Signature(new[] { DataTypes.DateTime, DataTypes.DateTime }), DataTypes.Boolean),
					new Operator("Greater", new Signature(new[] { DataTypes.Time, DataTypes.Time }), DataTypes.Boolean),
					new Operator("Greater", new Signature(new[] { DataTypes.Quantity, DataTypes.Quantity }), DataTypes.Boolean),
					//new Operator("Greater", new Signature(new[] { DataTypes.IntegerRatio, DataTypes.IntegerRatio }), DataTypes.Boolean),
					//new Operator("Greater", new Signature(new[] { DataTypes.QuantityRatio, DataTypes.QuantityRatio }), DataTypes.Boolean),

					// LessOrEqual
					new Operator("LessOrEqual", new Signature(new[] { DataTypes.Integer, DataTypes.Integer }), DataTypes.Boolean),
					new Operator("LessOrEqual", new Signature(new[] { DataTypes.Decimal, DataTypes.Decimal }), DataTypes.Boolean),
					new Operator("LessOrEqual", new Signature(new[] { DataTypes.String, DataTypes.String }), DataTypes.Boolean),
					new Operator("LessOrEqual", new Signature(new[] { DataTypes.DateTime, DataTypes.DateTime }), DataTypes.Boolean),
					new Operator("LessOrEqual", new Signature(new[] { DataTypes.Time, DataTypes.Time }), DataTypes.Boolean),
					new Operator("LessOrEqual", new Signature(new[] { DataTypes.Quantity, DataTypes.Quantity }), DataTypes.Boolean),
					//new Operator("LessOrEqual", new Signature(new[] { DataTypes.IntegerRatio, DataTypes.IntegerRatio }), DataTypes.Boolean),
					//new Operator("LessOrEqual", new Signature(new[] { DataTypes.QuantityRatio, DataTypes.QuantityRatio }), DataTypes.Boolean),

					// GreaterOrEqual
					new Operator("GreaterOrEqual", new Signature(new[] { DataTypes.Integer, DataTypes.Integer }), DataTypes.Boolean),
					new Operator("GreaterOrEqual", new Signature(new[] { DataTypes.Decimal, DataTypes.Decimal }), DataTypes.Boolean),
					new Operator("GreaterOrEqual", new Signature(new[] { DataTypes.String, DataTypes.String }), DataTypes.Boolean),
					new Operator("GreaterOrEqual", new Signature(new[] { DataTypes.DateTime, DataTypes.DateTime }), DataTypes.Boolean),
					new Operator("GreaterOrEqual", new Signature(new[] { DataTypes.Time, DataTypes.Time }), DataTypes.Boolean),
					new Operator("GreaterOrEqual", new Signature(new[] { DataTypes.Quantity, DataTypes.Quantity }), DataTypes.Boolean),
					//new Operator("GreaterOrEqual", new Signature(new[] { DataTypes.IntegerRatio, DataTypes.IntegerRatio }), DataTypes.Boolean),
					//new Operator("GreaterOrEqual", new Signature(new[] { DataTypes.QuantityRatio, DataTypes.QuantityRatio }), DataTypes.Boolean),

					// Add
					new Operator("Add", new Signature(new[] { DataTypes.Integer, DataTypes.Integer }), DataTypes.Integer),
					new Operator("Add", new Signature(new[] { DataTypes.Decimal, DataTypes.Decimal}), DataTypes.Decimal),
					new Operator("Add", new Signature(new[] { DataTypes.Quantity, DataTypes.Quantity}), DataTypes.Quantity),

					// Subtract
					new Operator("Subtract", new Signature(new[] { DataTypes.Integer, DataTypes.Integer }), DataTypes.Integer),
					new Operator("Subtract", new Signature(new[] { DataTypes.Decimal, DataTypes.Decimal }), DataTypes.Decimal),
					new Operator("Subtract", new Signature(new[] { DataTypes.Quantity, DataTypes.Quantity }), DataTypes.Quantity),

					// Multiply
					new Operator("Multiply", new Signature(new[] { DataTypes.Integer, DataTypes.Integer }), DataTypes.Integer),
					new Operator("Multiply", new Signature(new[] { DataTypes.Decimal, DataTypes.Decimal }), DataTypes.Decimal),
					new Operator("Multiply", new Signature(new[] { (DataType)DataTypes.Quantity, DataTypes.Decimal }), DataTypes.Quantity),
					new Operator("Multiply", new Signature(new[] { (DataType)DataTypes.Decimal, DataTypes.Quantity }), DataTypes.Quantity),
					new Operator("Multiply", new Signature(new[] { DataTypes.Quantity, DataTypes.Quantity }), DataTypes.Quantity),

					// Divide
					new Operator("Divide", new Signature(new[] { DataTypes.Integer, DataTypes.Integer }), DataTypes.Decimal),
					new Operator("Divide", new Signature(new[] { DataTypes.Decimal, DataTypes.Decimal }), DataTypes.Decimal),
					new Operator("Divide", new Signature(new[] { (DataType)DataTypes.Quantity, DataTypes.Decimal }), DataTypes.Quantity),
					new Operator("Divide", new Signature(new[] { DataTypes.Quantity, DataTypes.Quantity }), DataTypes.Quantity),

					// TruncatedDivide
					new Operator("TruncatedDivide", new Signature(new[] { DataTypes.Integer, DataTypes.Integer }), DataTypes.Integer),
					new Operator("TruncatedDivide", new Signature(new[] { DataTypes.Decimal, DataTypes.Decimal }), DataTypes.Decimal),

					// Modulo
					new Operator("Modulo", new Signature(new[] { DataTypes.Integer, DataTypes.Integer }), DataTypes.Integer),
					new Operator("Modulo", new Signature(new[] { DataTypes.Decimal, DataTypes.Decimal}), DataTypes.Decimal),

					// Ceiling
					new Operator("Ceiling", new Signature(new[] { DataTypes.Decimal }), DataTypes.Integer),

					// Floor
					new Operator("Floor", new Signature(new[] { DataTypes.Decimal }), DataTypes.Integer),

                    // Truncate
                    new Operator("Truncate", new Signature(new[] { DataTypes.Decimal }), DataTypes.Integer),

					// Abs
					new Operator("Abs", new Signature(new[] { DataTypes.Integer }), DataTypes.Integer),
					new Operator("Abs", new Signature(new[] { DataTypes.Decimal }), DataTypes.Decimal),
					new Operator("Abs", new Signature(new[] { DataTypes.Quantity }), DataTypes.Quantity),

					// Negate
					new Operator("Negate", new Signature(new[] { DataTypes.Integer }), DataTypes.Integer),
					new Operator("Negate", new Signature(new[] { DataTypes.Decimal }), DataTypes.Decimal),
					new Operator("Negate", new Signature(new[] { DataTypes.Quantity }), DataTypes.Quantity),

					// Round
					new Operator("Round", new Signature(new[] { DataTypes.Decimal }), DataTypes.Decimal),
					new Operator("Round", new Signature(new[] { DataTypes.Decimal, DataTypes.Integer }), DataTypes.Decimal),

					// Ln
					new Operator("Ln", new Signature(new[] { DataTypes.Decimal }), DataTypes.Decimal),

					// Log
					new Operator("Log", new Signature(new[] { DataTypes.Decimal, DataTypes.Decimal }), DataTypes.Decimal),

					// Power
					new Operator("Power", new Signature(new[] { DataTypes.Integer, DataTypes.Integer }), DataTypes.Integer),
					new Operator("Power", new Signature(new[] { DataTypes.Decimal, DataTypes.Decimal }), DataTypes.Decimal),

					// Succ
					new Operator("Successor", new Signature(new[] { DataTypes.Integer }), DataTypes.Integer),
					new Operator("Successor", new Signature(new[] { DataTypes.Decimal }), DataTypes.Decimal),
					new Operator("Successor", new Signature(new[] { DataTypes.DateTime }), DataTypes.DateTime),
					new Operator("Successor", new Signature(new[] { DataTypes.Time }), DataTypes.Time),
					new Operator("Successor", new Signature(new[] { DataTypes.Quantity }), DataTypes.Quantity),

					// Predecessor
					new Operator("Predecessor", new Signature(new[] { DataTypes.Integer }), DataTypes.Integer),
					new Operator("Predecessor", new Signature(new[] { DataTypes.Decimal }), DataTypes.Decimal),
					new Operator("Predecessor", new Signature(new[] { DataTypes.DateTime }), DataTypes.DateTime),
					new Operator("Predecessor", new Signature(new[] { DataTypes.Time }), DataTypes.Time),
					new Operator("Predecessor", new Signature(new[] { DataTypes.Quantity }), DataTypes.Quantity),

					// Concat - Nary, not registered
					new Operator("Concatenate", new Signature(new[] { DataTypes.String, DataTypes.String }), DataTypes.String),

					// Add
					new Operator("Add", new Signature(new[] { DataTypes.String, DataTypes.String }), DataTypes.String),

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

					// PositionOf
					new Operator("PositionOf", new Signature(new[] { DataTypes.String, DataTypes.String }), DataTypes.Integer),

					// Substring
					new Operator("Substring", new Signature(new[] { DataTypes.String, DataTypes.Integer }), DataTypes.String),
					new Operator("Substring", new Signature(new[] { DataTypes.String, DataTypes.Integer, DataTypes.Integer }), DataTypes.String),

					// DateAdd
					//new Operator("DateAdd", new Signature(new[] { DataTypes.DateTime, DataTypes.DateGranularity, DataTypes.Integer }), DataTypes.DateTime),
					//new Operator("DateAdd", new Signature(new[] { DataTypes.DateTime, DataTypes.DateGranularity, DataTypes.Decimal }), DataTypes.DateTime),

					// DateDiff
					//new Operator("DateDiff", new Signature(new[] { DataTypes.DateTime, DataTypes.DateTime, DataTypes.DateGranularity }), DataTypes.Decimal),

					// DatePart
					//new Operator("DatePart", new Signature(new[] { DataTypes.DateTime, DataTypes.DateGranularity }), DataTypes.Decimal),

					// Add
					new Operator("Add", new Signature(new[] { (DataType)DataTypes.DateTime, DataTypes.Quantity }), DataTypes.DateTime),
					new Operator("Add", new Signature(new[] { (DataType)DataTypes.Time, DataTypes.Quantity }), DataTypes.Time),
					new Operator("After", new Signature(new[] { DataTypes.DateTime, DataTypes.DateTime }), DataTypes.Boolean),
					new Operator("After", new Signature(new[] { DataTypes.Time, DataTypes.Time }), DataTypes.Boolean),
					new Operator("Before", new Signature(new[] { DataTypes.DateTime, DataTypes.DateTime }), DataTypes.Boolean),
					new Operator("Before", new Signature(new[] { DataTypes.Time, DataTypes.Time }), DataTypes.Boolean),
					new Operator("DateTime", new Signature(new[] { DataTypes.Integer }), DataTypes.DateTime),
					new Operator("DateTime", new Signature(new[] { DataTypes.Integer, DataTypes.Integer }), DataTypes.DateTime),
					new Operator("DateTime", new Signature(new[] { DataTypes.Integer, DataTypes.Integer, DataTypes.Integer }), DataTypes.DateTime),
					new Operator("DateTime", new Signature(new[] { DataTypes.Integer, DataTypes.Integer, DataTypes.Integer, DataTypes.Integer }), DataTypes.DateTime),
					new Operator("DateTime", new Signature(new[] { DataTypes.Integer, DataTypes.Integer, DataTypes.Integer, DataTypes.Integer, DataTypes.Integer }), DataTypes.DateTime),
					new Operator("DateTime", new Signature(new[] { DataTypes.Integer, DataTypes.Integer, DataTypes.Integer, DataTypes.Integer, DataTypes.Integer, DataTypes.Integer }), DataTypes.DateTime),
					new Operator("DateTime", new Signature(new[] { DataTypes.Integer, DataTypes.Integer, DataTypes.Integer, DataTypes.Integer, DataTypes.Integer, DataTypes.Integer, DataTypes.Integer }), DataTypes.DateTime),
					new Operator("DateTime", new Signature(new[] { DataTypes.Integer, DataTypes.Integer, DataTypes.Integer, DataTypes.Integer, DataTypes.Integer, DataTypes.Integer, DataTypes.Integer, DataTypes.Decimal }), DataTypes.DateTime),

					new Operator("DifferenceBetween", new Signature(new[] { DataTypes.DateTime, DataTypes.DateTime }), DataTypes.Integer),
					new Operator("DifferenceBetween", new Signature(new[] { DataTypes.Time, DataTypes.Time }), DataTypes.Integer),
					new Operator("DurationBetween", new Signature(new[] { DataTypes.DateTime, DataTypes.DateTime }), DataTypes.Integer),
					new Operator("DurationBetween", new Signature(new[] { DataTypes.Time, DataTypes.Time }), DataTypes.Integer),

					new Operator("SameAs", new Signature(new[] { DataTypes.DateTime, DataTypes.DateTime }), DataTypes.Boolean),
					new Operator("SameAs", new Signature(new[] { DataTypes.Time, DataTypes.Time }), DataTypes.Boolean),
					new Operator("SameOrAfter", new Signature(new[] { DataTypes.DateTime, DataTypes.DateTime }), DataTypes.Boolean),
					new Operator("SameOrAfter", new Signature(new[] { DataTypes.Time, DataTypes.Time }), DataTypes.Boolean),
					new Operator("SameOrBefore", new Signature(new[] { DataTypes.DateTime, DataTypes.DateTime }), DataTypes.Boolean),
					new Operator("SameOrBefore", new Signature(new[] { DataTypes.Time, DataTypes.Time }), DataTypes.Boolean),

					new Operator("Subtract", new Signature(new[] { (DataType)DataTypes.DateTime, DataTypes.Quantity }), DataTypes.DateTime),
					new Operator("Subtract", new Signature(new[] { (DataType)DataTypes.Time, DataTypes.Quantity }), DataTypes.Time),

					// Today
					new Operator("Today", new Signature(new DataType[] { }), DataTypes.DateTime),

					// Now
					new Operator("Now", new Signature(new DataType[] { }), DataTypes.DateTime),

					// TimeOfDay
					new Operator("TimeOfDay", new Signature(new DataType[] { }), DataTypes.Time),

                    // DateFrom
                    new Operator("DateFrom", new Signature(new[] { DataTypes.DateTime }), DataTypes.DateTime),

                    // TimeFrom
                    new Operator("TimeFrom", new Signature(new[] { DataTypes.DateTime }), DataTypes.Time),

					// TimezoneFrom
					new Operator("TimezoneFrom", new Signature(new[] { DataTypes.DateTime }), DataTypes.Decimal),
					new Operator("TimezoneFrom", new Signature(new[] { DataTypes.Time }), DataTypes.Decimal),

					// Time
					new Operator("Time", new Signature(new[] { DataTypes.Integer }), DataTypes.Time),
					new Operator("Time", new Signature(new[] { DataTypes.Integer, DataTypes.Integer }), DataTypes.Time),
					new Operator("Time", new Signature(new[] { DataTypes.Integer, DataTypes.Integer, DataTypes.Integer }), DataTypes.Time),
					new Operator("Time", new Signature(new[] { DataTypes.Integer, DataTypes.Integer, DataTypes.Integer, DataTypes.Integer }), DataTypes.Time),
					new Operator("Time", new Signature(new[] { DataTypes.Integer, DataTypes.Integer, DataTypes.Integer, DataTypes.Integer, DataTypes.Decimal }), DataTypes.Time),

					// InCodeSystem
					new Operator("InCodeSystem", new Signature(new[] { (DataType)DataTypes.Code, DataTypes.CodeList }), DataTypes.Boolean),
					new Operator("InCodeSystem", new Signature(new[] { (DataType)DataTypes.Concept, DataTypes.CodeList }), DataTypes.Boolean),

					// InValueSet
					new Operator("InValueSet", new Signature(new[] { (DataType)DataTypes.Code, DataTypes.CodeList }), DataTypes.Boolean),
					new Operator("InValueSet", new Signature(new[] { (DataType)DataTypes.Concept, DataTypes.CodeList }), DataTypes.Boolean),

					// ToConcept
					new Operator("ToConcept", new Signature(new[] { DataTypes.Code }), DataTypes.Concept)
				};
		}

		#endregion
	}
}
