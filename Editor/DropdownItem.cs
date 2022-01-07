namespace UnityDropdown.Editor
{
    using UnityEngine;

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