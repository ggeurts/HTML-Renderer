namespace TheArtOfDev.HtmlRenderer.Core.Css.Selectors
{
	using System.Text;
	using System.Xml;
	using TheArtOfDev.HtmlRenderer.Core.Utils;

	public abstract class CssPseudoClassSelector : CssSimpleSelector
	{
		#region Static fields

		private static readonly CssSpecificity DefaultSpecificity = new CssSpecificity(0, 1, 0);

		#endregion

		#region Instance fields

		private readonly string _name;

		#endregion

		#region Constructor(s)

		protected CssPseudoClassSelector(string name)
			: this(name, DefaultSpecificity)
		{}

		protected CssPseudoClassSelector(string name, CssSpecificity specificity)
			: base(specificity)
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

		public override void ToString(StringBuilder sb, IXmlNamespaceResolver namespaceResolver)
		{
			sb.Append(':').Append(_name);
		}

		#endregion
	}
}