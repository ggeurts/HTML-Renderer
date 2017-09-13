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
	using TheArtOfDev.HtmlRenderer.Core.Utils;

	public class CssNestedBlockCollection
	{
		private readonly Dictionary<string, CssBlockCollection> _items = new Dictionary<string, CssBlockCollection>(StringComparer.OrdinalIgnoreCase);
		private readonly string _defaultKey;

		public CssNestedBlockCollection(string defaultKey = null)
		{
			_defaultKey = defaultKey ?? "";
			if (defaultKey != null)
			{
				_items.Add(defaultKey, new CssBlockCollection());
			}
		}

		public CssNestedBlockCollection(CssNestedBlockCollection other)
		{
			ArgChecker.AssertArgNotNull(other, nameof(other));

			_defaultKey = other._defaultKey;
			foreach (var item in other._items)
			{
				_items.Add(item.Key, new CssBlockCollection(item.Value));
			}
		}

		public int Count
		{
			get { return _items.Count; }
		}

		public CssBlockCollection this[string key]
		{
			get { return _items[EnsureKey(key)]; }
		}

		public bool ContainsCssBlock(string className, string key = null)
		{
			CssBlockCollection item;
			return _items.TryGetValue(EnsureKey(key), out item)
				&& item.ContainsCssBlock(className);
		}

		/// <summary>
		/// Get collection of css blocks for the requested class selector.<br/>
		/// the <paramref name="className"/> can be: class name, html element name, html element and 
		/// class name (elm.class), hash tag with element id (#id).<br/>
		/// returned all the blocks that word on the requested class selector, it can contain simple
		/// selector or hierarchy selector.
		/// </summary>
		/// <param name="className">the class selector to get css blocks by</param>
		/// <param name="key">Key of @rule to locate css block in</param>
		/// <returns>collection of css blocks, empty collection if no blocks exists (never null)</returns>
		public IEnumerable<CssBlock> GetCssBlock(string className, string key = null)
		{
			CssBlockCollection item;
			return _items.TryGetValue(EnsureKey(key), out item)
				? item[className]
				: Enumerable.Empty<CssBlock>();
		}

		/// <summary>
		/// Add the given css block to the css data, merging to existing block if required.
		/// </summary>
		/// <remarks>
		/// If there is no css blocks for the same class it will be added to data collection.<br/>
		/// If there is already css blocks for the same class it will check for each existing block
		/// if the hierarchical selectors match (or not exists). if do the two css blocks will be merged into
		/// one where the new block properties overwrite existing if needed. if the new block doesn't mach any
		/// existing it will be added either to the beginning of the list if it has no  hierarchical selectors or at the end.<br/>
		/// Css block without hierarchical selectors must be added to the beginning of the list so more specific block
		/// can overwrite it when the style is applied.
		/// </remarks>
		/// <param name="key">Key of @rule to which css block is to be added</param>
		/// <param name="cssBlock">the css block to add</param>
		internal void AddCssBlock(string key, CssBlock cssBlock)
		{
			key = EnsureKey(key);

			CssBlockCollection item;
			if (!_items.TryGetValue(key, out item))
			{
				item = new CssBlockCollection();
				_items.Add(key, item);
			}
			item.Add(cssBlock);
		}

		internal void MergeWith(CssNestedBlockCollection other)
		{
			foreach (var otherItem in other._items)
			{
				CssBlockCollection item;
				if (!_items.TryGetValue(otherItem.Key, out item))
				{
					item = new CssBlockCollection(otherItem.Value);
					_items.Add(otherItem.Key, item);
					return;
				}

				item.MergeWith(otherItem.Value);
			}
		}

		private string EnsureKey(string key)
		{
			return key ?? _defaultKey;
		}
	}
}