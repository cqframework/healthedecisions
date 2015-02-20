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
	public static class IDictionaryExtensions
	{
		public static TValue GetValue<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key)
		{
			return dictionary.GetValue(key, default(TValue));
		}

		public static TValue GetValue<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, TValue defaultValue)
		{
			TValue value;
			if (dictionary.TryGetValue(key, out value))
			{
				return value;
			}

			return defaultValue;
		}
	}

	public static class NodeExtensions
	{
		public static T GetAttribute<T>(this Node node, string attributeName)
		{
			return node.GetAttribute(attributeName, default(T));
		}

		public static T GetAttribute<T>(this Node node, string attributeName, T defaultValue)
		{
			return (T)node.Attributes.GetValue(attributeName, defaultValue);
		}

		public static string FormatErrorMessage(this Node node, string message)
		{
			if (node != null && node.Line.HasValue)
			{
				return String.Format("{0},{1}: {2}", node.Line.GetValueOrDefault(), node.LinePos.GetValueOrDefault(), message);
			}

			return message;
		}
	}

	public static class StringExtensions
	{
		public static string GetQualifier(this string qualifiedName)
		{
			var qualifierIndex = qualifiedName.LastIndexOf(":");
			if (qualifierIndex >= 0)
			{
				return qualifiedName.Substring(0, qualifierIndex);
			}

			return qualifiedName;
		}

		public static string GetLocalName(this string qualifiedName)
		{
			var qualifierIndex = qualifiedName.LastIndexOf(":");
			if (qualifierIndex >= 0)
			{
				return qualifiedName.Substring(qualifierIndex + 1);
			}

			return qualifiedName;
		}
	}
}
