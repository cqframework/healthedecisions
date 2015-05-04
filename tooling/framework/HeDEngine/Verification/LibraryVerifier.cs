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
	public class LibraryVerifier
	{
		public IEnumerable<VerificationException> Verify(Library library)
		{
			// Parameters must be validated without access to parameter definitions, or expression definitions
			var initialContext = new VerificationContext(library.Models, library.Libraries, null, null, null, null);

			// Resolve parameter types and verify default expressions
			foreach (var parameter in library.Parameters)
			{
				try
				{
					parameter.ParameterType = initialContext.ResolveType(parameter.TypeName);

					if (parameter.Default != null)
					{
						Verifier.Verify(initialContext, parameter.Default);
					    initialContext.VerifyType(parameter.Default.ResultType, parameter.ParameterType);
					}
				}
				catch (Exception e)
				{
					initialContext.ReportMessage(new VerificationException(String.Format("Exceptions occurred verifying parameter {0}.", parameter.Name), e), null);
				}
			}

			var context = new VerificationContext(library.Models, library.Libraries, library.Parameters, library.Expressions, library.ValueSets, initialContext.Messages);

			// Verify expressions
			foreach (var expression in library.Expressions)
			{
				// If the result type is already set, this expression ref has already been resolved.
				if (expression.Expression.ResultType == null)
				{
					Verifier.Verify(context, expression.Expression);
				}
			}

            return context.Messages;
		}

		private void VerifyExpressionNodes(VerificationContext context, Node node)
		{
			var astNode = node as ASTNode;
			if (astNode != null)
			{
				Verifier.Verify(context, astNode);
			}
			else
			{
				foreach (var child in node.Children)
				{
					VerifyExpressionNodes(context, child);
				}
			}
		}
	}
}
