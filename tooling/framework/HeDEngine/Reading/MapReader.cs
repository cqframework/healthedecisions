/*
	HeD Schema Framework
	Copyright (c) 2012 - 2013 Office of the National Coordinator for Health Information Technology (ONC)
	This file is licensed under a modified BSD-license which can be found in the HED_License.txt file included with this distribution.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

using HeD.Engine.Model;

namespace HeD.Engine.Reading
{
	public static class MapReader
	{
		public static HandlerType ReadHandlerType(XDocument map)
		{
			return (HandlerType)Enum.Parse(typeof(HandlerType), map.Root.Attribute("handlerType").Value, true);
		}

		public static IEnumerable<KeyValuePair<string, string>> ReadMap(XDocument map)
		{
			var ns = map.Root.Name.Namespace;
			foreach (var element in map.Root.Elements(ns + "entry"))
			{
				yield return new KeyValuePair<string, string>(element.Attribute("key").Value, element.Attribute("value").Value);
			}
		}
	}
}
