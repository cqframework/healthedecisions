using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HeD.Engine.Model
{
	/// <summary>
	/// Represents an operator map used to store operator names and their associated overload signatures.
	/// </summary>
	/// <remarks>
	/// The operator map stores each registered operator together with its associated overload signatures.
	/// Calls can then be resolved against the operator map to determine whether or not an appropriate overload
	/// exists for a given call signature.
	/// </remarks>
	public class OperatorMap
	{
		private static Dictionary<string, OperatorEntry> _operatorMap = new Dictionary<string, OperatorEntry>();

		public static void RegisterOperator(Operator op)
		{
			OperatorEntry entry;
			if (!_operatorMap.TryGetValue(op.Name, out entry))
			{
				entry = new OperatorEntry(op.Name);
				_operatorMap.Add(op.Name, entry);
			}

			entry.RegisterOperator(op);
		}

		public static Operator ResolveCall(string operatorName, Signature signature)
		{
			if (String.IsNullOrEmpty(operatorName))
			{
				throw new ArgumentNullException("operatorName");
			}

			OperatorEntry entry;
			if (!_operatorMap.TryGetValue(operatorName, out entry))
			{
				throw new InvalidOperationException(String.Format("Could not resolve call to operator name '{0}'.", operatorName));
			}

			return entry.ResolveSignature(signature.OperandTypes);
		}
	}

	/// <summary>
	/// Represents a specific operator and call signature.
	/// </summary>
	public class Operator
	{
		public Operator(string name, Signature signature, DataType resultType)
		{
			if (String.IsNullOrEmpty(name))
			{
				throw new ArgumentNullException("name");
			}

			if (signature == null)
			{
				throw new ArgumentNullException("signature");
			}

			_name = name;
			_signature = signature;
			_resultType = resultType;
		}

		private string _name;
		public string Name { get { return _name; } }

		private Signature _signature;
		public Signature Signature { get { return _signature; } }

		private DataType _resultType;
		public DataType ResultType { get { return _resultType; } }
	}

	/// <summary>
	/// Represents a single entry in the operator map.
	/// </summary>
	public class OperatorEntry
	{
		public OperatorEntry(string name)
		{
			if (String.IsNullOrEmpty(name))
			{
				throw new ArgumentNullException("name");
			}

			_name = name;
		}

		private string _name;
		public string Name { get { return _name; } }

		private Dictionary<Signature, Operator> _operators = new Dictionary<Signature, Operator>();

		public void RegisterOperator(Operator op)
		{
			if (op == null)
			{
				throw new ArgumentNullException("op");
			}

			if (op.Name != _name)
			{
				throw new ArgumentException(String.Format("Operator being registered '{0}' does not match operator entry name '{1}'.", op.Name, _name), "op");
			}

			if (_operators.ContainsKey(op.Signature))
			{
				throw new InvalidOperationException(String.Format("Signature '{0}' is already registered for operator '{1}'.", op.Signature, _name));
			}

			_operators.Add(op.Signature, op);
		}

		public Operator ResolveSignature(IEnumerable<DataType> argumentTypes)
		{
			Operator result;
			var signature = new Signature(argumentTypes);
			if (_operators.TryGetValue(signature, out result))
			{
				return result;
			}

			throw new InvalidOperationException(String.Format("Could not resolve signature '{0}' for operator '{1}'.", signature.ToString(), _name)); 
		}
	}

	/// <summary>
	/// Represents an operator signature.
	/// </summary>
	public class Signature
	{
		public Signature(IEnumerable<DataType> operandTypes)
		{
			foreach (var operandType in operandTypes)
			{
				if (operandType == null)
				{
					throw new ArgumentNullException("Operand types cannot be null for a signature.");
				}
			}

			_operandTypes = operandTypes.ToList();
		}

		private List<DataType> _operandTypes;
		public IEnumerable<DataType> OperandTypes { get { return _operandTypes; } }

		public override bool Equals(object obj)
		{
			var other = obj as Signature;
			if (other != null)
			{
				if (_operandTypes.Count == other._operandTypes.Count)
				{
					for (int i = 0; i < _operandTypes.Count; i++)
					{
						if (!DataTypes.Equal(_operandTypes[i], other._operandTypes[i]))
						{
							return false;
						}

					}

					return true;
				}
			}

			return false;
		}

		public override int GetHashCode()
		{
			var hashCode = 17;
			var changeMultiplier = false;

			for (int i = 0; i < _operandTypes.Count; i++)
			{
				hashCode = hashCode + (_operandTypes[i].GetHashCode() * (changeMultiplier ? 43 : 47));
				changeMultiplier = !changeMultiplier;
			}

			return hashCode;
		}

		public override string ToString()
		{
			var sb = new StringBuilder();
			foreach (var operandType in _operandTypes)
			{
				if (sb.Length > 0)
				{
					sb.Append(", ");
				}

				sb.Append(operandType.ToString());
			}

			sb.Append(")");
			sb.Insert(0, "(");
			return sb.ToString();
		}
	}
}
