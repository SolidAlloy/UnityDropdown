namespace UnityDropdown.Editor
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class DropdownNode<T> : DropdownNode
    {
        public readonly T Value;
        public readonly DropdownNode<T> ParentNode;

        private readonly DropdownTree<T> _parentTree;
        protected override DropdownTree ParentTree => _parentTree;

        public readonly List<DropdownNode<T>> ChildNodes = new List<DropdownNode<T>>();
        protected override IReadOnlyCollection<DropdownNode> _ChildNodes => ChildNodes;

        protected DropdownNode(T value, DropdownNode<T> parentNode, DropdownTree<T> parentTree, string name, string searchName)
            : base(parentNode, name, searchName)
        {
            Value = value;
            ParentNode = parentNode;
            _parentTree = parentTree;
            ParentNode = parentNode;
        }

        /// <summary>Creates a root node that does not have a parent and does not show up in the popup.</summary>
        /// <param name="parentTree">The tree this node belongs to.</param>
        /// <returns>The root node.</returns>
        public static DropdownNode<T> CreateRoot(DropdownTree<T> parentTree) => new DropdownNode<T>(default, null, parentTree, string.Empty, null);

        /// <summary>Creates a dropdown item that represents a <see cref="System.Type"/>.</summary>
        /// <param name="name">Name that will show up in the popup.</param>
        /// <param name="value">The value this node represents.</param>
        /// <param name="searchName"> A name of the node that will show up when a search is performed.</param>
        public void CreateChildItem(string name, T value, string searchName)
        {
            var child = new DropdownNode<T>(value, this, _parentTree, name, searchName);
            ChildNodes.Add(child);
        }

        /// <summary>Creates a folder that contains dropdown items.</summary>
        /// <param name="name">Name of the folder.</param>
        /// <returns>A <see cref="DropdownNode"/> instance that represents the folder.</returns>
        public DropdownNode<T> CreateChildFolder(string name)
        {
            var child = new DropdownNode<T>(default, this, _parentTree, name, null);
            ChildNodes.Add(child);
            return child;
        }

        public IEnumerable<DropdownNode<T>> GetChildNodesRecursive()
        {
            if ( ! IsRoot)
                yield return this;

            foreach (var childNode in ChildNodes.SelectMany(node => node.GetChildNodesRecursive()))
                yield return childNode;
        }

        protected override void SetSelfSelected()
        {
            _parentTree.SetSelectedNode(this);
        }

        public DropdownNode<T> GetNextChild(DropdownNode<T> currentChild)
        {
            int currentIndex = ChildNodes.IndexOf(currentChild);

            if (currentIndex < 0)
                return currentChild;

            if (currentIndex == ChildNodes.Count - 1)
                return ParentNode?.GetNextChild(this) ?? currentChild;

            return ChildNodes[currentIndex + 1];
        }

        public DropdownNode<T> GetPreviousChild(DropdownNode<T> currentChild)
        {
            int currentIndex = ChildNodes.IndexOf(currentChild);

            if (currentIndex < 0)
                return currentChild;

            if (currentIndex == 0)
                return this;

            return ChildNodes[currentIndex - 1];
        }

        /// <summary>
        /// Returns the direct child node with the matching name, or null if the matching node was not found.
        /// </summary>
        /// <remarks>
        /// One of the usages of FindNode is to build the selection tree. When a new item is added, it is checked
        /// whether its parent folder is already created. If the folder is created, it is usually the most recently
        /// created folder, so the list is iterated backwards to give the result as quickly as possible.
        /// </remarks>
        /// <param name="name">Name of the node to find.</param>
        /// <returns>Direct child node with the matching name or null.</returns>
        public DropdownNode<T> FindChild(ReadOnlySpan<char> name)
        {
            for (int index = ChildNodes.Count - 1; index >= 0; --index)
            {
                if (name.Equals(ChildNodes[index]._name.AsSpan(), StringComparison.Ordinal))
                    return ChildNodes[index];
            }

            return null;
        }
    }
}