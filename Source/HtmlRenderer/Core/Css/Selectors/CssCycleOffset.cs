namespace TheArtOfDev.HtmlRenderer.Core.Css.Selectors
{
	using System;
	using System.Collections.Generic;
	using System.Text;
	using TheArtOfDev.HtmlRenderer.Core.Utils;

	/// <summary>
	/// Represents a cyclic selector of items in a collection. This type corresponds to the an+b pattern in 
	/// the CSS syntax and selectors specifications.
	/// </summary>
	/// <remarks>
	/// <para>Conceptually, the collection is partitioned into groups of <see cref="CycleSize"/> items, 
	/// and in each group the item with the given <see cref="Offset"/> is selected.</para>
	/// <para>Mathematically, only the items are selected that have an one-based index in the collection 
	/// for which there exists a non-negative value n where index = <see cref="CycleSize"/> * n - 1 + <see cref="Offset"/>.</para>
	/// </remarks>
	public struct CssCycleOffset : IEquatable<CssCycleOffset>
	{
		#region Static fields

		/// <summary>
		/// Selector for first item in collection.
		/// </summary>
		public static readonly CssCycleOffset First = new CssCycleOffset(0, 1);

		/// <summary>
		/// Selector for items at odd indices in the collection (first collection item has index 1).
		/// </summary>
		public static readonly CssCycleOffset Odd = new CssCycleOffset(2, 1);

		/// <summary>
		/// Selector for items at even indices in the collection (first collection item has index 1).
		/// </summary>
		public static readonly CssCycleOffset Even = new CssCycleOffset(2, 0);

		#endregion

		#region Instance fields

		private readonly int _cycleSize;
		private readonly int _offset;

		#endregion

		#region Constructor(s)

		public CssCycleOffset(int cycleSize, int offset)
		{
			_cycleSize = cycleSize;
			_offset = offset;
		}

		#endregion

		#region Properties

		/// <summary>
		/// Gets the size of the groups into which a collection of items is to be partitioned.
		/// For negative values, the group size equals the absolute value of <see cref="CycleSize"/>, but
		/// may limit the number of groups that have a selected item, depending on the value of <see cref="Offset"/>
		/// and the collection size.
		/// </summary>
		public int CycleSize
		{
			get { return _cycleSize; }
		}

		/// <summary>
		/// Gets the one-based offset of the selected item within each group. Negative offsets and offset
		/// values greater than or equal to <see cref="CycleSize"/> are allowed and may cause the existence
		/// of groups without a selected item, depending on the value of <see cref="CycleSize"/> and the 
		/// collection size.
		/// </summary>
		public int Offset
		{
			get { return _offset; }
		}

		#endregion

		#region Public methods

		/// <summary>
		/// Indicates whether a zero-based index matches this collection item selector. 
		/// </summary>
		/// <param name="index">The zero-based index to be matched.</param>
		/// <returns></returns>
		/// <remarks>NOTE: The CSS specification works with one-based collections (first collection item has index 1), 
		/// whereas .NET uses zero-based collections. This library follows the .NET conventions for calculations 
		/// involving indices.</remarks>
		public bool Matches(int index)
		{
			int remainder;
			return Math.DivRem(index - _offset, _cycleSize, out remainder) >= 0 && remainder == 0;
		}

		#endregion

		#region Equality operations

		public static bool operator ==(CssCycleOffset left, CssCycleOffset right)
		{
			return left._cycleSize == right._cycleSize
				&& left._offset == right._offset;
		}

		public static bool operator !=(CssCycleOffset left, CssCycleOffset right)
		{
			return !(left == right);
		}

		public bool Equals(CssCycleOffset other) => this == other;

		/// <inheritdoc />
		public override bool Equals(object obj)
		{
			return obj is CssCycleOffset && this == (CssCycleOffset)obj;
		}

		/// <inheritdoc />
		public override int GetHashCode()
		{
			return HashUtility.Hash(_cycleSize, _offset);
		}

		#endregion

		#region String operations

		private enum ParseState
		{
			Initial = 0,
			Sign,
			NumStart,
			Num,
			CyclePostfix,
			OffsetSign,
			OffsetStart,
			OffsetNum,
			Error = -1
		}

		public static bool TryParse<T>(T input, out CssCycleOffset result) where T : IEnumerable<char>
		{
			ArgChecker.AssertArgNotNull(input, nameof(input));

			var state = ParseState.Initial;
			var sign = 1;
			var num = 0;
			var cycleSize = 0;
			using (var inputEnum = input.GetEnumerator())
			{
				if (!inputEnum.MoveNext())
				{
					// Empty input
					result = default(CssCycleOffset);
					return false;
				}

				do
				{
					var ch = inputEnum.Current;
					if (char.IsWhiteSpace(ch)) continue;

					switch (state)
					{
						case ParseState.Initial:
							if (TryParse(inputEnum, "odd"))
							{
								result = new CssCycleOffset(2, 1);
								return true;
							}
							else if (TryParse(inputEnum, "even"))
							{
								result = new CssCycleOffset(2, 0);
								return true;
							}
							goto case ParseState.Sign;

						case ParseState.Sign:
							switch (ch)
							{
								case '-':
									sign = -1;
									state = ParseState.NumStart;
									continue;
								case '+':
									state = ParseState.NumStart;
									continue;
							}
							goto case ParseState.NumStart;

						case ParseState.NumStart:
							if (ch >= '0' && ch <= '9')
							{
								num = ch - '0';
								state = ParseState.Num;
								continue;
							}
							num = 1;
							goto case ParseState.CyclePostfix;

						case ParseState.Num:
							if (ch >= '0' && ch <= '9')
							{
								num = num * 10 + (ch - '0');
								continue;
							}
							goto case ParseState.CyclePostfix;

						case ParseState.CyclePostfix:
							if (ch == 'n')
							{
								cycleSize = num * sign;
								num = 0;
								sign = 1;
								state = ParseState.OffsetSign;
								continue;
							}
							goto case ParseState.Error;

						case ParseState.OffsetSign:
							switch (ch)
							{
								case '-':
									sign = -1;
									state = ParseState.OffsetStart;
									continue;
								case '+':
									state = ParseState.OffsetStart;
									continue;
							}
							goto case ParseState.OffsetStart;

						case ParseState.OffsetStart:
							if (ch >= '0' && ch <= '9')
							{
								num = ch - '0';
								state = ParseState.OffsetNum;
								continue;
							}
							goto case ParseState.Error;

						case ParseState.OffsetNum: // additional offset digits
							if (ch >= '0' && ch <= '9')
							{
								num = num * 10 + (ch - '0');
								continue;
							}
							goto case ParseState.Error;

						case ParseState.Error:
							result = default(CssCycleOffset);
							return false;

					}
				} while (inputEnum.MoveNext());
			}

			result = state == ParseState.Num
				? new CssCycleOffset(0, num * sign)
				: new CssCycleOffset(cycleSize, num * sign);
			return true;
		}

		private static bool TryParse<TEnum>(TEnum inputEnum, string expected) where TEnum : IEnumerator<char>
		{
			var i = 0;
			while (i < expected.Length)
			{
				if (inputEnum.Current.ToLowerAscii() != expected[i++]) return false;
				if (!inputEnum.MoveNext()) break;
			}
			return i >= expected.Length;
		}

		public static CssCycleOffset Parse<T>(T input) where T : IEnumerable<char>
		{
			CssCycleOffset result;
			if (!TryParse(input, out result))
			{
				throw new FormatException(string.Format("Value '{0}' does not match CSS an+b syntax", input));
			}
			return result;
		}

		public override string ToString()
		{
			var sb = new StringBuilder();
			ToString(sb);
			return sb.ToString();
		}

		public void ToString(StringBuilder sb)
		{
			ArgChecker.AssertArgNotNull(sb, nameof(sb));

			switch (_cycleSize)
			{
				case -1:
					sb.Append("-n");
					break;
				case 0:
					break;
				case 1:
					sb.Append("n");
					break;
				default:
					sb.Append(_cycleSize).Append('n');
					break;
			}
			if (_offset > 0)
			{
				if (_cycleSize != 0) sb.Append('+');
				sb.Append(_offset);
			}
			else if (_offset < 0)
			{
				sb.Append(_offset);
			}
			else if (_cycleSize == 0)
			{
				sb.Append('0');
			}
		}

		#endregion
	}
}
