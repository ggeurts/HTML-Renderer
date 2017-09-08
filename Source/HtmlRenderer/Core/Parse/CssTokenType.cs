namespace TheArtOfDev.HtmlRenderer.Core.Parse
{
	using System;

	[Flags]
	public enum CssTokenType
	{
		Undefined,
		Whitespace = 0x100,
		Identifier = 0x200,
		String = 0x400,
		Number = 0x800,
		Dimension = 0x1000,
		Percentage = 0x2000,
		Url = 0x4000,
		Function = 0x8000,
		Delimiter = 0x10000,
		Operator = 0x20000,
		Column = 0x40000,
		UnicodeRange = 0x80000,
		Hash = 0x100000,
		AtKeyword = 0x100000,
		Comment = 0x1000000,
		CDO = 0x2000000,
		CDC = 0x4000000,
		Comma = ',',
		Colon = ':',
		Semicolon = ';',
		LeftParenthesis = '(',
		RightParenthesis = ')',
		LeftSquareBracket = '[',
		RightSquareBracket = ']',
		LeftCurlyBracket = '{',
		RightCurlyBracket = '}',
		DashMatchOperator = Operator | '|',
		IncludeMatchOperator = Operator | '~',
		PrefixMatchOperator = Operator | '^',
		SuffixMatchOperator = Operator | '$',
		SubstringMatchOperator = Operator | '*',
		NumberType = 0x10000000,
		IdentifierType = 0x20000000,
		Invalid = 0x40000000
	}
}