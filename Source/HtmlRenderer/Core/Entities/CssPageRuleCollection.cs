namespace TheArtOfDev.HtmlRenderer.Core.Entities
{
	using System;
	using System.Collections.Generic;
	using TheArtOfDev.HtmlRenderer.Core.Utils;

	public class CssPageRuleCollection
	{
		private readonly Dictionary<string, CssNestedBlockCollection> _items = new Dictionary<string, CssNestedBlockCollection>(StringComparer.OrdinalIgnoreCase);

		public CssPageRuleCollection()
		{ }

		public CssPageRuleCollection(CssPageRuleCollection other)
		{
			ArgChecker.AssertArgNotNull(other, nameof(other));
			foreach (var item in other._items)
			{
				_items.Add(item.Key, new CssNestedBlockCollection(item.Value));
			}
		}

		public int Count
		{
			get { return _items.Count; }
		}

		internal void MergeWith(CssPageRuleCollection other)
		{
			foreach (var otherItem in other._items)
			{
				CssNestedBlockCollection item;
				if (!_items.TryGetValue(otherItem.Key, out item))
				{
					item = new CssNestedBlockCollection(otherItem.Value);
					_items.Add(otherItem.Key, item);
					return;
				}

				item.MergeWith(otherItem.Value);
			}
		}
	}
}