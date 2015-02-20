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
	/// Defines the generic interface for translation support within the HeD Schema Framework.
	/// </summary>
	public interface IArtifactTranslator
	{
		/// <summary>
		/// Translates the given HeD Artifact to the target format.
		/// </summary>
		object Translate(Artifact source);
	}

	/// <summary>
	/// Defines the typed interface for translation support within the HeD Schema Framework.
	/// </summary>
	/// <typeparam name="TTarget">The type of the target format.</typeparam>
	public interface IArtifactTranslator<TTarget>
	{
		/// <summary>
		/// Translates the given HeD Artifact to the target format.
		/// </summary>
		/// <param name="source"></param>
		/// <returns></returns>
		TTarget Translate(Artifact source);
	}
}
