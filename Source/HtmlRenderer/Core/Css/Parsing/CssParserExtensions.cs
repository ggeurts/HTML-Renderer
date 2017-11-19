namespace TheArtOfDev.HtmlRenderer.Core.Css.Parsing
{
	using System.Collections.Generic;
	using System.Globalization;
	using System.Text;

	/// <summary>
	/// Parser extension methods that correspond to CSS grammar production rules
	/// </summary>
	internal static class CssParserExtensions
	{
		private static readonly CssToken EofToken = default(CssToken);

		public static bool TryParseUnicodeRange(IEnumerator<CssToken> tokenEnum, ref CssUnicodeRange result)
		{
			var token = tokenEnum.MoveNext() ? tokenEnum.Current : EofToken;
			if (!token.HasValue('u')) return false;

			token = tokenEnum.MoveNext() ? tokenEnum.Current : EofToken;
			if (!token.StartsWith('+')) return false;

			var text = new StringBuilder(14);
			token.ToString(text);

			if (token.IsDelimiter('+'))
			{
				token = tokenEnum.MoveNext() ? tokenEnum.Current : EofToken;
				if (token.IsIdentifier)
				{
					token.ToString(text);
					token = tokenEnum.MoveNext() ? tokenEnum.Current : EofToken;
				}
				else if (!token.IsDelimiter('?'))
				{
					return false;
				}
			}
			else if (token.IsDimension)
			{
				token = tokenEnum.MoveNext() ? tokenEnum.Current : EofToken;
			}
			else if (token.IsNumber)
			{
				token = tokenEnum.MoveNext() ? tokenEnum.Current : EofToken;
				if (token.IsDimension || token.IsNumber)
				{
					token.ToString(text);
					return TryParseUnicodeRange(text, ref result);
				}
			}

			while (token.IsDelimiter('?'))
			{
				text.Append('?');
				token = tokenEnum.MoveNext() ? tokenEnum.Current : EofToken;
			}

			return TryParseUnicodeRange(text, ref result);
		}

		private static bool TryParseUnicodeRange(StringBuilder text, ref CssUnicodeRange result)
		{
			if (text.Length < 2 || text.Length > 14 || text[0] != '+') return false;

			var pos = 1;
			var len = 0;
			var ch = Peek(text, pos);

			while (IsHexDigit(ch))
			{
				len++;
				ch = Peek(text, pos + len);
			}
			if (len > 6) return false;

			int rangeStart, rangeEnd;
			if (ch == '?')
			{
				do
				{
					len++;
					ch = Peek(text, len);
				} while (ch == '?');
				if (len > 6 || len < text.Length) return false;

				var hexNumber = text.ToString(pos, len);
				rangeStart = int.Parse(hexNumber.Replace('?', '0'), NumberStyles.AllowHexSpecifier);
				rangeEnd = int.Parse(hexNumber.Replace('?', 'F'), NumberStyles.AllowHexSpecifier);
			}
			else
			{
				rangeStart = rangeEnd = int.Parse(text.ToString(pos, len), NumberStyles.AllowHexSpecifier);

				pos = len + 1;
				len = 0;
				if (ch == '-' && IsHexDigit(Peek(text, pos)))
				{
					do
					{
						len++;
						ch = Peek(text, pos + len);
					} while (IsHexDigit(ch));
					if (len > 6 || len < text.Length) return false;

					var hexNumber = text.ToString(pos, len);
					rangeEnd = int.Parse(hexNumber, NumberStyles.AllowHexSpecifier);
				}
			}

			result = new CssUnicodeRange(rangeStart, rangeEnd);
			return true;
		}

		private static int Peek(StringBuilder text, int index)
		{
			return index < text.Length
				? text[index]
				: -1;
		}

		private static bool IsHexDigit(int ch)
		{
			return IsDigit(ch)
				|| (ch >= 'a' && ch <= 'f')
				|| (ch >= 'A' && ch <= 'F');
		}

		private static bool IsDigit(int ch)
		{
			return ch >= '0' && ch <= '9';
		}
	}
}