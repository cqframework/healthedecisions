/*
	HeD Schema Framework
	Copyright (c) 2012 - 2013 Office of the National Coordinator for Health Information Technology (ONC)
	This file is licensed under a modified BSD-license which can be found in the HED_License.txt file included with this distribution.
*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.Xml.Schema;
using System.Xml.Serialization;
using HeD.Engine.Model;
using HeD.Model;

namespace HeD.Engine.Reading
{
	public static class ArtifactReader
	{
		public static Artifact Read(XDocument artifact, XmlSchemaSet schemas, string sourcePath)
		{
			artifact.Validate(schemas, onValidationEvent, true);

			if (artifact.Root.GetSchemaInfo().Validity != XmlSchemaValidity.Valid)
			{
				throw new InvalidOperationException("Artifact does not conform to the schema.");
			}

			var result = new Artifact();

			var ns = artifact.Root.GetDefaultNamespace();
			var elmns = XNamespace.Get("urn:hl7-org:elm:r1");

			var metaData = artifact.Root.Element(ns + "metadata");

			if (metaData != null)
			{
				result.MetaData = NodeReader.ReadNode(metaData);
			}

			// Pull models
			var dataModelsElement = artifact.Root.Element(ns + "metadata").Element(ns + "dataModels");
			if (dataModelsElement != null)
			{
				result.Models = ReadModels(ns, dataModelsElement.Elements(ns + "modelReference")).ToList();
			}
			else
			{
				result.Models = new List<ModelRef>();
			}

			// TODO: Add default model of HeDSchema?

			// Pull libraries
			var librariesElement = artifact.Root.Element(ns + "metadata").Element(ns + "libraries");
			if (librariesElement != null)
			{
				result.Libraries = ReadLibraries(ns, librariesElement.Elements(ns + "libraryReference"), sourcePath).ToList();
			}
			else
			{
				result.Libraries = new List<LibraryRef>();
			}

			var externalData = artifact.Root.Element(ns + "externalData");

			if (externalData != null)
			{
				// Pull Codesystems
				result.CodeSystems = ReadCodeSystems(ns, externalData.Elements(ns + "codesystem")).ToList();
				
				// Pull valuesets
				result.ValueSets = ReadValueSets(ns, externalData.Elements(ns + "valueset")).ToList();

				// Pull parameters
				result.Parameters = ReadParameters(ns, elmns, externalData.Elements(ns + "parameter")).ToList();

				// Pull external defs
				var externalDefs = ReadExpressionDefs(ns, elmns, externalData.Elements(ns + "def"));

				// Pull triggers
				var triggers = ReadTriggers(ns, elmns, externalData.Elements(ns + "trigger"));

                result.Expressions = externalDefs.Concat(triggers).ToList();
			}
			else
			{
				result.Parameters = new List<ParameterDef>();
				result.Expressions = new List<ExpressionDef>();
			}

			// Pull expressions
            var expressionsElement = artifact.Root.Element(ns + "expressions");
            if (expressionsElement != null)
            {
			    var expressions = ReadExpressionDefs(ns, elmns, expressionsElement.Elements(ns + "def"));

                result.Expressions = result.Expressions.Concat(expressions).ToList();
            }

			// Pull conditions
			var conditionsElement = artifact.Root.Element(ns + "conditions");
			if (conditionsElement != null)
			{
				result.Conditions = ReadConditions(ns, elmns, conditionsElement.Elements(ns + "condition")).ToList();
			}
			else
			{
				result.Conditions = new List<ASTNode>();
			}

			// Pull actions
			var actionGroupElement = artifact.Root.Element(ns + "actionGroup");
			if (actionGroupElement != null)
			{
				result.ActionGroup = NodeReader.ReadNode(actionGroupElement);
			}

			return result;
		}

		private static void onValidationEvent(object sender, ValidationEventArgs e)
		{
			if (e.Exception != null)
			{
                throw new InvalidOperationException(String.Format("{0},{1}: {2}", e.Exception.LineNumber, e.Exception.LinePosition, e.Message), e.Exception);
			}
			else
			{
				throw new InvalidOperationException(String.Format("Message: {0}\r\nSeverity: {1}\r\n", e.Message, e.Severity));
			}
		}

		private static IEnumerable<ModelRef> ReadModels(XNamespace ns, IEnumerable<XElement> models)
		{
			return
				from model in models
					select new ModelRef { Uri = model.Element(ns + "referencedModel").Attribute("value").Value };
		}

		private static string NormalizePath(string sourcePath, string libraryPath)
		{
			if (!Path.IsPathRooted(libraryPath))
			{
				return Path.Combine(sourcePath, libraryPath);
			}

			return libraryPath;
		}

		private static IEnumerable<LibraryRef> ReadLibraries(XNamespace ns, IEnumerable<XElement> libraries, string sourcePath)
		{
			return
				from library in libraries
					select 
						new LibraryRef 
						{ 
							Name = library.Attribute("name").Value, 
							MediaType = library.Attribute("mediaType").GetValue() ?? "application/hed+xml",
							Path = NormalizePath(sourcePath, library.Attribute("path").Value),
							Version = library.Attribute("version").GetValue()
						};
		}

		private static IEnumerable<HeD.Engine.Model.CodeSystemDef> ReadCodeSystems(XNamespace ns, IEnumerable<XElement> codesystems)
		{
			return
				from codesystem in codesystems
					let idAttribute = codesystem.Attribute("id")
					let versionAttribute = codesystem.Attribute("version")
					select
						new HeD.Engine.Model.CodeSystemDef
						{
							Name = codesystem.Attribute("name").Value,
							Id = idAttribute == null ? null : idAttribute.Value,
							Version = versionAttribute == null ? null : versionAttribute.Value
						};
		}

        private static IEnumerable<HeD.Engine.Model.ValueSetDef> ReadValueSets(XNamespace ns, IEnumerable<XElement> valuesets)
        {
            return
                from valueset in valuesets
                    let idAttribute = valueset.Attribute("id")
                    let versionAttribute = valueset.Attribute("version")
                    select 
                        new HeD.Engine.Model.ValueSetDef 
                        { 
                            Name = valueset.Attribute("name").Value, 
                            Id = idAttribute == null ? null : idAttribute.Value, 
                            Version = versionAttribute == null ? null : versionAttribute.Value
				            // TODO: Valueset CodeSystems
                        };
        }

		private static IEnumerable<ParameterDef> ReadParameters(XNamespace ns, XNamespace elmns, IEnumerable<XElement> parameters)
		{
			return
				from parameter in parameters
					let defaultNode = parameter.Element(elmns + "expression")
					select new ParameterDef { Name = parameter.Attribute("name").Value, TypeName = parameter.ExpandName(parameter.Attribute("parameterType").Value), Default = defaultNode != null ? NodeReader.ReadASTNode(defaultNode) : null };
		}

		private static IEnumerable<ExpressionDef> ReadExpressionDefs(XNamespace ns, XNamespace elmns, IEnumerable<XElement> expressionDefs)
		{
			return
				from expressionDef in expressionDefs
					select new ExpressionDef { Name = expressionDef.Attribute("name").Value, Expression = NodeReader.ReadASTNode(expressionDef.Element(elmns + "expression")) };
		}

		private static IEnumerable<ASTNode> ReadConditions(XNamespace ns, XNamespace elmns, IEnumerable<XElement> conditions)
		{
			return
				from condition in conditions
					select NodeReader.ReadASTNode(condition.Element(ns + "logic"));
		}

		private static IEnumerable<ExpressionDef> ReadTriggers(XNamespace ns, XNamespace elmns, IEnumerable<XElement> triggers)
		{
			return
				from trigger in triggers
					let triggerDef = trigger.Element(ns + "def")
					select new ExpressionDef { Name = triggerDef.Attribute("name").Value, Expression = NodeReader.ReadASTNode(triggerDef.Element(elmns + "expression")) };
		}
	}
}
