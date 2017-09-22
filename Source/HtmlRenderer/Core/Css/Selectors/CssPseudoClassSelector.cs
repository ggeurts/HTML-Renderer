namespace TheArtOfDev.HtmlRenderer.Core.Css.Selectors
{
	using System.Text;
	using TheArtOfDev.HtmlRenderer.Core.Utils;

	public abstract class CssPseudoClassSelector : CssSimpleSelector
	{
		#region Instance fields

		private readonly string _name;

		#endregion

		#region Constructor(s)

		protected CssPseudoClassSelector(string name)
		{
			ArgChecker.AssertArgNotNullOrEmpty(name, nameof(name));
			_name = name.ToLowerInvariant();
		}

		#endregion

		#region Properties

		public string Name
		{
			get { return _name; }
		}

		#endregion

		#region Public methods

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(this, obj)) return true;

			var other = obj as CssPseudoClassSelector;
			return other != null
			       && GetType() == other.GetType()
			       && _name == other._name;
		}

		public override int GetHashCode()
		{
			return _name.GetHashCode();
		}

		public override void ToString(StringBuilder sb)
		{
			sb.Append(':').Append(_name);
		}

		#endregion
	}
}