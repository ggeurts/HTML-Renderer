namespace TheArtOfDev.HtmlRenderer.Core.Css
{
	using System.Collections.Immutable;
	using System.Linq;
	using System.Text;
	using TheArtOfDev.HtmlRenderer.Core.Utils;

	public class CssCompositeComponent : CssComponent
	{
		private readonly IImmutableList<CssComponent> _components;

		public IImmutableList<CssComponent> Components
		{
			get { return _components; }
		}

		public CssCompositeComponent(IImmutableList<CssComponent> components)
		{
			ArgChecker.AssertArgNotNull(components, nameof(components));
			_components = components;
		}

		public override bool Equals(object obj)
		{
			var otherComposite = obj as CssCompositeComponent;
			return otherComposite != null
			    && _components.Count == otherComposite._components.Count
				&& _components.SequenceEqual(otherComposite._components);
		}

		public override int GetHashCode()
		{
			var hashCode = _components.Count;
			foreach (var component in _components)
			{
				hashCode = HashUtility.Hash(hashCode, component.GetHashCode());
			}
			return hashCode;
		}

		public override void ToString(StringBuilder sb)
		{
			foreach (var component in this.Components)
			{
				component.ToString(sb);
			}
		}
	}
}