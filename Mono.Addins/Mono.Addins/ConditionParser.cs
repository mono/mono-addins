//
// ConditionParser.cs
//
// Author:
//   Marek Sieradzki (marek.sieradzki@gmail.com)
//   Jaroslaw Kowalski <jaak@jkowalski.net>
//   Lluis Sanchez <lluis@xamarin.com>
// 
// (C) 2006 Marek Sieradzki
// (C) 2004-2006 Jaroslaw Kowalski
// (C) 2014 Lluis Sanchez
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


using System;
using System.Collections.Generic;
using System.Globalization;

namespace Mono.Addins {

	class ConditionParser
	{
		ConditionTokenizer tokenizer;

		ConditionParser (string condition)
		{
			tokenizer = new ConditionTokenizer ();
			tokenizer.Tokenize (condition);
		}
		
		public static ConditionExpression ParseCondition (string condition)
		{
			var parser = new ConditionParser (condition);
			var e = parser.ParseExpression ();
			
			if (!parser.tokenizer.IsEOF ())
				throw new ExpressionParseException (String.Format ("Unexpected token at end of condition: \"{0}\"", parser.tokenizer.Token.Value));
			
			return e;
		}
		
		ConditionExpression ParseExpression ()
		{
			return ParseBooleanExpression ();
		}
		
		ConditionExpression ParseBooleanExpression ()
		{
			return ParseBooleanAnd ();
		}
		
		ConditionExpression ParseBooleanAnd ()
		{
			ConditionExpression e = ParseBooleanOr ();
			
			while (tokenizer.IsToken (TokenType.And)) {
				tokenizer.GetNextToken ();
				e = new AndConditionExpression (e, ParseBooleanOr ());
			}
			
			return e;
		}
		
		ConditionExpression ParseBooleanOr ()
		{
			ConditionExpression e = ParseRelationalExpression ();
			
			while (tokenizer.IsToken (TokenType.Or)) {
				tokenizer.GetNextToken ();
				e = new OrConditionExpression (e, ParseRelationalExpression ());
			}
			
			return e;
		}
		
		ConditionExpression ParseRelationalExpression ()
		{
			ConditionExpression e = ParseArithmeticExpression ();
			
			Token opToken;

			if (tokenizer.IsToken (TokenType.Less) ||
				tokenizer.IsToken (TokenType.Greater) ||
				tokenizer.IsToken (TokenType.Equal) ||
				tokenizer.IsToken (TokenType.NotEqual) ||
				tokenizer.IsToken (TokenType.LessOrEqual) ||
				tokenizer.IsToken (TokenType.GreaterOrEqual)) {
				
				opToken = tokenizer.Token;
				tokenizer.GetNextToken ();
								
				switch (opToken.Type) {
				case TokenType.Equal:
					e = new EqualsConditionExpression (e, ParseArithmeticExpression ());
					break;
				case TokenType.NotEqual:
					e = new NotEqualsConditionExpression (e, ParseArithmeticExpression ());
					break;
				case TokenType.Less:
					e = new LessThanConditionExpression (e, ParseArithmeticExpression ());
					break;
				case TokenType.LessOrEqual:
					e = new LessThanOrEqualConditionExpression (e, ParseArithmeticExpression ());
					break;
				case TokenType.Greater:
					e = new GreaterThanConditionExpression (e, ParseArithmeticExpression ());
					break;
				case TokenType.GreaterOrEqual:
					e = new GreaterThanOrEqualConditionExpression (e, ParseArithmeticExpression ());
					break;
				default:
					throw new ExpressionParseException (String.Format ("Wrong relation operator {0}", opToken.Value));
				}
			}
			
			return e;
		}

		ConditionExpression ParseArithmeticExpression ()
		{
			ConditionExpression e = ParseArithmeticFactorExpression ();

			if (tokenizer.IsToken (TokenType.Addition)) {
				tokenizer.GetNextToken ();
				return new AdditionConditionExpression (e, ParseArithmeticExpression ());
			}
			if (tokenizer.IsToken (TokenType.Substraction)) {
				tokenizer.GetNextToken ();
				return new SubstractionConditionExpression (e, ParseArithmeticExpression ());
			}

			return e;
		}

		ConditionExpression ParseArithmeticFactorExpression ()
		{
			ConditionExpression e = ParseFactorExpression ();

			if (tokenizer.IsToken (TokenType.Multiplication)) {
				tokenizer.GetNextToken ();
				return new MultiplicationConditionExpression (e, ParseArithmeticFactorExpression ());
			}
			if (tokenizer.IsToken (TokenType.Division)) {
				tokenizer.GetNextToken ();
				return new DivisionConditionExpression (e, ParseArithmeticFactorExpression ());
			}
			if (tokenizer.IsToken (TokenType.Modulus)) {
				tokenizer.GetNextToken ();
				return new ModulusConditionExpression (e, ParseArithmeticFactorExpression ());
			}
			return e;
		}

		ConditionExpression ParseFactorExpression ()
		{
			ConditionExpression e;
			Token token = tokenizer.Token;
			tokenizer.GetNextToken ();

			if (token.Type == TokenType.LeftParen) {
				e = ParseExpression ();
				tokenizer.Expect (TokenType.RightParen);
			} else if (token.Type == TokenType.Property && tokenizer.Token.Type == TokenType.LeftParen) {
				e = ParseFunctionExpression (token.Value);
			} else if (token.Type == TokenType.Property && tokenizer.Token.Type == TokenType.Colon) {
				tokenizer.GetNextToken ();
				e = new NamedConditionExpression (token.Value, ParseExpression ());
			} else if (token.Type == TokenType.Property) {
				if (token.Value.Equals ("true", StringComparison.OrdinalIgnoreCase))
					e = new LiteralConditionExpression (true);
				else if (token.Value.Equals ("false", StringComparison.OrdinalIgnoreCase))
					e = new LiteralConditionExpression (false);
				else
					e = new ConditionPropertyExpression (token.Value);
			} else if (token.Type == TokenType.String) {
				e = new LiteralConditionExpression (token.Value);
			} else if (token.Type == TokenType.Number && token.Value.IndexOf ('.') != -1) {
				e = new LiteralConditionExpression (double.Parse (token.Value, CultureInfo.InvariantCulture));
			} else if (token.Type == TokenType.Number) {
				e = new LiteralConditionExpression (int.Parse (token.Value, CultureInfo.InvariantCulture));
			} else if (token.Type == TokenType.Not) {
				e = ParseNotExpression ();
			} else if (token.Type == TokenType.Substraction) {
				e = new NegateConditionExpression (ParseFactorExpression ());
			} else
				throw new ExpressionParseException (String.Format ("Unexpected token type {0}.", token.Type));
			
			return e;
		}

		ConditionExpression ParseNotExpression ()
		{
			return new NotConditionExpression (ParseFactorExpression ());
		}

		ConditionExpression ParseFunctionExpression (string functionName)
		{
			return new FunctionConditionExpression (functionName, ParseFunctionArguments ());
		}

		List <NamedConditionExpression> ParseFunctionArguments ()
		{
			var list = new List <NamedConditionExpression> ();
			NamedConditionExpression e;
			tokenizer.GetNextToken ();

			while (true) {
				if (tokenizer.Token.Type == TokenType.RightParen) {
					tokenizer.GetNextToken ();
					break;
				}
				if (tokenizer.Token.Type == TokenType.Comma) {
					tokenizer.GetNextToken ();
					continue;
				}
					
				e = ParseFunctionArgument ();
				list.Add (e);
			}
			
			return list;
		}

		NamedConditionExpression ParseFunctionArgument ()
		{
			var e = ParseExpression ();

			if (e is NamedConditionExpression)
				return (NamedConditionExpression)e;
			else
				return new NamedConditionExpression ("value", e);
		}

		void ExpectToken (TokenType type)
		{
			if (tokenizer.Token.Type != type)
				throw new ExpressionParseException ("Expected token type of type: " + type + ", got " +
						tokenizer.Token.Type + " (" + tokenizer.Token.Value + ") .");
		}
	}
}

