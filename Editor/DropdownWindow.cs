namespace UnityDropdown.Editor
{
    using System;
    using System.Reflection;
    using SolidUtilities;
    using SolidUtilities.Editor;
    using UnityEditor;
    using UnityEngine;

    internal enum DropdownWindowType { Context, Popup }

    /// <summary>Creates a dropdown window that shows the <see cref="DropdownMenu"/> elements.</summary>
    public partial class DropdownWindow : EditorWindow
    {
        public const string NoneElementName = "(None)";

        private DropdownMenu _dropdownMenu;

        internal static DropdownWindow ShowAsContext(DropdownMenu dropdownMenu, int windowHeight = 0)
        {
            var window = CreateInstance<DropdownWindow>();
            window.OnCreate(dropdownMenu, windowHeight, EditorHelper.GetCurrentMousePosition());
            return window;
        }

        internal static DropdownWindow ShowDropdown(DropdownMenu dropdownMenu, Vector2 windowPosition, int windowHeight = 0)
        {
            var window = CreateInstance<DropdownWindow>();
            window.OnCreate(dropdownMenu, windowHeight, windowPosition);
            return window;
        }

        internal static Vector2 GetCenteredPosition(DropdownMenu dropdownMenu)
        {
            Vector2 dropdownPosition = EditorGUIUtilityHelper.GetMainWindowPosition().center;
            dropdownPosition.x -= CalculateOptimalWidth(dropdownMenu.SelectionPaths) / 2f;
            return dropdownPosition.RoundUp();
        }

        /// <summary>
        /// This is basically a constructor. Since ScriptableObjects cannot have constructors,
        /// this one is called from a factory method.
        /// </summary>
        /// <param name="dropdownMenu">Tree that contains the dropdown items to show.</param>
        /// <param name="windowHeight">Height of the window. If set to 0, it will be auto-adjusted.</param>
        /// <param name="windowPosition">Position of the window to set.</param>
        private void OnCreate(DropdownMenu dropdownMenu, float windowHeight, Vector2 windowPosition)
        {
            ResetControl();
            wantsMouseMove = true;
            _dropdownMenu = dropdownMenu;
            _dropdownMenu.SelectionChanged += Close;
            _optimalWidth = CalculateOptimalWidth(_dropdownMenu.SelectionPaths);
            _preventExpandingHeight = new PreventExpandingHeight(windowHeight == 0f);
            _positionOnCreation = GetWindowRect(windowPosition, windowHeight);
            position = _positionOnCreation;
            ShowPopup();
        }

        private void OnGUI()
        {
            CloseOnEscPress();
            DrawContent();
            RepaintIfMouseWasUsed();
        }

        private void Update()
        {
            // Sometimes, Unity resets the window position to 0,0 after showing it as a drop-down, so it is necessary
            // to set it again once the window was created.
            if (!_positionWasSetAfterCreation)
            {
                _positionWasSetAfterCreation = true;
                position = _positionOnCreation;
            }

            // If called in OnGUI, the dropdown blinks before appearing for some reason. Thus, it works well only in Update.
            AdjustSizeIfNeeded();
        }

        private void OnLostFocus() => Close();

        private static void ResetControl()
        {
            GUIUtility.hotControl = 0;
            GUIUtility.keyboardControl = 0;
        }

        private void CloseOnEscPress()
        {
            if (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Escape)
            {
                Close();
                Event.current.Use();
            }
        }

        private void DrawContent()
        {
            using (new FixedRect(_preventExpandingHeight, position.width))
            {
                using (EditorGUILayoutHelper.VerticalBlock(_preventExpandingHeight,
                    DropdownStyle.BackgroundColor, out float contentHeight))
                {
                    _dropdownMenu.Draw();

                    if (Event.current.type == EventType.Repaint)
                        _contentHeight = contentHeight;
                }

                EditorGUIHelper.DrawBorders(position.width, position.height, DropdownStyle.BorderColor);
            }
        }

        private void RepaintIfMouseWasUsed()
        {
            if (Event.current.isMouse || Event.current.type == EventType.Used || _dropdownMenu.RepaintRequested)
            {
                Repaint();
                _dropdownMenu.RepaintRequested = false;
            }
        }

        private readonly struct FixedRect : IDisposable
        {
            private readonly bool _enable;

            public FixedRect(bool enable, float windowWidth)
            {
                _enable = enable;

                if (_enable)
                    GUILayout.BeginArea(new Rect(0f, 0f, windowWidth, DropdownStyle.MaxWindowHeight));
            }

            public void Dispose()
            {
                if (_enable)
                    GUILayout.EndArea();
            }
        }
    }
}