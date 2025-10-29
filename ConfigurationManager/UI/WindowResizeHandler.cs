using UnityEngine;

namespace ConfigurationManager.UI
{
    /// <summary>
    /// Handles window resizing functionality
    /// </summary>
    internal class WindowResizeHandler
    {
        private const int ResizeHandleSize = 30;
        private const int MinWindowWidth = 200;
        private const int MinWindowHeight = 100;

        private bool _isResizing;
        private Vector2 _resizeStart;

        /// <summary>
        /// Handle window resize logic
        /// </summary>
        /// <param name="windowRect">Current window rectangle</param>
        /// <param name="columnWidthsChanged">True if the window width changed</param>
        public Rect HandleWindowResize(Rect windowRect, out bool columnWidthsChanged)
        {
            columnWidthsChanged = false;

            var resizeHandleRect = new Rect(
                windowRect.x + windowRect.width - ResizeHandleSize / 2,
                windowRect.y + windowRect.height - ResizeHandleSize / 2,
                ResizeHandleSize,
                ResizeHandleSize
            );

            var currentEvent = Event.current;
            bool isHoveringResizeRect = resizeHandleRect.Contains(currentEvent.mousePosition);

            if (isHoveringResizeRect)
            {
                GUI.Box(resizeHandleRect, "↘", new GUIStyle(GUI.skin.label)
                {
                    alignment = TextAnchor.MiddleCenter,
                    fontSize = 30
                });
            }

            if (currentEvent.type == EventType.MouseDown && isHoveringResizeRect)
            {
                _isResizing = true;
                _resizeStart = currentEvent.mousePosition;
                currentEvent.Use();
            }
            else if (currentEvent.type == EventType.MouseUp && _isResizing)
            {
                _isResizing = false;
                currentEvent.Use();
            }
            else if (currentEvent.type == EventType.MouseDrag && _isResizing)
            {
                var delta = currentEvent.mousePosition - _resizeStart;
                var newWidth = Mathf.Max(MinWindowWidth, windowRect.width + delta.x);
                var newHeight = Mathf.Max(MinWindowHeight, windowRect.height + delta.y);

                windowRect = new Rect(
                    windowRect.x,
                    windowRect.y,
                    newWidth,
                    newHeight
                );

                columnWidthsChanged = true;
                _resizeStart = currentEvent.mousePosition;
                currentEvent.Use();
            }
            return windowRect;
        }
    }
}