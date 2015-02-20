/*
	HeD Schema Framework
	Copyright (c) 2012 - 2013 Office of the National Coordinator for Health Information Technology (ONC)
	This file is licensed under a modified BSD-license which can be found in the HED_License.txt file included with this distribution.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HeD.Engine.Verification
{
	public class VerificationException : ApplicationException
	{
		public VerificationException(string message) : base(message) { }
		public VerificationException(string message, Exception inner) : base(message, inner) { }
        public VerificationException(string message, bool isWarning) : base(message) { IsWarning = isWarning; }

        public bool IsWarning { get; private set; }
	}
}
