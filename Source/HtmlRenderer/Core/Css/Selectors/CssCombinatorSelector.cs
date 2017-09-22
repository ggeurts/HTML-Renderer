namespace TheArtOfDev.HtmlRenderer.Core.Css.Selectors
{
	using System.Text;
	using TheArtOfDev.HtmlRenderer.Core.Utils;

	public class CssCombinatorSelector : CssSelector
	{
		private readonly CssCombinator _combinator;
		private readonly ICssSelectorSequence _relativeSelector;
		private readonly CssCombinatorSelector _previous;

		internal CssCombinatorSelector(CssCombinator combinator, ICssSelectorSequence relativeSelector, CssCombinatorSelector previous)
		{
			ArgChecker.AssertArgNotNull(relativeSelector, nameof(relativeSelector));
			_combinator = combinator;
			_relativeSelector = relativeSelector;
			_previous = previous;
		}

		public CssCombinatorSelector Chain(CssCombinator combinator, ICssSelectorSequence relativeSelector)
		{
			return new CssCombinatorSelector(combinator, relativeSelector, this);
		}

		public override bool Matches<TElement>(TElement element)
		{
			TElement relatedElement;
			switch (_combinator)
			{
				case CssCombinator.DescendantOf:
					if (!TryGetParent(element, false, out relatedElement)) return false;
					break;
				case CssCombinator.ChildOf:
					if (!TryGetParent(element, true, out relatedElement)) return false;
					break;
				case CssCombinator.AdjacentSiblingOf:
					if (!element.TryGetPredecessor(_relativeSelector, true, out relatedElement)) return false;
					break;
				case CssCombinator.GeneralSiblingOf:
					if (!element.TryGetPredecessor(_relativeSelector, false, out relatedElement)) return false;
					break;
				default:
					return false;
			}

			return _previous == null || _previous.Matches(relatedElement);
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(this, obj)) return true;

			var other = obj as CssCombinatorSelector;
			return other != null
				&& _combinator == other._combinator
				&& _relativeSelector.Equals(other._relativeSelector);
		}

		public override int GetHashCode()
		{
			return HashUtility.Hash((int)_combinator, _relativeSelector.GetHashCode());
		}

		public override void ToString(StringBuilder sb)
		{
			_previous?.ToString(sb);
			_relativeSelector.ToString(sb);
			sb.Append(' ');
			if (_combinator != CssCombinator.DescendantOf)
			{
				sb.Append((char) _combinator).Append(' ');
			}
		}

		private bool TryGetParent<TElement>(TElement element, bool immediateOnly, out TElement result) where TElement : IElementInfo<TElement>
		{
			while (!element.IsRoot)
			{
				element = element.Parent;
				if (_relativeSelector.Matches(element))
				{
					result = element;
					return true;
				}
				if (immediateOnly)
				{
					break;
				}
			}

			result = default(TElement);
			return false;
		}
	}
}