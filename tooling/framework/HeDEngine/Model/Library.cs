﻿/*
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
	/// Represents an HeD accessible library.
	/// </summary>
	/// <remarks>
	/// Note that this is not a direct representation of the schema,
	/// rather it is a representation that is most suited to validation,
	/// translation, and evaluation functionality.
	/// </remarks>
	public class Library
	{
		public string Name { get; set; }

		public string Version { get; set; }

		public IEnumerable<ModelRef> Models { get; set; }

		public IEnumerable<LibraryRef> Libraries { get; set; }

		public IEnumerable<ParameterDef> Parameters { get; set; }

		public IEnumerable<ExpressionDef> Expressions { get; set; }

        public IEnumerable<FunctionDef> Functions { get; set; }

        public IEnumerable<CodeSystemDef> CodeSystems { get; set; }

        public IEnumerable<ValueSetDef> ValueSets { get; set; }
	}
}
