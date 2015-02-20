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

using HeD.Engine.Verification;

using Model = Alphora.Dataphor.DAE.Language;
using SQLModel = Alphora.Dataphor.DAE.Language.SQL;
using TSQLModel = Alphora.Dataphor.DAE.Language.TSQL;

namespace SQL.Translation
{
	public static class SQLTranslationUtilities
	{
		public static String DateTimePrecisionToDatePartSpecifier(string precision)
		{
			if (String.IsNullOrEmpty(precision))
			{
				throw new ArgumentNullException("precision");
			}

			return precision.ToLower();
		}

		public static Model.Expression EnsureExpression(object expression)
		{
			var result = expression as Model.Expression;
			if (result == null)
			{
				return EnsureSelectStatementExpression(expression);
			}

			return result;
		}

		/// <summary>
		/// Ensures that the given expression, if not a select expression, is promoted to a select expression.
		/// </summary>
		/// <param name="expression"></param>
		/// <returns></returns>
		public static Model.Expression EnsureSelectExpression(object expression)
		{
			if (!(expression is SQLModel.SelectExpression) && !(expression is SQLModel.QueryExpression) && !(expression is SQLModel.SelectStatement) && !(expression is SQLModel.SelectStatementExpression))
			{
				var selectExpression = new SQLModel.SelectExpression();
				selectExpression.SelectClause = new SQLModel.SelectClause();
				selectExpression.SelectClause.Columns.Add(new SQLModel.ColumnExpression((Model.Expression)expression, "value"));
				return selectExpression;
			}

			return EnsureSelectStatementExpression(expression);
		}

		public static Model.Expression PromoteBooleanValuedExpression(object expression)
		{
			if (expression == null)
			{
				throw new ArgumentNullException("expression");
			}

			var modelExpression = expression as Model.Expression;
			if (modelExpression == null)
			{
				throw new InvalidOperationException(String.Format("Statement of type {0} cannot be promoted to a boolean-valued expression.", expression.GetType().Name));
			}

			return new Model.CaseExpression(new Model.CaseItemExpression[] { new Model.CaseItemExpression(modelExpression, new Model.ValueExpression(1)) }, new Model.CaseElseExpression(new Model.ValueExpression(0))); 
		}

		public static Model.Expression DemoteBooleanValuedExpression(Model.Expression expression)
		{
			// Although the traditional test for true is "<> 0", we use "= 1" here because A) we have control over how boolean are translated to integers,
			// so we know they will always be represented as a 1 or 0, and the use of "=" rather than "<>" is more likely to be optimizable if it hits a sargable scenario. (we stay conjunctive).
			return new Model.BinaryExpression(expression, "iEqual", new Model.ValueExpression(1));
		}

		public static SQLModel.SelectStatementExpression EnsureSelectStatementExpression(object expression)
		{
			if (expression == null)
			{
				throw new ArgumentNullException("expression");
			}

			var result = expression as SQLModel.SelectStatementExpression;
			if (result == null)
			{
				var selectStatement = EnsureSelectStatement(expression);
				result = new SQLModel.SelectStatementExpression();
				result.SelectStatement = selectStatement;
			}

			return result;
		}

		public static TSQLModel.SelectStatement EnsureSelectStatement(object expression)
		{
			if (expression == null)
			{
				throw new ArgumentNullException("expression");
			}

			var result = expression as TSQLModel.SelectStatement;

			if (result == null)
			{
				var selectStatement = expression as SQLModel.SelectStatement;
				if (selectStatement != null)
				{
					result = new TSQLModel.SelectStatement();
					if (selectStatement.Modifiers != null)
					{
						result.Modifiers = new Model.LanguageModifiers();
						result.Modifiers.AddRange(selectStatement.Modifiers);
					}
					result.QueryExpression = selectStatement.QueryExpression;
					result.OrderClause = selectStatement.OrderClause;
				}

				if (result == null)
				{
					var queryExpression = expression as SQLModel.QueryExpression;
					if (queryExpression != null)
					{
						result = new TSQLModel.SelectStatement();
						result.QueryExpression = queryExpression;
					}

					if (result == null)
					{
						var selectExpression = expression as SQLModel.SelectExpression;
						if (selectExpression != null)
						{
							result = new TSQLModel.SelectStatement();
							result.QueryExpression = new SQLModel.QueryExpression();
							result.QueryExpression.SelectExpression = selectExpression;
						}
					}
				}
			}

			if (result == null)
			{
				throw new InvalidOperationException(String.Format("Could not promote expression of type {0} to a SelectStatement.", expression.GetType().Name));
			}

			return result;
		}

		public static Model.Expression ResolvePath(string path, string scope)
		{
			if (String.IsNullOrEmpty(path))
			{
				throw new ArgumentNullException("path");
			}

			Model.Expression result = null;

			// Path may contain any number of member accessors and indexers
			// e.g. code.coding[1]

			while (true)
			{
				int firstIndex = path.IndexOfAny(new[] { '.', '[' });
				if (firstIndex >= 0)
				{
					var memberName = path.Substring(0, firstIndex);
					if (memberName != String.Empty)
					{
						result = BuildMemberAccessor(result, memberName, scope);
					}

					if (path[firstIndex] == '[')
					{
						var indexOfMatchingBracket = IndexOfMatchingBracket(path, firstIndex);
						var indexPath = path.Substring(firstIndex + 1, indexOfMatchingBracket - (firstIndex + 1));
						var indexExpression = ResolvePath(indexPath, scope);
						// The use of .Get() here assumes the existence of appropriate list types with a Get method.
						// This is a T-SQL specific solution to the representation of FHIR within SQL.
						// Oracle would likely use a VARRAY, and PostgreSQL would use an array. 
						// Both those syntaxes would use an Indexer expression here, rather than a Get.
						result = new Model.QualifierExpression(result, new Model.CallExpression("Get", new Model.Expression[] { indexExpression }));
						if (indexOfMatchingBracket >= (path.Length - 1))
						{
							break;
						}

						path = path.Substring(indexOfMatchingBracket + 1);
					}
					else
					{
						path = path.Substring(firstIndex + 1);
					}
				}
				else
				{
					result = BuildMemberAccessor(result, path, scope);
					break;
				}
			}

			return result;
		}

		private static int IndexOfMatchingBracket(string path, int indexOfOpeningBracket)
		{
			int bracketCount = 1;
			int currentIndex = indexOfOpeningBracket + 1;
			while (currentIndex < path.Length)
			{
				switch (path[currentIndex])
				{
					case '[': bracketCount++; break;
					case ']': bracketCount--; break;
				}

				if (bracketCount == 0)
				{
					break;
				}

				currentIndex++;
			}

			if (currentIndex == path.Length)
			{
				throw new InvalidOperationException("Could not determine matching bracket index.");
			}

			return currentIndex;
		}

		private static Model.Expression BuildMemberAccessor(Model.Expression current, string path, string scope)
		{
			if (path.IsDigit())
			{
				if (current != null)
				{
					throw new InvalidOperationException("Only index expressions can be integer literals.");
				}

				return new Model.ValueExpression(Int32.Parse(path));
			}
			else
			{
				if (current != null)
				{
					return new Model.QualifierExpression(current, new SQLModel.QualifiedFieldExpression(path));
				}
				else
				{
					return new SQLModel.QualifiedFieldExpression(path, scope);
				}
			}
		}
	}
}
