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
	/// Represents a parameter definition within an HeD Schema Artifact.
	/// </summary>
	public class ParameterDef
	{
		public string Name { get; set; }

		public string TypeName { get; set; }

		public DataType ParameterType { get; set; }

		public ASTNode Default { get; set; }
	}
}
