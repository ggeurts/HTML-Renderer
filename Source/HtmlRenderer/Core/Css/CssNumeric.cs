namespace TheArtOfDev.HtmlRenderer.Core.Css
{
	using System;
	using System.Diagnostics.CodeAnalysis;
	using System.Globalization;
	using TheArtOfDev.HtmlRenderer.Core.Utils;

	public struct CssNumeric : IEquatable<CssNumeric>, IFormattable
	{
		private readonly double _value;
		private readonly string _unit;

		public CssNumeric(double value)
		{
			_value = value;
			_unit = null;
		}

		public CssNumeric(double value, string unit)
		{
			_value = value;
			_unit = string.IsNullOrEmpty(unit) ? null : unit;
		}

		public double Value
		{
			get { return _value; }
		}

		public string Unit
		{
			get { return _unit; }
		}

		[SuppressMessage("ReSharper", "CompareOfFloatsByEqualityOperator")]
		public bool Equals(CssNumeric other)
		{
			return _value == other._value
				&& CssEqualityComparer<string>.Default.Equals(_unit, other._unit);
		}

		public override bool Equals(object obj)
		{
			return obj is CssNumeric && Equals((CssNumeric)obj);
		}

		public override int GetHashCode()
		{
			return _unit == null
				? _value.GetHashCode()
				: HashUtility.Hash(
					_value.GetHashCode(),
					CssEqualityComparer<string>.Default.GetHashCode(_unit));
		}

		public override string ToString()
		{
			return _unit != null
				? string.Format(CultureInfo.InvariantCulture, "{0}{1}", _value, _unit)
				: _value.ToString(CultureInfo.InvariantCulture);
		}

		public string ToString(string format, IFormatProvider formatProvider)
		{
			var valueText = _value.ToString(format, formatProvider);
			return _unit != null
				? valueText + _unit
				: valueText;
		}
	}
}