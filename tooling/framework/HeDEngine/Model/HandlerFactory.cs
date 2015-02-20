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
	/// Defines the generic implementation of a handler factory, a named mapping for types used throughout the HeD Schema Framework.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public class HandlerFactory<T> where T : class
	{
		private static HandlerMap _handlerMap = new HandlerMap();

		/// <summary>
		/// Loads the given mappings into this factory.
		/// </summary>
		/// <param name="map"></param>
		public static void LoadMap(IEnumerable<KeyValuePair<string, string>> map)
		{
			_handlerMap.Load(map);
		}

		/// <summary>
		/// Resolves a handler based on the given name, returning an instance if it exists, null otherwise.
		/// </summary>
		/// <param name="key">The name of the handler.</param>
		/// <returns>An instance of the handler, if a mapping exists, null otherwise.</returns>
		public static T FindHandler(string key)
		{
			if (String.IsNullOrEmpty(key))
			{
				throw new ArgumentNullException("key");
			}

			var handlerType = _handlerMap.Find(key);

			if (handlerType != null)
			{
				var handler = Activator.CreateInstance(handlerType) as T;

				if (handler == null)
				{
					throw new InvalidOperationException(String.Format("Handler type {0} must denote a type that implements the {1} interface.", handlerType.Name, typeof(T).Name));
				}

				return handler;
			}

			return null;
		}

		/// <summary>
		/// Resolves a handler based on the given name, returning an instance if it exists, throwing an exception otherwise.
		/// </summary>
		/// <param name="key">The name of the handler.</param>
		/// <returns>An instance of the handler, if a mapping exists, otherwise an exception is thrown.</returns>
		public static T GetHandler(string key)
		{
			var handler = FindHandler(key);

			if (handler == null)
			{
				throw new InvalidOperationException(String.Format("Could not find a handler map for name: {0}.", key));
			}

			return handler;
		}

		/// <summary>
		/// Retrieves an instance of each type of handler registered with the factory.
		/// </summary>
		/// <returns></returns>
		public static IEnumerable<T> GetHandlers()
		{
			foreach (var handler in _handlerMap.GetAll())
			{
				yield return GetHandler(handler.Key);
			}
		}
	}
}
