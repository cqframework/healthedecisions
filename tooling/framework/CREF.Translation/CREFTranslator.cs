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

using CREFModel = CREF.Model;
using AllscriptsModel = Allscripts.Model;

namespace CREF.Translation
{
	public class CREFTranslator : IArtifactTranslator
	{
		#region IArtifactTranslator Members

		public object Translate(Artifact source)
		{
			var result = new CREFModel.DynamicRule();

			var context = new TranslationContext(this);

			// Metadata
			var identifier = source.MetaData.Children.First(c => c.Name == "identifiers").Children.First(c => c.Name == "identifier");
			result.Name = identifier.GetAttribute<string>("root");
			result.Description = source.MetaData.Children.First(c => c.Name == "title").GetAttribute<string>("value");
			result.SeverityID = "MED"; // TODO: HeD does not have an artifact level severity indicator.

			// Named Expressions
			result.DynamicRuleNamedExpressions = 
				(
					from expression in source.Expressions
						select TranslateNamedExpression(context, expression)
				).ToList();

			// Criteria
			result.DynamicRuleCriteria = TranslateCriteria(context, source.Conditions.First());

			// TODO: Assertion
			result.DynamicRuleAssertion = TranslateAssertion(context, source.ActionGroup);

			return result;
		}

		#endregion

		private CREFModel.DynamicRuleDynamicRuleCriteria TranslateCriteria(TranslationContext context, ASTNode condition)
		{
			var result = new CREFModel.DynamicRuleDynamicRuleCriteria();

			result.Item = context.TranslateNode(condition);

			return result;
		}

		private CREFModel.NamedExpression TranslateNamedExpression(TranslationContext context, ExpressionDef expression)
		{
			var result = new CREFModel.NamedExpression();

			result.Name = expression.Name;
			result.Item = context.TranslateNode(expression.Expression);

			return result;
		}

		private CREFModel.DynamicRuleDynamicRuleAssertion TranslateAssertion(TranslationContext context, Node node)
		{
			var result = new CREFModel.DynamicRuleDynamicRuleAssertion();

			result.Item = TranslateClinicalAssertion(context, node);

			return result;
		}

		private CREFModel.ClinicalAssertion TranslateClinicalAssertion(TranslationContext context, Node node)
		{
			// node is expected to be an ActionGroup
			// Action types determine the assertion to be created
				// CreateAction - MissingDataAssertion
				// UpdateAction - MissingDataAssertion
				// MessageAction - OutOfRangeAssertion
				// CollectInformationAction - MissingDataAssertion
				// Any other action cannot be represented

			var assertions = new List<CREFModel.ClinicalAssertion>();

			var subElements = node.Children.FirstOrDefault(c => c.Name == "subElements");

			if (subElements != null)
			{
				foreach (var element in subElements.Children)
				{
					if (element.Name == "simpleAction")
					{
						switch (element.NodeType.GetLocalName())
						{
							case "CreateAction" : assertions.Add(TranslateCreateAction(context, element)); break;
							case "UpdateAction" : assertions.Add(TranslateUpdateAction(context, element)); break;
							case "MessageAction" : assertions.Add(TranslateMessageAction(context, element)); break;
							case "CollectInformationAction" : assertions.Add(TranslateCollectInformationAction(context, element)); break;
							default: throw new NotSupportedException(String.Format("Translation of {0} actions is not supported.", element.NodeType.GetLocalName()));
						}
					}
					else if (element.Name == "actionGroup")
					{
						assertions.Add(TranslateClinicalAssertion(context, element));
					}
					else if (element.Name == "actionGroupReference")
					{
						throw new NotSupportedException("Translation of action group references is not supported.");
					}
				}
			}

			if (assertions.Count > 1)
			{
				var compositeAssertion = new CREFModel.CompositeAssertion();

				compositeAssertion.CompositionType = GetCompositionType(GetGroupSelectionBehavior(node));

				var descriptionNode = node.Children.FirstOrDefault(c => c.Name == "Description");
				if (descriptionNode != null)
				{
					compositeAssertion.Description = descriptionNode.GetAttribute<string>("value");
				}

				compositeAssertion.CompositeAssertionAssertions.AddRange(assertions);

				return compositeAssertion;
			}

			return assertions[0];
		}

		private void TranslateBaseClinicalAssertion(TranslationContext context, Node node, CREFModel.ClinicalAssertion assertion)
		{
			var actionIdNode = node.Children.FirstOrDefault(c => c.Name == "actionId");
			if (actionIdNode != null)
			{
				assertion.ID = actionIdNode.GetAttribute<string>("root") + ':' + actionIdNode.GetAttribute<string>("extension", "");
			}

			var textEquivalentNode = node.Children.FirstOrDefault(c => c.Name == "textEquivalent");
			if (textEquivalentNode != null)
			{
				assertion.Description = textEquivalentNode.GetAttribute<string>("value");
			}
		}

		private CREFModel.ClinicalAssertion TranslateCollectInformationAction(TranslationContext context, Node node)
		{
			var result = new CREFModel.MissingDataAssertion();
			TranslateBaseClinicalAssertion(context, node, result);

			var documentationConcept = node.Children.FirstOrDefault(c => c.Name == "documentationConcept");
			if (documentationConcept != null)
			{
				var itemCodes = documentationConcept.Children.FirstOrDefault(c => c.Name == "itemCodes");
				if (itemCodes != null)
				{
					var itemCode = itemCodes.Children.FirstOrDefault(c => c.Name == "itemCode");
					if (itemCode != null)
					{
						result.Code = itemCode.GetAttribute<string>("code");
						result.CodeSet = itemCode.GetAttribute<string>("codeSystemName");
					}
				}
			}

			return result;
		}

		private CREFModel.ClinicalAssertion TranslateMessageAction(TranslationContext context, Node node)
		{
			var result = new CREFModel.OutOfRangeAssertion();
			TranslateBaseClinicalAssertion(context, node, result);

			return result;
		}

		private void TranslateActionSentence(TranslationContext context, Node node, CREFModel.MissingDataAssertion result)
		{
			// If we assert that the source artifact can only use specific types of expressions, and that the expressions must be literal (evaluable at compile-time)
			// then we can translate the basic aspects of the assertion, code, code system, and severity.
			var proposalExpression = node.Children.FirstOrDefault(c => c.Name == "actionSentence") as ASTNode;
			if (proposalExpression != null)
			{
				switch (proposalExpression.ResultType.Name)
				{
					case "SubstanceAdministrationProposal" :
					case "SubstanceAdministrationEvent" :
					{
						var substanceCode = ((Node)proposalExpression).Children.FirstOrDefault(c => c.Name == "property" && c.Attributes.ContainsKey("name") && c.GetAttribute<string>("name") == "substance.substanceCode");
						if (substanceCode != null)
						{
							var codeValue = substanceCode.Children.FirstOrDefault(c => c.Name == "value" && c.NodeType.GetLocalName() == "CodeLiteral");
							if (codeValue != null)
							{
								result.Code = codeValue.GetAttribute<string>("code");
								result.CodeSet = codeValue.GetAttribute<string>("codeSystem");
							}
						}
					}
					break;
				}
			}
		}

		private CREFModel.ClinicalAssertion TranslateCreateAction(TranslationContext context, Node node)
		{
			var result = new CREFModel.MissingDataAssertion();
			TranslateBaseClinicalAssertion(context, node, result);

			// TODO: Translate the action sentence
				// In general, this will be extremely difficult because it involves figuring out what object type and code
				// is created as part of the resulting guidance, and then using that to set the code on the recommendation.
				// However, it is possible in HeD to actually set the code based on data provided as part of the evaluation.
				// In this case, it is not possible to determine the code to be returned. Manual translation would have to
				// be performed in that case.

				// Another option for translation is to evaluate the action and then just read the code from the appropriate 
				// property on the resulting object.

			TranslateActionSentence(context, node, result);

			return result;
		}

		private CREFModel.ClinicalAssertion TranslateUpdateAction(TranslationContext context, Node node)
		{
			var result = new CREFModel.MissingDataAssertion();
			TranslateBaseClinicalAssertion(context, node, result);

			// TODO: Translate the action sentence
				// See the related comment in TranslateCreateAction

			TranslateActionSentence(context, node, result);

			return result;
		}

		private CREFModel.CompositionType GetCompositionType(string groupSelectionBehavior)
		{
			// GroupSelectionBehavior determines what the composing operator should be
				// All - And
				// AllOrNone - And
				// Any - Or
				// AtMostOne - Or
				// ExactlyOne - Or
				// OneOrMore - Or

			switch (groupSelectionBehavior)
			{
				case "All" :
				case "AllOrNone" : return CREFModel.CompositionType.ALL;
				case "Any" :
				case "AtMostOne" :
				case "ExactlyOne" :
				case "OneOrMore" : return CREFModel.CompositionType.ANY;
				default : throw new NotSupportedException(String.Format("Unknown group selection behavior: {0}", groupSelectionBehavior));
			}
		}

		private string GetGroupSelectionBehavior(Node node)
		{
			var behaviorsNode = node.Children.FirstOrDefault(c => c.Name == "behaviors");
			if (behaviorsNode != null)
			{
				var behaviorNode = node.Children.FirstOrDefault(c => c.Name == "behavior" && c.NodeType.GetLocalName() == "GroupSelectionBehavior");
				if (behaviorNode != null)
				{
					return node.GetAttribute<string>("value");
				}
			}

			return "All";
		}
	}
}
