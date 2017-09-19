namespace TheArtOfDev.HtmlRenderer.Core.Css.Parsing
{
	using System;
	using System.Globalization;
	using System.IO;
	using System.Text;
	using TheArtOfDev.HtmlRenderer.Core.Utils;

	public class CssReader
	{
		private readonly TextReader _textReader;
		private char[] _cyclicBuffer = new char[16];
		private int _cyclicBufferPos;
		private int _cyclicBufferLen;
		private int _position = -1;

		public CssReader(string input)
			: this(new StringReader(input))
		{ }

		public CssReader(string input, int index, int count)
			: this(new SubstringReader(input, index, count))
		{ }

		public CssReader(TextReader textReader)
		{
			ArgChecker.AssertArgNotNull(textReader, nameof(textReader));
			_textReader = textReader;
		}

		public bool IsBof
		{
			get { return _position < 0; }
		}

		public bool IsEof
		{
			get { return _cyclicBuffer == null; }
		}

		public int Position
		{
			get { return _position; }
		}

		public int Peek()
		{
			return EnsureBufferLength(1)
				? _cyclicBuffer[_cyclicBufferPos]
				: -1;
		}

		public int Peek(int lookAheadIndex)
		{
			return EnsureBufferLength(lookAheadIndex + 1)
				? _cyclicBuffer[(_cyclicBufferPos + lookAheadIndex) % _cyclicBuffer.Length]
				: -1;
		}

		public int Read()
		{
			if (!EnsureBufferLength(1)) return -1;
			var result = _cyclicBuffer[_cyclicBufferPos];

			_cyclicBufferPos = (_cyclicBufferPos + 1) % _cyclicBuffer.Length;
			_cyclicBufferLen--;
			_position++;

			return result;
		}

		public int Read(StringBuilder buffer, int count)
		{
			ArgChecker.AssertArgNotNull(buffer, nameof(buffer));

			if (!EnsureBufferLength(count)) count = _cyclicBufferLen;

			for (var i = 0; i < count; i++)
			{
				buffer.Append(_cyclicBuffer[_cyclicBufferPos]);
				_cyclicBufferPos = (_cyclicBufferPos + 1) % _cyclicBuffer.Length;
				_cyclicBufferLen--;
			}
			return count;
		}

		public bool TryRead(string value)
		{
			ArgChecker.AssertArgNotNull(value, nameof(value));

			if (!Matches(value)) return false;

			_cyclicBufferPos = (_cyclicBufferPos + value.Length) % _cyclicBuffer.Length;
			_cyclicBufferLen -= value.Length;
			return true;
		}

		private bool EnsureBufferLength(int minLength)
		{
			if (_cyclicBuffer == null) return false;
			if (minLength > _cyclicBuffer.Length)
			{
				throw new ArgumentOutOfRangeException(nameof(minLength), minLength, string.Format(CultureInfo.InvariantCulture,
					"Length cannot be greater than read-ahead buffer capacity of {0} characters.", _cyclicBuffer.Length));
			}

			if (_cyclicBufferLen >= minLength) return true;

			var readPos = _cyclicBufferPos + _cyclicBufferLen % _cyclicBuffer.Length;
			var readCount = _cyclicBuffer.Length - readPos;
			_cyclicBufferLen += _textReader.ReadBlock(_cyclicBuffer, readPos, readCount);

			if (readCount < minLength)
			{
				_cyclicBufferLen += _textReader.ReadBlock(_cyclicBuffer, 0, _cyclicBufferPos);
				if (_cyclicBufferLen == 0) _cyclicBuffer = null;
			}
			return _cyclicBufferLen >= minLength;
		}

		private bool Matches(string prefix)
		{
			if (string.IsNullOrEmpty(prefix) || prefix.Length > _cyclicBuffer.Length)
			{
				throw new ArgumentException(
					string.Format(CultureInfo.InvariantCulture, "Prefix length must be between 0 and {0} (inclusive).", _cyclicBuffer.Length),
					nameof(prefix));
			}

			if (!EnsureBufferLength(prefix.Length)) return false;

			for (var i = 0; i < prefix.Length; i++)
			{
				if (prefix[i].ToLowerAscii() != _cyclicBuffer[(_cyclicBufferPos + i) % _cyclicBuffer.Length]) return false;
			}
			return true;
		}

		private class SubstringReader : TextReader
		{
			private readonly string _s;
			private readonly int _maxIndex;
			private int _index;

			public SubstringReader(string s, int index, int count)
			{
				ArgChecker.AssertArgNotNull(s, nameof(s));
				if (index < 0 || index > s.Length) throw new ArgumentOutOfRangeException(nameof(index));
				if (count < 0 || index + count > s.Length) throw new ArgumentOutOfRangeException(nameof(count));

				_s = s;
				_index = index;
				_maxIndex = index + count;
			}

			public override int Peek()
			{
				return _index < _maxIndex
					? _s[_index]
					: -1;
			}

			public override int Read()
			{
				return _index < _maxIndex
					? _s[_index++]
					: -1;
			}

			public override int Read(char[] buffer, int index, int count)
			{
				ArgChecker.AssertArgNotNull(buffer, nameof(buffer));
				if (index < 0 || index > buffer.Length) throw new ArgumentOutOfRangeException(nameof(index));
				if (count < 0 || index + count > buffer.Length) throw new ArgumentOutOfRangeException(nameof(count));

				count = Math.Min(count, _maxIndex - _index);
				_s.CopyTo(_index, buffer, index, count);
				_index += count;
				return count;
			}

			public override int ReadBlock(char[] buffer, int index, int count)
			{
				return Read(buffer, index, count);
			}
		}
	}
}
