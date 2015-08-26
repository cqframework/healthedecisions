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
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml.Schema;

using CommandLine;

using HeD.Engine.Model;
using HeD.Engine.Reading;
using HeD.Engine.Translation;
using HeD.Engine.Verification;
using HeD.Engine.Writing;

namespace HeDArtifactUtility
{
	public delegate void MessageEvent(string message);

	public class ArtifactUtility
	{
		[DefaultArgument(ArgumentType.AtMostOnce | ArgumentType.Required, HelpText = "The name of the file name containing the input artifact.", LongName = "InputFileName")]
		public string InputFileName;

		[Argument(ArgumentType.AtMostOnce, HelpText = "The target format. If present, the utility will attempt to convert the input file to the given format. Formats are registered in the TranslationHandlerMap.xml file.", LongName = "TargetFormat", ShortName = "t")]
		public string TargetFormat;

		[Argument(ArgumentType.AtMostOnce, HelpText = "The output file name. If not defined, and a translation is requested, the output file name will be the input file name with the target format appended.", LongName = "OutputFileName", ShortName = "o")]
		public string OutputFileName;

		public event MessageEvent OnMessage;

		private void DoMessage(string message)
		{
			if (OnMessage != null)
			{
				OnMessage(message);
			}
		}

		public void Run()
		{
			// Verifies the xml document specified as the argument contains a valid HeD Schema artifact
			// Performs XSD validation, as well as expression-level semantic validation

			DoMessage("Loading catalog modules...");

			try
			{
				LoadMaps();
				RegisterModules();

				using (var artifactFile = File.Open(InputFileName, FileMode.Open))
				{
					DoMessage("Loading artifact document...");
				
					var artifactDocument = XDocument.Load(artifactFile, LoadOptions.SetLineInfo);

					DoMessage("Verifying artifact schema...");

					var schemaSet = new XmlSchemaSet();
					foreach (var schemaLocation in UtilitySettings.Default.SchemaLocation.Split(';', ',', ' '))
					{
						schemaSet.Add(null, schemaLocation);
					}
					//schemaSet.Compile();

					var artifact = ArtifactReader.Read(artifactDocument, schemaSet, Path.GetDirectoryName(InputFileName));

					DoMessage("Verifying artifact semantics...");

					var artifactVerifier = new ArtifactVerifier();
					var messages = artifactVerifier.Verify(artifact);
                    bool isValid = true;
                    foreach (var message in messages)
                    {
                        DoMessage(message.Message);
                        if (!message.IsWarning)
                        {
                            isValid = false;
                        }
                    }

                    if (isValid)
                    {
					    DoMessage("Artifact is valid.");

					    if (!String.IsNullOrEmpty(TargetFormat))
					    {
						    DoMessage(String.Format("Translating artifact to {0} format.", TargetFormat));

						    var translator = ArtifactTranslatorFactory.GetHandler(TargetFormat);

						    var translatedArtifact = translator.Translate(artifact);

						    var writer = ArtifactWriterFactory.GetHandler(TargetFormat);

						    var outputFileName = GetOutputFileName(writer.GetExtension());
						    using (var outputFile = File.Open(outputFileName, FileMode.Create))
						    {
							    writer.Write(outputFile, translatedArtifact);
						    }

						    DoMessage(String.Format("Artifact translated to output file: {0}", outputFileName));
					    }
                    }
				}
			}
			catch (Exception ex)
			{
				DoMessage(ex.Message);
			}
		}

		private string GetOutputFileName(string targetExtension)
		{
			if (String.IsNullOrEmpty(OutputFileName))
			{
				OutputFileName = String.Format("{0}.{1}{2}", Path.GetFileNameWithoutExtension(InputFileName), TargetFormat, targetExtension);
			}

			return OutputFileName;
		}

		private static void LoadMaps()
		{
			foreach (var fileName in Directory.GetFiles(UtilitySettings.Default.MapLocation, "*.xml"))
			{
				using (var mapFile = File.Open(fileName, FileMode.Open))
				{
					var mapDocument = XDocument.Load(mapFile);
					switch (MapReader.ReadHandlerType(mapDocument))
					{
						case HandlerType.ModuleRegistration : ModuleRegistrarFactory.LoadMap(MapReader.ReadMap(mapDocument)); break;
						case HandlerType.TypeResolution : TypeResolverFactory.LoadMap(MapReader.ReadMap(mapDocument)); break;
						case HandlerType.LibraryReader : LibraryReaderFactory.LoadMap(MapReader.ReadMap(mapDocument)); break;
						case HandlerType.Verification : NodeVerifierFactory.LoadMap(MapReader.ReadMap(mapDocument)); break;
						case HandlerType.Translation : ArtifactTranslatorFactory.LoadMap(MapReader.ReadMap(mapDocument)); break;
						case HandlerType.NodeTranslation : NodeTranslatorFactory.LoadMap(MapReader.ReadMap(mapDocument)); break;
						case HandlerType.ModelTranslation : ModelTranslatorFactory.LoadMap(MapReader.ReadMap(mapDocument)); break;
						case HandlerType.Writing : ArtifactWriterFactory.LoadMap(MapReader.ReadMap(mapDocument)); break;
					}
				}
			}
		}

		private static void RegisterModules()
		{
			foreach (var moduleRegistrar in ModuleRegistrarFactory.GetHandlers())
			{
				foreach (var o in moduleRegistrar.Register())
				{
					OperatorMap.Core.RegisterOperator(o);
				}
			}
		}
	}
}
