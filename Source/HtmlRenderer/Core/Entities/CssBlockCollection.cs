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
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using TheArtOfDev.HtmlRenderer.Core.Utils;

	public class CssBlockCollection
	{
		private readonly Dictionary<string, List<CssBlock>> _items = new Dictionary<string, List<CssBlock>>(StringComparer.OrdinalIgnoreCase);

		public CssBlockCollection()
		{}

		public CssBlockCollection(CssBlockCollection other)
		{
			ArgChecker.AssertArgNotNull(other, nameof(other));
			foreach (var item in other._items)
			{
				_items.Add(item.Key, new List<CssBlock>(item.Value));
			}
		}

		public int Count
		{
			get { return _items.Count; }
		}

		public IEnumerable<CssBlock> this[string className]
		{
			get
			{
				List<CssBlock> item;
				return _items.TryGetValue(className, out item)
					? item
					: Enumerable.Empty<CssBlock>();
			}
		}

		public bool ContainsCssBlock(string className)
		{
			return _items.ContainsKey(className);
		}

		public void Write(StringBuilder sb)
		{
			foreach (var item in _items)
			{
				sb.Append(item.Key);
				sb.Append(" { ");
				foreach (var cssBlock in item.Value)
				{
					foreach (var property in cssBlock.Properties)
					{
						// TODO:a handle selectors
						sb.AppendFormat("{0}: {1};", property.Key, property.Value);
					}
				}
				sb.Append(" }");
				sb.AppendLine();
			}
		}

		internal void Add(CssBlock cssBlock)
		{
			List<CssBlock> existingBlocks;
			if (!_items.TryGetValue(cssBlock.Class, out existingBlocks))
			{
				existingBlocks = new List<CssBlock> { cssBlock };
				_items.Add(cssBlock.Class, existingBlocks);
				return;
			}

			foreach (var existingBlock in existingBlocks)
			{
				if (existingBlock.EqualsSelector(cssBlock))
				{
					existingBlock.Merge(cssBlock);
					return;
				}
			}

			// general block must be first
			existingBlocks.Insert(cssBlock.Selectors == null ? 0 : existingBlocks.Count, cssBlock);
		}

		internal void MergeWith(CssBlockCollection other)
		{
			foreach (var otherItem in other._items)
			{
				List<CssBlock> existingBlocks;
				if (!_items.TryGetValue(otherItem.Key, out existingBlocks))
				{
					existingBlocks = new List<CssBlock>(otherItem.Value);
					_items.Add(otherItem.Key, existingBlocks);
					return;
				}

				foreach (var cssBlock in otherItem.Value)
				{
					Add(cssBlock);
				}
			}
		}
	}
}