namespace UnityDropdown.Editor
{
    public class SelectionTreeItem<T> : SelectionTreeItem
    {
        public readonly T Value;

        public SelectionTreeItem(T value, string path, string searchName = null)
            : base(path, searchName)
        {
            Value = value;
        }
    }

    public class SelectionTreeItem
    {
        public readonly string Path;
        public readonly string SearchName;

        public SelectionTreeItem(string path, string searchName = null)
        {
            SearchName = searchName ?? path;
            Path = path;
        }
    }
}