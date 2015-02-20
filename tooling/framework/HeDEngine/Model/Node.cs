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
	/// Represents an element within an HeD Artifact
	/// </summary>
	public class Node
	{
		/// <summary>
		/// Gets or sets the name of the node. 
		/// </summary>
		/// <remarks>
		/// This corresponds to the name of the element in the expression tree.
		/// </remarks>
        public string Name { get; set; }

		/// <summary>
		/// Gets or sets the Line number of the element from which this node was read in the artifact.
		/// </summary>
		public int? Line { get; set; }

		/// <summary>
		/// Gets or sets the character position of the element from which this node was read in the artifact.
		/// </summary>
		public int? LinePos { get; set; }

		/// <summary>
		/// Gets or sets the xsi type of the node as a string.
		/// </summary>
		/// <remarks>
		/// Represented as &lt;namespace&gt;:&lt;typename&gt; where
		/// namespace is the fully resolved xml namespace of the type, and typename is the
		/// local name for the type.
		/// </remarks>
		public string NodeType { get; set; }

		/// <summary>
		/// Gets or sets the child nodes for this node.
		/// </summary>
        public IList<Node> Children { get; set; }

		/// <summary>
		/// Gets or sets the attributes associated with this node.
		/// </summary>
        public IDictionary<string, object> Attributes { get; set; }
	}

	/// <summary>
	/// Represents a node of an expression tree.
	/// </summary>
	/// <remarks>
	/// All expressions within the artifact are represented
	/// as trees of these nodes, regardless of the actual type
	/// of the node.
	/// </remarks>
    public class ASTNode : Node
    {
		/// <summary>
		/// Gets or sets the description of the node. 
		/// </summary>
		/// <remarks>
		/// This corresponds to the description element of the Expression type in the XSD schema.
		/// </remarks>
		public string Description { get; set; }

		/// <summary>
		/// Gets the ASTNodes that are children of this node.
		/// </summary>
		public new IList<ASTNode> Children
		{
			get
			{
				return base.Children.Where(c => c is ASTNode).Cast<ASTNode>().ToList();
			}
		}

		/// <summary>
		/// The result data type for the node.
		/// </summary>
		/// <remarks>
		/// This type is set as part of the validation process. It will be null until
		/// validation of this node has been performed and the type has been determined.
		/// </remarks>
        public DataType ResultType { get; set; }
    }
}
