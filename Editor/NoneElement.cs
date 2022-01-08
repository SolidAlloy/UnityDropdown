namespace UnityDropdown.Editor
{
    using UnityEditor;
    using UnityEngine;

    /// <summary>
    /// A node that represents the null type value. It is drawn separately from other nodes and has its own root.
    /// </summary>
    internal class NoneElement<T> : DropdownNode<T>
    {
        private NoneElement(DropdownNode<T> root, DropdownMenu<T> parentMenu)
            : base(default, root, parentMenu, DropdownWindow.NoneElementName, null, null) { }

        public static NoneElement<T> Create(DropdownMenu<T> parentMenu)
        {
            var root = CreateRoot(parentMenu);
            var child = new NoneElement<T>(root, parentMenu);
            root.ChildNodes.Add(child);
            return child;
        }

        public override void DrawSelfAndChildren(int indentLevel, Rect visibleRect)
        {
            if (ReserveSpaceAndStop())
                return;

            if (Event.current.type == EventType.Repaint)
                DrawNodeContent(0, 1);

            HandleMouseEvents();

            DrawBottomSeparator();
        }

        private void DrawBottomSeparator()
        {
            var lineRect = new Rect(Rect.x, Rect.y + Rect.height - 1f, Rect.width, 1f);
            EditorGUI.DrawRect(lineRect, DropdownStyle.BorderColor);
        }
    }
}