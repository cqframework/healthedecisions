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
	/// <summary>
	/// Defines an interface for performing model transformation for a specific source model type.
	/// </summary>
	public interface IModelTranslator
	{
		/// <summary>
		/// Transforms a model type from the source model to the target model type expected by the translated format.
		/// </summary>
		/// <param name="context">Provides access to the translation context.</param>
		/// <param name="type">The type descriptor of the source model to be transformed.</param>
		/// <returns>A type descriptor defining the type in the target model.</returns>
		ObjectType TransformModel(TranslationContext context, ObjectType sourceType);

		/// <summary>
		/// Transforms a model path reference from the source model to the target model type expected by the translated format.
		/// </summary>
		/// <param name="context">Provides access to the translation context.</param>
		/// <param name="type">The type descriptor for the source model type.</param>
		/// <param name="node">The property expression node for the source artifact.</param>
		/// <param name="path">The path on the source model to be transformed.</param>
		/// <returns>The equivalent expression on the target model type.</returns>
		object TransformModelPath(TranslationContext context, ObjectType sourceType, ASTNode node, string path);
	}
}
