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
using HeD.Engine.Verification;

namespace ELM.Model
{
	public class ELMModuleRegistrar : IModuleRegistrar
	{
		#region IModuleRegistrar Members

		public IEnumerable<Operator> Register()
		{
			return
				new Operator[]
				{
                    new Operator("CalculateAge", new Signature(new[] { DataTypes.DateTime }), DataTypes.Integer),
                    new Operator("CalculateAgeAt", new Signature(new[] { DataTypes.DateTime, DataTypes.DateTime }), DataTypes.Integer),
				};
		}

		#endregion
	}
}
