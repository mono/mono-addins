//
// ConditionType.cs
//
// Author:
//   Lluis Sanchez Gual
//
// Copyright (C) 2007 Novell, Inc (http://www.novell.com)
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//


using System;
using System.Xml;
using Mono.Addins.Description;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;

namespace Mono.Addins
{
	/// <summary>
	/// A condition evaluator.
	/// </summary>
	/// <remarks>
	/// Add-ins may use conditions to register nodes in an extension point which
	/// are only visible under some contexts. For example, an add-in registering
	/// a custom menu option to the main menu of a sample text editor might want
	/// to make that option visible only for some kind of files. To allow add-ins
	/// to do this kind of check, the host application needs to define a new condition.
	/// </remarks>
	public abstract class ConditionType
	{
		internal event EventHandler Changed;
		string id;
		
		/// <summary>
		/// Evaluates the condition.
		/// </summary>
		/// <param name="conditionNode">
		/// Condition node information.
		/// </param>
		/// <returns>
		/// 'true' if the condition is satisfied.
		/// </returns>
		public abstract bool Evaluate (NodeElement conditionNode);
		
		/// <summary>
		/// Notifies that the condition has changed, and that it has to be re-evaluated.
		/// </summary>
		/// This method must be called when there is a change in the state that determines
		/// the result of the evaluation. When this method is called, all node conditions
		/// depending on it are reevaluated and the corresponding events for adding or
		/// removing extension nodes are fired.
		/// <remarks>
		/// </remarks>
		public void NotifyChanged ()
		{
			if (Changed != null)
				Changed (this, EventArgs.Empty);
		}
		
		internal string Id {
			get { return id; }
			set { id = value; }
		}
	}
	
	internal abstract class ConditionExpression
	{
		internal ConditionExpression ()
		{
		}

		public bool BoolEvaluate (ExtensionContext context)
		{
			var val = Evaluate (context);
			if (!(val is bool))
				throw new EvaluationException (String.Format ("Can not evaluate \"{0}\" to bool.", ToString()));
			return (bool)val;
		}

		public abstract object Evaluate (ExtensionContext ctx);

		internal abstract void GetConditionTypes (List<string> listToFill);

		protected bool IsInteger (object value)
		{
			return value is int || value is byte || value is long;
		}

		protected long GetInteger (object value)
		{
			return Convert.ToInt64 (value);
		}

		protected bool IsFloat (object value)
		{
			return value is float || value is double;
		}

		protected bool IsNumber (object value)
		{
			return value is int || value is byte || value is long || value is float || value is double;
		}

		protected double GetFloat (object value)
		{
			return Convert.ToDouble (value);
		}
	}
	
	abstract class BinaryConditionExpression: ConditionExpression
	{
		protected ConditionExpression exp1;
		protected ConditionExpression exp2;

		protected BinaryConditionExpression ()
		{
		}

		protected BinaryConditionExpression (ConditionExpression exp1, ConditionExpression exp2)
		{
			this.exp1 = exp1;
			this.exp2 = exp2;
		}

		internal override void GetConditionTypes (List<string> listToFill)
		{
			exp1.GetConditionTypes (listToFill);
			exp2.GetConditionTypes (listToFill);
		}
	}

	abstract class UnaryConditionExpression: ConditionExpression
	{
		protected ConditionExpression exp;

		protected UnaryConditionExpression ()
		{
		}

		protected UnaryConditionExpression (ConditionExpression exp)
		{
			this.exp = exp;
		}

		internal override void GetConditionTypes (List<string> listToFill)
		{
			exp.GetConditionTypes (listToFill);
		}
	}

	class OrConditionExpression: BinaryConditionExpression
	{
		public OrConditionExpression (ConditionExpression exp1, ConditionExpression exp2): base (exp1, exp2)
		{
		}

		public OrConditionExpression (ConditionExpression[] conditionExpressions, int index = 0)
		{
			exp1 = conditionExpressions [index];
			if (conditionExpressions.Length == index + 2)
				exp2 = conditionExpressions [index + 1];
			else
				exp2 = new OrConditionExpression (conditionExpressions, index + 1);
		}

		public override object Evaluate (ExtensionContext ctx)
		{
			return exp1.BoolEvaluate (ctx) || exp2.BoolEvaluate (ctx);
		}
	}
	
	class AndConditionExpression: BinaryConditionExpression
	{
		public AndConditionExpression (ConditionExpression exp1, ConditionExpression exp2): base (exp1, exp2)
		{
		}

		public AndConditionExpression (ConditionExpression[] conditionExpressions, int index = 0)
		{
			exp1 = conditionExpressions [index];
			if (conditionExpressions.Length == index + 2)
				exp2 = conditionExpressions [index + 1];
			else
				exp2 = new AndConditionExpression (conditionExpressions, index + 1);
		}

		public override object Evaluate (ExtensionContext ctx)
		{
			return exp1.BoolEvaluate (ctx) && exp2.BoolEvaluate (ctx);
		}
	}
	
	class NotConditionExpression: UnaryConditionExpression
	{
		public NotConditionExpression (ConditionExpression exp): base (exp)
		{
		}
		
		public override object Evaluate (ExtensionContext ctx)
		{
			return !exp.BoolEvaluate (ctx);
		}
	}

	class NegateConditionExpression: UnaryConditionExpression
	{
		public NegateConditionExpression (ConditionExpression exp): base (exp)
		{
		}

		public override object Evaluate (ExtensionContext ctx)
		{
			var val = exp.Evaluate (ctx);
			if (IsInteger (val))
				return -GetInteger (val);
			if (IsFloat (val))
				return -GetFloat (val);
			throw new EvaluationException ("Invalid operand for negate operator");
		}
	}

	class EqualsConditionExpression: BinaryConditionExpression
	{
		public EqualsConditionExpression (ConditionExpression exp1, ConditionExpression exp2): base (exp1, exp2)
		{
		}

		public override object Evaluate (ExtensionContext ctx)
		{
			return object.Equals (exp1.Evaluate (ctx), exp2.Evaluate (ctx));
		}
	}

	class NotEqualsConditionExpression: BinaryConditionExpression
	{
		public NotEqualsConditionExpression (ConditionExpression exp1, ConditionExpression exp2): base (exp1, exp2)
		{
		}

		public override object Evaluate (ExtensionContext ctx)
		{
			return !object.Equals (exp1.Evaluate (ctx), exp2.Evaluate (ctx));
		}
	}

	class GreaterThanConditionExpression: BinaryConditionExpression
	{
		public GreaterThanConditionExpression (ConditionExpression exp1, ConditionExpression exp2): base (exp1, exp2)
		{
		}

		public override object Evaluate (ExtensionContext ctx)
		{
			var v1 = exp1.Evaluate (ctx);
			var v2 = exp2.Evaluate (ctx);
			if (IsInteger (v1) && IsInteger (v2))
				return GetInteger (v1) > GetInteger (v2);
			if (IsNumber (v1) && IsNumber (v2))
				return GetFloat (v1) > GetFloat (v2);
			if (v1 is string && v2 is string)
				return string.Compare ((string)v1, (string)v2, StringComparison.Ordinal) > 0;
			throw new EvaluationException ("Invalid operands for greater-than operation");
		}
	}

	class GreaterThanOrEqualConditionExpression: BinaryConditionExpression
	{
		public GreaterThanOrEqualConditionExpression (ConditionExpression exp1, ConditionExpression exp2): base (exp1, exp2)
		{
		}

		public override object Evaluate (ExtensionContext ctx)
		{
			var v1 = exp1.Evaluate (ctx);
			var v2 = exp2.Evaluate (ctx);
			if (IsInteger (v1) && IsInteger (v2))
				return GetInteger (v1) >= GetInteger (v2);
			if (IsNumber (v1) && IsNumber (v2))
				return GetFloat (v1) >= GetFloat (v2);
			if (v1 is string && v2 is string)
				return string.Compare ((string)v1, (string)v2, StringComparison.Ordinal) >= 0;
			throw new EvaluationException ("Invalid operands for greater-than operation");
		}
	}

	class LessThanConditionExpression: BinaryConditionExpression
	{
		public LessThanConditionExpression (ConditionExpression exp1, ConditionExpression exp2): base (exp1, exp2)
		{
		}

		public override object Evaluate (ExtensionContext ctx)
		{
			var v1 = exp1.Evaluate (ctx);
			var v2 = exp2.Evaluate (ctx);
			if (IsInteger (v1) && IsInteger (v2))
				return GetInteger (v1) < GetInteger (v2);
			if (IsNumber (v1) && IsNumber (v2))
				return GetFloat (v1) < GetFloat (v2);
			if (v1 is string && v2 is string)
				return string.Compare ((string)v1, (string)v2, StringComparison.Ordinal) < 0;
			throw new EvaluationException ("Invalid operands for less-than operation");
		}
	}

	class LessThanOrEqualConditionExpression: BinaryConditionExpression
	{
		public LessThanOrEqualConditionExpression (ConditionExpression exp1, ConditionExpression exp2): base (exp1, exp2)
		{
		}

		public override object Evaluate (ExtensionContext ctx)
		{
			var v1 = exp1.Evaluate (ctx);
			var v2 = exp2.Evaluate (ctx);
			if (IsInteger (v1) && IsInteger (v2))
				return GetInteger (v1) <= GetInteger (v2);
			if (IsNumber (v1) && IsNumber (v2))
				return GetFloat (v1) <= GetFloat (v2);
			if (v1 is string && v2 is string)
				return string.Compare ((string)v1, (string)v2, StringComparison.Ordinal) <= 0;
			throw new EvaluationException ("Invalid operands for less-than operation");
		}
	}

	class AdditionConditionExpression: BinaryConditionExpression
	{
		public AdditionConditionExpression (ConditionExpression exp1, ConditionExpression exp2): base (exp1, exp2)
		{
		}

		public override object Evaluate (ExtensionContext ctx)
		{
			var v1 = exp1.Evaluate (ctx);
			var v2 = exp2.Evaluate (ctx);
			if (IsInteger (v1) && IsInteger (v2))
				return GetInteger (v1) + GetInteger (v2);
			if (IsNumber (v1) && IsNumber (v2))
				return GetFloat (v1) + GetFloat (v2);
			if (v1 is string && v2 is string)
				return ((string)v1) + ((string)v2);
			throw new EvaluationException ("Invalid operands for addition operation");
		}
	}

	class SubstractionConditionExpression: BinaryConditionExpression
	{
		public SubstractionConditionExpression (ConditionExpression exp1, ConditionExpression exp2): base (exp1, exp2)
		{
		}

		public override object Evaluate (ExtensionContext ctx)
		{
			var v1 = exp1.Evaluate (ctx);
			var v2 = exp2.Evaluate (ctx);
			if (IsInteger (v1) && IsInteger (v2))
				return GetInteger (v1) - GetInteger (v2);
			if (IsNumber (v1) && IsNumber (v2))
				return GetFloat (v1) - GetFloat (v2);
			throw new EvaluationException ("Invalid operands for substraction operation");
		}
	}

	class MultiplicationConditionExpression: BinaryConditionExpression
	{
		public MultiplicationConditionExpression (ConditionExpression exp1, ConditionExpression exp2): base (exp1, exp2)
		{
		}

		public override object Evaluate (ExtensionContext ctx)
		{
			var v1 = exp1.Evaluate (ctx);
			var v2 = exp2.Evaluate (ctx);
			if (IsInteger (v1) && IsInteger (v2))
				return GetInteger (v1) * GetInteger (v2);
			if (IsNumber (v1) && IsNumber (v2))
				return GetFloat (v1) * GetFloat (v2);
			throw new EvaluationException ("Invalid operands for multiplication operation");
		}
	}

	class DivisionConditionExpression: BinaryConditionExpression
	{
		public DivisionConditionExpression (ConditionExpression exp1, ConditionExpression exp2): base (exp1, exp2)
		{
		}

		public override object Evaluate (ExtensionContext ctx)
		{
			var v1 = exp1.Evaluate (ctx);
			var v2 = exp2.Evaluate (ctx);
			if (IsInteger (v1) && IsInteger (v2))
				return GetInteger (v1) / GetInteger (v2);
			if (IsNumber (v1) && IsNumber (v2))
				return GetFloat (v1) / GetFloat (v2);
			throw new EvaluationException ("Invalid operands for division operation");
		}
	}

	class ModulusConditionExpression: BinaryConditionExpression
	{
		public ModulusConditionExpression (ConditionExpression exp1, ConditionExpression exp2): base (exp1, exp2)
		{
		}

		public override object Evaluate (ExtensionContext ctx)
		{
			var v1 = exp1.Evaluate (ctx);
			var v2 = exp2.Evaluate (ctx);
			if (IsInteger (v1) && IsInteger (v2))
				return GetInteger (v1) % GetInteger (v2);
			throw new EvaluationException ("Invalid operands for modulus operation");
		}
	}

	class LiteralConditionExpression: ConditionExpression
	{
		object value;

		public LiteralConditionExpression (object ob)
		{
			value = ob;
		}

		public override object Evaluate (ExtensionContext ctx)
		{
			return value;
		}

		internal override void GetConditionTypes (List<string> listToFill)
		{
		}
	}

	class NamedConditionExpression: UnaryConditionExpression
	{
		public string Name { get; set; }

		public NamedConditionExpression (string name, ConditionExpression exp): base (exp)
		{
			Name = name;
		}

		public override object Evaluate (ExtensionContext ctx)
		{
			return exp.Evaluate (ctx);
		}
	}

	internal sealed class CustomConditionExpression: ConditionExpression
	{
		ExtensionNodeDescription node;
		string typeId;
		string addin;
		List<NamedConditionExpression> args;

		internal const string SourceAddinAttribute = "__sourceAddin"; 
		
		internal CustomConditionExpression (ExtensionNodeDescription elem)
		{
			node = elem;
			typeId = elem.GetAttribute ("id");
			addin = elem.GetAttribute (SourceAddinAttribute);
		}

		internal CustomConditionExpression (string name, List<NamedConditionExpression> args)
		{
			typeId = name;
			this.args = args;
			node = new ExtensionNodeDescription ();
		}
		
		public override object Evaluate (ExtensionContext ctx)
		{
			if (!string.IsNullOrEmpty (addin)) {
				// Make sure the add-in that implements the condition is loaded
				ctx.AddinEngine.LoadAddin (null, addin, true);
				addin = null; // Don't try again
			}
			
			ConditionType type = ctx.GetCondition (typeId);
			if (type == null) {
				ctx.AddinEngine.ReportError ("Condition '" + typeId + "' not found in current extension context.", null, null, false);
				return false;
			}

			if (args != null) {
				foreach (var arg in args)
					node.SetAttribute (arg.Name, arg.Evaluate (ctx).ToString ());
			}

			try {
				return type.Evaluate (node);
			}
			catch (Exception ex) {
				ctx.AddinEngine.ReportError ("Error while evaluating condition '" + typeId + "'", null, ex, false);
				return false;
			}
		}
		
		internal override void GetConditionTypes (List<string> listToFill)
		{
			listToFill.Add (typeId);
			if (args != null) {
				foreach (var e in args)
					e.GetConditionTypes (listToFill);
			}
		}
	}

	class ConditionPropertyExpression: ConditionExpression
	{
		string name;

		public ConditionPropertyExpression (string name)
		{
			this.name = name;
		}

		public override object Evaluate (ExtensionContext ctx)
		{
			return ctx.GetConditionProperty (name);
		}

		internal override void GetConditionTypes (List<string> listToFill)
		{
			listToFill.Add ("$" + name);
		}
	}

	class EvaluationException: Exception
	{
		public EvaluationException (string message) : base (message)
		{
		}
	}
}
