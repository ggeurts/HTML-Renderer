namespace TheArtOfDev.HtmlRenderer.Core.Parse
{
	public abstract class CssTokenData
	{
		internal abstract string GetRawValue(ref CssToken token);
	}

	public class CssAsciiTokenData : CssTokenData
	{
		public static readonly CssAsciiTokenData Instance = new CssAsciiTokenData();

		private CssAsciiTokenData()
		{}

		internal override string GetRawValue(ref CssToken token)
		{
			return GetValue(ref token).ToString();
		}

		internal char GetValue(ref CssToken token)
		{
			return (char)((int)token.TokenType & 0xFF);
		}
	}

	internal abstract class AbstractCssStringTokenData : CssTokenData
	{
		protected readonly string _input;

		protected AbstractCssStringTokenData(string input)
		{
			_input = input;
		}

		internal abstract string GetValue(ref CssToken token);

		internal override string GetRawValue(ref CssToken token)
		{
			return _input.Substring(token.Position, token.Length);
		}
	}

	internal class CssSubstringTokenData : AbstractCssStringTokenData
	{
		private readonly int _offset;
		private readonly int _length;

		internal CssSubstringTokenData(string input, int offset, int length)
			: base(input)
		{
			_offset = offset;
			_length = length;
		}

		internal override string GetValue(ref CssToken token)
		{
			return _input.Substring(token.Position + _offset, _length);
		}
	}

	internal class CssStringTokenData : AbstractCssStringTokenData
	{
		private readonly string _value;

		internal CssStringTokenData(string input, string value)
			: base(input)
		{
			_value = value;
		}

		internal override string GetValue(ref CssToken token)
		{
			return _value ?? GetRawValue(ref token);
		}
	}

	internal class CssNumericTokenData : CssTokenData
	{
		private readonly string _input;
		private readonly double _value;
		private readonly string _unit;

		internal CssNumericTokenData(string input, double value, string unit)
		{
			_input = input;
			_value = value;
			_unit = unit;
		}

		internal double Value
		{
			get { return _value; }
		}

		internal string Unit
		{
			get { return _unit; }
		}

		internal override string GetRawValue(ref CssToken token)
		{
			return _input.Substring(token.Position, token.Length);
		}
	}

	internal class CssUnicodeRangeTokenData : CssTokenData
	{
		private readonly string _input;
		private readonly char _rangeStart, _rangeEnd;

		internal CssUnicodeRangeTokenData(string input, char rangeStart, char rangeEnd)
		{
			_input = input;
			_rangeStart = rangeStart;
			_rangeEnd = rangeEnd;
		}

		internal char RangeStart
		{
			get { return _rangeStart; }
		}

		internal char RangeEnd
		{
			get { return _rangeEnd; }
		}

		internal override string GetRawValue(ref CssToken token)
		{
			return _input.Substring(token.Position, token.Length);
		}
	}
}