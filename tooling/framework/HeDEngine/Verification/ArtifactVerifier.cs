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
	public class ArtifactVerifier
	{
		public IEnumerable<VerificationException> Verify(Artifact artifact)
		{
			// Parameters must be validated without access to parameter definitions, or expression definitions
			var initialContext = new VerificationContext(artifact.Models, artifact.Libraries, null, null, null, null, null);

			// Resolve parameter types and verify default expressions
			foreach (var parameter in artifact.Parameters)
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

			var context = new VerificationContext(artifact.Models, artifact.Libraries, artifact.Parameters, artifact.Expressions, artifact.CodeSystems, artifact.ValueSets, initialContext.Messages);

			// Verify expressions
			foreach (var expression in artifact.Expressions)
			{
				// If the result type is already set, this expression ref has already been resolved.
				if (expression.Expression.ResultType == null)
				{
					Verifier.Verify(context, expression.Expression);
				}
			}

			// Verify conditions
			foreach (var condition in artifact.Conditions)
			{
				Verifier.Verify(context, condition);

				if (!DataTypes.Equal(condition.ResultType, DataTypes.Boolean))
				{
					context.ReportMessage(new InvalidOperationException("Condition must evaluate to a value of type boolean."), condition);
				}
			}

			// Verify trigger expressions
			if (artifact.Triggers != null)
			{
				foreach (var trigger in artifact.Triggers)
				{
					VerifyExpressionNodes(context, trigger);
				}
			}

			// Verify action conditions
			if (artifact.ActionGroup != null)
			{
                var containers = new Dictionary<string, ParameterDef>();

    			// Verify documentation template conditions and binding expressions
                VerifyResponseBindings(context, artifact.ActionGroup, containers);

                foreach (var parameter in containers.Values)
                {
                    context.AddParameterDef(parameter);
                }

				VerifyExpressionNodes(context, artifact.ActionGroup);
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

        private void VerifyResponseBindings(VerificationContext context, Node node, Dictionary<string, ParameterDef> containers)
        {
            // foreach action
                // If DeclareResponseAction
                    // Create a parameter with that name
                // If CollectInformationAction
                    // Create a property on the appropriate parameter with the correct type
                        // Name = responseBinding.property
                        // Type = responseCardinality = Single ? responseDataType : List<responseDataType>

            try
            {
                switch (node.NodeType.GetLocalName())
                {
                    case "DeclareResponseAction" :
                    {
                        var containerName = node.GetAttribute<string>("name");
                        var container = new ParameterDef { Name = containerName, ParameterType = new ObjectType(containerName + "Type", new List<PropertyDef> { }) };
                        containers.Add(container.Name, container);
                    }
                    break;

                    case "CollectInformationAction" :
                    {
                        var responseBinding = node.Children.FirstOrDefault(c => c.Name == "responseBinding");
                        if (responseBinding != null)
                        {
                            var containerName = responseBinding.GetAttribute<string>("container");
                            if (String.IsNullOrEmpty(containerName))
                            {
                                containerName = "Responses";
                            }

                            if (!containers.ContainsKey(containerName))
                            {
                                throw new InvalidOperationException(String.Format("Could not resolve response container name {0}.", containerName));
                            }

                            var container = containers[containerName];
                            var containerType = (ObjectType)container.ParameterType;

                            DataType responseType = null;
                            var responseName = responseBinding.GetAttribute<string>("property");
                            var documentationConcept = node.Children.FirstOrDefault(c => c.Name == "documentationConcept");
                            if (documentationConcept != null)
                            {
                                var responseDataType = documentationConcept.Children.FirstOrDefault(c => c.Name == "responseDataType");
                                if (responseDataType != null)
                                {
                                    responseType = context.ResolveType(responseDataType.GetAttribute<string>("value"));
                                }
                                else
                                {
                                    responseType = DataTypes.String;
                                }

                                var responseCardinality = documentationConcept.Children.FirstOrDefault(c => c.Name == "responseCardinality");
                                if (responseCardinality != null)
                                {
                                    if (responseCardinality.GetAttribute<string>("value") == "Multiple")
                                    {
                                        responseType = new ListType(responseType);
                                    }
                                }
                            }

                            if (responseType == null)
                            {
                                responseType = DataTypes.String;
                            }

                            if (containerType.Properties.FirstOrDefault(pd => pd.Name == responseName) != null)
                            {
                                throw new InvalidOperationException(String.Format("The response container {0} already has a response named {1}.", container.Name, responseName));
                            }

                            containerType.Properties.Add(new PropertyDef(responseName, responseType));
                        }
                    }
                    break;
                }
            }
            catch (Exception e)
            {
                context.ReportMessage(e, node);
            }

            foreach (var child in node.Children)
            {
                VerifyResponseBindings(context, child, containers);
            }
        }
	}
}
