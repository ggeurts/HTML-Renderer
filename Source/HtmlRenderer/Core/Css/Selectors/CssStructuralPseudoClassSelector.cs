namespace TheArtOfDev.HtmlRenderer.Core.Css.Selectors
{
	using System;
	using System.Diagnostics.CodeAnalysis;
	using System.Text;
	using System.Xml;
	using TheArtOfDev.HtmlRenderer.Core.Utils;

	internal abstract class CssStructuralPseudoClassSelector : CssPseudoClassSelector
	{
		private readonly int _cycleSize;
		private readonly int _offset;

		protected CssStructuralPseudoClassSelector(string name, int cycleSize, int offset)
			: base(name)
		{
			_cycleSize = cycleSize;
			_offset = offset;
		}

		[SuppressMessage("ReSharper", "PossibleNullReferenceException")]
		public override bool Equals(object obj)
		{
			if (ReferenceEquals(this, obj)) return true;

			var other = obj as CssStructuralPseudoClassSelector;
			return other != null
			       && this.GetType() == other.GetType()
			       && _cycleSize == other._cycleSize
			       && _offset == other._offset;
		}

		public override int GetHashCode()
		{
			return HashUtility.Hash(this.GetType().GetHashCode(), HashUtility.Hash(_cycleSize, _offset));
		}

		public override void ToString(StringBuilder sb, IXmlNamespaceResolver namespaceResolver)
		{
			base.ToString(sb, namespaceResolver);
			sb.Append('(');

			bool useShortForm = false;
			switch (_cycleSize)
			{
				case -1:
					sb.Append("-n");
					break;
				case 0:
					useShortForm = true;
					break;
				case 1:
					sb.Append('n');
					break;
				default:
					sb.Append(_cycleSize).Append('n');
					break;
			}

			if (useShortForm || _offset < 0)
			{
				sb.Append(_offset);
			}
			else if (_offset > 0)
			{
				sb.Append('+').Append(_offset);
			}

			sb.Append(')');
		}

		protected bool Matches(int elementIndex)
		{
			int remainder;
			return Math.DivRem(elementIndex - _offset, _cycleSize, out remainder) >= 0 && remainder == 0;
		}
	}
}