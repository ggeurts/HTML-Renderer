namespace TheArtOfDev.HtmlRenderer.Core.Css.Selectors
{
	public enum CssAttributeMatchOperator
	{
		/// <summary>
		/// Matches any attribute value
		/// </summary>
		Any = 0,

		/// <summary>
		/// Matches any value that exactly equals a given value.
		/// </summary>
		Exact = '=',

		/// <summary>
		/// Matches any value that begins with a given value.
		/// </summary>
		Prefix = '^',

		/// <summary>
		/// Matches any value that ends with a given value.
		/// </summary>
		Suffix = '$',

		/// <summary>
		/// Matches any value that contains a given value.
		/// </summary>
		Contains = '*',

		/// <summary>
		/// Matches any value that contains a whitespace delimited word that equals a given value.
		/// </summary>
		ContainsWord = '~',

		/// <summary>
		/// Matches any value that exactly equals a given value or begins with the given value immediately 
		/// followed by a '-' character.
		/// </summary>
		LanguageCode = '|'
	}
}