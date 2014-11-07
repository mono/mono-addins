//
// TestConditionParser.cs
//
// Author:
//       Lluis Sanchez Gual <lluis@xamarin.com>
//
// Copyright (c) 2014 Xamarin, Inc (http://www.xamarin.com)
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
using System;
using NUnit.Framework;
using Mono.Addins;
using System.Collections.Generic;

namespace UnitTests
{
	[TestFixture]
	public class TestConditionParser: TestBase
	{
		ExtensionContext context; 

		public override void Setup ()
		{
			base.Setup ();
			context = AddinManager.CreateExtensionContext ();
		}

		object Eval (string exp)
		{
			var e = ConditionParser.ParseCondition (exp);
			return e.Evaluate (context);
		}

		[Test]
		public void Literals ()
		{
			Assert.AreEqual (1, Eval ("1"));
			Assert.AreEqual (23, Eval ("23"));
			Assert.AreEqual (-3, Eval ("-3"));
			Assert.AreEqual (14.1d, Eval ("14.1"));
			Assert.AreEqual (-4.5d, Eval ("-4.5"));
			Assert.AreEqual ("hello", Eval ("'hello'"));
			Assert.AreEqual (true, Eval ("true"));
			Assert.AreEqual (false, Eval ("false"));
			Assert.AreEqual (true, Eval ("tRuE"));
			Assert.AreEqual (false, Eval ("FaLsE"));
		}

		[Test]
		public void MathOperations ()
		{
			Assert.AreEqual (1, Eval ("-(2-3)"));
			Assert.AreEqual (3, Eval ("-(-3)"));
			Assert.AreEqual (3, Eval ("--3"));
			Assert.AreEqual (3, Eval ("1+2"));
			Assert.AreEqual (12, Eval ("23-11"));
			Assert.AreEqual (6, Eval ("2*3"));
			Assert.AreEqual (2, Eval ("10/5"));
			Assert.AreEqual (1, Eval ("11%5"));
			Assert.AreEqual (7, Eval ("1+2*3"));
			Assert.AreEqual (7, Eval ("2*3+1"));
			Assert.AreEqual (9, Eval ("(1+2)*3"));
			Assert.AreEqual (8, Eval ("2*(3+1)"));
			Assert.AreEqual (3, Eval ("1+10/5"));
			Assert.AreEqual (3, Eval ("10/5+1"));
		}

		[Test]
		public void RelationalOperations ()
		{
			Assert.AreEqual (true, Eval ("2 == 2"));
			Assert.AreEqual (false, Eval ("1 == 2"));
			Assert.AreEqual (true, Eval ("'aa' == 'aa'"));
			Assert.AreEqual (false, Eval ("'aa' == 'bb'"));

			Assert.AreEqual (false, Eval ("2 != 2"));
			Assert.AreEqual (true, Eval ("1 != 2"));
			Assert.AreEqual (false, Eval ("'aa' != 'aa'"));
			Assert.AreEqual (true, Eval ("'aa' != 'bb'"));

			Assert.AreEqual (true, Eval ("2 > 1"));
			Assert.AreEqual (false, Eval ("1 > 2"));
			Assert.AreEqual (true, Eval ("'bb' > 'aa'"));
			Assert.AreEqual (false, Eval ("'aa' > 'bb'"));

			Assert.AreEqual (true, Eval ("2 >= 1"));
			Assert.AreEqual (true, Eval ("2 >= 2"));
			Assert.AreEqual (false, Eval ("1 >= 2"));
			Assert.AreEqual (true, Eval ("'bb' >= 'aa'"));
			Assert.AreEqual (true, Eval ("'bb' >= 'bb'"));
			Assert.AreEqual (false, Eval ("'aa' >= 'bb'"));

			Assert.AreEqual (false, Eval ("2 < 1"));
			Assert.AreEqual (false, Eval ("2 < 2"));
			Assert.AreEqual (true, Eval ("1 < 2"));
			Assert.AreEqual (false, Eval ("'bb' < 'aa'"));
			Assert.AreEqual (true, Eval ("'aa' < 'bb'"));

			Assert.AreEqual (false, Eval ("2 <= 1"));
			Assert.AreEqual (true, Eval ("2 <= 2"));
			Assert.AreEqual (true, Eval ("1 <= 2"));
			Assert.AreEqual (false, Eval ("'bb' <= 'aa'"));
			Assert.AreEqual (true, Eval ("'bb' <= 'bb'"));
			Assert.AreEqual (true, Eval ("'aa' <= 'bb'"));
		}

		[Test]
		public void BooleanOperations ()
		{
			Assert.AreEqual (false, Eval ("!true"));
			Assert.AreEqual (true, Eval ("!false"));

			Assert.AreEqual (true, Eval ("true and true"));
			Assert.AreEqual (false, Eval ("true and false"));
			Assert.AreEqual (false, Eval ("false and true"));
			Assert.AreEqual (false, Eval ("false and false"));

			Assert.AreEqual (true, Eval ("true or true"));
			Assert.AreEqual (true, Eval ("true or false"));
			Assert.AreEqual (true, Eval ("false or true"));
			Assert.AreEqual (false, Eval ("false or false"));

			Assert.AreEqual (true, Eval ("(1>2) or (1<2)"));
		}

		[Test]
		public void Properties ()
		{
			var ctx = AddinManager.CreateExtensionContext ();

			var e = ConditionParser.ParseCondition ("prop1");

			ctx.SetConditionProperty ("prop1", 14);
			Assert.AreEqual (14, e.Evaluate (ctx));

			ctx.SetConditionProperty ("prop1", "hi");
			Assert.AreEqual ("hi", e.Evaluate (ctx));

			e = ConditionParser.ParseCondition ("2 * (prop1 + 4)");

			ctx.SetConditionProperty ("prop1", 2);
			Assert.AreEqual (12, e.Evaluate (ctx));

			ctx.SetConditionProperty ("prop1", 0.6);
			Assert.AreEqual (9.2, e.Evaluate (ctx));

			e = ConditionParser.ParseCondition ("!some_bool");

			ctx.SetConditionProperty ("some_bool", false);
			Assert.AreEqual (true, e.Evaluate (ctx));

			ctx.SetConditionProperty ("some_bool", true);
			Assert.AreEqual (false, e.Evaluate (ctx));

			e = ConditionParser.ParseCondition ("ab.cd.d + ' there'");

			ctx.SetConditionProperty ("ab.cd.d", "hi");
			Assert.AreEqual ("hi there", e.Evaluate (ctx));
		}

		[Test]
		public void CustomCondition ()
		{
			var c = new IsHello ();
			context.RegisterCondition ("IsHello", c);
			context.SetConditionProperty ("val", 5);

			Assert.AreEqual (true, Eval ("IsHello('hello')"));
			Assert.AreEqual (true, Eval ("IsHello(value:'hello')"));
			Assert.AreEqual (false, Eval ("IsHello('bye')"));

			Assert.AreEqual (false, Eval ("IsHello('Hello')"));
			Assert.AreEqual (true, Eval ("IsHello('Hello',ignoreCase:true)"));
			Assert.AreEqual (true, Eval ("IsHello (ignoreCase:(1 < 2), value:'He' + 'llo')"));

			Assert.AreEqual (true, Eval ("val == 5 and IsHello('hello')"));
		}

		void CheckConditions (string exp, params string[] references)
		{
			var e = ConditionParser.ParseCondition (exp);
			List<string> list = new List<string> ();
			e.GetConditionTypes (list);
			Assert.That (list, Is.EquivalentTo (references));
		}

		[Test]
		public void GetConditionTypes ()
		{
			CheckConditions ("prop", "$prop");
			CheckConditions ("1 + (2 * (prop / 3) + bar)", "$prop", "$bar");
			CheckConditions ("foo(1)", "foo");
			CheckConditions ("1 + (2 * (prop(car) / 3) + bar(2))", "prop", "bar", "$car");
		}
	}

	class IsHello: ConditionType
	{
		public override bool Evaluate (NodeElement conditionNode)
		{
			var ignoreCase = string.Compare (conditionNode.GetAttribute ("ignoreCase"), "true", StringComparison.OrdinalIgnoreCase) == 0;
			return string.Compare (conditionNode.GetAttribute ("value"), "hello", ignoreCase) == 0;
		}
	}
}

