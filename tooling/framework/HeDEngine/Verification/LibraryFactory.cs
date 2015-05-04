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
using HeD.Engine.Reading;

namespace HeD.Engine.Verification
{
	public static class LibraryFactory
	{
		private static Dictionary<string, Library> _libraries = new Dictionary<string, Library>();

		private static Stack<string> resolvingLibraries = new Stack<string>();

		public static Library ResolveLibrary(LibraryRef libraryRef, VerificationContext context)
		{
			Library library;
			if (!_libraries.TryGetValue(libraryRef.Name, out library))
			{
				if (context == null)
				{
					throw new InvalidOperationException("Cannot perform type verification on a library without a verification context.");
				}

				if (resolvingLibraries.Contains(libraryRef.Name))
				{
					throw new InvalidOperationException(String.Format("Circular library reference to library {0}.", libraryRef.Name));
				}

				resolvingLibraries.Push(libraryRef.Name);
				try
				{
					library = LibraryReaderFactory.GetHandler(libraryRef.MediaType).Read(libraryRef);
					var verifier = new LibraryVerifier();
					IEnumerable<VerificationException> results = verifier.Verify(library);
					context.Messages.AddRange(results);
					_libraries.Add(libraryRef.Name, library);

					if (results.Any(r => !r.IsWarning))
					{
						throw new InvalidOperationException(String.Format("Errors encountered while verifying library {0}.", libraryRef.Name));
					}
				}
				finally
				{
					resolvingLibraries.Pop();
				}
			}

			return library;
		}
	}
}
