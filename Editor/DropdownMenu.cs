namespace UnityDropdown.Editor
{
    using System;
    using System.Collections.Generic;
    using SolidUtilities;
    using SolidUtilities.Editor;
    using SolidUtilities.UnityEngineInternals;
    using UnityEditor;
    using UnityEngine;

    /// <summary>
    /// Represents a node tree that contains folders and items with values. It is also responsible for drawing all the
    /// nodes, along with search toolbar and scrollbar.
    /// </summary>
    public abstract class DropdownMenu : IRepainter
    {
        internal bool RepaintRequested;

        protected readonly Scrollbar _scrollbar;
        protected string _searchString = string.Empty;
        protected Rect _visibleRect;

        private readonly string _searchFieldControlName = GUID.Generate().ToString();
        private readonly bool _drawSearchbar;

        public bool DrawInSearchMode { get; private set; }

        internal abstract DropdownNode _SelectedNode { get; }

        internal abstract (string Path, bool HasIcon)[] SelectionPaths { get; }

        protected abstract IReadOnlyCollection<DropdownNode> SearchModeTree { get; }

        protected abstract DropdownNode NoneElement { get; }

        protected abstract IReadOnlyCollection<DropdownNode> Nodes { get; }

        internal event Action SelectionChanged;

        protected DropdownMenu(int itemsCount, int searchbarMinItemsCount)
        {
            _drawSearchbar = itemsCount >= searchbarMinItemsCount;
            _scrollbar = new Scrollbar(this);
        }

        public DropdownWindow ShowAsContext(int windowHeight = 0) => DropdownWindow.ShowAsContext(this, windowHeight);

        public DropdownWindow ShowDropdown(Vector2 windowPosition, int windowHeight = 0) => DropdownWindow.ShowDropdown(this, windowPosition, windowHeight);

        public Vector2 GetCenteredPosition() => DropdownWindow.GetCenteredPosition(this);

        public virtual void FinalizeSelection()
        {
            SelectionChanged?.Invoke();
        }

        public void Draw()
        {
            if (Nodes.Count == 0)
            {
                DrawInfoMessage();
                return;
            }

            if (_drawSearchbar)
            {
                using (new EditorDrawHelper.SearchToolbarStyle(DropdownStyle.SearchToolbarHeight))
                    DrawSearchToolbar();
            }

            if ( ! DrawInSearchMode)
                NoneElement?.DrawSelfAndChildren(default, default);

            using (_scrollbar.Draw())
            {
                _visibleRect = GUIClip.GetVisibleRect();
                DrawTree(_visibleRect);
            }
        }

        public void RequestRepaint()
        {
            RepaintRequested = true;
        }

        protected abstract void InitializeSearchModeTree();

        protected abstract void HandleKeyboardEvents();

        private static void DrawInfoMessage()
        {
            using (new GUILayoutHelper.Vertical(DropdownStyle.NoPadding))
            {
                EditorGUILayoutHelper.DrawInfoMessage("No types to select.");
            }
        }

        private void DrawSearchToolbar()
        {
            Rect innerToolbarArea = GetInnerToolbarArea();

            EditorGUI.BeginChangeCheck();
            _searchString = DrawSearchField(innerToolbarArea, _searchString);
            bool changed = EditorGUI.EndChangeCheck();

            if ( ! changed)
                return;

            if (string.IsNullOrEmpty(_searchString))
            {
                DisableSearchMode();
            }
            else
            {
                EnableSearchMode();
            }
        }

        private void DisableSearchMode()
        {
            // Without GUI.changed, the change will take place only on mouse move.
            GUI.changed = true;
            DrawInSearchMode = false;
            _scrollbar.RequestScrollToNode(_SelectedNode, Scrollbar.NodePosition.Center);
        }

        private void EnableSearchMode()
        {
            if ( ! DrawInSearchMode)
                _scrollbar.ToTop();

            DrawInSearchMode = true;

            InitializeSearchModeTree();
        }

        private static Rect GetInnerToolbarArea()
        {
            Rect outerToolbarArea = GUILayoutUtility.GetRect(
                0f,
                DropdownStyle.SearchToolbarHeight,
                GUILayoutHelper.ExpandWidth(true));

            Rect innerToolbarArea = outerToolbarArea
                .AddHorizontalPadding(10f, 2f)
                .AlignMiddleVertically(DropdownStyle.LabelHeight);

            return innerToolbarArea;
        }

        private void DrawTree(Rect visibleRect)
        {
            var nodes = DrawInSearchMode ? SearchModeTree : Nodes;

            foreach (DropdownNode node in nodes)
                node.DrawSelfAndChildren(0, visibleRect);

            HandleKeyboardEvents();
        }

        private string DrawSearchField(Rect innerToolbarArea, string searchText)
        {
            (Rect searchFieldArea, Rect buttonRect) = innerToolbarArea.CutVertically(DropdownStyle.IconSize, true);

            bool keyDown = Event.current.type == EventType.KeyDown;

            searchText = EditorGUIHelper.FocusedTextField(searchFieldArea, searchText, "Search",
                DropdownStyle.SearchToolbarStyle, _searchFieldControlName);

            // When the search field is in focus, it uses the keyDown event on DownArrow while doing nothing.
            // We need this event for moving through the tree nodes.
            if (keyDown)
                Event.current.type = EventType.KeyDown;

            if (GUIHelper.CloseButton(buttonRect))
            {
                searchText = string.Empty;
                GUI.FocusControl(null); // Without this, the old text does not disappear for some reason.
                GUI.changed = true;
            }

            return searchText;
        }
    }
}