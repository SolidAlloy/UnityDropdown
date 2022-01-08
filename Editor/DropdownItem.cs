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

        public DropdownItem(T value, string path, Texture icon = null, string searchName = null)
            : base(path, icon, searchName)
        {
            Value = value;
        }
    }

    public abstract class DropdownItem
    {
        public readonly string Path;
        public readonly string SearchName;
        public readonly Texture Icon;

        protected DropdownItem(string path, Texture icon = null, string searchName = null)
        {
            Path = path;
            Icon = icon;
            SearchName = searchName ?? path;
        }
    }
}