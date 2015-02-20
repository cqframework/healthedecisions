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
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml.Schema;

using HeD.Engine.Model;
using HeD.Engine.Reading;
using HeD.Engine.Translation;
using HeD.Engine.Verification;

namespace HeDArtifactUtility
{
	class Program
	{
		static void Main(string[] args)
		{
			var utility = new ArtifactUtility();
			utility.OnMessage += utility_OnMessage;

			if (CommandLine.Parser.ParseArgumentsWithUsage(args, utility))
			{
				utility.Run();
			}

			Console.ReadLine();
		}

		static void utility_OnMessage(string message)
		{
			Console.WriteLine(message);
		}
	}
}
