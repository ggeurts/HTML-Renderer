namespace TheArtOfDev.HtmlRenderer.Core.Css.Selectors
{
	using System;
	using System.Globalization;
	using TheArtOfDev.HtmlRenderer.Core.Utils;

	public struct CssSpecificity : IEquatable<CssSpecificity>, IComparable<CssSpecificity>, IComparable
	{
		#region Instance fields

		private readonly int _value;

		#endregion

		#region Constructor(s)

		public CssSpecificity(int a, int b, int c)
		{
			_value = (a & 0x3FF) << 20 | (b & 0x3FF) << 10 | (c & 0x3FF);
		}

		private CssSpecificity(int value)
		{
			_value = value;
		}

		#endregion

		#region Properties

		#pragma warning disable IDE1006 // Naming Styles

		private int a
		{
			get { return _value >> 20; }
		}

		private int b
		{
			get { return (_value >> 10) & 0x3FF; }
		}

		private int c
		{
			get { return _value & 0x3FF; }
		}
		
		#pragma warning restore IDE1006 // Naming Styles

		#endregion

		#region Equality operations

		public static bool operator ==(CssSpecificity left, CssSpecificity right)
		{
			return left._value == right._value;
		}

		public static bool operator !=(CssSpecificity left, CssSpecificity right)
		{
			return left._value != right._value;
		}

		/// <inheritdoc />
		public bool Equals(CssSpecificity other)
		{
			return this == other;
		}

		/// <inheritdoc />
		public override bool Equals(object obj)
		{
			return obj is CssSpecificity && this == (CssSpecificity)obj;
		}

		/// <inheritdoc />
		public override int GetHashCode()
		{
			return _value;
		}

		#endregion

		#region Comparison operations

		public static bool operator <(CssSpecificity left, CssSpecificity right)
		{
			return left.CompareTo(right) < 0;
		}

		public static bool operator >(CssSpecificity left, CssSpecificity right)
		{
			return left.CompareTo(right) > 0;
		}

		public static bool operator <=(CssSpecificity left, CssSpecificity right)
		{
			return left.CompareTo(right) <= 0;
		}

		public static bool operator >=(CssSpecificity left, CssSpecificity right)
		{
			return left.CompareTo(right) >= 0;
		}

		/// <inheritdoc />
		public int CompareTo(CssSpecificity other)
		{
			return _value.CompareTo(other._value);
		}

		/// <inheritdoc />
		public int CompareTo(object obj)
		{
			if (ReferenceEquals(null, obj)) return 1;

			ArgChecker.AssertArgOfType<CssSpecificity>(obj, nameof(obj));
			return CompareTo((CssSpecificity)obj);
		}

		#endregion

		#region Mathematical operations

		public static CssSpecificity operator +(CssSpecificity left, CssSpecificity right)
		{
			return left.Add(right);
		}

		public CssSpecificity Add(CssSpecificity other)
		{
			return new CssSpecificity(_value + other._value);
		}

		#endregion

		#region String operations

		public override string ToString()
		{
			return string.Format(CultureInfo.InvariantCulture, "{0},{1},{2}", a, b, c);
		}

		#endregion
	}
}
