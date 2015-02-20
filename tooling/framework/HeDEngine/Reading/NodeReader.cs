/*
	HeD Schema Framework
	Copyright (c) 2012 - 2013 Office of the National Coordinator for Health Information Technology (ONC)
	This file is licensed under a modified BSD-license which can be found in the HED_License.txt file included with this distribution.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;
using HeD.Engine.Model;

namespace HeD.Engine.Reading
{
	public static class NodeReader
	{
		private static bool IsExpression(XmlSchemaType schemaType)
		{
			if (schemaType.Name == "Expression")
			{
				return true;
			}

			if (schemaType.BaseXmlSchemaType != null)
			{
				return IsExpression(schemaType.BaseXmlSchemaType);
			}

			return false;
		}

		public static Node ReadNode(XElement element)
		{
			var nodeName = element.Name.LocalName;
			var nodeType = element.GetSchemaInfo().SchemaType;

			Node result = null;
			ASTNode astResult = null;

			if (IsExpression(nodeType))
			{
				astResult = new ASTNode();
				result = astResult;
			}
			else
			{
				result = new Node();
			}

			result.Name = nodeName;
			result.NodeType = nodeType.QualifiedName.ToString();
			var lineInfo = element as IXmlLineInfo;
			if (lineInfo != null && lineInfo.HasLineInfo())
			{
				result.Line = lineInfo.LineNumber;
				result.LinePos = lineInfo.LinePosition;
			}

			result.Attributes = new Dictionary<string, object>();
			foreach (var a in element.Attributes())
			{
				var schemaInfo = a.GetSchemaInfo();
				if (schemaInfo != null && schemaInfo.SchemaType != null && schemaInfo.SchemaType.QualifiedName != null && schemaInfo.SchemaType.QualifiedName.Name == "QName")
				{
					result.Attributes.Add(a.Name.LocalName, element.ExpandName(a.Value));
				}
				else
				{
					result.Attributes.Add(a.Name.LocalName, a.Value);
				}
			}

			result.Children = new List<Node>();
			foreach (var e in element.Elements())
			{
				if (e.Name.LocalName == "description" && astResult != null)
				{
					astResult.Description = e.Value;
				}
				else
				{
					result.Children.Add(ReadNode(e));
				}
			}

			return result;
		}

		public static ASTNode ReadASTNode(XElement element)
		{
			var result = ReadNode(element) as ASTNode;

			if (result == null)
			{
				throw new InvalidOperationException("Expected an expression node.");
			}

			return result;
		}
	}
}
