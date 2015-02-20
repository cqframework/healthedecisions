/*
	HeD Schema Framework
	Copyright (c) 2012 - 2013 Office of the National Coordinator for Health Information Technology (ONC)
	This file is licensed under a modified BSD-license which can be found in the HED_License.txt file included with this distribution.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HeD.Engine.Model
{
	/// <summary>
	/// Defines the known handler types in the HeD Schema Framework.
	/// </summary>
	/// <remarks>
	/// Each of these types is associated with an interface that defines
	/// the behavior expected to be implemented by each handler class of 
	/// that type.
	/// </remarks>
	public enum HandlerType
	{
		/// <summary>
		/// A handler type that provides module registration functionality.
		/// </summary>
		/// <remarks>
		/// Module registration handlers are loaded as part of initialization of the engine.
		/// </remarks>
		ModuleRegistration,

		/// <summary>
		/// A handler type that provides type resolution functionality.
		/// </summary>
		/// <remarks>
		/// Type resolvers are used to determine the logical type for a given type name within an HeD Schema Artifact.
		/// </remarks>
		TypeResolution,

		/// <summary>
		/// A handler type that provides verification functionality.
		/// </summary>
		/// <remarks>
		/// Verification handlers are used to determine whether or not a given expression within an HeD Schema Artifact is type safe.
		/// </remarks>
		Verification,

		/// <summary>
		/// A handler type that provides execution functionality.
		/// </summary>
		/// <remarks>
		/// Evaluation handlers are used to provide evaluation services for a given type of expression.
		/// </remarks>
		Evaluation,

		/// <summary>
		/// A handler type that provides artifact translation functionality.
		/// </summary>
		/// <remarks>
		/// Artifact translation handlers are used to perform translation to a given target format.
		/// </remarks>
		Translation,

		/// <summary>
		/// A handler type that provides node translation functionality.
		/// </summary>
		/// <remarks>
		/// Node translation handlers are used to perform translation for a given type of expression.
		/// </remarks>
		NodeTranslation,

		/// <summary>
		/// A handler type that provides model transformation functionality.
		/// </summary>
		/// <remarks>
		/// Model translation handlers are used to transform model type from source to target, as well as path expressions on those types.
		/// </remarks>
		ModelTranslation,

		/// <summary>
		/// A handler type that provides artifact serialization functionality.
		/// </summary>
		Writing
	}
}
