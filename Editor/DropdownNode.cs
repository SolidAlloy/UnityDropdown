namespace UnityDropdown.Editor
{
    using System.Collections.Generic;
    using JetBrains.Annotations;
    using SolidUtilities;
    using SolidUtilities.Editor;
    using UnityEditor;
    using UnityEngine;
    using UnityEngine.Assertions;

    /// <summary>
    /// A node in the dropdown tree. It may be a folder or an item that represents a value.
    /// </summary>
    public abstract class DropdownNode
    {
        [PublicAPI]
        public readonly string Name;

        public readonly string SearchName;

        private readonly DropdownNode _parentNode;
        private readonly Texture _icon;
        private bool _expanded;

        private Rect _rect;
        public Rect Rect => _rect;

        /// <summary>
        /// If the node is folder, this shows whether it is expanded or closed. If the node is type item, setting this
        /// will do nothing, and its value is always false.
        /// </summary>
        public bool Expanded
        {
            get => IsFolder && _expanded;
            set => _expanded = value;
        }

        public bool IsFolder => _ChildNodes.Count != 0;

        public bool IsRoot => _parentNode == null;

        public bool IsSelected => ParentMenu._SelectedNode == this;

        protected abstract DropdownMenu ParentMenu { get; }

        protected abstract IReadOnlyCollection<DropdownNode> _ChildNodes { get; }

        private bool IsHoveredOver => _rect.Contains(Event.current.mousePosition);

        protected DropdownNode(DropdownNode parentNode, string name, string searchName, Texture icon)
        {
            _parentNode = parentNode;
            Assert.IsNotNull(name);
            Name = name;
            SearchName = searchName;
            _icon = icon;
        }

        public IEnumerable<DropdownNode> GetParentNodesRecursive(
            bool includeSelf)
        {
            if (includeSelf)
                yield return this;

            if (IsRoot)
                yield break;

            foreach (DropdownNode node in _parentNode.GetParentNodesRecursive(true))
                yield return node;
        }

        public virtual void DrawSelfAndChildren(int indentLevel, Rect visibleRect)
        {
            Draw(indentLevel, visibleRect);

            if ( ! Expanded)
                return;

            foreach (DropdownNode childItem in _ChildNodes)
                childItem.DrawSelfAndChildren(indentLevel + 1, visibleRect);
        }

        /// <summary>Reserves a space for the rect but does not draw its content.</summary>
        /// <returns>True if there is no need to draw the contents.</returns>
        protected bool ReserveSpaceAndStop()
        {
            Rect rect = GUILayoutUtility.GetRect(0f, DropdownStyle.NodeHeight);

            if (Event.current.type == EventType.Layout)
                return true;

            if (Event.current.type == EventType.Repaint || _rect.width == 0f)
                _rect = rect;

            return false;
        }

        protected void DrawNodeContent(int indentLevel, int raiseText = 0)
        {
            if (IsSelected)
            {
                EditorGUI.DrawRect(_rect, DropdownStyle.SelectedColor);
            }
            else if (IsHoveredOver)
            {
                EditorGUI.DrawRect(_rect, DropdownStyle.HighlightedColor);
            }

            Rect indentedNodeRect = _rect;
            indentedNodeRect.xMin += DropdownStyle.GlobalOffset + indentLevel * DropdownStyle.IndentWidth;
            indentedNodeRect.y -= raiseText;

            if (IsFolder)
            {
                Rect triangleRect = GetTriangleRect(indentedNodeRect);
                DrawTriangleIcon(triangleRect);
            }

            DrawLabel(indentedNodeRect);

            DrawSeparator();
        }

        protected abstract void SetSelfSelected();

        protected void HandleMouseEvents()
        {
            bool leftMouseButtonWasPressed = Event.current.type == EventType.MouseDown
                                             && IsHoveredOver
                                             && Event.current.button == 0;

            if ( ! leftMouseButtonWasPressed)
                return;

            if (IsFolder)
            {
                Expanded = !Expanded;
            }
            else
            {
                SetSelfSelected();
                ParentMenu.FinalizeSelection();
            }

            Event.current.Use();
        }

        private void Draw(int indentLevel, Rect visibleRect)
        {
            if (ReserveSpaceAndStop())
                return;

            if (_rect.y > 1000f && NodeIsOutsideOfVisibleRect(visibleRect))
                return;

            if (Event.current.type == EventType.Repaint)
                DrawNodeContent(indentLevel);

            HandleMouseEvents();
        }

        private bool NodeIsOutsideOfVisibleRect(Rect visibleRect) =>
            _rect.y + _rect.height < visibleRect.y || _rect.y > visibleRect.y + visibleRect.height;

        private static Rect GetTriangleRect(Rect nodeRect)
        {
            Rect triangleRect = nodeRect.AlignMiddleVertically(DropdownStyle.IconSize);
            triangleRect.width = DropdownStyle.IconSize;
            triangleRect.x -= DropdownStyle.IconSize;
            return triangleRect;
        }

        private void DrawTriangleIcon(Rect triangleRect)
        {
            EditorIcon triangleIcon = Expanded ? EditorIcons.TriangleDown : EditorIcons.TriangleRight;

            Texture2D tintedIcon = IsHoveredOver
                ? triangleIcon.Highlighted
                : triangleIcon.Active;

            tintedIcon.Draw(triangleRect);
        }

        private void DrawLabel(Rect indentedNodeRect)
        {
            Rect labelRect = indentedNodeRect.AlignMiddleVertically(DropdownStyle.LabelHeight);
            string label = ParentMenu.DrawInSearchMode ? SearchName : Name;
            GUIStyle style = IsSelected ? DropdownStyle.SelectedLabelStyle : DropdownStyle.DefaultLabelStyle;
            GUI.Label(labelRect, GUIContentHelper.Temp(label, _icon), style);
        }

        private void DrawSeparator()
        {
            var lineRect = new Rect(_rect.x, _rect.y - 1f, _rect.width, 1f);
            EditorGUI.DrawRect(lineRect, DropdownStyle.DarkSeparatorLine);
            ++lineRect.y;
            EditorGUI.DrawRect(lineRect, DropdownStyle.LightSeparatorLine);
        }
    }
}