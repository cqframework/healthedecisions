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
	public abstract class NodeVerifier : INodeVerifier
	{
		protected virtual void InternalVerify(VerificationContext context, ASTNode node)
		{
			foreach (var child in node.Children)
			{
				Verifier.Verify(context, child);
			}
		}

		#region INodeVerifier Members

		public void Verify(VerificationContext context, ASTNode node)
		{
			InternalVerify(context, node);
		}

		#endregion
	}

	// ExpressionRef
	public class ExpressionRefVerifier : INodeVerifier
	{
		#region INodeVerifier Members

		public void Verify(VerificationContext context, ASTNode node)
		{
			var expressionType = context.ResolveExpressionRef(node.GetAttribute<string>("libraryName"), node.GetAttribute<string>("name"));
			if (expressionType.Expression.ResultType == null)
			{
				throw new InvalidOperationException("Invalid forward reference.");
			}

			node.ResultType = expressionType.Expression.ResultType;
		}

		#endregion
	}

	// ParameterRef
	public class ParameterRefVerifier : INodeVerifier
	{
		#region INodeVerifier Members

		public void Verify(VerificationContext context, ASTNode node)
		{
			var parameterDef = context.ResolveParameterRef(node.GetAttribute<string>("libraryName"), node.GetAttribute<string>("name"));
			node.ResultType = parameterDef.ParameterType;
		}

		#endregion
	}

	// Literal
	public class LiteralVerifier : INodeVerifier
	{
		#region INodeVerifier Members

		public void Verify(VerificationContext context, ASTNode node)
		{
			node.ResultType = context.ResolveType((String)node.Attributes["valueType"]);
		}

		#endregion
	}

	// ComplexLiteral
	public class ComplexLiteralVerifier : INodeVerifier
	{
		#region INodeVerifier Members

		public void Verify(VerificationContext context, ASTNode node)
		{
            var valueNode = ((Node)node).Children.FirstOrDefault(c => c.Name == "value");

            if (valueNode == null)
            {
                throw new InvalidOperationException("Could not resolve value element for complex literal.");
            }

            var resultType = context.ResolveType(valueNode.NodeType);

            node.ResultType = resultType;
		}

		#endregion
	}

	// ObjectExpression
	public class ObjectExpressionVerifier : INodeVerifier
	{
		#region INodeVerifier Members

		public void Verify(VerificationContext context, ASTNode node)
		{
			// objectType
            var objectTypeName = node.GetAttribute<string>("classType");
            if (objectTypeName != null)
            {
			    var objectType = context.ResolveType(node.GetAttribute<string>("classType")) as ObjectType;

			    // foreach property
			    foreach (var child in ((Node)node).Children)
			    {
					if (child.Name == "element") 
					{
						// Verify the property exists
						var childPropertyType = context.ResolveProperty(objectType, child.GetAttribute<string>("name"));

						// Verify the value
						var value = (ASTNode)child.Children[0];
						Verifier.Verify(context, value);

						// Verify the value type is appropriate for the property type
						context.VerifyType(value.ResultType, childPropertyType);
					}
			    }

			    node.ResultType = objectType;
            }
            else
            {
                var propertyList = new List<PropertyDef>();

                foreach (var child in ((Node)node).Children)
                {
					if (child.Name == "element")
					{
						var value = (ASTNode)child.Children[0];
						Verifier.Verify(context, value);

						var property = new PropertyDef(child.GetAttribute<string>("name"), value.ResultType);
						propertyList.Add(property);
					}
                }

                var objectType = new ObjectType(Guid.NewGuid().ToString(), propertyList);

                node.ResultType = objectType;
            }
		}

		#endregion
	}

	// ObjectRedefine
	public class ObjectRedefineVerifier : INodeVerifier
	{
		#region INodeVerifier Members

		public void Verify(VerificationContext context, ASTNode node)
		{
			// Verify source
			Verifier.Verify(context, node.Children[0]);

			var objectType = node.Children[0].ResultType as ObjectType;
			if (objectType == null)
			{
				throw new InvalidOperationException("The source expression for an object redefine must evaluate to a value of a structured type.");
			}

			// Push the source into context as a symbol
			context.PushSymbol(new Symbol(node.GetAttribute<string>("Scope", VerificationContext.Current), objectType));
			try
			{
				// foreach property
				for (int i = 1; i < ((Node)node).Children.Count; i++)
				{
					// Verify the property exists
					var child = ((Node)node).Children[i];
					var childPropertyType = context.ResolveProperty(objectType, child.GetAttribute<string>("name"));

					// Verify the value
					var value = (ASTNode)child.Children[0];
					Verifier.Verify(context, value);

					// Verify the value type is appropriate for the property type
					context.VerifyType(value.ResultType, childPropertyType);
				}
			}
			finally
			{
				context.PopSymbol();
			}

			node.ResultType = objectType;
		}

		#endregion
	}

	// Interval
	public class IntervalVerifier : INodeVerifier
	{
		#region INodeVerifier Members

		public void Verify(VerificationContext context, ASTNode node)
		{
            var begin = node.Children.FirstOrDefault(c => c.Name == "begin" || c.Name == "low");
            if (begin != null)
            {
    			Verifier.Verify(context, begin);
            }

            var end = node.Children.FirstOrDefault(c => c.Name == "end" || c.Name == "high");
            if (end != null)
            {
    			Verifier.Verify(context, end);
            }

            if (begin == null && end == null)
            {
                throw new InvalidOperationException("Interval selector must specify at least one of the beginning or ending points.");
            }

			if (begin != null && end != null && !DataTypes.Equal(begin.ResultType, end.ResultType))
			{
				throw new InvalidOperationException("Beginning and ending expressions for an interval must evaluate to the same type.");
			}

			node.ResultType = new IntervalType(begin == null ? end.ResultType : begin.ResultType);
		}

		#endregion
	}

	// List
	public class ListVerifier : INodeVerifier
	{
		#region INodeVerifier Members

		public void Verify(VerificationContext context, ASTNode node)
		{
			// Type of the first node determines the type of the list
			var child = node.Children.FirstOrDefault();

			if (child != null)
			{
				DataType elementType = null;
	
				Verifier.Verify(context, child);
				elementType = child.ResultType;

				// The type of all subsequent nodes must be the same
				for (int i = 1; i < node.Children.Count; i++)
				{
					Verifier.Verify(context, node.Children[i]);

					if (!DataTypes.Equal(node.Children[i].ResultType, elementType))
					{
						throw new InvalidOperationException("All elements of a list must be of the same type.");
					}
				}

				node.ResultType = new ListType(elementType);
			}
			else
			{
				// If there is no type, the list is List<Any>
				node.ResultType = new ListType(DataTypes.Any);
			}
		}

		#endregion
	}

	public class OperatorNodeVerifier : NodeVerifier
	{
		protected virtual string GetOperatorName(ASTNode node)
		{
			return node.NodeType.GetLocalName();
		}

		protected override void InternalVerify(VerificationContext context, ASTNode node)
		{
			base.InternalVerify(context, node);

			var dataTypes = new List<DataType>();
			foreach (var child in node.Children)
			{
				if (child.ResultType == null)
				{
					throw new InvalidOperationException(String.Format("Could not determine type of '{0}' expression.", child.Name));
				}

				dataTypes.Add(child.ResultType);
			}

			var op = context.ResolveCall(GetOperatorName(node), new Signature(dataTypes));

			node.ResultType = op.ResultType;
		}
	}

	public abstract class NaryNodeVerifier : NodeVerifier
	{
		protected virtual string GetOperatorName(ASTNode node)
		{
			return node.NodeType.GetLocalName();
		}

		protected abstract DataType GetOperandType();

		protected abstract DataType GetResultType();

		protected override void InternalVerify(VerificationContext context, ASTNode node)
		{
			base.InternalVerify(context, node);

			foreach (var child in node.Children)
			{
				context.VerifyType(child.ResultType, GetOperandType());
			}

			node.ResultType = GetResultType();
		}
	}

	public class NaryLogicalNodeVerifier : NaryNodeVerifier
	{
		protected override DataType GetOperandType()
		{
			return DataTypes.Boolean;
		}

		protected override DataType GetResultType()
		{
			return DataTypes.Boolean;
		}
	}

	// And : NaryLogicalNodeVerifier
	// Or : NaryLogicalNodeVerifier
	// Not : OperatorNodeVerifier

	// Conditional
	public class ConditionalVerifier : NodeVerifier
	{
		protected override void InternalVerify(VerificationContext context, ASTNode node)
		{
			base.InternalVerify(context, node);
			var conditionNode = node.Children[0];
			var thenNode = node.Children[1];
			var elseNode = node.Children[2];

			context.VerifyType(conditionNode.ResultType, DataTypes.Boolean);

			var resultType = thenNode.ResultType;

			if (resultType == null)
			{
				throw new InvalidOperationException("Could not determine type of then expression for conditional.");
			}

			context.VerifyType(elseNode.ResultType, resultType);

			node.ResultType = resultType;
		}
	}

	// Case
	public class CaseVerifier : INodeVerifier
	{
		#region INodeVerifier Members

		public void Verify(VerificationContext context, ASTNode node)
		{
			var comparand = node.Children.FirstOrDefault(n => n.Name == "comparand");

			DataType comparisonType = null;
			if (comparand != null)
			{
				Verifier.Verify(context, comparand);
				comparisonType = comparand.ResultType;
			}
			else
			{
				comparisonType = DataTypes.Boolean;
			}

			DataType resultType = null;

			foreach (var caseItem in ((Node)node).Children.Where(n => n.Name == "caseItem"))
			{
				var when = caseItem.Children[0] as ASTNode;
				var then = caseItem.Children[1] as ASTNode;

				Verifier.Verify(context, when);
				context.VerifyType(when.ResultType, comparisonType);

				Verifier.Verify(context, then);
				if (resultType == null)
				{
					resultType = then.ResultType;
					if (resultType == null)
					{
						throw new InvalidOperationException("Could not determine result type for case expression based on then element of first case item.");
					}
				}
				else
				{
					context.VerifyType(then.ResultType, resultType);
				}
			}

			var elseNode = node.Children.FirstOrDefault(n => n.Name == "else") as ASTNode;
			if (elseNode != null)
			{
				Verifier.Verify(context, elseNode);
				context.VerifyType(elseNode.ResultType, resultType);
			}

			node.ResultType = resultType;
		}

		#endregion
	}

	// Null
	public class NullVerifier : INodeVerifier
	{
		#region INodeVerifier Members

		public void Verify(VerificationContext context, ASTNode node)
		{
			node.ResultType = context.ResolveType(node.GetAttribute<string>("valueType"));
		}

		#endregion
	}

	// IsNull
	public class IsNullVerifier : INodeVerifier
	{
		#region INodeVerifier Members

		public void Verify(VerificationContext context, ASTNode node)
		{
			Verifier.Verify(context, node.Children[0]);

			node.ResultType = DataTypes.Boolean;
		}

		#endregion
	}

	// IsTrue
	// IsFalse
	public class IsTrueOrFalseVerifier : INodeVerifier
	{
		#region INodeVerifier Members

		public void Verify(VerificationContext context, ASTNode node)
		{
			Verifier.Verify(context, node.Children[0]);

			context.VerifyType(node.Children[0].ResultType, DataTypes.Boolean);

			node.ResultType = DataTypes.Boolean;
		}

		#endregion
	}

	// IfNull
	public class IfNullVerifier : INodeVerifier
	{
		#region INodeVerifier Members

		public void Verify(VerificationContext context, ASTNode node)
		{
			Verifier.Verify(context, node.Children[0]);
			Verifier.Verify(context, node.Children[1]);

			var comparandType = node.Children[0].ResultType;
			context.VerifyType(node.Children[1].ResultType, comparandType);

			node.ResultType = comparandType;
		}

		#endregion
	}

	// Coalesce
	public class CoalesceVerifier : NodeVerifier
	{
		protected override void InternalVerify(VerificationContext context, ASTNode node)
		{
			base.InternalVerify(context, node);

			DataType operandType = null;

			foreach (var child in node.Children)
			{
				if (operandType == null)
				{
					operandType = child.ResultType;
				}
				else
				{
					context.VerifyType(child.ResultType, operandType);
				}
			}

			node.ResultType = operandType;
		}
	}

    // Is
    public class IsVerifier : NodeVerifier
    {
        protected override void InternalVerify(VerificationContext context, ASTNode node)
        {
            base.InternalVerify(context, node);

            var testType = context.ResolveType(node.GetAttribute<string>("isType"));

            node.ResultType = DataTypes.Boolean;
        }
    }

    // As
    public class AsVerifier : NodeVerifier
    {
        protected override void InternalVerify(VerificationContext context, ASTNode node)
        {
            base.InternalVerify(context, node);

            var targetType = context.ResolveType(node.GetAttribute<string>("asType"));

            node.ResultType = targetType;
        }
    }

    // Convert
    public class ConvertVerifier : NodeVerifier
    {
        protected override void InternalVerify(VerificationContext context, ASTNode node)
        {
            base.InternalVerify(context, node);

            var targetType = context.ResolveType(node.GetAttribute<string>("toType"));

            node.ResultType = targetType;
        }
    }

	// Equal
	public class EqualVerifier : NodeVerifier
	{
		protected virtual string GetOperatorName()
		{
			return "Equal";
		}

		#region INodeVerifier Members

		protected override void InternalVerify(VerificationContext context, ASTNode node)
		{
			base.InternalVerify(context, node);

			var leftType = node.Children[0].ResultType;
			var rightType = node.Children[1].ResultType;

			if (leftType is ScalarType || rightType is ScalarType)
			{
				var op = context.ResolveCall(GetOperatorName(), new Signature(new[] { leftType, rightType }));
				node.ResultType = op.ResultType;
			}
			else
			{
				if (DataTypes.Equal(leftType, rightType))
				{
					node.ResultType = DataTypes.Boolean;
				}
				else
				{
					throw 
						new InvalidOperationException
						(
							String.Format
							(
								"Cannot resolve operator {0} for operands of type {1} and {2}", 
								GetOperatorName(),
								leftType != null ? leftType.Name : "<unknown>", 
								rightType != null ? rightType.Name : "<unknown>"
							)
						);
				}
			}
		}

		#endregion
	}

	// NotEqual
	public class NotEqualVerifier : EqualVerifier
	{
		protected override string GetOperatorName()
		{
			return "NotEqual";
		}
	}

	// Less : OperatorNodeVerifier
	// Greater : OperatorNodeVerifier
	// LessOrEqual : OperatorNodeVerifier
	// GreaterOrEqual : OperatorNodeVerifier

	// Add : OperatorNodeVerifier
	// Subtract : OperatorNodeVerifier
	// Multiply : OperatorNodeVerifier
	// Divide : OperatorNodeVerifier
	// TruncatedDivide : OperatorNodeVerifier
	// Modulo : OperatorNodeVerifier
	// Ceiling : OperatorNodeVerifier
	// Floor : OperatorNodeVerifier
    // Truncate : OperatorNodeVerifier
	// Abs : OperatorNodeVerifier
	// Negate : OperatorNodeVerifier
	// Round : OperatorNodeVerifier
	// Ln : OperatorNodeVerifier
	// Log : OperatorNodeVerifier
	// Power : OperatorNodeVerifier
	// Succ : OperatorNodeVerifier
	// Pred : OperatorNodeVerifier

    // MinValue
    // MaxValue
    public class BoundaryValueVerifier : NodeVerifier
    {
        protected override void InternalVerify(VerificationContext context, ASTNode node)
        {
            base.InternalVerify(context, node);

			node.ResultType = context.ResolveType(node.GetAttribute<String>("valueType"));
        }
    }

	// Concat
	public class ConcatVerifier : NaryNodeVerifier
	{
		protected override DataType GetOperandType()
		{
			return DataTypes.String;
		}

		protected override DataType GetResultType()
		{
			return DataTypes.String;
		}
	}

	// Combine : OperatorNodeVerifier
	// Split : OperatorNodeVerifier
	
	// Length
	public class LengthVerifier : NodeVerifier
	{
		protected override void InternalVerify(VerificationContext context, ASTNode node)
		{
			base.InternalVerify(context, node);

			var child = node.Children[0];
			var intervalType = child.ResultType as IntervalType;
			if (intervalType != null)
			{
				var lengthOperator = context.ResolveCall("Subtract", new Signature(new DataType[] { intervalType.PointType, intervalType.PointType }));
				node.ResultType = lengthOperator.ResultType;
			}
			else
			{
				context.VerifyType(child.ResultType, DataTypes.String);
				node.ResultType = DataTypes.Integer;
			}
		}
	}

	// Upper : OperatorNodeVerifier
	// Lower : OperatorNodeVerifier

	// First
	public class FirstVerifier : NodeVerifier
	{
		protected override void InternalVerify(VerificationContext context, ASTNode node)
		{
			base.InternalVerify(context, node);

			var source = node.Children[0];
			var sourceListType = source.ResultType as ListType;
			if (sourceListType == null)
			{
				throw new InvalidOperationException("Source must be a list value.");
			}

			node.ResultType = sourceListType.ElementType;

            var orderBy = node.GetAttribute<string>("orderBy");
            if (!String.IsNullOrEmpty(orderBy))
            {
                context.ResolveProperty(node.ResultType, orderBy);
            }
		}
	}

	// Last
	public class LastVerifier : NodeVerifier
	{
		protected override void InternalVerify(VerificationContext context, ASTNode node)
		{
			base.InternalVerify(context, node);

			var source = node.Children[0];
			var sourceListType = source.ResultType as ListType;
			if (sourceListType == null)
			{
				throw new InvalidOperationException("Source must be a list value.");
			}

			node.ResultType = sourceListType.ElementType;

            var orderBy = node.GetAttribute<string>("orderBy");
            if (!String.IsNullOrEmpty(orderBy))
            {
                context.ResolveProperty(node.ResultType, orderBy);
            }
		}
	}

	// Indexer
	public class IndexerVerifier : NodeVerifier
	{
		protected override void InternalVerify(VerificationContext context, ASTNode node)
		{
			base.InternalVerify(context, node);

			var source = node.Children[0];
			var index = node.Children[1];

			var sourceListType = source.ResultType as ListType;
			if (sourceListType != null)
			{
				node.ResultType = sourceListType.ElementType;
			}
			else
			{
				context.VerifyType(source.ResultType, DataTypes.String);
				node.ResultType = DataTypes.String;
			}

			context.VerifyType(index.ResultType, DataTypes.Integer);
		}
	}

	// Pos : OperatorNodeVerifier
	// Substring : OperatorNodeVerifier

	// DateAdd : OperatorNodeVerifier
	// DateDiff : OperatorNodeVerifier
	// DatePart : OperatorNodeVerifier
	// Today : OperatorNodeVerifier
	// Now : OperatorNodeVerifier
	// Date : OperatorNodeVerifier

	// Contains
	public class ContainsVerifier : NodeVerifier
	{
		protected override void InternalVerify(VerificationContext context, ASTNode node)
		{
			base.InternalVerify(context, node);

			var source = node.Children[0];
			var element = node.Children[1];

			var sourceListType = source.ResultType as ListType;
			if (sourceListType != null)
			{
				context.VerifyType(element.ResultType, sourceListType.ElementType);

				node.ResultType = DataTypes.Boolean;
			}

			var sourceIntervalType = source.ResultType as IntervalType;
			if (sourceIntervalType != null)
			{
				context.VerifyType(element.ResultType, sourceIntervalType.PointType);

				node.ResultType = DataTypes.Boolean;
			}

			if (node.ResultType == null)
			{
				throw new InvalidOperationException("Expected an argument of type list or interval.");
			}
		}
	}

	public class BinaryListOrIntervalVerifier : NodeVerifier
	{
		protected override void InternalVerify(VerificationContext context, ASTNode node)
		{
			base.InternalVerify(context, node);

			var leftNode = node.Children[0];
			var rightNode = node.Children[1];

			if (leftNode.ResultType is ListType || leftNode.ResultType is IntervalType)
			{
				context.VerifyType(rightNode.ResultType, leftNode.ResultType);
				node.ResultType = DataTypes.Boolean;
			}
			else
			{
				throw new InvalidOperationException("List or interval type expected.");
			}
		}
	}

    // Includes : BinaryListOrIntervalVerifier
    // IncludedIn : BinaryListOrIntervalVerifier
    // ProperIncludes : BinaryListOrIntervalVerifier
    // ProperIncludedIn : BinaryListOrIntervalVerifier

	public class BinaryIntervalVerifier : NodeVerifier
	{
		protected override void InternalVerify(VerificationContext context, ASTNode node)
		{
			base.InternalVerify(context, node);

			var left = node.Children[0];
			var right = node.Children[1];

			var leftIntervalType = left.ResultType as IntervalType;
			if (leftIntervalType != null)
			{
				context.VerifyType(right.ResultType, leftIntervalType);
				node.ResultType = DataTypes.Boolean;
			}
			else
			{
				throw new InvalidOperationException("List type expected.");
			}
		}
	}

	// Before : BinaryIntervalVerifier
	// After : BinaryIntervalVerifier
	// Meets : BinaryIntervalVerifier
	// Overlaps : BinaryIntervalVerifier

	public class NaryListOrIntervalVerifier : NodeVerifier
	{
		protected override void InternalVerify(VerificationContext context, ASTNode node)
		{
			base.InternalVerify(context, node);

			if (node.Children.Count == 0)
			{
				throw new InvalidOperationException("Expected at least one child argument.");
			}

			DataType operandType = null;
			foreach (var child in node.Children)
			{
				if (operandType == null)
				{
					operandType = child.ResultType;

					if (!(operandType is ListType || operandType is IntervalType))
					{
						throw new InvalidOperationException("List or interval type expected.");
					}
				}
				else
				{
					context.VerifyType(child.ResultType, operandType);
				}
			}

			node.ResultType = operandType;
		}
	}

	// Union : NaryListOrIntervalVerifier
	// Intersect : NaryListOrIntervalVerifier
	// Difference : NaryListOrIntervalVerifier

	public class UnaryIntervalVerifier : NodeVerifier
	{
		protected override void InternalVerify(VerificationContext context, ASTNode node)
		{
			base.InternalVerify(context, node);

			var source = node.Children[0];
			var sourceIntervalType = source.ResultType as IntervalType;
			if (sourceIntervalType == null)
			{
				throw new InvalidOperationException("Interval type expected.");
			}

			node.ResultType = sourceIntervalType.PointType;
		}
	}

	// Begin : UnaryIntervalVerifier
	// End : UnaryIntervalVerifier

	public abstract class UnaryListVerifier : NodeVerifier
	{
		protected abstract DataType GetResultType(ASTNode node);

		protected override void InternalVerify(VerificationContext context, ASTNode node)
		{
			base.InternalVerify(context, node);

			var child = node.Children[0];
			var childListType = child.ResultType as ListType;
			if (childListType == null)
			{
				throw new InvalidOperationException("Expected an expression of a list type.");
			}

			node.ResultType = GetResultType(child);
		}
	}

    // Collapse
    public class CollapseVerifier : UnaryListVerifier
    {
        protected override DataType GetResultType(ASTNode node)
        {
            var listType = node.ResultType as ListType;
            if (!(listType.ElementType is IntervalType))
            {
                throw new InvalidOperationException("Collapse operator must be invoked on a list of intervals.");
            }

            return listType;
        }
    }

    // Expand
    public class ExpandVerifier : UnaryListVerifier
    {
        protected override DataType GetResultType(ASTNode node)
        {
            var listType = node.ResultType as ListType;
            var childListType = listType.ElementType as ListType;
            if (childListType == null)
            {
                throw new InvalidOperationException("Expand operator must be invoked on a list of lists.");
            }

            return childListType;
        }
    }

	// IsEmpty
	public class IsEmptyVerifier : UnaryListVerifier
	{
		protected override DataType GetResultType(ASTNode node)
		{
			return DataTypes.Boolean;
		}
	}

	// IsNotEmpty
	public class IsNotEmptyVerifier : UnaryListVerifier
	{
		protected override DataType GetResultType(ASTNode node)
		{
			return DataTypes.Boolean;
		}
	}

	// Filter
	public class FilterVerifier : INodeVerifier
	{
		#region INodeVerifier Members

		public void Verify(VerificationContext context, ASTNode node)
		{
			var source = node.Children[0];
			var condition = node.Children[1];

			Verifier.Verify(context, source);
			var sourceListType = source.ResultType as ListType;
			if (sourceListType == null)
			{
				throw new InvalidOperationException("Filter expression source must be a list type expression.");
			}

			context.PushSymbol(new Symbol(node.GetAttribute<string>("scope", VerificationContext.Current), sourceListType.ElementType));
			try
			{
				Verifier.Verify(context, condition);
				context.VerifyType(condition.ResultType, DataTypes.Boolean);
			}
			finally
			{
				context.PopSymbol();
			}

			node.ResultType = sourceListType;
		}

		#endregion
	}

	// IndexOf
	public class IndexOfVerifier : NodeVerifier
	{
		protected override void InternalVerify(VerificationContext context, ASTNode node)
		{
			base.InternalVerify(context, node);

			var source = node.Children[0];
			var element = node.Children[1];

			var sourceListType = source.ResultType as ListType;
			if (sourceListType == null)
			{
				throw new InvalidOperationException("List type expected.");
			}

			context.VerifyType(element.ResultType, sourceListType.ElementType);
			node.ResultType = DataTypes.Integer;
		}
	}

	// In
	public class InVerifier : NodeVerifier
	{
		protected override void InternalVerify(VerificationContext context, ASTNode node)
		{
			base.InternalVerify(context, node);

			var element = node.Children[0];
			var source = node.Children[1];

			var sourceListType = source.ResultType as ListType;
			if (sourceListType != null)
			{
				context.VerifyType(element.ResultType, sourceListType.ElementType);

				node.ResultType = DataTypes.Boolean;
			}

			var sourceIntervalType = source.ResultType as IntervalType;
			if (sourceIntervalType != null)
			{
				context.VerifyType(element.ResultType, sourceIntervalType.PointType);

				node.ResultType = DataTypes.Boolean;
			}

			if (node.ResultType == null)
			{
				throw new InvalidOperationException("Expected an argument of type list or interval.");
			}
		}
	}

	// Sort
	public class SortVerifier : UnaryListVerifier
	{
		protected override DataType GetResultType(ASTNode node)
		{
			var sourceListType = node.ResultType as ListType;

			return sourceListType;
		}
	}

	// ForEach
	public class ForEachVerifier : INodeVerifier
	{
		#region INodeVerifier Members

		public void Verify(VerificationContext context, ASTNode node)
		{
			var source = node.Children[0];
			var condition = node.Children[1];

			Verifier.Verify(context, source);
			var sourceListType = source.ResultType as ListType;
			if (sourceListType == null)
			{
				throw new InvalidOperationException("ForEach expression source must be a list type expression.");
			}

			context.PushSymbol(new Symbol(node.GetAttribute<string>("sopce", VerificationContext.Current), sourceListType.ElementType));
			try
			{
				Verifier.Verify(context, condition);
				node.ResultType = new ListType(condition.ResultType);
			}
			finally
			{
				context.PopSymbol();
			}
		}

		#endregion
	}

	// Distinct
	public class DistinctVerifier : UnaryListVerifier
	{
		protected override DataType GetResultType(ASTNode node)
		{
			var sourceListType = node.ResultType as ListType;

			return sourceListType;
		}
	}

	// Current
	public class CurrentVerifier : INodeVerifier
	{
		#region INodeVerifier Members

		public void Verify(VerificationContext context, ASTNode node)
		{
			var symbol = context.ResolveSymbol(node.GetAttribute<string>("scope", VerificationContext.Current));
			node.ResultType = symbol.DataType;
		}

		#endregion
	}

	public abstract class AggregateNodeVerifier : NodeVerifier
	{
		protected abstract DataType GetResultType(VerificationContext context, ListType listType);

		protected override void InternalVerify(VerificationContext context, ASTNode node)
		{
			base.InternalVerify(context, node);

			var source = node.Children[0];
			var sourceListType = source.ResultType as ListType;
			if (sourceListType == null)
			{
				throw new InvalidOperationException("List type expected.");
			}

			var path = node.GetAttribute<string>("path");
			if (!String.IsNullOrEmpty(path))
			{
				var sourceObjectType = sourceListType.ElementType as ObjectType;
				if (sourceObjectType == null)
				{
					throw new InvalidOperationException("List of object type expected for aggregate expression with path reference.");
				}

				context.ResolveProperty(sourceObjectType, path);
			}

			node.ResultType = GetResultType(context, sourceListType);
		}
	}

	// Count
	public class CountVerifier : AggregateNodeVerifier
	{
		protected override DataType GetResultType(VerificationContext context, ListType listType)
		{
			return DataTypes.Integer;
		}
	}

	// Sum
	public class SumVerifier : AggregateNodeVerifier
	{
		protected override DataType GetResultType(VerificationContext context, ListType listType)
		{
			var addOperator = context.ResolveCall("Add", new Signature(new[] { listType.ElementType, listType.ElementType }));
			return addOperator.ResultType;
		}
	}

	// Min
	public class MinVerifier : AggregateNodeVerifier
	{
		protected override DataType GetResultType(VerificationContext context, ListType listType)
		{
			var lessOperator = context.ResolveCall("Less", new Signature(new[] { listType.ElementType, listType.ElementType }));
			return listType.ElementType;
		}
	}

	// Max
	public class MaxVerifier : AggregateNodeVerifier
	{
		protected override DataType GetResultType(VerificationContext context, ListType listType)
		{
			var greaterOperator = context.ResolveCall("Greater", new Signature(new[] { listType.ElementType, listType.ElementType }));
			return listType.ElementType;
		}
	}

	// Avg
	public class AvgVerifier : AggregateNodeVerifier
	{
		protected override DataType GetResultType(VerificationContext context, ListType listType)
		{
			var addOperator = context.ResolveCall("Add", new Signature(new[] { listType.ElementType, listType.ElementType }));
			var divideOperator = context.ResolveCall("Divide", new Signature(new[] { listType.ElementType, listType.ElementType }));
			return divideOperator.ResultType;
		}
	}

    // Median
    public class MedianVerifier : AggregateNodeVerifier
    {
        protected override DataType GetResultType(VerificationContext context, ListType listType)
        {
			var addOperator = context.ResolveCall("Add", new Signature(new[] { listType.ElementType, listType.ElementType }));
			var divideOperator = context.ResolveCall("Divide", new Signature(new[] { listType.ElementType, listType.ElementType }));
			return divideOperator.ResultType;
        }
    }

    // Mode
    public class ModeVerifier : AggregateNodeVerifier
    {
        protected override DataType GetResultType(VerificationContext context, ListType listType)
        {
            return listType.ElementType;
        }
    }

    // Variance
    // PopulationVariance
    public class VarianceVerifier : AggregateNodeVerifier
    {
        protected override DataType GetResultType(VerificationContext context, ListType listType)
        {
			var addOperator = context.ResolveCall("Add", new Signature(new[] { listType.ElementType, listType.ElementType }));
			var divideOperator = context.ResolveCall("Divide", new Signature(new[] { listType.ElementType, listType.ElementType }));
			return divideOperator.ResultType;
        }
    }

    // StdDev
    // PopulationStdDev
    public class StdDevVerifier : AggregateNodeVerifier
    {
        protected override DataType GetResultType(VerificationContext context, ListType listType)
        {
			var addOperator = context.ResolveCall("Add", new Signature(new[] { listType.ElementType, listType.ElementType }));
			var divideOperator = context.ResolveCall("Divide", new Signature(new[] { listType.ElementType, listType.ElementType }));
			return divideOperator.ResultType;
        }
    }

	public class LogicalAggregateVerifier : AggregateNodeVerifier
	{
		protected override DataType GetResultType(VerificationContext context, ListType listType)
		{
			context.VerifyType(listType.ElementType, DataTypes.Boolean);
			return DataTypes.Boolean;
		}
	}

	// AllTrue : LogicalAggregateVerifier
	// AnyTrue : LogicalAggregateVerifier
	
	// Property
	public class PropertyVerifier : INodeVerifier
	{
		#region INodeVerifier Members

		public void Verify(VerificationContext context, ASTNode node)
		{
			DataType sourceType = null;
			if (node.Children.Count > 0)
			{
				Verifier.Verify(context, node.Children[0]);
				sourceType = node.Children[0].ResultType;
			}
			else
			{
				var symbol = context.ResolveSymbol(node.GetAttribute<string>("scope", VerificationContext.Current));
				sourceType = symbol.DataType;
			}

			node.ResultType = context.ResolveProperty(sourceType, node.GetAttribute<string>("path"));

			// Save sourceType for use in translation and/or evaluation
			node.Attributes.Add("sourceType", sourceType);
		}

		#endregion
	}

	// AddressLiteral
	public class AddressLiteralVerifier : INodeVerifier
	{
		#region INodeVerifier Members

		public void Verify(VerificationContext context, ASTNode node)
		{
			node.ResultType = DataTypes.ResolveType(typeof(AD));
		}

		#endregion
	}

	// BooleanLiteral
	public class BooleanLiteralVerifier : INodeVerifier
	{
		#region INodeVerifier Members

		public void Verify(VerificationContext context, ASTNode node)
		{
			node.ResultType = DataTypes.Boolean;
		}

		#endregion
	}
	
	// CodeLiteral
	public class CodeLiteralVerifier : INodeVerifier
	{
		#region INodeVerifier Members

		public void Verify(VerificationContext context, ASTNode node)
		{
			node.ResultType = DataTypes.Code;

			// TODO: Validate code system reference
		}

		#endregion
	}

	// ConceptLiteral
	public class ConceptLiteralVerifier : INodeVerifier
	{
		#region INodeVerifier Members

		public void Verify(VerificationContext context, ASTNode node)
		{
			node.ResultType = DataTypes.Concept;

			// TODO: Validate contained codes
		}

		#endregion
	}

	// CodedOrdinalLiteral
	public class CodedOrdinalLiteralVerifier : INodeVerifier
	{
		#region INodeVerifier Members

		public void Verify(VerificationContext context, ASTNode node)
		{
			node.ResultType = DataTypes.ResolveType(typeof(CO));
		}

		#endregion
	}

	// SimpleCodeLiteral
	public class SimpleCodeLiteralVerifier : INodeVerifier
	{
		#region INodeVerifier Members

		public void Verify(VerificationContext context, ASTNode node)
		{
			node.ResultType = DataTypes.ResolveType(typeof(CS));
		}

		#endregion
	}

	// EntityNameLiteral
	public class EntityNameLiteralVerifier : INodeVerifier
	{
		#region INodeVerifier Members

		public void Verify(VerificationContext context, ASTNode node)
		{
			node.ResultType = DataTypes.ResolveType(typeof(EN));
		}

		#endregion
	}

	// IdentifierLiteral
	public class IdentifierLiteralVerifier : INodeVerifier
	{
		#region INodeVerifier Members

		public void Verify(VerificationContext context, ASTNode node)
		{
			node.ResultType = DataTypes.ResolveType(typeof(II));
		}

		#endregion
	}

	// IntegerLiteral
	public class IntegerLiteralVerifier : INodeVerifier
	{
		#region INodeVerifier Members

		public void Verify(VerificationContext context, ASTNode node)
		{
			node.ResultType = DataTypes.Integer;
		}

		#endregion
	}

	// IntegerIntervalLiteral
	public class IntegerIntervalLiteralVerifier : INodeVerifier
	{
		#region INodeVerifier Members

		public void Verify(VerificationContext context, ASTNode node)
		{
			node.ResultType = DataTypes.ResolveType(typeof(IVL_INT));
		}

		#endregion
	}

	// PhysicalQuantityIntervalLiteral
	public class PhysicalQuantityIntervalLiteralVerifier : INodeVerifier
	{
		#region INodeVerifier Members

		public void Verify(VerificationContext context, ASTNode node)
		{
			node.ResultType = DataTypes.ResolveType(typeof(IVL_PQ));
		}

		#endregion
	}

    // QuantityIntervalLiteral
    public class QuantityIntervalLiteralVerifier : INodeVerifier
    {
        #region INodeVerifier Members

        public void Verify(VerificationContext context, ASTNode node)
        {
            node.ResultType = DataTypes.QuantityInterval;
        }

        #endregion
    }

    //// IntegerRatioIntervalLiteral
    //public class IntegerRatioIntervalLiteralVerifier : INodeVerifier
    //{
    //    #region INodeVerifier Members

    //    public void Verify(VerificationContext context, ASTNode node)
    //    {
    //        node.ResultType = DataTypes.ResolveType(typeof(IVL_RTO_INT));
    //    }

    //    #endregion
    //}

    //// PhysicalQuantityRatioIntervalLiteral
    //public class PhysicalQuantityRatioIntervalLiteralVerifier : INodeVerifier
    //{
    //    #region INodeVerifier Members

    //    public void Verify(VerificationContext context, ASTNode node)
    //    {
    //        node.ResultType = DataTypes.ResolveType(typeof(IVL_RTO_PQ));
    //    }

    //    #endregion
    //}

	// RealIntervalLiteral
	public class RealIntervalLiteralVerifier : INodeVerifier
	{
		#region INodeVerifier Members

		public void Verify(VerificationContext context, ASTNode node)
		{
			node.ResultType = DataTypes.ResolveType(typeof(IVL_REAL));
		}

		#endregion
	}

	// TimestampIntervalLiteral
	public class TimestampIntervalLiteralVerifier : INodeVerifier
	{
		#region INodeVerifier Members

		public void Verify(VerificationContext context, ASTNode node)
		{
			node.ResultType = DataTypes.ResolveType(typeof(IVL_TS));
		}

		#endregion
	}

	// PhysicalQuantityLiteral
	public class PhysicalQuantityLiteralVerifier : INodeVerifier
	{
		#region INodeVerifier Members

		public void Verify(VerificationContext context, ASTNode node)
		{
			node.ResultType = DataTypes.ResolveType(typeof(PQ));
		}

		#endregion
	}

	// RealLiteral
	public class RealLiteralVerifier : INodeVerifier
	{
		#region INodeVerifier Members

		public void Verify(VerificationContext context, ASTNode node)
		{
			node.ResultType = DataTypes.Decimal;
		}

		#endregion
	}

    // RatioLiteral
    public class RatioLiteralVerifier : NodeVerifier
    {
        protected override void InternalVerify(VerificationContext context, ASTNode node)
        {
            base.InternalVerify(context, node);

            var numerator = ((Node)node).Children[0];
            var denominator = ((Node)node).Children[1];

            var numeratorType = context.ResolveType(numerator.NodeType);
            var denominatorType = context.ResolveType(denominator.NodeType);

            context.VerifyType(numeratorType, DataTypes.Quantity);
            context.VerifyType(denominatorType, DataTypes.Quantity);

            node.ResultType = DataTypes.ResolveType(typeof(RTO));
        }
    }

    //// IntegerRatioLiteral
    //public class IntegerRatioLiteralVerifier : INodeVerifier
    //{
    //    #region INodeVerifier Members

    //    public void Verify(VerificationContext context, ASTNode node)
    //    {
    //        node.ResultType = DataTypes.ResolveType(typeof(IVL_RTO_INT));
    //    }

    //    #endregion
    //}

    //// PhysicalQuantityRatioLiteral
    //public class PhysicalQuantityRatioLiteralVerifier : INodeVerifier
    //{
    //    #region INodeVerifier Members

    //    public void Verify(VerificationContext context, ASTNode node)
    //    {
    //        node.ResultType = DataTypes.ResolveType(typeof(IVL_RTO_PQ));
    //    }

    //    #endregion
    //}

	// StringLiteral
	public class StringLiteralVerifier : INodeVerifier
	{
		#region INodeVerifier Members

		public void Verify(VerificationContext context, ASTNode node)
		{
			node.ResultType = DataTypes.String;
		}

		#endregion
	}

	// UrlLiteral
	public class UrlLiteralVerifier : INodeVerifier
	{
		#region INodeVerifier Members

		public void Verify(VerificationContext context, ASTNode node)
		{
			node.ResultType = DataTypes.ResolveType(typeof(TEL));
		}

		#endregion
	}

	// TimestampLiteral
	public class TimestampLiteralVerifier : INodeVerifier
	{
		#region INodeVerifier Members

		public void Verify(VerificationContext context, ASTNode node)
		{
			node.ResultType = DataTypes.ResolveType(typeof(TS));
		}

		#endregion
	}

	// PeriodLiteral
	public class PeriodLiteralVerifier : INodeVerifier
	{
		#region INodeVerifier Members

		public void Verify(VerificationContext context, ASTNode node)
		{
			node.ResultType = DataTypes.ResolveType(typeof(PIVL_TS));
		}

		#endregion
	}

	public class DataRequestVerifier : INodeVerifier
	{
		protected virtual void InternalVerify(VerificationContext context, ASTNode node, ObjectType dataType)
		{
			// idProperty - If present, must reference a property of type String (or explicitly convertible to string) on the resolved model type.
			var idProperty = node.GetAttribute<string>("idProperty");
			if (!String.IsNullOrEmpty(idProperty))
			{
				var idPropertyType = context.ResolveProperty(dataType, idProperty);
				if (!(DataTypes.Equivalent(idPropertyType, DataTypes.ResolveType(typeof(II))) || DataTypes.Equal(idPropertyType, DataTypes.String)))
				{
					throw new InvalidOperationException("Id property must be either an Identifier or a String.");
				}
			}

			// Validate children
			// timeOffset - If present, must evaluate to a value of type PIVL_TS
			var timeOffset = node.Children.Where(n => n.Name == "timeOffset").FirstOrDefault();
			if (timeOffset != null)
			{
				Verifier.Verify(context, timeOffset);
				context.VerifyType(timeOffset.ResultType, DataTypes.ResolveType(typeof(PIVL_TS)));
			}
		}

		#region INodeVerifier Members

		public void Verify(VerificationContext context, ASTNode node)
		{
			// Determine model type and validate relevant attributes
			// dataType - Must reference a valid type in a known model in the context
			var dataType = context.ResolveType(node.GetAttribute<string>("dataType")) as ObjectType;

			InternalVerify(context, node, dataType);
			
			// cardinality - If single, the result is an object type with all the properties of the type in question.
			var cardinality = (RequestCardinality)Enum.Parse(typeof(RequestCardinality), node.GetAttribute<string>("cardinality", "Multiple"), true);

			// If multiple, the result is a list type of that type.
			if (cardinality == RequestCardinality.Single)
			{
				node.ResultType = dataType;
			}
			else
			{
				node.ResultType = new ListType(dataType);
			}
		}

		#endregion
	}

	// ClinicalRequest
	public class ClinicalRequestVerifier : DataRequestVerifier
	{
		protected override void InternalVerify(VerificationContext context, ASTNode node, ObjectType dataType)
		{
			base.InternalVerify(context, node, dataType);

			// codeProperty - If present, must reference a property of type Code on the resolved model type.
			var codeProperty = node.GetAttribute<string>("codeProperty");
			if (!String.IsNullOrEmpty(codeProperty))
			{
				var codePropertyType = context.ResolveProperty(dataType, codeProperty);
				context.VerifyType(codePropertyType, DataTypes.Code);
			}

			// dateProperty - If present, must reference a property of type Timestamp on the resolved model type. (TODO: Handle Interval<Timestamp>?)
			var dateProperty = node.GetAttribute<string>("dateProperty");
			if (!String.IsNullOrEmpty(dateProperty))
			{
				var datePropertyType = context.ResolveProperty(dataType, dateProperty);
				context.VerifyType(datePropertyType, DataTypes.DateTime);
			}

			// codes - If present, must evaluate to a value of type List<Code>
			var codes = node.Children.Where(n => n.Name == "codes").FirstOrDefault();
			if (codes != null)
			{
				Verifier.Verify(context, codes);
				context.VerifyType(codes.ResultType, DataTypes.CodeList);
			}

			// dateRange - If present, must evaluate to a value of type Interval<Timestamp>
			var dateRange = node.Children.Where(n => n.Name == "dateRange").FirstOrDefault();
			if (dateRange != null)
			{
				Verifier.Verify(context, dateRange);
				context.VerifyType(dateRange.ResultType, DataTypes.DateTimeInterval);
			}
		}
	}

	// ValueSet
	public class ValueSetVerifier : INodeVerifier
	{
		#region INodeVerifier Members

		public void Verify(VerificationContext context, ASTNode node)
		{
			node.ResultType = new ListType(DataTypes.Code);
		}

		#endregion
	}

	// InValueSet : OperatorNodeVerifier
	// Subsumes : OperatorNodeVerifier
	// SetSubsumes : OperatorNodeVerifier
}
