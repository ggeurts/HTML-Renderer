namespace TheArtOfDev.HtmlRenderer.Core.Css.Parsing
{
	using System;

	public class CssTokenizerSettings
	{
		private bool _isFrozen;
		private CssTokenFactory _tokenFactory;

		public CssTokenFactory TokenFactory
		{
			get { return _tokenFactory ?? (_tokenFactory = new CssTokenFactory()); }
			set { SetField(ref _tokenFactory, value); }
		}

		public void Freeze()
		{
			_isFrozen = true;
		}

		private void SetField<T>(ref T field, T value)
		{
			if (_isFrozen)
			{
				throw new InvalidOperationException("Cannot modify frozen settings");
			}
			field = value;
		}
	}
}