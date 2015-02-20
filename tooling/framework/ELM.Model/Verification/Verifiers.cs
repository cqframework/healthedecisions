/*
	HeD Schema Framework
	Copyright (c) 2012 - 2014 Office of the National Coordinator for Health Information Technology (ONC)
	This file is licensed under a modified BSD-license which can be found in the HED_License.txt file included with this distribution.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CQL.ELM.Model;
using HeD.Engine.Model;
using HeD.Engine.Verification;

namespace ELM.Model.Verification
{
    public class IdentifierRefVerifier : INodeVerifier
    {
        public void Verify(VerificationContext context, HeD.Engine.Model.ASTNode node)
        {
            // TODO: Implement generic identifier resolution. In theory, this isn't necessary because the CQL-to-ELM translator should never spit out this type of node.
            throw new InvalidOperationException(String.Format("Could not resolve identifier {0}{1}.", node.GetAttribute<String>("libraryName") ?? "", node.GetAttribute<String>("name")));
        }
    }

    public class FunctionRefVerifier : INodeVerifier
    {
        public void Verify(VerificationContext context, ASTNode node)
        {
            throw new NotImplementedException();
        }
    }

    public class TimesVerifier : INodeVerifier
    {
        public void Verify(VerificationContext context, HeD.Engine.Model.ASTNode node)
        {
            throw new NotImplementedException();
        }
    }

    public class QueryVerifier : INodeVerifier
    {
        private Symbol ProcessAliasedQuerySource(VerificationContext context, HeD.Engine.Model.Node source)
        {
            var sourceAlias = source.GetAttribute<String>("alias");
            var sourceExpression = source.Children[0] as ASTNode;
            if (sourceExpression == null)
            {
                throw new InvalidOperationException(String.Format("Could not determine source expression for alias '{0}'.", sourceAlias));
            }

            Verifier.Verify(context, sourceExpression);
            return new Symbol(sourceAlias, sourceExpression.ResultType);
        }

        public void Verify(VerificationContext context, HeD.Engine.Model.ASTNode node)
        {
            Symbol sourceSymbol = null;

            // foreach source
                // add to the output context
            var sources = ((Node)node).Children.Where(c => c.Name == "source").ToList();
            foreach (var source in sources)
            {
                if (sourceSymbol == null)
                {
                    sourceSymbol = ProcessAliasedQuerySource(context, source);
                }
                else
                {
                    throw new NotImplementedException("Multi-source query verification is not implemented.");
                }
            }

            if (sourceSymbol == null)
            {
                throw new InvalidOperationException("Could not determine query source type.");
            }

            var sourceSymbolListType = sourceSymbol.DataType as ListType;
            var sourceSymbolElementType = sourceSymbolListType != null ? sourceSymbolListType.ElementType : null;
            var sourceElementSymbol = sourceSymbolElementType != null ? new Symbol(sourceSymbol.Name, sourceSymbolElementType) : null;

            context.PushSymbol(sourceElementSymbol ?? sourceSymbol);
            try
            {
                // foreach define
                    // add to the current scope
                var defines = ((Node)node).Children.Where(c => c.Name == "define").ToList();
                foreach (var define in defines)
                {
                    throw new NotImplementedException("Query defines are not implemented.");
                }

                // verify each with/without clause
                var relationships = ((Node)node).Children.Where(c => c.Name == "relationship").ToList();
                foreach (var relationship in relationships)
                {
                    var relatedSourceSymbol = ProcessAliasedQuerySource(context, relationship);
                    var relatedSourceSymbolListType = relatedSourceSymbol.DataType as ListType;
                    var relatedSourceSymbolElementType = relatedSourceSymbolListType != null ? relatedSourceSymbolListType.ElementType : null;
                    var relatedSourceElementSymbol = relatedSourceSymbolElementType != null ? new Symbol(relatedSourceSymbol.Name, relatedSourceSymbolElementType) : null;

                    context.PushSymbol(relatedSourceElementSymbol ?? relatedSourceSymbol);
                    try
                    {
                        var suchThat = relationship.Children[1] as ASTNode;
                        if (suchThat == null)
                        {
                            throw new InvalidOperationException(String.Format("Could not determine such that for relationship '{0}'.", relatedSourceSymbol.Name));
                        }

                        Verifier.Verify(context, suchThat);
                        context.VerifyType(suchThat.ResultType, DataTypes.Boolean);
                    }
                    finally
                    {
                        context.PopSymbol();
                    }
                }

                // verify the where clause
                var whereClause = ((Node)node).Children.Where(c => c.Name == "where").FirstOrDefault() as ASTNode;
                if (whereClause != null)
                {
                    Verifier.Verify(context, whereClause);
                    context.VerifyType(whereClause.ResultType, DataTypes.Boolean);
                }

                // verify the return clause
                var returnClause = ((Node)node).Children.Where(c => c.Name == "return").FirstOrDefault();
                if (returnClause != null)
                {
                    throw new NotImplementedException("Return clause is not implemented.");
                }

                // verify the sort clause
                var sortClause = ((Node)node).Children.Where(c => c.Name == "sort").FirstOrDefault();
                if (sortClause != null)
                {
                    throw new NotImplementedException("Sort clause is not implemented.");
                }

                node.ResultType = sourceSymbol.DataType;
            }
            finally
            {
                context.PopSymbol();
            }
        }
    }

    public class AliasRefVerifier : INodeVerifier
    {
        public void Verify(VerificationContext context, HeD.Engine.Model.ASTNode node)
        {
            throw new NotImplementedException();
        }
    }

    public class QueryDefineRefVerifier : INodeVerifier
    {
        public void Verify(VerificationContext context, HeD.Engine.Model.ASTNode node)
        {
            throw new NotImplementedException();
        }
    }

    public class ValueSetRefVerifier : INodeVerifier
    {
        public void Verify(VerificationContext context, ASTNode node)
        {
            var valuesetDef = context.ResolveValueSetRef(node.GetAttribute<String>("libraryName"), node.GetAttribute<String>("name"));
            node.ResultType = DataTypes.CodeList;
        }
    }

    public class QuantityVerifier : INodeVerifier
    {
        public void Verify(VerificationContext context, ASTNode node)
        {
            throw new NotImplementedException();
        }
    }
}
