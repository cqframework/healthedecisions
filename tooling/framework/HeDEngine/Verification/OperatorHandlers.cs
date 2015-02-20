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

namespace HeD.Engine.Verification
{
	public class EqualOperatorHandler : IOperatorHandler
	{
		#region IOperatorHandler Members

		public IEnumerable<Signature> RegisterSignatures()
		{
			yield return new Signature(new DataType[] { DataTypes.Integer, DataTypes.Integer });
			yield return new Signature(new DataType[] { DataTypes.Real, DataTypes.Real });
			yield return new Signature(new DataType[] { DataTypes.String, DataTypes.String });
			yield return new Signature(new DataType[] { DataTypes.Timestamp, DataTypes.Timestamp });
		}

		#endregion
	}

}
