namespace UnityDropdown.Editor
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using JetBrains.Annotations;
    using SolidUtilities;

    /// <inheritdoc cref="DropdownMenu"/>
    public partial class DropdownMenu<T> : DropdownMenu
    {
        /// <summary>
        /// The root node that is not shown in the list but instead contains all root folders and items of the list as its <see cref="DropdownNode{T}.ChildNodes"/>.
        /// </summary>
        [PublicAPI]
        public readonly DropdownNode<T> Root;

        /// <summary>
        /// Action to do when a value has been selected in the menu. This is triggered only when the selection was confirmed i.e. the dropdown menu is closed,
        /// not when the user has not decided yet and selects different items by clicking up or down arrow buttons.
        /// </summary>
        public event Action<T> OnValueSelected;

        internal sealed override (string Path, bool HasIcon)[] SelectionPaths { get; }

        /// <summary>
        /// The node that is currently selected. This may be not the final choice of a user.
        /// Subscribe to <see cref="OnValueSelected"/> or pass the action to <see cref="DropdownMenu{T}"/> constructor to receive a callback when a final value is chosen.
        /// </summary>
        public DropdownNode<T> SelectedNode;
        internal override DropdownNode _SelectedNode => SelectedNode;

        private readonly NoneElement<T> _noneElement;
        protected override DropdownNode NoneElement => _noneElement;

        private readonly List<DropdownNode<T>> _searchModeTree = new List<DropdownNode<T>>();
        protected override IReadOnlyCollection<DropdownNode> SearchModeTree => _searchModeTree;

        protected override IReadOnlyCollection<DropdownNode> Nodes => Root.ChildNodes;

        public DropdownMenu(
            IList<DropdownItem<T>> items,
            Action<T> onValueSelected,
            int searchbarMinItemsCount = 10,
            bool sortItems = false,
            bool showNoneElement = false)
            : base(items.Count, searchbarMinItemsCount)
        {
            Root = DropdownNode<T>.CreateRoot(this);

            if (showNoneElement)
                _noneElement = NoneElement<T>.Create(this);

            if (sortItems)
                MultiKeyQuickSort.SortInPlace(items);

            FillTreeWithItems(items);

            // A node is selected in FillTreeWithItems. If no node appeared to be selected, _noneElement is selected instead.
            SelectedNode ??= _noneElement;
            _scrollbar.RequestScrollToNode(SelectedNode, Scrollbar.NodePosition.Center);

            OnValueSelected = onValueSelected;

            SelectionPaths = new (string Path, bool HasIcon)[items.Count];

            for (int i = 0; i < SelectionPaths.Length; i++)
            {
                var item = items[i];
                // comparing to null using 'is' to bypass the overloaded comparison behaviour which takes more time but is not useful here.
                SelectionPaths[i] = (item.Path, !(item.Icon is null));
            }
        }

        public override void FinalizeSelection()
        {
            base.FinalizeSelection();
            OnValueSelected?.Invoke(SelectedNode.Value);
        }

        /// <summary>
        /// Expands all folders in the hierarchy. All the folders except for ones where the selected item is located
        /// are closed by default when the dropdown window is first shown.
        /// </summary>
        public void ExpandAllFolders()
        {
            foreach (var node in EnumerateNodes())
                node.Expanded = true;
        }

        /// <summary>
        /// Enumerates the nodes in the hierarchy recursively, going from the root folders to their child folders, to the most deeply placed items.
        /// </summary>
        /// <returns>Collection of nodes in the hierarchy.</returns>
        [PublicAPI]
        public IEnumerable<DropdownNode<T>> EnumerateNodes() => Root.GetChildNodesRecursive();

        protected override void InitializeSearchModeTree()
        {
            _searchModeTree.Clear();
            _searchModeTree.AddRange(EnumerateNodes()
                .Where(node => node.Value != null)
                .Select(node =>
                {
                    bool includeInSearch = FuzzySearch.CanBeIncluded(_searchString, node.SearchName, out int score);
                    return new { score, item = node, include = includeInSearch };
                })
                .Where(x => x.include)
                .OrderByDescending(x => x.score)
                .Select(x => x.item));
        }
    }
}