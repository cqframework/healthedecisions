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
	/// Represents a symbol used during validation and/or compilation to track names that are available within the current scope.
	/// </summary>
	public class Symbol
	{
		public Symbol(string name, DataType dataType)
		{
			if (string.IsNullOrEmpty(name))
			{
				throw new ArgumentNullException("name");
			}

			if (dataType == null)
			{
				throw new ArgumentNullException("dataType");
			}

			_name = name;
			_dataType = dataType;
		}

		private string _name;
		public string Name { get { return _name; } }

		private DataType _dataType;
		public DataType DataType { get { return _dataType; } }
	}
}
