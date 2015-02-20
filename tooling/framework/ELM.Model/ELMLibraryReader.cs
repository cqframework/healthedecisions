using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml.Schema;
using HeD.Engine.Reading;

namespace ELM.Model
{
	public class ELMLibraryReader : ILibraryReader
	{
		public HeD.Engine.Model.Library Read(HeD.Engine.Model.LibraryRef libraryRef)
		{
			DoMessage("Loading library document...");
				
			var libraryDocument = XDocument.Load(libraryRef.Path, LoadOptions.SetLineInfo);

			DoMessage("Verifying artifact schema...");

			var schemaSet = new XmlSchemaSet();
			foreach (var schemaLocation in ELMLibrarySettings.Default.SchemaLocation.Split(';', ',', ' '))
			{
				schemaSet.Add(null, schemaLocation);
			}

			libraryDocument.Validate(schemaSet, onValidationEvent, true);

			if (libraryDocument.Root.GetSchemaInfo().Validity != XmlSchemaValidity.Valid)
			{
				throw new InvalidOperationException("Library does not conform to the schema.");
			}

			var result = new HeD.Engine.Model.Library();

			// TODO: Read library nodes...

			result.Name = libraryRef.Name;
			//result.Version;

			var ns = libraryDocument.Root.GetDefaultNamespace();

			// Pull models
			var dataModelsElement = libraryDocument.Root.Element(ns + "usings");
			if (dataModelsElement != null)
			{
				result.Models = ReadModels(ns, dataModelsElement.Elements(ns + "def")).ToList();
			}
			else
			{
				result.Models = new List<HeD.Engine.Model.ModelRef>();
			}

			// Pull libraries
			var librariesElement = libraryDocument.Root.Element(ns + "includes");
			if (librariesElement != null)
			{
				result.Libraries = ReadLibraries(ns, librariesElement.Elements(ns + "def")).ToList();
			}
			else
			{
				result.Libraries = new List<HeD.Engine.Model.LibraryRef>();
			}

            // TODO: Codesystems
            result.CodeSystems = new List<HeD.Engine.Model.CodeSystemDef>();

			// Pull Valuesets
            var valuesetsElement = libraryDocument.Root.Element(ns + "valueSets");
            if (valuesetsElement != null)
            {
                result.ValueSets = ReadValueSets(ns, valuesetsElement.Elements(ns + "def")).ToList();
            }
            else
            {
                result.ValueSets = new List<HeD.Engine.Model.ValueSetDef>();
            }

			// Pull parameters
			var parametersElement = libraryDocument.Root.Element(ns + "parameters");
			if (parametersElement != null)
			{
				result.Parameters = ReadParameters(ns, parametersElement.Elements(ns + "def")).ToList();
			}
			else
			{
				result.Parameters = new List<HeD.Engine.Model.ParameterDef>();
			}

            // Pull expression defs
			var statementsElement = libraryDocument.Root.Element(ns + "statements");
			if (statementsElement != null)
			{
                // TODO: FunctionDefs
				result.Expressions = ReadExpressionDefs(ns, statementsElement.Elements(ns + "def")).ToList();
			}
			else
			{
				result.Expressions = new List<HeD.Engine.Model.ExpressionDef>();
			}

			return result;
		}

		private void DoMessage(string message)
		{
			// TODO: Report progress
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

		private static IEnumerable<HeD.Engine.Model.ModelRef> ReadModels(XNamespace ns, IEnumerable<XElement> models)
		{
			return
				from model in models
					select new HeD.Engine.Model.ModelRef { Uri = model.Attribute("uri").Value };
		}

		private static IEnumerable<HeD.Engine.Model.LibraryRef> ReadLibraries(XNamespace ns, IEnumerable<XElement> libraries)
		{
			return
				from library in libraries
					select 
						new HeD.Engine.Model.LibraryRef 
						{ 
							Name = library.Attribute("localIdentifier").Value, 
							MediaType = library.Attribute("mediaType").Value,
							Path = library.Attribute("path").Value,
							Version = library.Attribute("version").Value
						};
		}

        // TODO: Parameters with type specifiers...
		private static IEnumerable<HeD.Engine.Model.ParameterDef> ReadParameters(XNamespace ns, IEnumerable<XElement> parameters)
		{
			return
				from parameter in parameters
					let defaultNode = parameter.Element(ns + "expression")
					select new HeD.Engine.Model.ParameterDef { Name = parameter.Attribute("name").Value, TypeName = parameter.ExpandName(parameter.Attribute("parameterType").Value), Default = defaultNode != null ? NodeReader.ReadASTNode(defaultNode) : null };
		}

        private static IEnumerable<HeD.Engine.Model.ValueSetDef> ReadValueSets(XNamespace ns, IEnumerable<XElement> valuesets)
        {
            // TODO: Valueset CodeSystems
            return
                from valueset in valuesets
                    let idAttribute = valueset.Attribute("id")
                    let versionAttribute = valueset.Attribute("version")
                    select 
                        new HeD.Engine.Model.ValueSetDef 
                        { 
                            Name = valueset.Attribute("name").Value, 
                            Id = idAttribute == null ? null : valueset.Attribute("id").Value, 
                            Version = versionAttribute == null ? null : valueset.Attribute("version").Value 
                        };
        }

		private static IEnumerable<HeD.Engine.Model.ExpressionDef> ReadExpressionDefs(XNamespace ns, IEnumerable<XElement> expressionDefs)
		{
			// TODO: FunctionDefs....
			return
				from expressionDef in expressionDefs
					select new HeD.Engine.Model.ExpressionDef { Name = expressionDef.Attribute("name").Value, Expression = NodeReader.ReadASTNode(expressionDef.Element(ns + "expression")) };
		}
	}
}
