namespace UnityDropdown.Editor
{
    using System;
    using System.Collections.Generic;
    using SolidUtilities;

    // Part of the class, responsible solely for filling the tree with items. Only FillTreeWithItems method is used in
    // the main part of the class.
    public partial class DropdownMenu<T>
    {
        private void FillTreeWithItems(IList<DropdownItem<T>> items)
        {
            if (items == null)
                return;

            bool foundSelected = false;

            foreach (var item in items)
            {
                // DropdownMenu currently supports only a single selected value,
                // so if multiple items are marked as selected, only the first one will be shown as selected.
                if (item.IsSelected)
                {
                    if (foundSelected)
                    {
                        item.IsSelected = false;
                    }
                    else
                    {
                        foundSelected = true;
                    }
                }

                CreateDropdownItem(item);
            }
        }

        private void CreateDropdownItem(DropdownItem<T> item)
        {
            SplitFullItemPath(item.Path, out string folderPath, out string itemName);
            var directParentOfNewNode = folderPath.Length == 0 ? Root : CreateFoldersInPathIfNecessary(folderPath);
            directParentOfNewNode.AddChild(itemName, item);
        }

        private static void SplitFullItemPath(string nodePath, out string namespaceName, out string typeName)
        {
            int indexOfLastSeparator = nodePath.LastIndexOf('/');

            if (indexOfLastSeparator == -1)
            {
                namespaceName = string.Empty;
                typeName = nodePath;
            }
            else
            {
                namespaceName = nodePath.Substring(0, indexOfLastSeparator);
                typeName = nodePath.Substring(indexOfLastSeparator + 1);
            }
        }

        private DropdownNode<T> CreateFoldersInPathIfNecessary(string path)
        {
            var parentNode = Root;

            foreach (var folderName in path.AsSpan().Split('/'))
            {
                parentNode = parentNode.FindChild(folderName) ?? parentNode.AddChildFolder(folderName.ToString());
            }

            return parentNode;
        }
    }
}