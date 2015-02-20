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
using HeD.Engine.Translation;

namespace SQL.Translation
{
	public class SQLTranslationContext : TranslationContext
	{
		private Dictionary<String, String> expressionMap = new Dictionary<String, String>();
		private Stack<SQLQueryContext> queryContexts = new Stack<SQLQueryContext>();
		private Stack<String> artifactNames = new Stack<String>();

		public SQLTranslationContext(SQLTranslator translator) : base(translator)
		{
		}

		/// <summary>
		/// Gets the name of the SQL view corresponding to this expression.
		/// </summary>
		/// <param name="expressionName">The artifact qualified name of the expression.</param>
		/// <returns>The name of the SQL view corresponding to this expression.</returns>
		public String GetExpressionObjectName(String expressionName)
		{
			String objectName = null;
			if (!expressionMap.TryGetValue(expressionName, out objectName))
			{
				// TODO: Needs to handle SQL identifier length limits
				objectName = expressionName.Replace(".", "_");
				expressionMap.Add(expressionName, objectName);
			}

			return objectName;
		}

		public void PushQueryContext()
		{
			queryContexts.Push(new SQLQueryContext());
		}

		public void PopQueryContext()
		{
			queryContexts.Pop();
		}

		public bool InQueryContext()
		{
			return CurrentQueryContext() != null;
		}

		public SQLQueryContext CurrentQueryContext()
		{
			return queryContexts.Count > 0 ? queryContexts.Peek() : null;
		}

		public String ArtifactName
		{
			get
			{
				return artifactNames.Peek();
			}
		}

		public void StartArtifact(string artifactName)
		{
			artifactNames.Push(artifactName);
		}

		public void EndArtifact()
		{
			artifactNames.Pop();
		}
	}
}
