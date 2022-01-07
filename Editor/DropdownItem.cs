namespace UnityDropdown.Editor
{
    public class DropdownItem<T> : DropdownItem
    {
        public readonly T Value;

        public DropdownItem(T value, string path, string searchName = null)
            : base(path, searchName)
        {
            Value = value;
        }
    }

    public abstract class DropdownItem
    {
        public readonly string Path;
        public readonly string SearchName;

        protected DropdownItem(string path, string searchName = null)
        {
            SearchName = searchName ?? path;
            Path = path;
        }
    }
}