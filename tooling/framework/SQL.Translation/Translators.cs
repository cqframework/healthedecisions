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

using Model = Alphora.Dataphor.DAE.Language;
using SQLModel = Alphora.Dataphor.DAE.Language.SQL;
using TSQLModel = Alphora.Dataphor.DAE.Language.TSQL;

namespace SQL.Translation
{
	// ExpressionRef
	public class ExpressionRefTranslator : INodeTranslator
	{
		#region INodeTranslator Members

		public object Translate(TranslationContext context, ASTNode node)
		{
			var result = new SQLModel.TableExpression();
			var sqlContext = (SQLTranslationContext)context;
			result.TableName = sqlContext.GetExpressionObjectName(String.Format("{0}.{1}", node.GetAttribute<string>("libraryName", sqlContext.ArtifactName), node.GetAttribute<string>("name")));

			// If the expression being referenced is scalar, it will be automatically promoted to a query by the expression def translation
			// In this case, it must be demoted back to a scalar expression with a subquery access.
			if (!(node.ResultType is ListType) && !(node.ResultType is ObjectType))
			{
				var selectExpression = new SQLModel.SelectExpression();
				selectExpression.SelectClause = new SQLModel.SelectClause();
				selectExpression.SelectClause.Columns.Add(new SQLModel.ColumnExpression(new SQLModel.QualifiedFieldExpression("value")));
				selectExpression.FromClause = new SQLModel.CalculusFromClause(new SQLModel.TableSpecifier(result));

				// If the result type is also boolean, the expression will be converted to a 1 or 0, so it must be demoted back to an actual boolean-valued expression
				if (DataTypes.Equal(node.ResultType, DataTypes.Boolean))
				{
					return SQLTranslationUtilities.DemoteBooleanValuedExpression(selectExpression);
				}

				return selectExpression;
			}

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
			var result = new SQLModel.TableExpression();

			// TODO: Handle Cardinality
			// TODO: Handle whether we are in a FromClause context.
			var sqlContext = (SQLTranslationContext)context;
			result.TableName = sqlContext.GetExpressionObjectName(String.Format("{0}.{1}", node.GetAttribute<string>("libraryName", sqlContext.ArtifactName), node.GetAttribute<string>("name")));

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
			if (DataTypes.Equal(node.ResultType, DataTypes.Boolean))
			{
				return new Model.BinaryExpression(new Model.ValueExpression(1), "iEqual", new Model.ValueExpression(Boolean.Parse(node.GetAttribute<string>("value")) ? 1 : 0));
			}
			else if (DataTypes.Equal(node.ResultType, DataTypes.DateTime))
			{
				// TODO: Convert to format expected by T-SQL
				return new Model.CallExpression("convert", new Model.Expression[] { new Model.IdentifierExpression("datetime"), new Model.ValueExpression(node.GetAttribute<string>("value")) }); 
			}
			else
			{
				var result = new Model.ValueExpression();

				if (DataTypes.Equal(node.ResultType, DataTypes.String))
				{
					result.Token = Model.TokenType.String;
					result.Value = node.GetAttribute<string>("value");
				}
				else if (DataTypes.Equal(node.ResultType, DataTypes.Integer))
				{
					result.Token = Model.TokenType.Integer;
					result.Value = Int32.Parse(node.GetAttribute<string>("value"));
				}
				else if (DataTypes.Equal(node.ResultType, DataTypes.Decimal))
				{
					result.Token = Model.TokenType.Decimal;
					result.Value = Decimal.Parse(node.GetAttribute<string>("value"));
				}
				else
				{
					throw new NotSupportedException(String.Format("Unsupported literal type: {0}.", node.ResultType.Name));
				}

				return result;
			}
		}

		#endregion
	}

	// ComplexLiteral
	// PropertyExpression
	// ObjectExpression
	// ObjectRedefine

	// Tuple

	// Interval
	//public class IntervalTranslator : INodeTranslator
	//{
	//	#region INodeTranslator Members

	//	public object Translate(TranslationContext context, ASTNode node)
	//	{
	//		if (DataTypes.Equal(node.ResultType, DataTypes.DateTimeInterval))
	//		{
	//			var result = new SQLModel.DateRange();

	//			var beginOpen = Convert.ToBoolean(node.GetAttribute<string>("beginOpen"));
	//			var endOpen = Convert.ToBoolean(node.GetAttribute<string>("endOpen"));
	//			if (beginOpen || endOpen)
	//			{
	//				throw new NotSupportedException("Translation for open intervals is not supported because CREF only supports closed interval values.");
	//			}

	//			foreach (var child in node.Children)
	//			{
	//				result.Items.Add(context.TranslateNode(child));
	//			}

	//			return result;
	//		}
	//		else
	//		{
	//			throw new NotSupportedException("Translation for intervals with point types other than Timestamp is not supported because CREF does not support generic interval values.");
	//		}
	//	}

	//	#endregion
	//}

	// List
	public class ListTranslator : INodeTranslator
	{
		#region INodeTranslator Members

		public object Translate(TranslationContext context, ASTNode node)
		{
			var result = new Model.ListExpression();

			foreach (var child in node.Children)
			{
				result.Expressions.Add(context.TranslateNode(child));
			}

			return result;
		}

		#endregion
	}

    //Base classes for operators
	public abstract class NaryLogicalNodeTranslator : INodeTranslator
	{
		protected abstract string GetOperator();
	
		#region INodeTranslator Members

		public object Translate(TranslationContext context, ASTNode node)
		{
			// Use LogicalConnective
			Model.Expression result = null;

			foreach (var child in node.Children)
			{
				var expression = (Model.Expression)context.TranslateNode(child);
				if (result == null)
				{
					result = expression;
				}
				else
				{
					var binaryExpression = new Model.BinaryExpression();
					binaryExpression.Instruction = GetOperator();
					binaryExpression.LeftExpression = result;
					binaryExpression.RightExpression = expression;
					result = binaryExpression;
				}
			}

			return result;
		}

		#endregion
	}

    public abstract class BinaryNodeTranslator : INodeTranslator
    {
        protected abstract string GetOperator();

        #region INodeTranslator Members

        public object Translate(TranslationContext context, ASTNode node)
        {
            var result = new Model.BinaryExpression();
			result.Instruction = GetOperator();

			result.LeftExpression = (Model.Expression)context.TranslateNode(node.Children[0]);
			result.RightExpression = (Model.Expression)context.TranslateNode(node.Children[1]);

            return result;
        }

        #endregion
    }

    public abstract class UnaryNodeTranslator : INodeTranslator
    {
        protected abstract string GetOperator();

        #region INodeTranslator Members

        public object Translate(TranslationContext context, ASTNode node)
        {
			var result = new Model.UnaryExpression();
			result.Instruction = GetOperator();

			result.Expression = (Model.Expression)context.TranslateNode(node.Children[0]);

			return result;
        }

        #endregion
    }

    // And
    public class AndTranslator : NaryLogicalNodeTranslator
    {
        protected override String GetOperator()
        {
			return "iAnd";
        }
    }

    // Or
    public class OrTranslator : NaryLogicalNodeTranslator
    {
        protected override String GetOperator()
        {
			return "iOr";
        }
    }

	// Xor

	// Not
    public class NotTranslator : UnaryNodeTranslator
    {
        protected override String GetOperator()
        {
            return "iNot";
        }
    }

	// If
	// Conditional
    public class ConditionalTranslator : INodeTranslator
    {
        #region INodeTranslator Members

        public object Translate(TranslationContext context, ASTNode node)
        {
			var result = new Model.CaseExpression();
			var resultItem = new Model.CaseItemExpression();
			resultItem.WhenExpression = (Model.Expression)context.TranslateNode(node.Children[0]);
			resultItem.ThenExpression = (Model.Expression)context.TranslateNode(node.Children[1]);
			result.CaseItems.Add(resultItem);
			result.ElseExpression = (Model.Expression)context.TranslateNode(node.Children[2]);

            return result;
        }

        #endregion
    }

    // Case
	//public class CaseTranslator : INodeTranslator
	//{
	//	#region INodeTranslator Members

	//	public object Translate(TranslationContext context, ASTNode node)
	//	{
	//		var result = new SQLModel.Choice();

	//		var comparand = node.Children.FirstOrDefault(n => n.Name == "comparand");

	//		foreach (var caseItem in ((Node)node).Children.Where(n => n.Name == "caseItem"))
	//		{
	//			var whenNode = caseItem.Children[0] as ASTNode;
	//			var thenNode = caseItem.Children[1] as ASTNode;

	//			var condition = new SQLModel.Condition();
	//			if (comparand == null)
	//			{
	//				condition.Items.Add(context.TranslateNode(whenNode));
	//			}
	//			else
	//			{
	//				var equal = new SQLModel.BinaryExpression();
	//				equal.Operator = SQLModel.BinaryOperator.opEqual;
	//				equal.OperatorSpecified = true;
	//				equal.Items.Add(context.TranslateNode(comparand));
	//				equal.Items.Add(context.TranslateNode(whenNode));
	//				condition.Items.Add(equal);
	//			}

	//			condition.Items.Add(context.TranslateNode(thenNode));

	//			result.Items.Add(condition);
	//		}

	//		var elseNode = node.Children.FirstOrDefault(n => n.Name == "else") as ASTNode;

	//		result.Items.Add(context.TranslateNode(elseNode));

	//		return result;
	//	}

	//	#endregion
	//}

	// Null
    public class NullTranslator : INodeTranslator
    {
        #region INodeTranslator Members

        public object Translate(TranslationContext context, ASTNode node)
        {
            var result = new Model.ValueExpression();
			result.Token = Model.TokenType.Nil;

            return result;
        }

        #endregion
    }

	// IsNull
	public class IsNullTranslator : INodeTranslator
	{
		public object Translate(TranslationContext context, ASTNode node)
		{
			return new Model.UnaryExpression("iIsNull", (Model.Expression)context.TranslateNode(node.Children[0]));
		}
	}

	// IfNull
    public class IfNullTranslator : INodeTranslator
    {
        #region INodeTranslator Members

        public object Translate(TranslationContext context, ASTNode node)
        {
            var result = new Model.CallExpression();
			result.Identifier = "IsNull"; // TODO: This is T-SQL dialect specific...
			result.Expressions.Add(context.TranslateNode(node.Children[0]));
			result.Expressions.Add(context.TranslateNode(node.Children[1]));
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
            var result = new Model.CallExpression();
			result.Identifier = "Coalesce";

            foreach (var child in node.Children)
            {
                result.Expressions.Add(context.TranslateNode(child));
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
        protected override String GetOperator()
        {
			return "iEqual";
        }
	}

	// NotEqual
    public class NotEqualTranslator : BinaryNodeTranslator
    {
        protected override String GetOperator()
        {
            return "iNotEqual";
        }
    }
	
    // Less
    public class LessTranslator : BinaryNodeTranslator
    {
        protected override String GetOperator()
        {
            return "iLess";
        }
    }
	
    // Greater
    public class GreaterTranslator : BinaryNodeTranslator
    {
        protected override String GetOperator()
        {
            return "iGreater";
        }
    }

	// LessOrEqual
    public class LessOrEqualTranslator : BinaryNodeTranslator
    {
        protected override String GetOperator()
        {
            return "iInclusiveLess";
        }
    }

    // GreaterOrqual
    public class GreaterOrEqualTranslator : BinaryNodeTranslator
    {
        protected override String GetOperator()
        {
            return "iInclusiveGreater";
        }
    }

    // Add
    public class AddTranslator : BinaryNodeTranslator
    {
        protected override String GetOperator()
        {
            return "iAddition";
        }
    }

	// Subtract
    public class SubtractTranslator : BinaryNodeTranslator
    {
        protected override String GetOperator()
        {
            return "iSubtraction";
        }
    }

	// Multiply
    public class MultiplyTranslator : BinaryNodeTranslator
    {
        protected override String GetOperator()
        {
            return "iMultiplication";
        }
    }

    // Divide - hmm, div or divide, how do I know?
	// BTR -> div is integer division, which is TruncatedDivide in HeD, so this mapping is correct
    public class DivideTranslator : BinaryNodeTranslator
    {
        protected override String GetOperator()
        {
            return "iDivision";
        }
    }

	// TruncatedDivide - hmm, div or divide, how do I know?
    public class TruncatedDivideTranslator : BinaryNodeTranslator
    {
        protected override String GetOperator()
        {
			return "iDiv";
        }
    }

	// Modulo
    public class ModuloTranslator : BinaryNodeTranslator
    {
        protected override String GetOperator()
        {
            return "iMod";
        }
    }

	// Ceiling
    public class CeilingTranslator : UnaryNodeTranslator
    {
        protected override String GetOperator()
        {
			// TODO: This instruction won't work, this actually needs to be translated as a call to a Ceiling function.
			return "iCeiling";
        }
    }

	// Floor
    public class FloorTranslator : UnaryNodeTranslator
    {
        protected override String GetOperator()
        {
			// TODO: This instruction won't work, this actually needs to be translated as a call to a Ceiling function.
            return "iFloor";
        }
    }

	// Truncate
	// Abs

	// Negate
    public class NegateTranslator : UnaryNodeTranslator
    {
        protected override String GetOperator()
        {
            return "iNegate";
        }
    }

	// Round
    public class RoundTranslator : UnaryNodeTranslator
    {
        protected override String GetOperator()
        {
			// TODO: This needs to return a call to the round function
            return "iRound";
        }
    }

	// Ln
	// Log

	// Power
    public class PowerTranslator : BinaryNodeTranslator
    {
        protected override String GetOperator()
        {
            return "iPower";
        }
    }

	// Succ/Successor
	// Pred/Predecessor
	// MinValue
	// MaxValue
	// Concat
	// Combine
	// Split
	// Length
	// Upper
	// Lower

	// First
	public class FirstTranslator : INodeTranslator
	{
		public object Translate(TranslationContext context, ASTNode node)
		{
			var selectStatement = SQLTranslationUtilities.EnsureSelectStatement(context.TranslateNode(node.Children[0]));

			selectStatement.TopClause = new TSQLModel.TopClause();
			selectStatement.TopClause.Quota = 1;

			// TODO: If OrderClause is not null...
			if (selectStatement.OrderClause == null)
			{
				selectStatement.OrderClause = new SQLModel.OrderClause();
				// TODO: If ResultType is not a tuple type...
				foreach (var element in ((ObjectType)node.ResultType).Properties)
				{
					selectStatement.OrderClause.Columns.Add(new SQLModel.OrderFieldExpression(element.Name, null, true));
				}
			}

			return selectStatement;
		}
	}

	// Last
	public class LastTranslator : INodeTranslator
	{
		public object Translate(TranslationContext context, ASTNode node)
		{
			var selectStatement = SQLTranslationUtilities.EnsureSelectStatement(context.TranslateNode(node.Children[0]));

			selectStatement.TopClause = new TSQLModel.TopClause();
			selectStatement.TopClause.Quota = 1;

			// TODO: If OrderClause is not null...
			if (selectStatement.OrderClause == null)
			{
				selectStatement.OrderClause = new SQLModel.OrderClause();
				// TODO: If ResultType is not a tuple type...
				foreach (var element in ((ObjectType)node.ResultType).Properties)
				{
					selectStatement.OrderClause.Columns.Add(new SQLModel.OrderFieldExpression(element.Name, null, false));
				}
			}

			return selectStatement;
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
			var result = new Model.CallExpression("DateAdd", new Model.Expression[] { }); // TODO: This is a T-SQL specific translation

			var granularity = node.Children[1];
			if (granularity.NodeType == "urn:hl7-org:v3:knowledgeartifact:r1:Literal") // TODO: Better story for this type of thing....
			{
				result.Expressions.Add(new Model.IdentifierExpression(granularity.GetAttribute<string>("value")));
			}
			else
			{
				throw new NotSupportedException("Date granularity argument to a DateAdd expression must be a literal because CREF does not support granularity as an argument, only as an attribute of the target DateAdd expression.");
			}

			var numberOfPeriods = node.Children[2];
			result.Expressions.Add(context.TranslateNode(numberOfPeriods));

			var date = node.Children[0];
			result.Expressions.Add(context.TranslateNode(date));

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
			// TODO: This is a T-SQL specific call
			var result = 
				new Model.CallExpression
				(
					"Convert", 
					new Model.Expression[] 
					{ 
						new Model.IdentifierExpression("dateTime"), 
						new Model.CallExpression
						(
							"Floor", 
							new Model.Expression[] 
							{ 
								new Model.CallExpression
								(
									"Convert", 
									new Model.Expression[] 
									{ 
										new Model.IdentifierExpression("float"), 
										new Model.CallExpression("GetDate", new Model.Expression[] { }) 
									}
								) 
							}
						) 
					}
				);

			return result;
		}

		#endregion
	}

	// Now
	public class NowTranslator : INodeTranslator
	{
		public object Translate(TranslationContext context, ASTNode node)
		{
			return new Model.CallExpression("GetDate", new Model.Expression[] { }); // TODO: This is a T-SQL specific call
		}
	}

	// Date
	// DurationBetween
	public class DurationBetweenTranslator : INodeTranslator
	{
		public object Translate(TranslationContext context, ASTNode node)
		{
			var precision = node.GetAttribute<string>("precision");
			var startDate = (Model.Expression)context.TranslateNode(node.Children[0]);
			var endDate = (Model.Expression)context.TranslateNode(node.Children[1]);
			return
				new Model.CallExpression
				(
					"DateDiff",
					new Model.Expression[]
					{
						new Model.ValueExpression(SQLTranslationUtilities.DateTimePrecisionToDatePartSpecifier(precision), Model.TokenType.String),
						startDate,
						endDate
					}
				);
		}
	}

	// DateFrom
	public class DateFromTranslator : INodeTranslator
	{
		#region INodeTranslator Members

		public object Translate(TranslationContext context, ASTNode node)
		{
			// TODO: This is a T-SQL specific call
			var result = 
				new Model.CallExpression
				(
					"Convert", 
					new Model.Expression[] 
					{ 
						new Model.IdentifierExpression("dateTime"), 
						new Model.CallExpression
						(
							"Floor", 
							new Model.Expression[] 
							{ 
								new Model.CallExpression
								(
									"Convert", 
									new Model.Expression[] 
									{ 
										new Model.IdentifierExpression("float"), 
										SQLTranslationUtilities.EnsureExpression(context.TranslateNode(node.Children[0]))
									}
								) 
							}
						) 
					}
				);

			return result;
		}

		#endregion
	}

	// TimeFrom
	// TimezoneFrom
	// DateTimeComponentFrom
	// DateTime

	// SameAs
	// SameOrBefore
	// SameOrAfter

	// Width

	// Contains
	//public class ContainsTranslator : INodeTranslator
	//{
	//	#region INodeTranslator Members

	//	public object Translate(TranslationContext context, ASTNode node)
	//	{
	//		// As long as this is list containment, it can be translated using an Exists(Filter(Source, Condition(Current = Value)));
	//		// TODO: Contains on a date interval could be translated as well
	//		var sourceNode = node.Children.FirstOrDefault(c => c.Name == "source");
	//		var elementNode = node.Children.FirstOrDefault(c => c.Name == "element");

	//		if (sourceNode.ResultType is IntervalType)
	//		{
	//			throw new NotImplementedException("Contains translation with Interval source is not yet supported.");
	//		}

	//		var filter = new SQLModel.FilterExpression();

	//		filter.Items.Add(context.TranslateNode(sourceNode));
            
	//		var condition = new SQLModel.BinaryExpression();
	//		condition.Operator = SQLModel.BinaryOperator.opEqual;
	//		condition.OperatorSpecified = true;

	//		// Assumption: A property expression with no path specified is equivalent to a "current" reference
	//		var property = new SQLModel.PropertyExpression();

	//		condition.Items.Add(property);
	//		condition.Items.Add(context.TranslateNode(elementNode));

	//		var exists = new Model.UnaryExpression();
	//		exists.Instruction = "iExists";
	//		exists.Expression = filter;

	//		return exists;
	//	}

	//	#endregion
	//}

	// In
	// Within
	// ProperContains
	// ProperIn
	// Includes
	// IncludedIn
	// ProperIncludes
	// ProperIncludedIn
	// Before
	// After
	// Starts
	// Ends
	// Meets
	// MeetsBefore
	// MeetsAfter
	// Overlaps
	// OverlapsBefore
	// OverlapsAfter
	// Union
	// Intersect
	// Difference/Except
	// Begin/Start
	// End
	// Collapse
	// Expand

	// IsEmpty
	public class IsEmptyTranslator : INodeTranslator
	{
		#region INodeTranslator Members

		public object Translate(TranslationContext context, ASTNode node)
		{
			var unaryExpression = new Model.UnaryExpression();
			unaryExpression.Instruction = "iExists";
			unaryExpression.Expression = SQLTranslationUtilities.EnsureSelectStatementExpression(context.TranslateNode(node.Children[0]));

			var notExpression = new Model.UnaryExpression();
			notExpression.Instruction = "iNot";
			notExpression.Expression = unaryExpression;

			return notExpression;
		}

		#endregion
	}

	// IsNotEmpty/Exists
	public class IsNotEmptyTranslator : INodeTranslator
	{
		#region INodeTranslator Members

		public object Translate(TranslationContext context, ASTNode node)
		{
			var unaryExpression = new Model.UnaryExpression();
			unaryExpression.Instruction = "iExists";
			unaryExpression.Expression = SQLTranslationUtilities.EnsureSelectStatementExpression(context.TranslateNode(node.Children[0]));

			return unaryExpression;
		}

		#endregion
	}

	// Times

	// First
	// Last

	// Filter
	//public class FilterTranslator : INodeTranslator
	//{
	//	#region INodeTranslator Members

	//	public object Translate(TranslationContext context, ASTNode node)
	//	{
	//		var scope = node.GetAttribute<string>("scope", VerificationContext.Current);
	//		if (scope != VerificationContext.Current)
	//		{
	//			throw new NotSupportedException("Translation of filter expression is not supported because it involves a named scope reference, which is not supported in CREF.");
	//		}

	//		var source = node.Children[0];
	//		var condition = node.Children[1];

	//		var result = new SQLModel.FilterExpression();
	//		result.Items.Add(context.TranslateNode(source));
	//		result.Items.Add(context.TranslateNode(condition));

	//		return result;
	//	}

	//	#endregion
	//}

	// IndexOf

	// In
	public class InTranslator : INodeTranslator
	{
		#region INodeTranslator Members

		public object Translate(TranslationContext context, ASTNode node)
		{
			// In(element, interval)...
			// In(element, list)
			// Translate as:
			// exists (select * from (<list>) T where <element equality condition>)

			// As long as this is list containment, it can be translated using an Exists(Filter(Collection, Condition(Current = Element)));
			var elementNode = node.Children[0];
			var collectionNode = node.Children[1];

			if (elementNode.ResultType is ListType)
			{
				throw new NotSupportedException("In translation with an element of type list is not supported because there is no equivalent SQL representation.");
			}

			if (collectionNode.ResultType is ListType)
			{
				var selectExpression = new SQLModel.SelectExpression();
				selectExpression.SelectClause = new SQLModel.SelectClause();
				selectExpression.SelectClause.NonProject = true;
				selectExpression.FromClause = new SQLModel.CalculusFromClause(new SQLModel.TableSpecifier(SQLTranslationUtilities.EnsureSelectStatementExpression(context.TranslateNode(collectionNode)), "T"));
				selectExpression.WhereClause = new SQLModel.WhereClause();

				Model.Expression condition = null;
				if (elementNode.ResultType is ObjectType)
				{
					foreach (var property in ((ObjectType)elementNode.ResultType).Properties)
					{
						// TODO: O is an assumed alias here, need a way to establish the outer context for comparison of the element....
						var propertyCondition = new Model.BinaryExpression(new SQLModel.QualifiedFieldExpression(property.Name, "T"), "iEqual", new SQLModel.QualifiedFieldExpression(property.Name, "O"));

						if (condition == null)
						{
							condition = propertyCondition;
						}
						else
						{
							condition = new Model.BinaryExpression(condition, "iAnd", propertyCondition);
						}
					}
				}
				else
				{
					condition = new Model.BinaryExpression(new SQLModel.QualifiedFieldExpression("value", "T"), "iEqual", (Model.Expression)context.TranslateNode(elementNode));
				}

				selectExpression.WhereClause.Expression = condition;
				return selectExpression;
			}
			else
			{
				var betweenExpression = new Model.BetweenExpression();
				betweenExpression.Expression = (Model.Expression)context.TranslateNode(elementNode);
				// TODO: Translate an interval selector....
				betweenExpression.LowerExpression = new Model.ValueExpression(1);
				betweenExpression.UpperExpression = new Model.ValueExpression(1);
				return betweenExpression;
			}
		}

		#endregion
	}

	// Sort
	// ForEach
	// Distinct
	// Current

	// SingletonFrom
	public class SingletonFromTranslator : INodeTranslator
	{
		public object Translate(TranslationContext context, ASTNode node)
		{
			var selectStatement = SQLTranslationUtilities.EnsureSelectStatement(context.TranslateNode(node.Children[0]));

			selectStatement.TopClause = new TSQLModel.TopClause();
			selectStatement.TopClause.Quota = 1;

			return selectStatement;
		}
	}

	// Count
	// Sum
	// Min
	// Max
	// Avg
	// Median
	// Mode
	// Variance
	// PopulationVariance
	// StdDev
	// PopulationStdDev
	// AllTrue
	// AnyTrue

	// Property
	public class PropertyTranslator : INodeTranslator
	{
		#region INodeTranslator Members

		public object Translate(TranslationContext context, ASTNode node)
		{
			// If the property has a source
				// select propertyName as value from (<source>)
			// Else
				// If the property has a scope
					// It's a qualified field expression
				// Else
					// It's an identifier expression

			// TODO: Handle nested and indexed paths...
			var path = node.GetAttribute<string>("path");
			var source = node.Children.Where(c => c.Name == "source").FirstOrDefault();
			if (source != null)
			{
				var selectExpression = new SQLModel.SelectExpression();
				selectExpression.SelectClause = new SQLModel.SelectClause();
				selectExpression.SelectClause.Columns.Add(new SQLModel.ColumnExpression(SQLTranslationUtilities.ResolvePath(path, "T"), "value"));
				selectExpression.FromClause = new SQLModel.CalculusFromClause(new SQLModel.TableSpecifier(SQLTranslationUtilities.EnsureExpression(context.TranslateNode(source)), "T"));
				return selectExpression;
			}
			else
			{
				var scope = node.GetAttribute<string>("scope", VerificationContext.Current);
				if (scope != VerificationContext.Current)
				{
					return SQLTranslationUtilities.ResolvePath(path, "T");
				}
				else
				{
					return SQLTranslationUtilities.ResolvePath(path, null);
				}
			}

			//var sourceType = node.GetAttribute<ObjectType>("sourceType");

			// Translate path information
			//return context.TransformModelPath(sourceType, node, node.GetAttribute<string>("path"));
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
            var result = new Model.ValueExpression();
            
			result.Token = Model.TokenType.Boolean;
            result.Value = node.GetAttribute<string>("value"); // TODO: This assumes boolean representation will translate...

            return result;
        }

        #endregion
    }

	// CodeLiteral
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
			// Physical quantity is not supported as a value type within SQL, so the literal will need to be transformed to a "unit normalized representation"
			// TODO: First-class support for Quantity values within the target SQL environment.
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

			var result = new Model.ValueExpression();
			result.Token = Model.TokenType.Decimal;
			result.Value = value;

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
            var result = new Model.ValueExpression();
			result.Token = Model.TokenType.String;

            result.Value = node.GetAttribute<string>("value");

            return result;
        }

        #endregion
    }

	// UrlLiteral
	// TimestampLiteral
	// PeriodLiteral

	// AliasedQuerySource
	// DefineClause
	// With
	// Without
	// SortClause
	// ReturnClause
	// Query
	public class QueryTranslator : INodeTranslator
	{
		private SQLModel.TableSpecifier TranslateAliasedQuerySource(TranslationContext context, Node source)
		{
			var expression = (ASTNode)source.Children.Where(n => n.Name == "expression").Single();
			var alias = source.GetAttribute<String>("alias");
			var sqlContext = (SQLTranslationContext)context;
			sqlContext.PushQueryContext();
			try
			{
				return new SQLModel.TableSpecifier(SQLTranslationUtilities.EnsureExpression(context.TranslateNode(expression)), alias);
			}
			finally
			{
				sqlContext.PopQueryContext();
			}
		}

		private Model.Expression TranslateRelationshipClause(TranslationContext context, Node relationship)
		{
			var relatedTableSpecifier = TranslateAliasedQuerySource(context, relationship);
			var relatedConditionNode = (ASTNode)relationship.Children.Where(n => n.Name == "suchThat").Single();
			var relatedCondition = (Model.Expression)context.TranslateNode(relatedConditionNode);
			var relatedSelect = new SQLModel.SelectExpression();
			relatedSelect.SelectClause = new SQLModel.SelectClause();
			relatedSelect.SelectClause.NonProject = true;
			relatedSelect.SelectClause.Distinct = false;
			relatedSelect.FromClause = new SQLModel.CalculusFromClause(relatedTableSpecifier);
			relatedSelect.WhereClause = new SQLModel.WhereClause(relatedCondition);
			var relatedExists = new Model.UnaryExpression("iExists", relatedSelect);
			if (relationship.NodeType.GetLocalName() == "Without")
			{
				return new Model.UnaryExpression("iNot", relatedExists);
			}
			else
			{
				return relatedExists;
			}
		}

		private SQLModel.SelectClause TranslateReturnClause(TranslationContext context, Node returnClause)
		{
			var result = new SQLModel.SelectClause();
			result.Distinct = returnClause.GetAttribute<Boolean>("distinct", true);
			var expression = returnClause.Children.Single(n => n.Name == "expression");
			if (expression.NodeType.GetLocalName() == "Tuple")
			{
				foreach (var element in expression.Children)
				{
					var column = 
						new SQLModel.ColumnExpression
						(
							(Model.Expression)context.TranslateNode((ASTNode)element.Children.Single(n => n.Name == "value")), 
							element.GetAttribute<String>("name")
						);
				}
			}

			return result;
		}

		private SQLModel.OrderClause TranslateSortClause(TranslationContext context, Node node)
		{
			throw new NotImplementedException();
		}

		public object Translate(TranslationContext context, ASTNode node)
		{
			var selectExpression = new SQLModel.SelectExpression();

			var fromClause = new SQLModel.CalculusFromClause();
			selectExpression.FromClause = fromClause;

			var queryExpression = new SQLModel.QueryExpression();
			queryExpression.SelectExpression = selectExpression;

			var selectStatement = new SQLModel.SelectStatement();
			selectStatement.QueryExpression = queryExpression;

			// 1..* source: AliasedQuerySource
			foreach (var child in ((Node)node).Children.Where(n => n.Name == "source"))
			{
				var tableSpecifier = TranslateAliasedQuerySource(context, child);
				fromClause.TableSpecifiers.Add(tableSpecifier);
			}

			// 0..* define: DefineClause
			foreach (var child in ((Node)node).Children.Where(n => n.Name == "define"))
			{
				// TODO: This would need to be nested to be accessible within context in the SQL query
				throw new NotImplementedException("Define clause translation is not yet implemented");
			}

			// 0..* relationship: With | Without
			Model.Expression relationshipConditions = null;
			foreach (var child in ((Node)node).Children.Where(n => n.Name == "relationship"))
			{
				var relationshipCondition = TranslateRelationshipClause(context, child);
				if (relationshipConditions != null)
				{
					relationshipConditions = new Model.BinaryExpression(relationshipConditions, "iAnd", relationshipCondition);
				}
				else
				{
					relationshipConditions = relationshipCondition;
				}
			}

			// 0..1 where: Expression
			Model.Expression whereCondition = null;
			var whereConditionNode = node.Children.SingleOrDefault(n => n.Name == "where");
			if (whereConditionNode != null)
			{
				whereCondition = (Model.Expression)context.TranslateNode(whereConditionNode);
			}

			// 0..1 return: ReturnClause
			var returnClause = ((Node)node).Children.SingleOrDefault(n => n.Name == "return");
			if (returnClause != null)
			{
				var selectClause = TranslateReturnClause(context, returnClause);
				selectExpression.SelectClause = selectClause;
			}

			if (selectExpression.SelectClause == null)
			{
				selectExpression.SelectClause = new SQLModel.SelectClause();
				selectExpression.SelectClause.NonProject = true;
			}

			// 0..1 sort: SortClause
			var sortClause = ((Node)node).Children.SingleOrDefault(n => n.Name == "sort");
			if (sortClause != null)
			{
				var orderClause = TranslateSortClause(context, sortClause);
				selectStatement.OrderClause = orderClause;
			}

			return selectStatement;
		}
	}

	// AliasRef
	public class AliasRefTranslator : INodeTranslator
	{
		public object Translate(TranslationContext context, ASTNode node)
		{
			// name: String
			return new Model.IdentifierExpression(node.GetAttribute<String>("name"));
		}
	}

	// QueryDefineRef
	public class QueryDefineRefTranslator : INodeTranslator
	{
		public object Translate(TranslationContext context, ASTNode node)
		{
			// name: String
			return new Model.IdentifierExpression(node.GetAttribute<String>("name"));
		}
	}

	// Retrieve
	public class RetrieveTranslator : INodeTranslator
	{
		public object Translate(TranslationContext context, ASTNode node)
		{
			var requestListType = node.ResultType as ListType;
			var requestType = (requestListType == null ? node.ResultType : requestListType.ElementType) as ObjectType;
			if (requestType == null)
			{
				throw new InvalidOperationException(String.Format("Unable to determine request type from source type: '{0}'.", node.ResultType.Name));
			}

			Model.Expression condition = null;

			// Translate Codes
			var codes = node.Children.Where(n => n.Name == "codes").FirstOrDefault();
			if (codes != null)
			{
				var codesResult = context.TranslateNode(codes);
				// codesResult will be a select statement returning the list of codes
				// So the retrieve must include a where condition limiting the code property to the set of codes (same as InValueSet translation)

				var codeExpression = SQLTranslationUtilities.ResolvePath(node.GetAttribute<string>("codeProperty"), "T");
				var selectExpression = (SQLModel.SelectExpression)codesResult; // This assumes the codes element is a ValueSetRef...
				selectExpression.WhereClause.Expression =
					new Model.BinaryExpression
					(
						selectExpression.WhereClause.Expression,
						"iAnd",
						new Model.BinaryExpression(codeExpression, "iEqual", new SQLModel.QualifiedFieldExpression("Code", "VS"))
					);

				var codeCondition = new Model.UnaryExpression("iExists", selectExpression);
				if (condition != null)
				{
					condition = new Model.BinaryExpression(condition, "iAnd", codeCondition);
				}
				else
				{
					condition = codeCondition;
				}
			}

			// Translate DateRange
			var dateRange = node.Children.Where(n => n.Name == "dateRange").FirstOrDefault();
			if (dateRange != null)
			{
				var dateRangeResult = context.TranslateNode(dateRange);
				var dateExpression = SQLTranslationUtilities.ResolvePath(node.GetAttribute<string>("dateProperty"), "T");
				// dateRangeResult will be an interval-valued expression
				// So the retrieve must include a where condition limiting the date range property to the interval (same as In(date, interval) translation)
				var dateRangeCondition = new Model.BetweenExpression();
				dateRangeCondition.Expression = dateExpression;
				// TODO:
				//dateRangeCondition.LowerExpression = // dateRangeResult.Low...
				//dateRangeCondition.UpperExpression = // dateRangeResult.High...

				//if (condition != null)
				//{
				//	condition = new Model.BinaryExpression(condition, "iAnd", dateRangeCondition);
				//}
				//else
				//{
				//	condition = codeCondition;
				//}
			}

			// For FHIR, there will also potentially need to be profile filters...
			// This depends on how the FHIR model is realized within the SQL structures...

			var inQueryContext = ((SQLTranslationContext)context).InQueryContext();
			// Within a Query Context, a retrieve is part of a FromClause
			var tableExpression = new SQLModel.TableExpression(requestType.Name);
			if (inQueryContext && (condition == null))
			{
				return tableExpression;
			}
			else
			{
				var selectExpression = new SQLModel.SelectExpression();
				selectExpression.SelectClause = new SQLModel.SelectClause();
				selectExpression.SelectClause.Distinct = false;
				selectExpression.SelectClause.NonProject = true;
				selectExpression.FromClause = new SQLModel.CalculusFromClause(new SQLModel.TableSpecifier(tableExpression, "T"));
				if (condition != null)
				{
					selectExpression.WhereClause = new SQLModel.WhereClause();
					selectExpression.WhereClause.Expression = condition;
				}
				return selectExpression;
			}
		}
	}

/*
	// DataRequest
	// ClinicalRequest
	public class ClinicalRequestTranslator : INodeTranslator
	{
		#region INodeTranslator Members

		public object Translate(TranslationContext context, ASTNode node)
		{
			// Model mapping
			var result = new SQLModel.RequestExpression();

			if (node.ResultType == null)
			{
				throw new InvalidOperationException("Clinical request expression has no result type.");
			}

			var cardinality = (RequestCardinality)Enum.Parse(typeof(RequestCardinality), node.Attributes["cardinality"].ToString(), true);
			result.Cardinality = cardinality == RequestCardinality.Single ? SQLModel.RequestCardinality.Single : SQLModel.RequestCardinality.Multiple;
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
				var filter = new SQLModel.FilterExpression();

				filter.Items.Add(result);

				var condition = new SQLModel.BinaryExpression();
				condition.Operator = SQLModel.BinaryOperator.opEqual;
				condition.OperatorSpecified = true;

				condition.Items.Add(new SQLModel.PropertyExpression() { Path = "Status" });
				condition.Items.Add(new SQLModel.ValueExpression() { Type = SQLModel.ValueType.String, TypeSpecified = true, Value = filterValue });

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

		private SQLModel.PatientQueryType GetQueryType(ObjectType type)
		{
			if (type == null)
			{
				throw new ArgumentNullException("type");
			}

			return (SQLModel.PatientQueryType)Enum.Parse(typeof(SQLModel.PatientQueryType), type.Name);
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
			var result = new SQLModel.ValueSetExpression();

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
*/

	// ValueSetRef
	public class ValueSetRefTranslator : INodeTranslator
	{
		public object Translate(TranslationContext context, ASTNode node)
		{
			var selectExpression = new SQLModel.SelectExpression();
			selectExpression.SelectClause = new SQLModel.SelectClause();
			selectExpression.SelectClause.NonProject = true;
			selectExpression.FromClause = new SQLModel.CalculusFromClause(new SQLModel.TableSpecifier(new SQLModel.TableExpression("ValueSet"), "VS"));
			selectExpression.WhereClause = new SQLModel.WhereClause();

			var valueSetName = node.GetAttribute<String>("name");
			var valueSetLibraryName = node.GetAttribute<String>("libraryName", ((SQLTranslationContext)context).ArtifactName);
			var valueSetCondition = 
				new Model.BinaryExpression
				(
					new SQLModel.QualifiedFieldExpression("ValueSetName", "VS"), 
					"iEqual", 
					new Model.ValueExpression(String.Format("{0}.{1}", valueSetLibraryName, valueSetName), Model.TokenType.String)
				);

			selectExpression.WhereClause.Expression = valueSetCondition;

			return selectExpression;
		}
	}

	// InValueSet
    public class InValueSetTranslator : INodeTranslator
    {
        #region INodeTranslator Members

        public object Translate(TranslationContext context, ASTNode node)
        {
			// Translates to a table-based solution
			// exists ( select * from ValueSets where ValueSetName = X and ValueSetVersion = Y and Code = Z )

			var codeExpression = (Model.Expression)context.TranslateNode(node.Children[0]);
			var selectExpression = (SQLModel.SelectExpression)context.TranslateNode(node.Children[1]);
			selectExpression.WhereClause.Expression =
				new Model.BinaryExpression
				(
					selectExpression.WhereClause.Expression,
					"iAnd",
					new Model.BinaryExpression(codeExpression, "iEqual", new SQLModel.QualifiedFieldExpression("Code", "VS"))
				);

			return new Model.UnaryExpression("iExists", selectExpression);
        }

        #endregion
    }

	// Subsumes
	// SetSubsumes

	// Quantity
	// CalculateAge
	public class CalculateAgeTranslator : INodeTranslator
	{
		public object Translate(TranslationContext context, ASTNode node)
		{
			var precision = node.GetAttribute<string>("precision");
			return 
				new Model.CallExpression
				(
					String.Format("dbo.CalculateAgeIn{0}s", precision),
					new Model.Expression[]
					{
						(Model.Expression)context.TranslateNode(node.Children[0])
					}
				);
		}
	}

	// CalculateAgeAt
	public class CalculateAgeAtTranslator : INodeTranslator
	{
		public object Translate(TranslationContext context, ASTNode node)
		{
			var precision = node.GetAttribute<string>("precision");
			return
				new Model.CallExpression
				(
					String.Format("dbo.CalculateAgeIn{0}sAt", precision),
					new Model.Expression[]
					{
						(Model.Expression)context.TranslateNode(node.Children[0]),
						(Model.Expression)context.TranslateNode(node.Children[1])
					}
				);
		}
	}
}
