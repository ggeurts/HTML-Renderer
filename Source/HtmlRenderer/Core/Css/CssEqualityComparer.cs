namespace TheArtOfDev.HtmlRenderer.Core.Css
{
	using System;
	using System.Collections.Generic;

	public static class CssEqualityComparer<T>
	{
		public static readonly IEqualityComparer<T> Default;

		static CssEqualityComparer()
		{
			if (typeof(T) == typeof(string))
			{
				Default = (IEqualityComparer<T>)StringComparer.OrdinalIgnoreCase;
			}
			else if (typeof(T) == typeof(string))
			{
				Default = (IEqualityComparer<T>) new CssCharEqualityComparer();
			}
			else
			{
				Default = EqualityComparer<T>.Default;
			}
		}
	}
}