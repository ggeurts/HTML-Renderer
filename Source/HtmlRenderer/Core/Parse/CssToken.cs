namespace TheArtOfDev.HtmlRenderer.Core.Parse
{
	public struct CssToken
	{
		private readonly CssTokenType _tokenType;
		private readonly int _position;
		private readonly int _length;
		private readonly CssTokenData _data;

		internal CssToken(CssTokenType tokenType, int startPos, int length, CssTokenData data)
		{
			_tokenType = tokenType;
			_position = startPos;
			_length = length;
			_data = data;
		}

		public CssTokenType TokenType
		{
			get { return _tokenType; }
		}

		public int Position
		{
			get { return _position; }
		}

		public int Length
		{
			get { return _length; }
		}

		public string RawValue
		{
			get { return _data.GetRawValue(ref this); }
		}

		public string StringValue
		{
			get
			{
				var stringData = _data as CssStringTokenData;
				return stringData?.GetValue(ref this);
			}
		}

		public bool IsInvalid
		{
			get { return (_tokenType & CssTokenType.Invalid) != 0; }
		}

		public bool IsBadStringToken
		{
			get
			{
				const CssTokenType BAD_STRING = CssTokenType.String | CssTokenType.Invalid;
				return (_tokenType & BAD_STRING) == BAD_STRING;
			}
		}
	}
}