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
	public class VerificationContext
	{
		// Default scope name
		public const string Current = "Current";

		public VerificationContext
		(
			IEnumerable<ModelRef> models,
			IEnumerable<LibraryRef> libraries,
			IEnumerable<ParameterDef> parameters,
			IEnumerable<ExpressionDef> expressions,
			IEnumerable<CodeSystemDef> codesystems,
            IEnumerable<ValueSetDef> valuesets,
            IEnumerable<VerificationException> messages
		)
		{
			if (models != null)
			{
				AddModels(models);
			}

			if (libraries != null)
			{
				AddLibraries(libraries);
			}

			if (parameters != null)
			{
				AddParameters(parameters);
			}

			if (expressions != null)
			{
				AddExpressions(expressions);
			}

			if (codesystems != null)
			{
				AddCodeSystems(codesystems);
			}

            if (valuesets != null)
            {
                AddValueSets(valuesets);
            }

            if (messages != null)
            {
                AddMessages(messages);
            }
		}

        // Messages
        private List<VerificationException> _messages = new List<VerificationException>();
        public List<VerificationException> Messages { get { return _messages; } }

        private void AddMessages(IEnumerable<VerificationException> messages)
        {
            _messages.AddRange(messages);
        }

        public void ReportMessage(Exception e, Node node)
        {
            var message = e as VerificationException;

            if (message == null)
            {
                message = new VerificationException(node.FormatErrorMessage(e.Message), e);
            }

            _messages.Add(message);
        }

		// Models
		private Dictionary<string, ModelRef> _models = new Dictionary<string, ModelRef>(StringComparer.InvariantCultureIgnoreCase);

		public DataType ResolveType(string typeName)
		{
			var typeResolver = TypeResolverFactory.FindHandler(typeName);

			if (typeResolver == null)
			{
				var qualifier = typeName.GetQualifier();
				typeResolver = TypeResolverFactory.FindHandler(qualifier);
			}

			if (typeResolver == null)
			{
				throw new InvalidOperationException(String.Format("Could not resolve a type handler for type name {0}.", typeName));
			}

			return typeResolver.Resolve(typeName);
		}

        /// <summary>
        /// Processes a property name to determine if it contains an indexer.
        /// </summary>
        /// <param name="pn">The input name of the property. Will contain the name of the property without the indexer if the property has an indexer.</param>
        /// <returns>True if the property contained an indexer; false otherwise.</returns>
        private bool ProcessIndexer(ref string pn)
        {
            var leftIndex = pn.IndexOf('[');
            if (leftIndex >= 1)
            {
                var rightIndex = pn.IndexOf(']', leftIndex);
                if (rightIndex > leftIndex) // && pn.Substring(leftIndex + 1, rightIndex - leftIndex - 1).IsDigit())
                {
                    pn = pn.Substring(0, leftIndex);
                    return true;
                }

                throw new InvalidOperationException(String.Format("Badly formed property indexer expression: '{0}'", pn));
            }

            return false;
        }

        private DataType ResolveIndexerType(DataType currentType, string pn)
        {
            var currentListType = currentType as ListType;
            if (currentListType == null)
            {
                throw new InvalidOperationException(String.Format("Could not index into property '{0}' of type '{1}' because it is not a list type.", pn, currentType.Name));
            }
            else
            {
                return currentListType.ElementType;
            }
        }

		public DataType ResolveProperty(DataType dataType, string path)
		{
            // TODO: Need to handle this scenario: contained[medication.reference].Medication.code.coding[1]
            // The use of Split here incorrectly splits on the path within the indexer
			var propertyNames = path.Split('.');
			DataType currentType = dataType;
			foreach (var propertyName in propertyNames)
			{
                var pn = propertyName;
                var isIndexer = ProcessIndexer(ref pn);
				var currentIntervalType = currentType as IntervalType;
				if (currentIntervalType != null)
				{
					var propertyDef = currentIntervalType.Properties.FirstOrDefault(p => String.Compare(p.Name, pn, true) == 0);
					if (propertyDef == null)
					{
						throw new InvalidOperationException(String.Format("Could not resolve property name '{0}' on interval type '{1}'.", pn, currentIntervalType.Name));
					}

					currentType = propertyDef.PropertyType;

                    if (isIndexer)
                    {
                        currentType = ResolveIndexerType(currentType, pn);
                    }
				}
				else
				{
					var currentObjectType = currentType as ObjectType;
					if (currentObjectType == null)
					{
						throw new InvalidOperationException(String.Format("Could not resolve property path '{0}'.", path));
					}

                    PropertyDef propertyDef = null;
                    while (currentObjectType != null)
                    {
					    propertyDef = currentObjectType.Properties.FirstOrDefault(p => String.Compare(p.Name, pn, true) == 0);
                        if (propertyDef != null)
                        {
                            break;
                        }

                        currentObjectType = currentObjectType.BaseType as ObjectType;
                    }

					if (propertyDef == null)
					{
						throw new InvalidOperationException(String.Format("Could not resolve property name '{0}'.", pn));
					}

					currentType = propertyDef.PropertyType;

                    if (isIndexer)
                    {
                        currentType = ResolveIndexerType(currentType, pn);
                    }
				}
			}

			return currentType;
		}

		/// <summary>
		/// Resolves an operator call, throwing an exception if no resolution is possible.
		/// </summary>
		/// <param name="operatorName">The name of the operator being called.</param>
		/// <param name="signature">The signature of the arguments to the call.</param>
		/// <returns>The operator matching the given name and signature if one exists, otherwise an exception is thrown.</returns>
		public Operator ResolveCall(string operatorName, Signature signature)
		{
			return OperatorMap.Core.ResolveCall(operatorName, signature);
		}

		public void VerifyType(DataType actualType, DataType expectedType)
		{
            if (!DataTypes.SubTypeOf(actualType, expectedType))
			//if (!DataTypes.Equivalent(actualType, expectedType))
			{
				throw 
					new InvalidOperationException
					(
						String.Format
						(
							"Expected an expression of type '{0}', but found an expression of type '{1}'.", 
							expectedType != null ? expectedType.ToString() : "<unknown>", 
							actualType != null ? actualType.ToString() : "<unknown>"
						)
					);
			}
		}

		private void AddModels(IEnumerable<ModelRef> models)
		{
			foreach (var model in models)
			{
				_models.Add(model.Uri, model);
			}
		}

		// Libraries
		private Dictionary<string, LibraryRef> _libraries = new Dictionary<string, LibraryRef>(StringComparer.InvariantCultureIgnoreCase);

		private void AddLibraries(IEnumerable<LibraryRef> libraries)
		{
			foreach (var library in libraries)
			{
				_libraries.Add(library.Name, library);
			}
		}

		public Library ResolveLibrary(string libraryName)
		{
			LibraryRef libraryRef;
			if (_libraries.TryGetValue(libraryName, out libraryRef))
			{
				return LibraryFactory.ResolveLibrary(libraryRef, this);
			}

			throw new InvalidOperationException(String.Format("Could not resolve library name %s.", libraryName));
		}

		// Symbols
		private Stack<Symbol> _symbols = new Stack<Symbol>();

		public void PushSymbol(Symbol symbol)
		{											
			_symbols.Push(symbol);
		}

		public Symbol PopSymbol()
		{
			return _symbols.Pop();
		}

		public Symbol ResolveSymbol(string symbolName)
		{
			var symbol = _symbols.FirstOrDefault(s => String.Compare(s.Name, symbolName, true) == 0);
			if (symbol == null)
			{
				throw new InvalidOperationException(String.Format("Could not resolve symbol name {0}.", symbolName));
			}
			return symbol;
		}

		// Codesystems
		private Dictionary<string, CodeSystemDef> _codesystems = new Dictionary<string, CodeSystemDef>(StringComparer.InvariantCultureIgnoreCase);

		public void AddCodeSystemDef(CodeSystemDef codesystem)
		{
			if (codesystem == null)
			{
				throw new ArgumentNullException("codesystem");
			}

			if (_codesystems.ContainsKey(codesystem.Name))
			{
				throw new InvalidOperationException(String.Format("A codesystem named {0} is already defined in this scope.", codesystem.Name));
			}

			_codesystems.Add(codesystem.Name, codesystem);
		}

		public CodeSystemDef ResolveCodeSystemRef(string libraryName, string codesystemName)
		{
            if (!string.IsNullOrEmpty(libraryName))
            {
                var library = ResolveLibrary(libraryName);
                var result = library.CodeSystems.FirstOrDefault(e => String.Compare(e.Name, codesystemName, true) == 0);
                if (result == null)
                {
                    throw new InvalidOperationException(String.Format("Could not resolve codesystem reference {0} in library {1}.", codesystemName, libraryName));
                }

                return result;
            }
            else
            {
                CodeSystemDef result;
                if (!_codesystems.TryGetValue(codesystemName, out result))
                {  
                    throw new InvalidOperationException(String.Format("Could not resolve codesystem name {0}.", codesystemName));
                }

                return result;
            }
		}

		private void AddCodeSystems(IEnumerable<CodeSystemDef> codesystems)
		{
			foreach (var codesystem in codesystems)
			{
				_codesystems.Add(codesystem.Name, codesystem);
			}
		}

        // Valuesets
        private Dictionary<string, ValueSetDef> _valuesets = new Dictionary<string, ValueSetDef>(StringComparer.InvariantCultureIgnoreCase);

        public void AddValueSetDef(ValueSetDef valueset)
        {
            if (valueset == null)
            {
                throw new ArgumentNullException("valueset");
            }

            if (_valuesets.ContainsKey(valueset.Name))
            {
                throw new InvalidOperationException(String.Format("A valueset named {0} is already defined in this scope.", valueset.Name));
            }

            _valuesets.Add(valueset.Name, valueset);
        }

        public ValueSetDef ResolveValueSetRef(string libraryName, string valuesetName)
        {
            if (!string.IsNullOrEmpty(libraryName))
            {
                var library = ResolveLibrary(libraryName);
                var result = library.ValueSets.FirstOrDefault(e => String.Compare(e.Name, valuesetName, true) == 0);
                if (result == null)
                {
                    throw new InvalidOperationException(String.Format("Could not resolve valueset reference {0} in library {1}.", valuesetName, libraryName));
                }

                return result;
            }
            else
            {
                ValueSetDef result;
                if (!_valuesets.TryGetValue(valuesetName, out result))
                {  
                    throw new InvalidOperationException(String.Format("Could not resolve valueset name {0}.", valuesetName));
                }

                return result;
            }
        }

		private void AddValueSets(IEnumerable<ValueSetDef> valuesets)
		{
			foreach (var valueset in valuesets)
			{
				_valuesets.Add(valueset.Name, valueset);
			}
		}

		// Parameters
		private Dictionary<string, ParameterDef> _parameters = new Dictionary<string, ParameterDef>(StringComparer.InvariantCultureIgnoreCase);

        public void AddParameterDef(ParameterDef parameter)
        {
            if (parameter == null)
            {
                throw new ArgumentNullException("parameter");
            }

            if (_parameters.ContainsKey(parameter.Name))
            {
                throw new InvalidOperationException(String.Format("A parameter named {0} is already defined in this scope.", parameter.Name));
            }

            _parameters.Add(parameter.Name, parameter);
        }

		public ParameterDef ResolveParameterRef(string libraryName, string parameterName)
		{
			if (!string.IsNullOrEmpty(libraryName))
			{
				var library = ResolveLibrary(libraryName);
				var result = library.Parameters.FirstOrDefault(e => String.Compare(e.Name, parameterName, true) == 0);
				if (result == null)
				{
					throw new InvalidOperationException(String.Format("Could not resolve parameter reference {0} in library {1}.", parameterName, libraryName));
				}

				return result;
			}
			else
			{
				ParameterDef result;
				if (!_parameters.TryGetValue(parameterName, out result))
				{
					throw new InvalidOperationException(String.Format("Could not resolve parameter name {0}.", parameterName));
				}

				return result;
			}
		}

		private void AddParameters(IEnumerable<ParameterDef> parameters)
		{
			foreach (var parameter in parameters)
			{
				_parameters.Add(parameter.Name, parameter);
			}
		}

		// Expressions
		private Dictionary<string, ExpressionDef> _expressions = new Dictionary<string, ExpressionDef>(StringComparer.InvariantCultureIgnoreCase);

		private ExpressionDefStack _expressionDefStack = new ExpressionDefStack();

		private class ExpressionDefStack
		{
			private Stack<string> _stack = new Stack<string>();

			public void Push(ExpressionDef def)
			{
				if (def == null)
				{
					throw new ArgumentNullException("def");
				}

				if (_stack.Contains(def.Name))
				{
					throw new InvalidOperationException(String.Format("Reference to expression {0} is invalid because it results in a circular reference.", def.Name));
				}

				_stack.Push(def.Name);
			}

			public void Pop()
			{
				_stack.Pop();
			}
		}

		public ExpressionDef ResolveExpressionRef(string libraryName, string expressionName)
		{
			if (string.IsNullOrEmpty(expressionName))
			{
				throw new ArgumentNullException("expressionName");
			}

			if (!string.IsNullOrEmpty(libraryName))
			{
				var library = ResolveLibrary(libraryName);
				var expressionDef = library.Expressions.FirstOrDefault(e => String.Compare(e.Name, expressionName, true) == 0);
				if (expressionDef == null)
				{
					throw new InvalidOperationException(String.Format("Could not resolve expression reference {0} in library {1}.", expressionName, libraryName));
				}

				return expressionDef;
			}
			else
			{
				ExpressionDef expressionDef;
				if (!_expressions.TryGetValue(expressionName, out expressionDef))
				{
					throw new InvalidOperationException(String.Format("Could not resolve expression reference {0}.", expressionName));
				}

				if (expressionDef.Expression.ResultType == null)
				{
					_expressionDefStack.Push(expressionDef);
					try
					{
						Verifier.Verify(this, expressionDef.Expression);
					}
					finally
					{
						_expressionDefStack.Pop();
					}
				}

				return expressionDef;
			}
		}

		private void AddExpressions(IEnumerable<ExpressionDef> expressions)
		{
			foreach (var expression in expressions)
			{
				_expressions.Add(expression.Name, expression);
			}
		}
	}
}
