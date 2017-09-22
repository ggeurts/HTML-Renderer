namespace TheArtOfDev.HtmlRenderer.Core.Css.Selectors
{
	public enum CssDynamicElementState
	{
		None = 0,

		/// <summary>
		/// Element is a link that has not yet been visited by the user. Elements in this state match
		/// the :link pseudo class.
		/// </summary>
		/// <remarks>
		/// Note: It is possible for style sheet authors to abuse the :link and :visited 
		/// pseudo-classes to determine which sites a user has visited without the user's consent. 
		/// </remarks>
		IsUnvisitedLink,

		/// <summary>
		/// Element is a link that has been visited by the user. Elements in this state match
		/// the :visited pseudo class.
		/// </summary>
		/// <remarks>
		/// Note: It is possible for style sheet authors to abuse the :link and :visited 
		/// pseudo-classes to determine which sites a user has visited without the user's consent. 
		/// </remarks>
		IsVisitedLink,

		/// <summary>
		/// The element is designated by the user with a pointing device, but not necessarily activated.
		/// Elements in this state match the :hover pseudo class.
		/// </summary>
		IsPointerOver,

		/// <summary>
		/// The element is being activated by the user. Elements in this state match the :active pseudo class.
		/// </summary>
		IsActive,

		/// <summary>
		/// The element accepts keyboard, mouse or other input events. Elements in this state match 
		/// the :focus pseudo class.
		/// </summary>
		HasFocus,

		/// <summary>
		/// The user interface element is enabled. Elements in this state match 
		/// the :enabled pseudo class.
		/// </summary>
		IsEnabled,

		/// <summary>
		/// The user interface element is disabled. Elements in this state match 
		/// the :disabled pseudo class.
		/// </summary>
		IsDisabled
	}
}