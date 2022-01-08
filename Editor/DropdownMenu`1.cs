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
        private readonly Action<T> _onValueSelected;
        private readonly DropdownNode<T> _root;
        private readonly IEqualityComparer<T> _customComparer;

        internal sealed override (string Path, bool HasIcon)[] SelectionPaths { get; }

        public DropdownNode<T> SelectedNode;
        internal override DropdownNode _SelectedNode => SelectedNode;

        private readonly NoneElement<T> _noneElement;
        protected override DropdownNode NoneElement => _noneElement;

        private readonly List<DropdownNode<T>> _searchModeTree = new List<DropdownNode<T>>();
        protected override IReadOnlyCollection<DropdownNode> SearchModeTree => _searchModeTree;

        protected override IReadOnlyCollection<DropdownNode> Nodes => _root.ChildNodes;

        public DropdownMenu(
            IList<DropdownItem<T>> items,
            T currentValue,
            Action<T> onValueSelected,
            int searchbarMinItemsCount = 10,
            bool sortItems = false,
            bool hideNoneElement = true,
            IEqualityComparer<T> customComparer = null)
            : base(items.Count, searchbarMinItemsCount)
        {
            _root = DropdownNode<T>.CreateRoot(this);

            if ( ! hideNoneElement)
                _noneElement = NoneElement<T>.Create(this);

            if (sortItems)
                MultiKeyQuickSort.SortInPlace(items);

            FillTreeWithItems(items);

            _customComparer = customComparer;
            SetSelection(items, currentValue);
            _onValueSelected = onValueSelected;

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
            _onValueSelected?.Invoke(SelectedNode.Value);
        }

        public void ExpandAllFolders()
        {
            foreach (var node in EnumerateNodes())
                node.Expanded = true;
        }

        [PublicAPI]
        public IEnumerable<DropdownNode<T>> EnumerateNodes() => _root.GetChildNodesRecursive();

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

        private void SetSelection(IList<DropdownItem<T>> items, T selectedValue)
        {
            if (selectedValue == null)
            {
                SelectedNode = _noneElement;
                return;
            }

            ReadOnlySpan<char> nameOfItemToSelect = default;

            foreach (DropdownItem<T> item in items)
            {
                if ((_customComparer ?? EqualityComparer<T>.Default).Equals(selectedValue, item.Value))
                    nameOfItemToSelect = item.Path.AsSpan();
            }

            if (nameOfItemToSelect == default)
                return;

            var itemToSelect = _root;

            foreach (var part in nameOfItemToSelect.Split('/'))
                itemToSelect = itemToSelect.FindChild(part);

            SelectedNode = itemToSelect;
            _scrollbar.RequestScrollToNode(itemToSelect, Scrollbar.NodePosition.Center);
        }
    }
}