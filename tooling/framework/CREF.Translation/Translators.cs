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
using HeD.Engine.Translation;
using HeD.Engine.Verification;
using CREFModel = CREF.Model;

namespace CREF.Translation
{
	// ExpressionRef
	public class ExpressionRefTranslator : INodeTranslator
	{
		#region INodeTranslator Members

		public object Translate(TranslationContext context, ASTNode node)
		{
			var result = new CREFModel.ExpressionReference();

			result.Name = node.GetAttribute<string>("name");

			return result;
		}

		#endregion
	}

	// ParameterRef
	public class ParameterRefTranslator : INodeTranslator
	{
		#region INodeTranslator Members

		public object Translate(TranslationContext context, ASTNode node)
		{
			var result = new CREFModel.ParameterExpression();

			result.Name = node.GetAttribute<string>("name");

			// TODO: Default value?

			return result;
		}

		#endregion
	}

	// Literal
	public class LiteralTranslator : INodeTranslator
	{
		#region INodeTranslator Members

		public object Translate(TranslationContext context, ASTNode node)
		{
			var result = new CREFModel.ValueExpression();

			if (DataTypes.Equal(node.ResultType, DataTypes.String))
			{
				result.Type = Model.ValueType.String;
				result.TypeSpecified = true;
				result.Value = node.GetAttribute<string>("value");
			}
			else if (DataTypes.Equal(node.ResultType, DataTypes.Integer))
			{
				result.Type = Model.ValueType.Int32;
				result.TypeSpecified = true;
				result.Value = node.GetAttribute<string>("value");
			}
			else if (DataTypes.Equal(node.ResultType, DataTypes.Boolean))
			{
				result.Type = Model.ValueType.Boolean;
				result.TypeSpecified = true;
				result.Value = node.GetAttribute<string>("value");
			}
			else if (DataTypes.Equal(node.ResultType, DataTypes.Real))
			{
				result.Type = Model.ValueType.Decimal;
				result.TypeSpecified = true;
				result.Value = node.GetAttribute<string>("value");
			}
			else if (DataTypes.Equal(node.ResultType, DataTypes.Timestamp))
			{
				result.Type = Model.ValueType.Date;
				result.TypeSpecified = true;
				result.Value = node.GetAttribute<string>("value"); // TODO: Convert to format expected by CREF
			}
			else
			{
				throw new NotSupportedException(String.Format("Unsupported literal type: {0}.", node.ResultType.Name));
			}

			return result;
		}

		#endregion
	}

	// ComplexLiteral
	// PropertyExpression
	// ObjectExpression
	// ObjectRedefine

	// Interval
	public class IntervalTranslator : INodeTranslator
	{
		#region INodeTranslator Members

		public object Translate(TranslationContext context, ASTNode node)
		{
			if (DataTypes.Equal(node.ResultType, DataTypes.TimestampInterval))
			{
				var result = new CREFModel.DateRange();

				var beginOpen = Convert.ToBoolean(node.GetAttribute<string>("beginOpen"));
				var endOpen = Convert.ToBoolean(node.GetAttribute<string>("endOpen"));
				if (beginOpen || endOpen)
				{
					throw new NotSupportedException("Translation for open intervals is not supported because CREF only supports closed interval values.");
				}

				foreach (var child in node.Children)
				{
					result.Items.Add(context.TranslateNode(child));
				}

				return result;
			}
			else
			{
				throw new NotSupportedException("Translation for intervals with point types other than Timestamp is not supported because CREF does not support generic interval values.");
			}
		}

		#endregion
	}

	// List
	public class ListTranslator : INodeTranslator
	{
		#region INodeTranslator Members

		public object Translate(TranslationContext context, ASTNode node)
		{
			var result = new CREFModel.ListExpression();

			foreach (var child in node.Children)
			{
				result.Items.Add(context.TranslateNode(child));
			}

			return result;
		}

		#endregion
	}

    //Base classes for operators
	public abstract class NaryLogicalNodeTranslator : INodeTranslator
	{
		protected abstract CREFModel.LogicalConnectiveOperator GetOperator();
	
		#region INodeTranslator Members

		public object Translate(TranslationContext context, ASTNode node)
		{
			// Use LogicalConnective
			var result = new CREFModel.LogicalConnective();

			result.Operator = GetOperator();
			result.OperatorSpecified = true;

			foreach (var child in node.Children)
			{
				var childResult = context.TranslateNode(child);
				result.Items.Add(childResult);
			}

			return result;
		}

		#endregion
	}

    public abstract class BinaryNodeTranslator : INodeTranslator
    {
        protected abstract CREFModel.BinaryOperator GetOperator();

        #region INodeTranslator Members

        public object Translate(TranslationContext context, ASTNode node)
        {
            var result = new CREFModel.BinaryExpression();
            result.Operator = GetOperator();
			result.OperatorSpecified = true;

            foreach (var child in node.Children)
            {
                result.Items.Add(context.TranslateNode(child));
            }

            return result;
        }

        #endregion
    }

    public abstract class UnaryNodeTranslator : INodeTranslator
    {
        protected abstract CREFModel.UnaryOperator GetOperator();

        #region INodeTranslator Members

        public object Translate(TranslationContext context, ASTNode node)
        {
			var result = new CREFModel.UnaryExpression();
			result.Operator = GetOperator();
			result.OperatorSpecified = true;

			result.Item = context.TranslateNode(node.Children[0]);

			return result;
        }

        #endregion
    }

    // And
    public class AndTranslator : NaryLogicalNodeTranslator
    {
        protected override CREFModel.LogicalConnectiveOperator GetOperator()
        {
            return CREFModel.LogicalConnectiveOperator.opAnd;
        }
    }

    // Or
    public class OrTranslator : NaryLogicalNodeTranslator
    {
        protected override CREFModel.LogicalConnectiveOperator GetOperator()
        {
            return CREFModel.LogicalConnectiveOperator.opOr;
        }
    }

	// Not
    public class NotTranslator : UnaryNodeTranslator
    {
        protected override CREFModel.UnaryOperator GetOperator()
        {
            return CREFModel.UnaryOperator.opNot;
        }
    }

	// Conditional
    public class ConditionalTranslator : INodeTranslator
    {
        #region INodeTranslator Members

        public object Translate(TranslationContext context, ASTNode node)
        {
            var result = new CREFModel.Condition();

            foreach (var child in node.Children)
            {
                result.Items.Add(context.TranslateNode(child));
            }

            return result;
        }

        #endregion
    }

    // Case
    public class CaseTranslator : INodeTranslator
    {
        #region INodeTranslator Members

        public object Translate(TranslationContext context, ASTNode node)
        {
            var result = new CREFModel.Choice();

			var comparand = node.Children.FirstOrDefault(n => n.Name == "comparand");

			foreach (var caseItem in ((Node)node).Children.Where(n => n.Name == "caseItem"))
			{
                var whenNode = caseItem.Children[0] as ASTNode;
                var thenNode = caseItem.Children[1] as ASTNode;

                var condition = new CREFModel.Condition();
                if (comparand == null)
                {
                    condition.Items.Add(context.TranslateNode(whenNode));
                }
                else
                {
                    var equal = new CREFModel.BinaryExpression();
                    equal.Operator = CREFModel.BinaryOperator.opEqual;
                    equal.OperatorSpecified = true;
                    equal.Items.Add(context.TranslateNode(comparand));
                    equal.Items.Add(context.TranslateNode(whenNode));
                    condition.Items.Add(equal);
                }

                condition.Items.Add(context.TranslateNode(thenNode));

                result.Items.Add(condition);
            }

			var elseNode = node.Children.FirstOrDefault(n => n.Name == "else") as ASTNode;

            result.Items.Add(context.TranslateNode(elseNode));

            return result;
        }

        #endregion
    }

	// Null
    public class NullTranslator : INodeTranslator
    {
        #region INodeTranslator Members

        public object Translate(TranslationContext context, ASTNode node)
        {
            // There is no Null expression in CREF, but returning an empty Coalesce is equivalent to null
            var result = new CREFModel.Coalesce();

            return result;
        }

        #endregion
    }

	// IsNull

	// IfNull
    public class IfNullTranslator : INodeTranslator
    {
        #region INodeTranslator Members

        public object Translate(TranslationContext context, ASTNode node)
        {
            var result = new CREFModel.Coalesce();

            foreach (var child in node.Children)
            {
                result.Items.Add(context.TranslateNode(child));
            }

            return result;
        }

        #endregion
    }

	// Coalesce
    public class CoalesceTranslator : INodeTranslator
    {
        #region INodeTranslator Members

        public object Translate(TranslationContext context, ASTNode node)
        {
            var result = new CREFModel.Coalesce();

            foreach (var child in node.Children)
            {
                result.Items.Add(context.TranslateNode(child));
            }

            return result;
        }

        #endregion
    }

    // Is

    // As
    public class AsTranslator : INodeTranslator
    {
        #region INodeTranslator Members

        public object Translate(TranslationContext context, ASTNode node)
        {
            var child = node.Children[0];
            return context.TranslateNode(child);
        }

        #endregion
    }

    // Convert

	// Equal
    public class EqualTranslator : BinaryNodeTranslator
    {
        protected override CREFModel.BinaryOperator GetOperator()
        {
           return CREFModel.BinaryOperator.opEqual;
        }
    }

	// NotEqual
    public class NotEqualTranslator : BinaryNodeTranslator
    {
        protected override CREFModel.BinaryOperator GetOperator()
        {
            return CREFModel.BinaryOperator.opNotEqual;
        }
    }
	
    // Less
    public class LessTranslator : BinaryNodeTranslator
    {
        protected override CREFModel.BinaryOperator GetOperator()
        {
            return CREFModel.BinaryOperator.opLess;
        }
    }
	
    // Greater
    public class GreaterTranslator : BinaryNodeTranslator
    {
        protected override CREFModel.BinaryOperator GetOperator()
        {
            return CREFModel.BinaryOperator.opGreater;
        }
    }

	// LessOrEqual
    public class LessOrEqualTranslator : BinaryNodeTranslator
    {
        protected override CREFModel.BinaryOperator GetOperator()
        {
            return CREFModel.BinaryOperator.opLessOrEqual;
        }
    }

    // GreaterOrqual
    public class GreaterOrEqualTranslator : BinaryNodeTranslator
    {
        protected override CREFModel.BinaryOperator GetOperator()
        {
            return CREFModel.BinaryOperator.opGreaterOrEqual;
        }
    }

    // Add
    public class AddTranslator : BinaryNodeTranslator
    {
        protected override CREFModel.BinaryOperator GetOperator()
        {
            return CREFModel.BinaryOperator.opAdd;
        }
    }

	// Subtract
    public class SubtractTranslator : BinaryNodeTranslator
    {
        protected override CREFModel.BinaryOperator GetOperator()
        {
            return CREFModel.BinaryOperator.opSubtract;
        }
    }

	// Multiply
    public class MultiplyTranslator : BinaryNodeTranslator
    {
        protected override CREFModel.BinaryOperator GetOperator()
        {
            return CREFModel.BinaryOperator.opMultiply;
        }
    }

    // Divide - hmm, div or divide, how do I know?
	// BTR -> div is integer division, which is TruncatedDivide in HeD, so this mapping is correct
    public class DivideTranslator : BinaryNodeTranslator
    {
        protected override CREFModel.BinaryOperator GetOperator()
        {
            return CREFModel.BinaryOperator.opDivide;
        }
    }

	// TruncatedDivide - hmm, div or divide, how do I know?
    public class TruncatedDivideTranslator : BinaryNodeTranslator
    {
        protected override CREFModel.BinaryOperator GetOperator()
        {
            return CREFModel.BinaryOperator.opDiv;
        }
    }

	// Modulo
    public class ModuloTranslator : BinaryNodeTranslator
    {
        protected override CREFModel.BinaryOperator GetOperator()
        {
            return CREFModel.BinaryOperator.opMod;
        }
    }

	// Ceiling
    public class CeilingTranslator : UnaryNodeTranslator
    {
        protected override CREFModel.UnaryOperator GetOperator()
        {
            return CREFModel.UnaryOperator.opCeiling;
        }
    }

	// Floor
    public class FloorTranslator : UnaryNodeTranslator
    {
        protected override CREFModel.UnaryOperator GetOperator()
        {
            return CREFModel.UnaryOperator.opFloor;
        }
    }

	// Abs

	// Negate
    public class NegateTranslator : UnaryNodeTranslator
    {
        protected override CREFModel.UnaryOperator GetOperator()
        {
            return CREFModel.UnaryOperator.opNegate;
        }
    }

	// Round
    public class RoundTranslator : UnaryNodeTranslator
    {
        protected override CREFModel.UnaryOperator GetOperator()
        {
            return CREFModel.UnaryOperator.opRound;
        }
    }

	// Ln
	// Log

	// Power
    public class PowerTranslator : BinaryNodeTranslator
    {
        protected override CREFModel.BinaryOperator GetOperator()
        {
            return CREFModel.BinaryOperator.opPower;
        }
    }

	// Succ
	// Pred
	// Concat
	// Combine
	// Split
	// Length
	// Upper
	// Lower

	// First
    public class FirstTranslator : UnaryNodeTranslator
    {
        protected override CREFModel.UnaryOperator GetOperator()
        {
            return CREFModel.UnaryOperator.opFirst;
        }
    }

	// Last
    public class LastTranslator : UnaryNodeTranslator
    {
        protected override CREFModel.UnaryOperator GetOperator()
        {
            return CREFModel.UnaryOperator.opLast;
        }
    }

	// Indexer
	// Pos
	// Substring

	// DateAdd
	public class DateAddTranslator : INodeTranslator
	{
		#region INodeTranslator Members

		// Children
			// date
			// granularity
			// numberOfPeriods
		public object Translate(TranslationContext context, ASTNode node)
		{
			var result = new CREFModel.DateAdd();

			var date = node.Children[0];
			result.Items.Add(context.TranslateNode(date));

			var granularity = node.Children[1];
			if (granularity.NodeType == "urn:hl7-org:v3:knowledgeartifact:r1:Literal") // TODO: Better story for this type of thing....
			{
				result.Granularity = (CREFModel.DateGranularity)Enum.Parse(typeof(CREFModel.DateGranularity), granularity.GetAttribute<string>("value"));
				result.GranularitySpecified = true;
			}
			else
			{
				throw new NotSupportedException("Date granularity argument to a DateAdd expression must be a literal because CREF does not support granularity as an argument, only as an attribute of the target DateAdd expression.");
			}

			var numberOfPeriods = node.Children[2];
			result.Items.Add(context.TranslateNode(numberOfPeriods));

			return result;
		}

		#endregion
	}

	// DateDiff
	// DatePart

	// Today
	public class TodayTranslator : INodeTranslator
	{
		#region INodeTranslator Members

		public object Translate(TranslationContext context, ASTNode node)
		{
			var result = new CREFModel.Today();

			return result;
		}

		#endregion
	}

	// Now
	// Date

	// Contains
    public class ContainsTranslator : INodeTranslator
    {
        #region INodeTranslator Members

        public object Translate(TranslationContext context, ASTNode node)
        {
            // As long as this is list containment, it can be translated using an Exists(Filter(Source, Condition(Current = Value)));
            // TODO: Contains on a date interval could be translated as well
            var sourceNode = node.Children.FirstOrDefault(c => c.Name == "source");
            var elementNode = node.Children.FirstOrDefault(c => c.Name == "element");

            if (sourceNode.ResultType is IntervalType)
            {
                throw new NotImplementedException("Contains translation with Interval source is not yet supported.");
            }

            var filter = new CREFModel.FilterExpression();

            filter.Items.Add(context.TranslateNode(sourceNode));
            
            var condition = new CREFModel.BinaryExpression();
            condition.Operator = CREFModel.BinaryOperator.opEqual;
            condition.OperatorSpecified = true;

            // Assumption: A property expression with no path specified is equivalent to a "current" reference
            var property = new CREFModel.PropertyExpression();

            condition.Items.Add(property);
            condition.Items.Add(context.TranslateNode(elementNode));

            var exists = new CREFModel.UnaryExpression();
            exists.Operator = CREFModel.UnaryOperator.opExists;
            exists.OperatorSpecified = true;
            exists.Item = filter;

            return exists;
        }

        #endregion
    }

	// Within
	// ProperContains
	// ProperIn
	// Before
	// After
	// Meets
	// Overlaps
	// Union
	// Intersect
	// Difference
	// Begin
	// End

	// IsEmpty
	public class IsEmptyTranslator : INodeTranslator
	{
		#region INodeTranslator Members

		public object Translate(TranslationContext context, ASTNode node)
		{
			var unaryExpression = new CREFModel.UnaryExpression();
			unaryExpression.Operator = CREFModel.UnaryOperator.opExists;
			unaryExpression.OperatorSpecified = true;
			unaryExpression.Item = context.TranslateNode(node.Children[0]);

			var notExpression = new CREFModel.UnaryExpression();
			notExpression.Operator = CREFModel.UnaryOperator.opNot;
			notExpression.OperatorSpecified = true;
			notExpression.Item = unaryExpression;

			return notExpression;
		}

		#endregion
	}

	// IsNotEmpty
	public class IsNotEmptyTranslator : INodeTranslator
	{
		#region INodeTranslator Members

		public object Translate(TranslationContext context, ASTNode node)
		{
			var unaryExpression = new CREFModel.UnaryExpression();
			unaryExpression.Operator = CREFModel.UnaryOperator.opExists;
			unaryExpression.OperatorSpecified = true;
			unaryExpression.Item = context.TranslateNode(node.Children[0]);

			return unaryExpression;
		}

		#endregion
	}

	// Filter
    public class FilterTranslator : INodeTranslator
    {
        #region INodeTranslator Members

        public object Translate(TranslationContext context, ASTNode node)
        {
            var scope = node.GetAttribute<string>("scope", VerificationContext.Current);
            if (scope != VerificationContext.Current)
            {
                throw new NotSupportedException("Translation of filter expression is not supported because it involves a named scope reference, which is not supported in CREF.");
            }

            var source = node.Children[0];
            var condition = node.Children[1];

            var result = new CREFModel.FilterExpression();
            result.Items.Add(context.TranslateNode(source));
            result.Items.Add(context.TranslateNode(condition));

            return result;
        }

        #endregion
    }

	// IndexOf

	// In
    public class InTranslator : INodeTranslator
    {
        #region INodeTranslator Members

        public object Translate(TranslationContext context, ASTNode node)
        {
            // As long as this is list containment, it can be translated using an Exists(Filter(Collection, Condition(Current = Element)));
            var collectionNode = node.Children.FirstOrDefault(c => c.Name == "collection");
            var elementNode = node.Children.FirstOrDefault(c => c.Name == "element");

            if (elementNode.ResultType is ListType)
            {
                throw new NotSupportedException("In translation with an element of type list is not supported because there is no equivalent CREF representation.");
            }

            var filter = new CREFModel.FilterExpression();

            filter.Items.Add(context.TranslateNode(collectionNode));
            
            var condition = new CREFModel.BinaryExpression();
            condition.Operator = CREFModel.BinaryOperator.opEqual;
            condition.OperatorSpecified = true;

            // Assumption: A property expression with no path specified is equivalent to a "current" reference
            var property = new CREFModel.PropertyExpression();

            condition.Items.Add(property);
            condition.Items.Add(context.TranslateNode(elementNode));

            var exists = new CREFModel.UnaryExpression();
            exists.Operator = CREFModel.UnaryOperator.opExists;
            exists.OperatorSpecified = true;
            exists.Item = filter;

            return exists;
        }

        #endregion
    }

	// Sort
	// ForEach
	// Distinct
	// Current
	// Count
	// Sum
	// Min
	// Max
	// Avg
	// AllTrue
	// AnyTrue

	// Property
	public class PropertyTranslator : INodeTranslator
	{
		#region INodeTranslator Members

		public object Translate(TranslationContext context, ASTNode node)
		{
			// Reject translation for properties with an explicit scope
			var scope = node.GetAttribute<string>("scope", VerificationContext.Current);

			if (scope != VerificationContext.Current)
			{
				throw new NotSupportedException("Property path cannot be translated because it references an explicit scope, which is not supported in CREF.");
			}

			var sourceType = node.GetAttribute<ObjectType>("sourceType");

			// Translate path information
			return context.TransformModelPath(sourceType, node, node.GetAttribute<string>("path"));
		}

		#endregion
	}

	// AddressLiteral

	// BooleanLiteral
    public class BooleanLiteralTranslator : INodeTranslator
    {
        #region INodeTranslator Members

        public object Translate(TranslationContext context, ASTNode node)
        {
            var result = new CREFModel.ValueExpression();
            
            result.Type = CREFModel.ValueType.Boolean;
            result.TypeSpecified = true;

            result.Value = node.GetAttribute<string>("value"); // TODO: This assumes boolean representation will translate...

            return result;
        }

        #endregion
    }

	// CodeLiteral
	public class CodeLiteralTranslator : INodeTranslator
	{
		#region INodeTranslator Members

		public object Translate(TranslationContext context, ASTNode node)
		{
			var result = new CREFModel.Code();

			result.CodeValue = node.GetAttribute<string>("code");
			result.CodingSystem = node.GetAttribute<string>("codeSystemName");
			result.CodeType = node.GetAttribute<string>("codeSystem"); // TODO: Verify w/ Allscripts
			result.CodeDescription = node.GetAttribute<string>("displayName");

			return result;
		}

		#endregion
	}

	// CodedOrdinalLiteral
	// SimpleCodeLiteral
	// EntityNameLiteral
	// IdentifierLiteral
	// IntegerLiteral
	// IntegerIntervalLiteral
	// PhysicalQuantityIntervalLiteral
	// IntegerRatioIntervalLiteral
	// PhysicalQuantityRatioIntervalLiteral
	// RealIntervalLiteral
	// TimestampIntervalLiteral

	// PhysicalQuantityLiteral
	public class PhysicalQuantityLiteralTranslator : INodeTranslator
	{
		#region INodeTranslator Members

		public object Translate(TranslationContext context, ASTNode node)
		{
			// Physical quantity is not supported as a value type within CREF, so the literal will need to be transformed to a "unit normalized representation"
			var value = Convert.ToDecimal(node.GetAttribute<string>("value"));
			var unit = node.GetAttribute<string>("unit");

			switch (unit)
			{
				case "1" : break; // Do nothing, this is the unity unit in UCUM, value is unadjusted

				// Time units
				case "a" : value *= 365.25m; break; // Year, convert to days
				case "mo" : value *= 30.4375m; break; // Month, convert to days
				case "wk" : value *= 7m; break; // Week, convert to days
				case "d" : break; // Day

				// These time units are not yet supported
				//"h" : break; // Hour
				//"min" : break; // Minute
				//"Ms" : break; // Megasecond
				//"ks" : break; // Kilosecond
				//"s" : break; // Second
				//"ms" : break; // Millisecond
				//"us" : break; // Microsecond
				//"ns" : break; // Nanosecond
				//"ps" : break; // Picosecond

				default : throw new NotSupportedException(String.Format("Physical quantity unit translation for unit type of '{0}' is not supported.", unit));
			}

			var result = new CREFModel.ValueExpression();

			result.Type = CREFModel.ValueType.Decimal;
			result.TypeSpecified = true;

			result.Value = Convert.ToString(value);

			return result;
		}

		#endregion
	}

	// RealLiteral
	// IntegerRatioLiteral
	// PhysicalQuantityRatioLiteral

	// StringLiteral
    public class StringLiteralTranslator : INodeTranslator
    {
        #region INodeTranslator Members

        public object Translate(TranslationContext context, ASTNode node)
        {
            var result = new CREFModel.ValueExpression();
            
            result.Type = CREFModel.ValueType.String;
            result.TypeSpecified = true;

            result.Value = node.GetAttribute<string>("value");

            return result;
        }

        #endregion
    }

	// UrlLiteral
	// TimestampLiteral
	// PeriodLiteral

	// DataRequest
	// ClinicalRequest
	public class ClinicalRequestTranslator : INodeTranslator
	{
		#region INodeTranslator Members

		public object Translate(TranslationContext context, ASTNode node)
		{
			// Model mapping
			var result = new CREFModel.RequestExpression();

			if (node.ResultType == null)
			{
				throw new InvalidOperationException("Clinical request expression has no result type.");
			}

			var cardinality = (RequestCardinality)Enum.Parse(typeof(RequestCardinality), node.Attributes["cardinality"].ToString(), true);
			result.Cardinality = cardinality == RequestCardinality.Single ? CREFModel.RequestCardinality.Single : CREFModel.RequestCardinality.Multiple;
			result.CardinalitySpecified = true;

			var requestListType = node.ResultType as ListType;
			var requestType = (requestListType == null ? node.ResultType : requestListType.ElementType) as ObjectType;
			if (requestType == null)
			{
				throw new InvalidOperationException(String.Format("Unable to determine request type from source type: '{0}'.", node.ResultType.Name));
			}

			result.Type = GetQueryType(context.TransformModelType(requestType));
			result.TypeSpecified = true;

			// Translate Codes
			var codes = node.Children.Where(n => n.Name == "codes").FirstOrDefault();
			if (codes != null)
			{
				var codesResult = context.TranslateNode(codes);
				result.Items.Add(codesResult);
			}

			// Translate DateRange
			var dateRange = node.Children.Where(n => n.Name == "dateRange").FirstOrDefault();
			if (dateRange != null)
			{
				var dateRangeResult = context.TranslateNode(dateRange);
				result.Items.Add(dateRangeResult);
			}

			// Validate idProperty, dateProperty, and codeProperty
			ValidateIdProperty(requestType, node.GetAttribute<string>("idProperty"));
			ValidateDateProperty(requestType, node.GetAttribute<string>("dateProperty"));
			ValidateCodeProperty(requestType, node.GetAttribute<string>("codeProperty"));

			// Add status filters if necessary
			var filterValue = GetStatusFilterValue(requestType);
			if (!String.IsNullOrEmpty(filterValue))
			{
				var filter = new CREFModel.FilterExpression();

				filter.Items.Add(result);

				var condition = new CREFModel.BinaryExpression();
				condition.Operator = CREFModel.BinaryOperator.opEqual;
				condition.OperatorSpecified = true;

				condition.Items.Add(new CREFModel.PropertyExpression() { Path = "Status" });
				condition.Items.Add(new CREFModel.ValueExpression() { Type = CREFModel.ValueType.String, TypeSpecified = true, Value = filterValue });

				filter.Items.Add(condition);

				return filter;
			}

			return result;
		}

		#endregion

		private string GetStatusFilterValue(ObjectType type)
		{
			switch (type.Name)
			{
				case "BodySite" : break;
				case "DietQualifier" : break;
				case "DoseRestriction" : break;
				case "Documentation" : break;
				case "AdministrableSubstanceSimple" : break;
				case "AdministrableSubstance" : break;
				case "EntitySimple" : break;
				case "Entity" : break;
				case "EvaluatedPerson" : break;
				case "FacilitySimple" : break;
				case "Facility" : break;
				case "OrganizationSimple" : break;
				case "Organization" : break;
				case "PersonSimple" : break;
				case "Person" : break;
				case "SpecimenSimple" : break;
				case "Specimen" : break;
				case "RelatedClinicalStatement" : break;
				case "RelatedEntity" : break;
				case "ClinicalStatementRelationship" : break;
				case "EntityRelationship" : break;
				case "VMR" : break;
				case "AdverseEvent" : return "Active";
				case "DeniedAdverseEvent" : return "Denied";
				case "AppointmentProposal" : break;
				case "AppointmentRequest" : break;
				case "EncounterEvent" : return "Complete";
				case "MissedAppointment" : break;
				case "ScheduledAppointment" : break;
				case "Goal" : break;
				case "GoalProposal" : break;
				case "ObservationOrder" : return "Complete";
				case "ObservationProposal" : return "Ordered";
				case "ObservationResult" : return "Active";
				case "UnconductedObservation" : return "Denied";
				case "DeniedProblem" : return "Denied";
				case "Problem" : return "Active";
				case "DietProposal" : break;
				case "ImagingProposal" : break;
				case "LaboratoryProposal" : break;
				case "ProcedureEvent" : return "Complete";
				case "ProcedureOrder" : return "Ordered";
				case "ProcedureProposal" : return "Ordered";
				case "RespiratoryCareProposal" : break;
				case "ScheduledProcedure" : break;
				case "UndeliveredProcedure" : break;
				case "ComplexIVProposal" : break;
				case "PCAProposal" : break;
				case "SubstanceAdministrationEvent" : return "Active";
				case "SubstanceAdministrationOrder" : return "Complete";
				case "SubstanceAdministrationProposal" : return "Ordered";
				case "SubstanceDispensationEvent" : break;
				case "TubeFeedingProposal" : break;
				case "UndeliveredSubstanceAdministration" : return "Denied";
				case "SupplyEvent" : break;
				case "SupplyOrder" : break;
				case "SupplyProposal" : break;
				case "UndeliveredSupply" : break;
				default : break;
			}

			return null;
		}

		private CREFModel.PatientQueryType GetQueryType(ObjectType type)
		{
			if (type == null)
			{
				throw new ArgumentNullException("type");
			}

			return (CREFModel.PatientQueryType)Enum.Parse(typeof(CREFModel.PatientQueryType), type.Name);
		}

		private void ValidateIdProperty(ObjectType requestType, string idProperty)
		{
		}

		private void ValidateDateProperty(ObjectType requestType, string dateProperty)
		{
		}

		private void ValidateCodeProperty(ObjectType requestType, string codeProperty)
		{
		}
	}

	// ValueSet
	public class ValueSetTranslator : INodeTranslator
	{
		#region INodeTranslator Members

		public object Translate(TranslationContext context, ASTNode node)
		{
			var result = new CREFModel.ValueSetExpression();

			// TODO: Handle Value Set Mapping
			result.ValueSetID = node.GetAttribute<string>("id");

			var version = node.GetAttribute<string>("version");
			if (!String.IsNullOrEmpty(version))
			{
				result.Version = Int32.Parse(version);
				result.VersionSpecified = true;
			}

			// TODO: Value Set Authority

			return result;
		}

		#endregion
	}

	// InValueSet
    public class InValueSetTranslator : INodeTranslator
    {
        #region INodeTranslator Members

        public object Translate(TranslationContext context, ASTNode node)
        {
            // Ideally, this would be expressed as an InValueSet expression within CREF, but in the absence of such an operator, the translation
            // Can be supporting using: Exists(Filter(ValueSet(ValueSetId), Condition(Current = Operand)))
            var valueSetId = node.GetAttribute<string>("id");
            var valueSetVersion = node.GetAttribute<string>("version");
            var valueSetAuthority = node.GetAttribute<string>("authority"); // TODO: Authority resolution?

            var operand = node.Children.FirstOrDefault(c => c.Name == "operand");

            var valueSetExpression = new CREFModel.ValueSetExpression();
            valueSetExpression.ValueSetID = valueSetId;
            if (!String.IsNullOrEmpty(valueSetVersion))
            {
                valueSetExpression.Version = Convert.ToInt32(valueSetVersion);
                valueSetExpression.VersionSpecified = true;
            }

            var filter = new CREFModel.FilterExpression();

            filter.Items.Add(valueSetExpression);
            
            var condition = new CREFModel.BinaryExpression();
            condition.Operator = CREFModel.BinaryOperator.opEqual;
            condition.OperatorSpecified = true;

            // Assumption: A property expression with no path specified is equivalent to a "current" reference
            var property = new CREFModel.PropertyExpression();

            condition.Items.Add(property);
            condition.Items.Add(context.TranslateNode(operand));

            var exists = new CREFModel.UnaryExpression();
            exists.Operator = CREFModel.UnaryOperator.opExists;
            exists.OperatorSpecified = true;
            exists.Item = filter;

            return exists;
        }

        #endregion
    }

	// Subsumes
	// SetSubsumes
}
