/*
	HeD Schema Framework
	Copyright (c) 2012 - 2014 Office of the National Coordinator for Health Information Technology (ONC)
	This file is licensed under a modified BSD-license which can be found in the HED_License.txt file included with this distribution.
*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Alphora.Dataphor.DAE.Language.TSQL;
using HeD.Engine.Writing;

using SQLModel = Alphora.Dataphor.DAE.Language.SQL;

namespace SQL.Translation
{
	public class SQLWriter : IArtifactWriter
	{
		#region IArtifactWriter Members

		public String GetExtension()
		{
			return ".sql";
		}

		public void Write(Stream outputStream, object artifact)
		{
			SQLModel.Batch batch = artifact as SQLModel.Batch;
			if (artifact == null)
			{
				throw new ArgumentException("SQL Artifact Writer expects an object of type SQLModel.Batch");
			}

			// Emit using TSQLTextEmitter, though in theory we could emit using any emitter, the translator would have to be aware of the dialectic differences as well.
			// Easiest solution would probably be a translator per language type.
			var emitter = new TSQLTextEmitter();
			emitter.UseQuotedIdentifiers = true;

			using (var sw = new StreamWriter(outputStream))
			{
				sw.Write(emitter.Emit(batch));
			}
		}

		#endregion
	}
}
