namespace UnityDropdown.Editor
{
    using System;
    using System.Collections.Generic;
    using JetBrains.Annotations;
    using UnityEngine;

    /// <inheritdoc cref="DropdownNode"/>
    public class DropdownNode<T> : DropdownNode
    {
        /// <summary>
        /// The value of a dropdown node. It is null for the root node and folders.
        /// </summary>
        public readonly T Value;

        /// <summary>
        /// The parent node of this node.
        /// </summary>
        public readonly DropdownNode<T> ParentNode;

        private readonly DropdownMenu<T> _parentMenu;
        protected override DropdownMenu ParentMenu => _parentMenu;

        /// <summary>
        /// A list of child nodes this node has.
        /// </summary>
        public readonly List<DropdownNode<T>> ChildNodes = new List<DropdownNode<T>>();
        protected override IReadOnlyCollection<DropdownNode> _ChildNodes => ChildNodes;

        protected DropdownNode(T value, DropdownNode<T> parentNode, DropdownMenu<T> parentMenu, string name, string searchName, Texture icon)
            : base(parentNode, name, searchName, icon)
        {
            Value = value;
            ParentNode = parentNode;
            _parentMenu = parentMenu;
            ParentNode = parentNode;
        }

        /// <summary>Creates a root node that does not have a parent and does not show up in the popup.</summary>
        /// <param name="parentMenu">The tree this node belongs to.</param>
        /// <returns>The root node.</returns>
        public static DropdownNode<T> CreateRoot(DropdownMenu<T> parentMenu) => new DropdownNode<T>(default, null, parentMenu, string.Empty, null, null);

        /// <summary>Creates a dropdown item that represents a <see cref="System.Type"/>.</summary>
        /// <param name="name">Name that will show up in the popup.</param>
        /// <param name="item">An item this node represents.</param>
        /// <returns>The newly created child node.</returns>
        [PublicAPI]
        public DropdownNode<T> AddChild(string name, DropdownItem<T> item)
        {
            var child = new DropdownNode<T>(item.Value, this, _parentMenu, name, item.SearchName, item.Icon);

            if (item.IsSelected)
                child.SetSelected();

            ChildNodes.Add(child);
            return child;
        }

        /// <summary>Creates a folder that contains dropdown items.</summary>
        /// <param name="name">Name of the folder.</param>
        /// <returns>A <see cref="DropdownNode"/> instance that represents the folder.</returns>
        public DropdownNode<T> AddChildFolder(string name)
        {
            var child = new DropdownNode<T>(default, this, _parentMenu, name, null, null);
            ChildNodes.Add(child);
            return child;
        }

        /// <summary>
        /// Returns a collection of child nodes recursively, starting from the immediate children.
        /// </summary>
        /// <returns>A collection of child nodes recursively, starting from the immediate children.</returns>
        public IEnumerable<DropdownNode<T>> GetChildNodesRecursive()
        {
            foreach (var childNode in ChildNodes)
            {
                yield return childNode;

                foreach (var childOfChild in childNode.GetChildNodesRecursive())
                {
                    yield return childOfChild;
                }
            }
        }

        /// <inheritdoc cref="FindChild(ReadOnlySpan{char})"/>
        [PublicAPI]
        public DropdownNode<T> FindChild(string name) => FindChild(name.AsSpan());

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
                if (name.Equals(ChildNodes[index].Name.AsSpan(), StringComparison.Ordinal))
                    return ChildNodes[index];
            }

            return null;
        }

        internal DropdownNode<T> GetNextChild(DropdownNode<T> currentChild)
        {
            int currentIndex = ChildNodes.IndexOf(currentChild);

            if (currentIndex < 0)
                return currentChild;

            if (currentIndex == ChildNodes.Count - 1)
                return ParentNode?.GetNextChild(this) ?? currentChild;

            return ChildNodes[currentIndex + 1];
        }

        internal DropdownNode<T> GetPreviousChild(DropdownNode<T> currentChild)
        {
            int currentIndex = ChildNodes.IndexOf(currentChild);

            if (currentIndex < 0)
                return currentChild;

            if (currentIndex == 0)
                return this;

            return ChildNodes[currentIndex - 1];
        }

        protected override void SetSelected()
        {
            _parentMenu.SelectedNode = this;
        }
    }
}