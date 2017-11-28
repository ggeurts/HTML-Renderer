namespace TheArtOfDev.HtmlRenderer.Core.Css.Selectors
{
	using System;
	using System.Text;
	using System.Xml;
	using TheArtOfDev.HtmlRenderer.Core.Utils;

	internal class CssSelectorCombination : CssSelector, ICssSelectorChain
	{
		private readonly CssCombinator _combinator;
		private readonly ICssSelectorChain _leftOperand;
		private readonly ICssSelectorSequence _rightOperand;

		internal CssSelectorCombination(CssCombinator combinator, ICssSelectorChain leftOperand, ICssSelectorSequence rightOperand)
		{
			ArgChecker.AssertArgNotNull(leftOperand, nameof(leftOperand));
			ArgChecker.AssertArgNotNull(rightOperand, nameof(rightOperand));
			ArgChecker.AssertIsTrue<ArgumentException>(leftOperand.Subject.PseudoElement == null, 
				"Pseudo-elements may only occur in the right operand of a combinator");
			_combinator = combinator;
			_leftOperand = leftOperand;
			_rightOperand = rightOperand;
		}

		public ICssSelectorSubject Subject
		{
			get { return _rightOperand.Subject; }
		}

		public override bool Matches<TElement>(TElement element)
		{
			TElement relatedElement;
			switch (_combinator)
			{
				case CssCombinator.Descendant:
					if (!TryGetParent(element, false, out relatedElement)) return false;
					break;
				case CssCombinator.Child:
					if (!TryGetParent(element, true, out relatedElement)) return false;
					break;
				case CssCombinator.AdjacentSibling:
					if (!element.TryGetPredecessor(_rightOperand, true, out relatedElement)) return false;
					break;
				case CssCombinator.GeneralSibling:
					if (!element.TryGetPredecessor(_rightOperand, false, out relatedElement)) return false;
					break;
				default:
					return false;
			}

			return _leftOperand.Matches(relatedElement);
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(this, obj)) return true;

			var other = obj as CssSelectorCombination;
			return other != null
				&& _combinator == other._combinator
				&& Equals(_leftOperand, other._leftOperand)
				&& Equals(_rightOperand, other._rightOperand);
		}

		public override int GetHashCode()
		{
			return HashUtility.Hash((int)_combinator, _rightOperand.GetHashCode());
		}

		public override void ToString(StringBuilder sb, IXmlNamespaceResolver namespaceResolver)
		{
			_leftOperand.ToString(sb, namespaceResolver);
			sb.Append(' ');
			if (_combinator != CssCombinator.Descendant)
			{
				sb.Append((char) _combinator).Append(' ');
			}
			_rightOperand.ToString(sb, namespaceResolver);
		}

		private bool TryGetParent<TElement>(TElement element, bool immediateOnly, out TElement result) where TElement : IElementInfo<TElement>
		{
			while (!element.IsRoot)
			{
				element = element.Parent;
				if (_leftOperand.Matches(element))
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