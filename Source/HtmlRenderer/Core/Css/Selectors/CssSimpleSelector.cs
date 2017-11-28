namespace TheArtOfDev.HtmlRenderer.Core.Css.Selectors
{
	using System;
	using TheArtOfDev.HtmlRenderer.Core.Utils;

	public abstract class CssSimpleSelector : CssSelector
	{
		/// <summary>
		/// Creates a selector that consists of the :not() pseudo-class with this selector as argument.
		/// </summary>
		/// <returns>The newly created selector.</returns>
		public CssSimpleSelector Negate()
		{
			ArgChecker.AssertIsTrue<InvalidOperationException>(!(this is CssNotPseudoClassSelector), "Double negation of selectors is not allowed");
			return new CssNotPseudoClassSelector(this);
		}
	}
}