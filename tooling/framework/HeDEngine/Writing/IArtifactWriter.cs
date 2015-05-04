/*
	HeD Schema Framework
	Copyright (c) 2012 - 2013 Office of the National Coordinator for Health Information Technology (ONC)
	This file is licensed under a modified BSD-license which can be found in the HED_License.txt file included with this distribution.
*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace HeD.Engine.Writing
{
	public interface IArtifactWriter
	{
		void Write(Stream outputStream, object artifact);
		
		string GetExtension();
	}
}
