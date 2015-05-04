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
	/// <summary>
	/// Provides an implementation of a handler map, a dictionary of named references to native C# types.
	/// </summary>
	/// <remarks>
	/// Handler maps are used throughout the HeD Schema Framework as a mechanism for providing extensibility
	/// and integration with the engine. A handler map consists of any number of entries, each of which is a named
	/// reference to a native C# class. The classes can then be indexed by name, and a HandlerFactory[T] is used
	/// to instantiate the classes within the map.
	/// </remarks>
	public class HandlerMap
	{
		private Dictionary<string, Type> _handlerMap = new Dictionary<string, Type>(StringComparer.InvariantCultureIgnoreCase);

		private object _syncHandle = new object();

		public void Load(IEnumerable<KeyValuePair<string, string>> map)
		{
			foreach (var entry in map)
			{
				Ensure(entry.Key, Type.GetType(entry.Value, true));
			}
		}

		public void Ensure(string name, Type value)
		{
			lock (_syncHandle)
			{
				if (_handlerMap.ContainsKey(name))
				{
					_handlerMap[name] = value;
				}
				else
				{
					_handlerMap.Add(name, value);
				}
			}
		}

		public Type Find(string name)
		{
			lock (_syncHandle)
			{
				Type type;
				if (_handlerMap.TryGetValue(name, out type))
				{
					return type;
				}
			}

			return null;
		}

		public Type Get(string name)
		{
			var type = Find(name);

			if (type != null)
			{
				return type;
			}

			throw new InvalidOperationException(String.Format("Could not find a type map for name: {0}.", name));
		}

		public IEnumerable<KeyValuePair<string, Type>> GetAll()
		{
			return _handlerMap;
		}
	}
}
