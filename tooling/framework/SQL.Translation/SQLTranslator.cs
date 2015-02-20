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
using HeD.Engine.Translation;
using HeD.Engine.Verification;
using Model = Alphora.Dataphor.DAE.Language;
using SQLModel = Alphora.Dataphor.DAE.Language.SQL;

namespace SQL.Translation
{
    /// <summary>
    /// Translates an HeD artifact to an equivalent SQL representation.
    /// </summary>
    /// <remarks>
    /// Translation of an HeD artifact to SQL results in an SQL view being created for each expression
    /// defined in the Artifact (and any associated libraries).
    /// </remarks>
	public class SQLTranslator : IArtifactTranslator
	{
		private Artifact source;
		private Dictionary<String, Library> translatedLibraries = new Dictionary<String, Library>();

		#region IArtifactTranslator Members

		public object Translate(Artifact source)
		{
            var result = new SQLModel.Batch();

			var context = new SQLTranslationContext(this);

			var identifier = source.MetaData.Children.First(c => c.Name == "identifiers").Children.First(c => c.Name == "identifier");
			context.StartArtifact(identifier.GetAttribute<string>("root"));
			try
			{
				// Libraries
				foreach (LibraryRef libraryRef in source.Libraries)
				{
					// Okay to pass null for the verification context here, each of the referenced libraries should already be verified at this point.
					result.Statements.AddRange(TranslateLibrary(context, libraryRef.Name, LibraryFactory.ResolveLibrary(libraryRef, null)));
				}

				// TODO: Parameters

				// ExpressionDefs
				foreach (ExpressionDef expression in source.Expressions) 
				{
					result.Statements.Add(TranslateExpressionDef(context, source.MetaData.Name, expression));
				}

				// Criteria
				// Criteria are translated as "create view <artifact name>_<condition><conditionNumber>"
				int conditionNumber = 0;
				foreach (ASTNode condition in source.Conditions) 
				{
					conditionNumber++;
					result.Statements.Add(TranslateCondition(context, source.MetaData.Name, String.Format("Condition{0}", conditionNumber), condition));
				}

				// TODO: Assertion

				return result;
			}
			finally
			{
				context.EndArtifact();
			}
		}

		#endregion

		private Model.Statements TranslateLibrary(SQLTranslationContext context, String localName, Library library)
		{
			var result = new Model.Statements();

			if (!translatedLibraries.ContainsKey(library.Name))
			{
				context.StartArtifact(localName);
				try
				{
					// Libraries
					foreach (LibraryRef libraryRef in library.Libraries)
					{
						TranslateLibrary(context, libraryRef.Name, LibraryFactory.ResolveLibrary(libraryRef, null));
					}

					// TODO: CodeSystems
					// TODO: ValueSets
					// TODO: Parameters
				
					// ExpressionDefs
					foreach (ExpressionDef expressionDef in library.Expressions)
					{
						result.Add(TranslateExpressionDef(context, localName, expressionDef));
					}

					// TODO: Functions
				}
				finally
				{
					context.EndArtifact();
				}
			}

			return result;
		}

		private Model.Statement TranslateExpressionDef(SQLTranslationContext context, string artifactName, ExpressionDef expressionDef)
		{
			var result = new SQLModel.CreateViewStatement();
			result.ViewName = context.GetExpressionObjectName(String.Format("{0}.{1}", artifactName, expressionDef.Name));
			var translatedExpression = context.TranslateNode(expressionDef.Expression);
			if (DataTypes.Equal(expressionDef.Expression.ResultType, DataTypes.Boolean)) 
			{
				translatedExpression = SQLTranslationUtilities.PromoteBooleanValuedExpression(translatedExpression);
			}

			result.Expression = SQLTranslationUtilities.EnsureSelectExpression(translatedExpression);
			return result;
		}

		private Model.Statement TranslateCondition(SQLTranslationContext context, string artifactName, string conditionName, ASTNode condition)
		{
			var result = new SQLModel.CreateViewStatement();
			result.ViewName = context.GetExpressionObjectName(String.Format("{0}.{1}", artifactName, conditionName));
			result.Expression = SQLTranslationUtilities.EnsureSelectExpression(context.TranslateNode(condition));
			return result;
		}
	}
}
