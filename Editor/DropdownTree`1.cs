namespace UnityDropdown.Editor
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using SolidUtilities;
    using UnityEngine;

    public partial class DropdownTree<T> : DropdownTree
    {
        private readonly Action<T> _onValueSelected;
        private readonly DropdownNode<T> _root;
        private readonly IEqualityComparer<T> _customComparer;

        public sealed override string[] SelectionPaths { get; }

        private DropdownNode<T> _selectedNode;
        public override DropdownNode SelectedNode => _selectedNode;

        private readonly NoneElement<T> _noneElement;
        protected override DropdownNode NoneElement => _noneElement;

        private readonly List<DropdownNode<T>> _searchModeTree = new List<DropdownNode<T>>();
        protected override IReadOnlyCollection<DropdownNode> SearchModeTree => _searchModeTree;

        protected override IReadOnlyCollection<DropdownNode> Nodes => _root.ChildNodes;

        public DropdownTree(
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
                Sedgewick.SortInPlace(items);

            FillTreeWithItems(items);

            _customComparer = customComparer;
            SetSelection(items, currentValue);
            _onValueSelected = onValueSelected;

            SelectionPaths = new string[items.Count];

            for (int i = 0; i < SelectionPaths.Length; i++)
            {
                SelectionPaths[i] = items[i].Path;
            }
        }

        public override void FinalizeSelection()
        {
            base.FinalizeSelection();
            _onValueSelected?.Invoke(_selectedNode.Value);
        }

        public void SetSelectedNode(DropdownNode<T> dropdownNode) => _selectedNode = dropdownNode;

        protected override void InitializeSearchModeTree()
        {
            _searchModeTree.Clear();
            _searchModeTree.AddRange(EnumerateTreeTyped()
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

        protected override IEnumerable<DropdownNode> EnumerateTree() => EnumerateTreeTyped();

        private IEnumerable<DropdownNode<T>> EnumerateTreeTyped() => _root.GetChildNodesRecursive();

        private void SetSelection(IList<DropdownItem<T>> items, T selectedValue)
        {
            if (selectedValue == null)
            {
                _selectedNode = _noneElement;
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

            _selectedNode = itemToSelect;
            _scrollbar.RequestScrollToNode(itemToSelect, Scrollbar.NodePosition.Center);
        }
    }
}