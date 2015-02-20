/*
	HeD Schema Framework
	Copyright (c) 2012 - 2013 Office of the National Coordinator for Health Information Technology (ONC)
	This file is licensed under a modified BSD-license which can be found in the HED_License.txt file included with this distribution.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using HeD.Engine.Model;
using HeD.Engine.Verification;

namespace vMR.Model
{
	public class vMRModuleRegistrar : IModuleRegistrar
	{
		#region IModuleRegistrar Members

		public IEnumerable<Operator> Register()
		{
			return
				new Operator[]
				{
				};
		}

		#endregion
	}
}
