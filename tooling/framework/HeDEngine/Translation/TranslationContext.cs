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

namespace HeD.Engine.Translation
{
	public class TranslationContext
	{
		public TranslationContext(IArtifactTranslator translator)
		{
			if (translator == null)
			{
				throw new ArgumentNullException("translator");
			}

			_translator = translator;
		}

		private IArtifactTranslator _translator;
		public IArtifactTranslator Translator { get { return _translator; } }

		public object TranslateNode(ASTNode node)
		{
			var translator = NodeTranslatorFactory.GetHandler(node.NodeType);

			return translator.Translate(this, node);
		}

		public ObjectType TransformModelType(ObjectType type)
		{
			var translator = ModelTranslatorFactory.GetHandler(type.Name);

			return translator.TransformModel(this, type);
		}

		public object TransformModelPath(ObjectType type, ASTNode node, string path)
		{
			var translator = ModelTranslatorFactory.GetHandler(type.Name);

			return translator.TransformModelPath(this, type, node, path);
		}
	}
}
