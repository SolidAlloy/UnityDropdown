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
    /// Represents a node tree that contains folders and items with values.
    /// </summary>
    /// <remarks>It is also responsible for drawing all the nodes, along with search toolbar and scrollbar.</remarks>
    public abstract class DropdownMenu : IRepainter
    {
        internal bool RepaintRequested;

        protected readonly Scrollbar _scrollbar;
        protected string _searchString = string.Empty;
        protected Rect _visibleRect;

        private readonly string _searchFieldControlName = GUID.Generate().ToString();
        private readonly bool _drawSearchbar;

        /// <summary>
        /// Whether the dropdown is in search mode at the moment (items are filtered by a string entered in the search field).
        /// </summary>
        public bool IsInSearchMode { get; private set; }

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

        /// <summary>
        /// Shows the dropdown window under a mouse pointer.
        /// </summary>
        /// <param name="windowHeight">
        ///     The height of the dropdown in pixels. If not set, the height is dynamic (min 100, max 600 pixels).
        ///     If set outside the height limits, the height will be clamped.
        /// </param>
        /// <returns>An instance of <see cref="DropdownWindow"/>.</returns>
        public DropdownWindow ShowAsContext(int windowHeight = 0) => DropdownWindow.ShowAsContext(this, windowHeight);

        /// <summary>
        /// Show the dropdown window at a specific window position.
        /// </summary>
        /// <param name="windowPosition">The position of the top left corner of a dropdown window.</param>
        /// <param name="windowHeight">
        ///     The height of the dropdown in pixels. If not set, the height is dynamic (min 100, max 600 pixels).
        ///     If set outside the height limits, the height will be clamped.
        /// </param>
        /// <returns>An instance of <see cref="DropdownWindow"/>.</returns>
        /// <seealso cref="GetCenteredPosition"/>
        public DropdownWindow ShowDropdown(Vector2 windowPosition, int windowHeight = 0) => DropdownWindow.ShowDropdown(this, windowPosition, windowHeight);

        /// <summary>
        /// Returns a position on the screen that can be passed to <see cref="ShowDropdown"/> to show a dropdown window at the center of the screen.
        /// </summary>
        /// <returns>A position on the screen that can be passed to <see cref="ShowDropdown"/> to show a dropdown window at the center of the screen.</returns>
        public Vector2 GetCenteredPosition() => DropdownWindow.GetCenteredPosition(this);

        /// <summary>
        /// Closes the dropdown window and invokes <see cref="DropdownMenu{T}.OnValueSelected"/> with the currently selected item.
        /// </summary>
        public virtual void FinalizeSelection()
        {
            SelectionChanged?.Invoke();
        }

        /// <summary>
        /// Draws the menu using <see cref="GUILayout"/>. Invoked by <see cref="DropdownWindow"/>.
        /// </summary>
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

            if ( ! IsInSearchMode)
                NoneElement?.DrawSelfAndChildren(default, default);

            using (_scrollbar.Draw())
            {
                _visibleRect = GUIClip.GetVisibleRect();
                DrawTree(_visibleRect);
            }
        }

        /// <summary>
        /// Triggers a dropdown window repaint. Useful when the window doesn't have a mouse focus and the selected
        /// item was not changed but something else changed in the visual look of the menu, so it needs to be repainted.
        /// For example, called by <see cref="Scrollbar"/> when it needs to scroll to a certain node.
        /// </summary>
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
            IsInSearchMode = false;
            _scrollbar.RequestScrollToNode(_SelectedNode, Scrollbar.NodePosition.Center);
        }

        private void EnableSearchMode()
        {
            if ( ! IsInSearchMode)
                _scrollbar.ToTop();

            IsInSearchMode = true;

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
            var nodes = IsInSearchMode ? SearchModeTree : Nodes;

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