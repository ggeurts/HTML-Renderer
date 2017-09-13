// "Therefore those skilled at the unorthodox
// are infinite as heaven and earth,
// inexhaustible as the great rivers.
// When they come to an end,
// they begin again,
// like the days and months;
// they die and are reborn,
// like the four seasons."
// 
// - Sun Tsu,
// "The Art of War"


namespace TheArtOfDev.HtmlRenderer.Core.Entities
{
	public sealed class CssMediaRuleCollection : CssNestedBlockCollection
	{
		public CssMediaRuleCollection()
			: base("all")
		{}

		public CssMediaRuleCollection(CssMediaRuleCollection other) 
			: base(other)
		{}
	}
}