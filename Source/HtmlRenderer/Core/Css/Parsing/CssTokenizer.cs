namespace TheArtOfDev.HtmlRenderer.Core.Css.Parsing
{
	using System;
	using System.Collections.Generic;
	using System.Globalization;
	using System.IO;
	using System.Linq;
	using System.Text;
	using TheArtOfDev.HtmlRenderer.Core.Utils;

	public class CssTokenizer
	{
		private const int MAX_CODE_POINT = 0x10FFFF;
		private const char REPLACEMENT_CHAR = (char)0xFFFD;

		private readonly CssReader _reader;
		private readonly CssTokenFactory _factory;
		private readonly StringBuilder _valueBuilder = new StringBuilder();
		private int _lineCount;
		private int _linePos;

		public EventHandler<CssErrorEventArgs> ParseError = delegate {};

		private CssTokenizer(CssReader reader)
		{
			ArgChecker.AssertArgNotNull(reader, nameof(reader));
			_reader = reader;
			_factory = new CssTokenFactory();
		}

		public static IEnumerable<CssToken> Tokenize(string input)
		{
			return input != null 
				? new CssTokenizer(new CssReader(new StringReader(input))).Tokenize()
				: Enumerable.Empty<CssToken>();
		}

		//public static IEnumerable<CssToken> Tokenize(string input, int start, int count)
		//{
		//	return new CssTokenizer(new CssReader(new StringReader(input))).Tokenize();
		//}

		private IEnumerable<CssToken> Tokenize()
		{
			CssToken token = default(CssToken);

			int ch;
			while ((ch = _reader.Peek()) >= 0)
			{
				switch (ch)
				{
					case ' ':
					case '\t':
					case '\f':
					case '\r':
					case '\n':
						yield return ConsumeWhitespaceToken();
						continue;

					case ',':
					case ':':
					case ';':
					case '(':
					case '[':
					case '{':
					case ')':
					case ']':
					case '}':
						yield return _factory.CreateToken((char)_reader.Read());
						continue;

					case '\'':
					case '"':
						yield return ConsumeStringToken();
						continue;

					case '#':
						_reader.Read();

						string hashNameOrIdentifier;
						if (TryConsumeIdentifier(out hashNameOrIdentifier))
						{
							yield return _factory.CreateHashToken(hashNameOrIdentifier, true);
						}
						else if (TryConsumeName(out hashNameOrIdentifier))
						{
							yield return _factory.CreateHashToken(hashNameOrIdentifier, false);
						}
						else
						{
							yield return _factory.CreateToken((char)ch);
						}
						continue;

					case '^':
					case '$':
					case '*':
					case '~':
						_reader.Read();
						yield return _reader.TryRead("=") 
							? _factory.CreateOperatorToken((char) ch) 
							: _factory.CreateToken((char) ch);
						continue;

					case '+':
					case '.':
						if (TryConsumeNumericToken(ref token))
						{
							yield return token;
							continue;
						}

						yield return _factory.CreateToken((char) _reader.Read());
						continue;

					case '-':
						if (TryConsumeNumericToken(ref token) || TryConsumeIdentifierToken(ref token))
						{
							yield return token;
							continue;
						}

						yield return _reader.TryRead("-->")
							? _factory.CreateCdcToken()
							: _factory.CreateToken((char) _reader.Read());
						continue;

					case '/':
						string comment;
						if (!TryConsumeComment(out comment))
						{
							yield return _factory.CreateToken((char)_reader.Read());
						}
						continue;

					case '<':
						yield return _reader.TryRead("<!--") 
							? _factory.CreateCdoToken() 
							: _factory.CreateToken((char)_reader.Read());
						continue;

					case '@':
						_reader.Read();

						string identifier;
						yield return TryConsumeIdentifier(out identifier)
							? _factory.CreateAtKeywordToken(identifier)
							: _factory.CreateToken((char) ch);
						continue;

					case '\\':
						if (TryConsumeIdentifierToken(ref token))
						{
							yield return token;
							continue;
						}

						NotifyError("Invalid escape sequence.");

						yield return _factory.CreateToken((char) _reader.Read());
						continue;

					case '|':
						_reader.Read();
						switch (_reader.Peek())
						{
							case '=':
								yield return _factory.CreateOperatorToken((char) ch);
								_reader.Read();
								break;
							case '|':
								yield return _factory.CreateColumnToken();
								_reader.Read();
								break;
							default:
								yield return _factory.CreateToken((char) ch);
								break;
						}
						continue;

					case 'u':
					case 'U':
						if (_reader.Peek(1) == '+' && ((ch = _reader.Peek(2)) == '?' || IsHexDigit(ch)))
						{
							_reader.Read();
							_reader.Read();
							yield return ConsumeUnicodeRangeToken();
						}
						else
						{
							yield return ConsumeIdentifierLikeToken();
						}
						continue;

					default:
						if (TryConsumeNumericToken(ref token))
						{
							yield return token;
							continue;
						}
						if (IsNameStartCodePoint((char) ch))
						{
							yield return ConsumeIdentifierLikeToken();
							continue;
						}

						yield return _factory.CreateToken((char)_reader.Read());
						continue;
				}
			}
		}

		private CssToken ConsumeIdentifierLikeToken()
		{
			_valueBuilder.Length = 0;
			_valueBuilder.Append((char)_reader.Read());
			TryConsumeName(_valueBuilder);

			var name = BuildValue();
			if (_reader.Peek() == '(')
			{
				_reader.Read();
				if (name.Equals("url", StringComparison.OrdinalIgnoreCase))
				{
					bool isInvalid;
					var url = ConsumeUrl(out isInvalid);
					return _factory.CreateUrlToken(url, isInvalid);
				}
				return _factory.CreateFunctionToken(name);
			}
			return _factory.CreateIdentifierToken(name);
		}

		private CssToken ConsumeStringToken()
		{
			_valueBuilder.Length = 0;

			// Consume leading string delimiter
			var delimiter = _reader.Read();
			var isComplete = false;

			int ch;
			while ((ch = _reader.Peek()) >= 0 && !isComplete)
			{
				switch (ch)
				{
					case '\n':
					case '\r':
					case '\f':
						NotifyError("Unescaped newline character in quoted string literal.");
						return _factory.CreateStringToken(BuildValue(), true);

					case '\\':
						var ch1 = _reader.Peek(1);
						if (IsNewLine(ch1))
						{
							_reader.Read();
							TryConsumeNewline();
							break;
						}
						if (IsValidEscape(ch, ch1))
						{
							_reader.Read();
							ConsumeEscapedCodePoint(_valueBuilder);
							break;
						}
						goto default;

					default:
						_reader.Read();
						if (ch != delimiter)
						{
							_valueBuilder.Append((char) ch);
						}
						else
						{
							isComplete = true;
						}
						break;
				}
			}

			return _factory.CreateStringToken(BuildValue(), false);
		}

		private string ConsumeUrl(out bool isInvalid)
		{
			while (TryConsumeWhitespace()) {}

			var ch = _reader.Peek();
			if (ch < 0)
			{
				isInvalid = false;
				return null;
			}

			// Handle quoted string values
			switch (ch)
			{
				case '\'':
				case '"':
					var stringToken = ConsumeStringToken();
					if (!stringToken.IsInvalid)
					{
						while (TryConsumeWhitespace()) { }

						if (_reader.Peek() == ')')
						{
							_reader.Read();
							isInvalid = false;
							return stringToken.StringValue;
						}
					}

					ConsumeBadUrlRemnants();
					isInvalid = true;
					return stringToken.StringValue;

				default:
					_valueBuilder.Length = 0;
					do
					{
						switch (ch)
						{
							case ')':
								_reader.Read();
								isInvalid = false;
								return BuildValue();

							case '\'':
							case '"':
							case '(':
								NotifyError("Unexpected '(' character in url literal");
								ConsumeBadUrlRemnants();
								isInvalid = true;
								return BuildValue();

							case '\\':
								_reader.Read();
								if (IsValidEscape(ch, _reader.Peek()))
								{
									ConsumeEscapedCodePoint(_valueBuilder);
								}
								else
								{
									NotifyError("Invalid escape sequence in url literal");
									ConsumeBadUrlRemnants();
									isInvalid = true;
									return BuildValue();
								}
								break;

							default:
								if (IsNonPrintable(ch))
								{
									NotifyError("Non-printable character in url literal");
									ConsumeBadUrlRemnants();
									isInvalid = true;
									return BuildValue();
								}

								_valueBuilder.Append((char)_reader.Read());
								break;
						}
						while (TryConsumeWhitespace()) { }

						ch = _reader.Peek();
					} while (ch >= 0);

					isInvalid = false;
					return BuildValue();
			}
		}

		private void ConsumeBadUrlRemnants()
		{
			int ch;
			while ((ch = _reader.Peek()) >= 0)
			{
				switch (ch)
				{
					case ')':
						return;
					case '\\':
						_reader.Read();
						ConsumeEscapedCodePoint(null);
						break;
					default:
						_reader.Read();
						break;
				}
			}
		}

		private bool TryConsumeIdentifierToken(ref CssToken result)
		{
			string identifier;
			if (!TryConsumeIdentifier(out identifier)) return false;

			result = _factory.CreateIdentifierToken(identifier);
			return true;
		}

		private bool TryConsumeNumericToken(ref CssToken result)
		{
			_valueBuilder.Length = 0;

			bool isFloatingPoint;
			if (!TryConsumeNumber(_valueBuilder, out isFloatingPoint)) return false;

			var value = double.Parse(BuildValue(), CultureInfo.InvariantCulture);

			if (_reader.TryRead("%"))
			{
				result = _factory.CreateNumericToken(isFloatingPoint, value, "%");
				return true;
			}

			string identifier;
			if (TryConsumeIdentifier(out identifier))
			{
				result = _factory.CreateNumericToken(isFloatingPoint, value, identifier);
				return true;
			}

			result = _factory.CreateNumericToken(isFloatingPoint, value, null);
			return true;
		}

		private CssToken ConsumeUnicodeRangeToken()
		{
			var ch = _reader.Peek();

			_valueBuilder.Length = 0;
			while (_valueBuilder.Length < 6 && IsHexDigit(ch))
			{
				_valueBuilder.Append((char)_reader.Read());
				ch = _reader.Peek();
			}

			int rangeStart, rangeEnd;
			if (_valueBuilder.Length < 6 && ch == '?')
			{
				do
				{
					_valueBuilder.Append((char) _reader.Read());
					ch = _reader.Peek();
				} while (_valueBuilder.Length < 6 && ch == '?');

				var hexNumber = BuildValue();
				rangeStart = int.Parse(hexNumber.Replace('?', '0'), NumberStyles.AllowHexSpecifier);
				rangeEnd = int.Parse(hexNumber.Replace('?', 'F'), NumberStyles.AllowHexSpecifier);
			}
			else
			{
				rangeStart = rangeEnd = int.Parse(BuildValue(), NumberStyles.AllowHexSpecifier);
			}

			if (ch == '-' && IsHexDigit(_reader.Peek(1)))
			{
				_reader.Read();
				_valueBuilder.Length = 0;
				do
				{
					_valueBuilder.Append((char)_reader.Read());
					ch = _reader.Peek();
				} while (_valueBuilder.Length < 6 && IsHexDigit(ch));

				var hexNumber = BuildValue();
				rangeEnd = int.Parse(hexNumber, NumberStyles.AllowHexSpecifier);
			}

			return _factory.CreateUnicodeRangeToken(rangeStart, rangeEnd);
		}

		private CssToken ConsumeWhitespaceToken()
		{
			while (TryConsumeWhitespace()) {}
			return _factory.CreateWhitespaceToken();
		}

		private bool TryConsumeComment(out string comment)
		{
			if (!_reader.TryRead("/*"))
			{
				comment = null;
				return false;
			}

			_reader.Read();
			var ch =_reader.Read();
			while (ch >= 0 && ch != '*' && _reader.Peek(1) != '/')
			{
				_valueBuilder.Append((char) ch);
				ch = _reader.Read();
			}
			_reader.Read();
			_reader.Read();

			comment = BuildValue();
			return true;
		}

		private bool TryConsumeWhitespace()
		{
			switch (_reader.Peek())
			{
				case '\t':
				case ' ':
					_reader.Read();
					return true;
				default:
					return TryConsumeNewline();
			}
		}

		private bool TryConsumeNewline()
		{
			switch (_reader.Peek())
			{
				case '\n':
					_reader.Read();
					_lineCount++;
					_linePos = _reader.Position;
					return true;
				case '\f':
				case ' ':
					_reader.Read();
					return true;
				case '\r':
					_reader.Read();
					if (_reader.Peek() == '\n') _reader.Read();
					_lineCount++;
					_linePos = _reader.Position;
					return true;
				default:
					return false;
			}
		}

		private bool TryConsumeIdentifier(out string result)
		{
			_valueBuilder.Length = 0;
			if (!TryConsumeIdentifierStart(_valueBuilder))
			{
				result = null;
				return false;
			}

			// Consume any remainder of identifier
			TryConsumeName(_valueBuilder);

			result = BuildValue();
			return true;
		}

		private bool TryConsumeIdentifierStart(StringBuilder valueBuilder)
		{
			var ch = _reader.Peek();
			var lookaheadIndex = 1;
			if (ch == '-')
			{
				ch = _reader.Peek(lookaheadIndex++);
			}

			if (IsNameStartCodePoint(ch))
			{
				_reader.Read(valueBuilder, lookaheadIndex);
				return true;
			}

			if (IsValidEscape(ch, _reader.Peek(lookaheadIndex)))
			{
				_reader.Read(valueBuilder, lookaheadIndex);
				valueBuilder.Length--;
				ConsumeEscapedCodePoint(valueBuilder);
				return true;
			}

			return false;
		}

		private bool TryConsumeName(out string result)
		{
			_valueBuilder.Length = 0;
			if (TryConsumeName(_valueBuilder))
			{
				result = BuildValue();
				return true;
			}

			result = null;
			return false;
		}

		private bool TryConsumeName(StringBuilder valueBuilder)
		{
			var namePosition = _reader.Position;

			int ch;
			while ((ch = _reader.Peek()) >= 0)
			{
				if (IsNameCodePoint(ch))
				{
					valueBuilder.Append((char)_reader.Read());
				}
				else if (IsValidEscape(ch, _reader.Peek(1)))
				{
					_reader.Read();
					ConsumeEscapedCodePoint(valueBuilder);
				}
				else
				{
					break;
				}
			}

			return _reader.Position > namePosition;
		}

		private void ConsumeEscapedCodePoint(StringBuilder valueBuilder)
		{
			var ch = _reader.Read();

			// EOF
			if (ch < 0)
			{
				valueBuilder?.Append(REPLACEMENT_CHAR);
				return;
			}

			// Hex encoded code point
			int hexDigit;
			if (!TryParseHexDigit(ch, out hexDigit))
			{
				valueBuilder?.Append((char) ch);
				return;
			}

			int codePoint = hexDigit;
			int hexDigits = 1;
			while (hexDigits < 6 && (ch = _reader.Peek()) >= 0 && TryParseHexDigit(ch, out hexDigit))
			{
				_reader.Read();
				codePoint = (codePoint << 4) + hexDigit;
				hexDigits++;
			}
			TryConsumeWhitespace();

			if (valueBuilder != null)
			{
				if (codePoint == 0 || codePoint > MAX_CODE_POINT || IsSurrogateCodePoint(codePoint))
				{
					valueBuilder.Append(REPLACEMENT_CHAR);
				}
				else if (codePoint <= char.MaxValue)
				{
					valueBuilder.Append((char) codePoint);
				}
				else
				{
					valueBuilder.Append(char.ConvertFromUtf32(codePoint));
				}
			}
		}

		private bool TryConsumeNumber(StringBuilder valueBuilder, out bool isFloatingPoint)
		{
			isFloatingPoint = false;

			// Read optional sign
			var foundFraction = false;

			var lookaheadIndex = 0;
			var ch = _reader.Peek(lookaheadIndex++);
			if (ch == '+' || ch == '-')
			{
				ch = _reader.Peek(lookaheadIndex++);
			}
			if (ch == '.')
			{
				foundFraction = true;
				ch = _reader.Peek(lookaheadIndex++);
			}
			if (!IsDigit(ch)) return false;

			_reader.Read(valueBuilder, lookaheadIndex - 1);
			ch = _reader.Peek();

			if (!foundFraction)
			{
				// Read additional integer digits
				while (IsDigit(ch))
				{
					valueBuilder.Append((char) _reader.Read());
					ch = _reader.Peek();
				}

				if (ch == '.' && IsDigit(_reader.Peek(1)))
				{
					foundFraction = true;
					_reader.Read(valueBuilder, 2);
					ch = _reader.Peek();
				}
			}

			// Test for optional decimal point and first decimal digit
			if (foundFraction)
			{
				// Read additional integer digits
				while (IsDigit(ch))
				{
					valueBuilder.Append((char)_reader.Read());
					ch = _reader.Peek();
				}

				isFloatingPoint = true;
			}

			// Test for exponent
			if (ch == 'e' || ch == 'E')
			{
				// Test for optional sign
				lookaheadIndex = 1;
				ch = _reader.Peek(lookaheadIndex++);
				if (ch == '+' || ch == '-')
				{
					ch = _reader.Peek(lookaheadIndex++);
				}

				// Test for exponent digit
				if (IsDigit(ch))
				{
					// Read exponent and optional sign
					_reader.Read(valueBuilder, lookaheadIndex - 1);

					do
					{
						valueBuilder.Append((char)_reader.Read());
						ch = _reader.Peek();
					} while (IsDigit(ch));

					isFloatingPoint = true;
				}
			}

			return true;
		}

		private bool IsValidEscape(int ch1, int ch2)
		{
			return ch1 == '\\' && !IsNewLine(ch2);
		}

		private static bool IsDigit(int ch)
		{
			return ch >= '0' && ch <= '9';
		}

		private static bool IsHexDigit(int ch)
		{
			return IsDigit(ch)
				|| (ch >= 'a' && ch <= 'f')
			    || (ch >= 'A' && ch <= 'F');
		}

		private static bool IsNonPrintable(int ch)
		{
			return (ch >= '\u0000' && ch <= '\u0008')
				|| ch == '\u000B'
				|| (ch >= '\u000E' && ch <= '\u001F')
				|| ch == '\u007F';
		}

		private static bool TryParseHexDigit(int ch, out int result)
		{
			if (ch >= '0' && ch <= '9')
			{
				result = ch - '0';
			}
			else if (ch >= 'A' && ch <= 'F')
			{
				result =  ch + 10 - 'A';
			}
			else if (ch >= 'a' && ch <= 'f')
			{
				result = ch + 10 - 'a';
			}
			else
			{
				result = 0;
				return false;
			}
			return true;
		}

		private static bool IsLetter(int ch)
		{
			return (ch >= 'a' && ch <= 'z') || (ch >= 'A' && ch <= 'Z');
		}

		private static bool IsNewLine(int ch)
		{
			switch (ch)
			{
				case '\n':
				case '\r':
				case '\f':
					return true;
				default:
					return false;
			}
		}

		private static bool IsNonAscii(int ch)
		{
			return ch > '\u0080';
		}

		private static bool IsNameStartCodePoint(int ch)
		{
			return IsLetter(ch) || ch == '_' || IsNonAscii(ch);
		}

		private static bool IsNameCodePoint(int ch)
		{
			return IsNameStartCodePoint(ch) || IsDigit(ch) || ch == '-';
		}

		private static bool IsSurrogateCodePoint(int codePoint)
		{
			return codePoint >= 0xD800 && codePoint <= 0xDFFF;
		}

		private string BuildValue()
		{
			var result = _valueBuilder.ToString();
			_valueBuilder.Length = 0;
			return result;
		}

		private void NotifyError(string message)
		{
			ParseError(this, new CssErrorEventArgs(_lineCount, _reader.Position - _linePos, message));
		}
	}
}
