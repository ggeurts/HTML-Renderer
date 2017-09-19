namespace TheArtOfDev.HtmlRenderer.Core.Css
{
	using System;
	using System.Collections.Generic;
	using System.Collections.Immutable;
	using System.Linq;
	using System.Text;
	using TheArtOfDev.HtmlRenderer.Core.Css.Parsing;
	using TheArtOfDev.HtmlRenderer.Core.Utils;

	public class CssDeclaration : CssComponent
	{
		private readonly string _name;
		private readonly ImmutableArray<CssToken> _values;
		private readonly bool _isImportant;

		public CssDeclaration(string name, CssToken value, bool isImportant)
			: this(name, ImmutableArray.Create(value), isImportant)
		{ }

		public CssDeclaration(string name, IEnumerable<CssToken> values, bool isImportant)
			: this(name, ImmutableArray.CreateRange(values), isImportant)
		{}

		public CssDeclaration(string name, ImmutableArray<CssToken> values, bool isImportant)
		{
			ArgChecker.AssertArgNotNullOrEmpty(name, nameof(name));
			_name = name;
			_values = values;
			_isImportant = isImportant;
		}

		public string Name
		{
			get { return _name; }
		}

		public ImmutableArray<CssToken> Values
		{
			get { return _values; }
		}

		public bool IsImportant
		{
			get { return _isImportant; }
		}

		public override bool Equals(object obj)
		{
			var otherDeclaration = obj as CssDeclaration;
			return otherDeclaration != null
				&& CssEqualityComparer<string>.Default.Equals(_name, otherDeclaration._name)
			    && _values.Length == otherDeclaration._values.Length
				&& _values.SequenceEqual(otherDeclaration._values);
		}

		public override int GetHashCode()
		{
			var hashCode = HashUtility.Hash(
				CssEqualityComparer<string>.Default.GetHashCode(_name),
				_isImportant.GetHashCode());
			foreach (var value in _values)
			{
				hashCode = HashUtility.Hash(hashCode, value.GetHashCode());
			}
			return hashCode;
		}

		public override void ToString(StringBuilder sb)
		{
			sb.Append(_name).Append(":");
			foreach (var value in _values)
			{
				value.ToString(sb);
			}
			if (_isImportant)
			{
				sb.Append("!important");
			}
		}
	}
}