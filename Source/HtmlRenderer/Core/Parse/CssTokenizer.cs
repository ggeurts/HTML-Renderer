namespace TheArtOfDev.HtmlRenderer.Core.Parse
{
	using System;
	using System.Collections.Generic;
	using System.Globalization;
	using System.Text;

	public class CssTokenizer
	{
		private const int MAX_CODE_POINT = 0x10FFFF;
		private const char REPLACEMENT_CHAR = (char)0xFFFD;

		private readonly string _input;
		private readonly int _start;
		private readonly int _eof;
		private readonly CssTokenFactory _factory;
		private readonly StringBuilder _valueBuilder = new StringBuilder();

		private CssTokenizer(string input, int start, int count)
		{
			if (input == null) throw new ArgumentNullException(nameof(input));
			if (start < 0 || start >= input.Length)
			{
				throw new ArgumentOutOfRangeException(nameof(start), "start must be greater than or equal to 0 and less than input length");
			}
			if (count < 0 || count + start > input.Length)
			{
				throw new ArgumentOutOfRangeException(nameof(count), "count must be greater than or equal to 0 and count + index must be less than or equal to input length");
			}

			_input = input;
			_factory = new CssTokenFactory(input);
			_start = start;
			_eof = start + count;
		}

		public static IEnumerable<CssToken> Tokenize(string input, int start, int count)
		{
			return new CssTokenizer(input, start, count).Tokenize();
		}

		private IEnumerable<CssToken> Tokenize()
		{
			CssToken token = default(CssToken);

			if (_start >= _eof) yield break;

			var i = _start;
			while (i < _eof)
			{
				if (TryConsumeWhitespaceToken(ref i, ref token))
				{
					yield return token;
					if (i >= _eof) break;
				}

				var ch = _input[i];
				switch (ch)
				{
					case ',':
					case ':':
					case ';':
					case '(':
					case '[':
					case '{':
					case ')':
					case ']':
					case '}':
						yield return _factory.CreateToken(i++);
						break;

					case '\'':
					case '"':
						yield return ConsumeStringToken(ref i);
						break;

					case '#':
						yield return ConsumeHashToken(ref i);
						break;

					case '^':
					case '$':
					case '*':
					case '~':
						if (i < _eof - 1 && _input[i+1] == '=')
						{
							yield return _factory.CreateOperatorToken(i);
							i += 2;
							break;
						}
						yield return _factory.CreateToken(i++);
						break;

					case '+':
					case '.':
						if (TryConsumeNumericToken(ref i, ref token))
						{
							yield return token;
							break;
						}
						yield return _factory.CreateToken(i++);
						break;

					case '-':
						if (TryConsumeNumericToken(ref i, ref token) || TryConsumeIdentifierToken(ref i, ref token))
						{
							yield return token;
							break;
						}
						if (i < _eof - 2 && _input[i+1] == '-' && _input[i+2] == '>')
						{
							yield return _factory.CreateCdcToken(i);
							i += 3;
							break;
						}
						yield return _factory.CreateToken(i++);
						break;

					case '/':
						if (!TryConsumeComment(ref i))
						{
							yield return _factory.CreateToken(i++);
						}
						break;

					case '<':
						if (i < _eof - 2 && _input[i + 1] == '-' && _input[i + 2] == '-')
						{
							yield return _factory.CreateCdoToken(i);
							i += 3;
							break;
						}
						yield return _factory.CreateToken(i++);
						break;

					case '@':
						var atStart = i++;
						string identifier;
						if (TryConsumeIdentifier(ref i, out identifier))
						{
							yield return _factory.CreateAtKeywordToken(atStart, i - atStart, identifier);
							break;
						}
						yield return _factory.CreateToken(atStart);
						break;

					case '\\':
						if (TryConsumeIdentifierToken(ref i, ref token))
						{
							yield return token;
							break;
						}

						// Parse error
						yield return _factory.CreateToken(i++);
						break;

					case '|':
						if (i < _eof - 1)
						{
							if (_input[i + 1] == '=')
							{
								yield return _factory.CreateOperatorToken(i);
								i += 2;
								break;
							}
							if (_input[i + 1] == '|')
							{
								yield return _factory.CreateColumnToken(i);
								i += 2;
								break;
							}
						}
						yield return _factory.CreateToken(i++);
						break;

					case 'u':
					case 'U':
						if (TryConsumeUnicodeRangeToken(ref i, ref token))
						{
							yield return token;
							break;
						}
						yield return ConsumeIdentifierLikeToken(ref i);
						break;

					default:
						if (IsDigit(_input[i]))
						{
							TryConsumeNumericToken(ref i, ref token);
							yield return token;
							break;
						}
						if (IsNameStartCodePoint(_input[i]))
						{
							yield return ConsumeIdentifierLikeToken(ref i);
						}

						yield return _factory.CreateToken(i++);
						break;
				}
			}
		}

		private CssToken ConsumeHashToken(ref int i)
		{
			var tokenStart = i++;
			_valueBuilder.Length = 0;

			// Try to consume name, otherwise return # as delimiter
			string identifier;
			if (TryConsumeIdentifier(ref i, out identifier))
			{
				return _factory.CreateHashToken(tokenStart, i - tokenStart, identifier, true);
			}

			string name;
			if (TryConsumeName(ref i, out name))
			{
				return _factory.CreateHashToken(tokenStart, i - tokenStart, name, false);
			}

			return _factory.CreateToken(tokenStart);
		}

		private CssToken ConsumeIdentifierLikeToken(ref int i)
		{
			var tokenStart = i;
			_valueBuilder.Length = 0;
			_valueBuilder.Append(_input[i++]);
			TryConsumeName(ref i, _valueBuilder);

			var name = BuildValue();
			if (i < _eof && _input[i] == '(')
			{
				i++;
				if (name.Equals("url", StringComparison.OrdinalIgnoreCase))
				{
					bool isInvalid;
					var url = ConsumeUrl(ref i, out isInvalid);
					return _factory.CreateUrlToken(tokenStart, i - tokenStart, url, isInvalid);
				}
				return _factory.CreateFunctionToken(tokenStart, i - tokenStart, name);
			}
			return _factory.CreateIdentifierToken(tokenStart, i - tokenStart, name);
		}

		private CssToken ConsumeStringToken(ref int i)
		{
			var tokenStart = i;
			_valueBuilder.Length = 0;

			// Consume leading string delimiter
			var delimiter = _input[i++];

			char ch;
			while (i < _eof && (ch = _input[i]) != delimiter)
			{
				switch (ch)
				{
					case '\n':
					case '\r':
					case '\f':
						// Bad string token
						return _factory.CreateStringToken(tokenStart, i - tokenStart, BuildValue(), true);
					case '\\':
						// Escaped code point
						i++;
						if (i >= _eof || TryConsumeNewline(ref i)) continue;

						_valueBuilder.Append(ConsumeEscapedCodePoint(ref i));
						break;
					default:
						_valueBuilder.Append(ch);
						break;
				}
				i++;
			}

			return _factory.CreateStringToken(tokenStart, i - tokenStart, BuildValue(), false);
		}

		private string ConsumeUrl(ref int i, out bool isInvalid)
		{
			while (TryConsumeWhitespace(ref i)) {}
			if (i >= _eof)
			{
				isInvalid = false;
				return null;
			}

			switch (_input[i])
			{
				case '\'':
				case '"':
					var stringToken = ConsumeStringToken(ref i);
					if (!stringToken.IsBadStringToken)
					{
						while (TryConsumeWhitespace(ref i)) { }
						if (i >= _eof || _input[i] == ')')
						{
							i++;
							isInvalid = false;
							return stringToken.StringValue;
						}
					}

					ConsumeBadUrlRemnants(ref i);
					isInvalid = true;
					return stringToken.StringValue;
			}

			_valueBuilder.Length = 0;
			isInvalid = false;
			do
			{
				var ch = _input[i];
				switch (_input[i])
				{
					case ')':
						i++;
						return BuildValue();
					case '\'':
					case '"':
					case '(':
						// Parse error
						isInvalid = true;
						break;
					case '\\':
						if (IsValidEscape(i))
						{
							i++;
							_valueBuilder.Append(ConsumeEscapedCodePoint(ref i));
							break;
						}

						// Parse error
						isInvalid = true;
						break;
					default:
						_valueBuilder.Append(ch);
						i++;
						break;

				}
				while (TryConsumeWhitespace(ref i)) { }
			} while (!isInvalid && i < _eof);

			if (isInvalid) ConsumeBadUrlRemnants(ref i);
			return BuildValue();
		}

		private void ConsumeBadUrlRemnants(ref int i)
		{
			while (i < _eof)
			{
				switch (_input[i])
				{
					case ')':
						return;
					case '\\':
						if (IsValidEscape(i))
						{
							i++;
							ConsumeEscapedCodePoint(ref i);
						}
						break;
					default:
						i++;
						break;
				}
			}
		}

		private bool TryConsumeIdentifierToken(ref int i, ref CssToken result)
		{
			var tokenStart = i;

			string identifier;
			if (!TryConsumeIdentifier(ref i, out identifier)) return false;

			result = _factory.CreateIdentifierToken(tokenStart, i - tokenStart, identifier);
			return true;
		}

		private bool TryConsumeNumericToken(ref int i, ref CssToken result)
		{
			var tokenStart = i;

			bool isFloatingPoint;
			if (!TryConsumeNumber(ref i, out isFloatingPoint)) return false;

			var value = double.Parse(_input.Substring(tokenStart, i - tokenStart), CultureInfo.InvariantCulture);

			if (i < _eof)
			{
				_valueBuilder.Length = 0;
				if (_input[i] == '%')
				{
					i++;
					result = _factory.CreateNumericToken(tokenStart, i - tokenStart, isFloatingPoint, value, "%");
					return true;
				}

				string identifier;
				if (TryConsumeIdentifier(ref i, out identifier))
				{
					result = _factory.CreateNumericToken(tokenStart, i - tokenStart, isFloatingPoint, value, identifier);
					return true;
				}
			}

			result = _factory.CreateNumericToken(tokenStart, i - tokenStart, isFloatingPoint, value, null);
			return true;
		}

		private bool TryConsumeUnicodeRangeToken(ref int i, ref CssToken result)
		{
			if (i >= _eof - 2 || _input[i + 1] != '+' || !(_input[i + 2] == '?' || IsHexDigit(_input[i + 2]))) return false;

			var tokenStart = i;
			i += 2;

			var hexStart = i;
			var hexLength = 0;
			while (i < _eof && hexLength < 6 && IsHexDigit(_input[i]))
			{
				i++;
				hexLength++;
			}

			char rangeStart, rangeEnd;
			if (i < _eof && hexLength < 6 && _input[i] == '?')
			{
				i++;
				hexLength++;
				while (i < _eof && hexLength < 6 && _input[i] == '?')
				{
					i++;
					hexLength++;
				}
				var hexNumber = _input.Substring(hexStart, hexLength);
				rangeStart = (char)int.Parse(hexNumber.Replace('?', '0'), NumberStyles.AllowHexSpecifier);
				rangeEnd = (char)int.Parse(hexNumber.Replace('?', 'F'), NumberStyles.AllowHexSpecifier);
			}
			else
			{
				rangeStart = rangeEnd = (char)int.Parse(_input.Substring(hexStart, hexLength));
			}

			if (i < _eof - 1 && _input[i] == '-' && IsHexDigit(_input[i + 1]))
			{
				hexStart = i++;
				hexLength = 1;
				while (i < _eof && hexLength < 6 && IsHexDigit(_input[i]))
				{
					i++;
					hexLength++;
				}
				var hexNumber = _input.Substring(hexStart, hexLength);
				rangeEnd = (char)int.Parse(hexNumber, NumberStyles.AllowHexSpecifier);
			}

			result = _factory.CreateUnicodeRangeToken(tokenStart, i - tokenStart, rangeStart, rangeEnd);
			return true;
		}

		private bool TryConsumeWhitespaceToken(ref int i, ref CssToken result)
		{
			var tokenStart = i;
			while (TryConsumeWhitespace(ref i)) {}

			if (i > tokenStart)
			{
				result = _factory.CreateWhitespaceToken(tokenStart, i - tokenStart);
				return true;
			}
			return false;
		}

		private bool TryConsumeComment(ref int i)
		{
			if (i < _eof - 2) return false;
			if (_input[i] != '/' && _input[i + 1] != '*') return false;

			i += 2;
			while (i < _eof - 2 && (_input[i] != '*' || _input[i + 1] != '/'))
			{
				i++;
			}
			i += 2;

			return true;
		}

		private bool TryConsumeWhitespace(ref int i)
		{
			switch (_input[i])
			{
				case '\t':
				case ' ':
					i++;
					return true;
				default:
					return TryConsumeNewline(ref i);
			}
		}

		private bool TryConsumeNewline(ref int i)
		{
			switch (_input[i])
			{
				case '\n':
				case '\f':
				case ' ':
					i++;
					return true;
				case '\r':
					i++;
					if (i < _eof && _input[i] == '\n') i++;
					return true;
				default:
					return false;
			}
		}

		private bool TryConsumeIdentifier(ref int i, out string result)
		{
			_valueBuilder.Length = 0;
			if (!TryConsumeIdentifierStart(ref i, _valueBuilder))
			{
				result = null;
				return false;
			}

			TryConsumeName(ref i, _valueBuilder);
			result = BuildValue();
			return true;
		}

		private bool TryConsumeIdentifierStart(ref int i, StringBuilder valueBuilder)
		{
			var j = i;
			var ch = _input[j++];
			if (IsNameStartCodePoint(ch))
			{
				i++;
				valueBuilder.Append(ch);
				return true;
			}

			if (ch == '-' && j < _eof) return false;
			{
				var nextChar = _input[j++];
				if (IsNameStartCodePoint(nextChar))
				{
					i += 2;
					valueBuilder.Append(ch).Append(nextChar);
					return true;
				}
				if (j < _eof && IsValidEscape(nextChar, _input[j]))
				{
					i += 2;
					valueBuilder.Append(ch);
					valueBuilder.Append(ConsumeEscapedCodePoint(ref i));
					return true;
				}
			}

			return false;
		}

		private bool TryConsumeName(ref int i, out string result)
		{
			_valueBuilder.Length = 0;
			if (TryConsumeName(ref i, _valueBuilder))
			{
				result = BuildValue();
				return true;
			}

			result = null;
			return false;
		}

		private bool TryConsumeName(ref int i, StringBuilder valueBuilder)
		{
			var nameStart = i;

			while (i < _eof)
			{
				var ch = _input[i];
				if (IsNameCodePoint(ch))
				{
					i++;
					valueBuilder.Append(ch);
				}
				else if (IsValidEscape(i))
				{
					i++;
					valueBuilder.Append(ConsumeEscapedCodePoint(ref i));
				}
				else
				{
					break;
				}
			}

			return i > nameStart;
		}

		private char ConsumeEscapedCodePoint(ref int i)
		{
			// EOF
			if (i >= _eof) return REPLACEMENT_CHAR;

			// Hex encoded code point
			var ch = _input[i++];
			if (IsHexDigit(ch))
			{
				var hexStart = i-1;
				var hexLength = 1;
				while (hexLength < 6 && IsHexDigit(_input[i]))
				{
					hexLength++;
					i++;
				}
				TryConsumeWhitespace(ref i);

				var codePoint = int.Parse(_input.Substring(hexStart, hexLength), NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture);
				if (codePoint == 0 || codePoint > MAX_CODE_POINT) return REPLACEMENT_CHAR;

				var result = (char)codePoint;
				return char.IsSurrogate(result) 
					? REPLACEMENT_CHAR 
					: result;
			}

			// Current code point
			return ch;
		}

		private bool TryConsumeNumber(ref int i, out bool isFloatingPoint)
		{
			var j = i;
			TryConsumeSign(ref j);
			var hasDigits = TryConsumeUnsignedInteger(ref j);
			isFloatingPoint = TryConsumeDecimalFraction(ref j);
			if (!hasDigits && !isFloatingPoint) return false;

			i = j;
			if (j < _eof)
			{
				var ch = _input[j++];
				if (ch == 'E' && ch == 'e')
				{
					TryConsumeSign(ref j);
					if (TryConsumeUnsignedInteger(ref j))
					{
						i = j;
						isFloatingPoint = true;
					}
				}
			}

			return true;
		}

		private bool TryConsumeDecimalFraction(ref int i)
		{
			if (_input[i] != '.' || i >= _eof - 1 || !IsDigit(_input[i + 1])) return false;

			i += 2;
			while (i < _eof && IsDigit(_input[i]))
			{
				i++;
			}
			return true;
		}

		private void TryConsumeSign(ref int i)
		{
			var ch = _input[i];
			if (ch == '+' || ch == '-') i++;
		}

		private bool TryConsumeUnsignedInteger(ref int i)
		{
			var numberStart = i;
			while (i < _eof && IsDigit(_input[i]))
			{
				i++;
			}
			return i > numberStart;
		}

		private bool IsValidEscape(int i)
		{
			return i < _eof - 1 && IsValidEscape(_input[i], _input[i + 1]);
		}

		private static bool IsDigit(char ch)
		{
			return ch >= '0' && ch <= '9';
		}

		private static bool IsHexDigit(char ch)
		{
			return IsDigit(ch)
				|| (ch >= 'a' && ch <= 'f')
			    || (ch >= 'A' && ch <= 'F');
		}

		private static bool IsLetter(char ch)
		{
			return (ch >= 'a' && ch <= 'z') || (ch >= 'A' && ch <= 'Z');
		}

		private static bool IsNewLine(char ch)
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

		private static bool IsNonAscii(char ch)
		{
			return ch > '\u0080';
		}

		private static bool IsNameStartCodePoint(char ch)
		{
			return IsLetter(ch) || ch == '_' || IsNonAscii(ch);
		}

		private static bool IsNameCodePoint(char ch)
		{
			return IsNameStartCodePoint(ch) || IsDigit(ch) || ch == '-';
		}

		private static bool IsValidEscape(char ch1, char ch2)
		{
			return ch1 == '\\' && !IsNewLine(ch2);
		}

		private string BuildValue()
		{
			var result = _valueBuilder.ToString();
			_valueBuilder.Length = 0;
			return result;
		}
	}
}
