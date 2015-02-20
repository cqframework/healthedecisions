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

namespace HeD.Engine.Verification
{
	public static class Verifier
	{
		public static void Verify(VerificationContext context, ASTNode node)
		{
			try
			{
				var verifier = NodeVerifierFactory.GetHandler(node.NodeType);

				// Traversal occurs within the verify, because traversal may happen differently depending on the operation.
				verifier.Verify(context, node);
			}
			catch (Exception e)
			{
                context.ReportMessage(e, node);
			}
		}
	}
}
