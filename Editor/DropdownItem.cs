namespace UnityDropdown.Editor
{
    using UnityEngine;

    /// <summary>
    /// An item with a value and path that will be shown in a dropdown window.
    /// </summary>
    /// <typeparam name="T">The type of value the item holds.</typeparam>
    public class DropdownItem<T> : DropdownItem
    {
        public readonly T Value;

        public DropdownItem(T value, string path, Texture icon = null, string searchName = null, bool isSelected = false)
            : base(path, icon, searchName, isSelected)
        {
            Value = value;
        }
    }

    public abstract class DropdownItem
    {
        public readonly string Path;

        /// <summary>
        /// A name of the item that will appear in the dropdown list when a search is performed.
        /// </summary>
        public readonly string SearchName;

        public readonly Texture Icon;

        public bool IsSelected;

        /// <summary>
        /// Creates a new instance of DropdownItem.
        /// </summary>
        /// <param name="path">A path to the item, separated by '/'.</param>
        /// <param name="icon">An optional icon for representing the item.</param>
        /// <param name="searchName">An optional special name of the item that will appear in the dropdown list when a search is performed.
        /// Equals to <paramref name="path"/> unless specified otherwise.</param>
        protected DropdownItem(string path, Texture icon = null, string searchName = null, bool isSelected = false)
        {
            Path = path;
            Icon = icon;
            SearchName = searchName ?? path;
            IsSelected = isSelected;
        }
    }
}