namespace TheArtOfDev.HtmlRenderer.Core.Css.Parsing
{
	using System;

	public enum CssErrorCode
	{
		None = 0
	}

	public class CssErrorEventArgs : EventArgs
	{
		private readonly int _line;
		private readonly int _column;
		private readonly string _message;

		public CssErrorEventArgs(int line, int column, string message)
		{
			_line = line;
			_column = column;
			_message = message;
		}

		public int Line
		{
			get { return _line; }
		}

		public int Column
		{
			get { return _column; }
		}

		public string Message
		{
			get { return _message; }
		}
	}
}
