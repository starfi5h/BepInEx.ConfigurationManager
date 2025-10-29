using System;
using System.Reflection;
using UnityEngine;

namespace ConfigurationManager.Utilities
{
    /// <summary>
    /// Manages cursor lock state and visibility for Unity 4.x compatibility
    /// </summary>
    internal class CursorManager
    {
        private PropertyInfo _curLockState;
        private PropertyInfo _curVisible;
        private int _previousCursorLockState;
        private bool _previousCursorVisible;
        private bool _obsoleteCursor;

        /// <summary>
        /// Initialize cursor management using reflection for Unity 4.x compatibility
        /// </summary>
        public void Initialize()
        {
            // Use reflection to keep compatibility with unity 4.x since it doesn't have Cursor
            var tCursor = typeof(Cursor);
            _curLockState = tCursor.GetProperty("lockState", BindingFlags.Static | BindingFlags.Public);
            _curVisible = tCursor.GetProperty("visible", BindingFlags.Static | BindingFlags.Public);

            if (_curLockState == null && _curVisible == null)
            {
                _obsoleteCursor = true;

                _curLockState = typeof(Screen).GetProperty("lockCursor", BindingFlags.Static | BindingFlags.Public);
                _curVisible = typeof(Screen).GetProperty("showCursor", BindingFlags.Static | BindingFlags.Public);
            }
        }

        /// <summary>
        /// Save the current cursor state before showing the config window
        /// </summary>
        public void SaveCursorState()
        {
            if (_curLockState != null)
            {
                _previousCursorLockState = _obsoleteCursor
                    ? Convert.ToInt32((bool)_curLockState.GetValue(null, null))
                    : (int)_curLockState.GetValue(null, null);
                _previousCursorVisible = (bool)_curVisible.GetValue(null, null);
            }
        }

        /// <summary>
        /// Restore the cursor state to what it was before showing the config window
        /// </summary>
        public void RestoreCursorState()
        {
            if (!_previousCursorVisible || _previousCursorLockState != 0) // 0 = CursorLockMode.None
                SetUnlockCursor(_previousCursorLockState, _previousCursorVisible);
        }

        /// <summary>
        /// Set cursor lock state and visibility
        /// </summary>
        /// <param name="lockState">Lock state (0 = CursorLockMode.None)</param>
        /// <param name="cursorVisible">Whether cursor should be visible</param>
        public void SetUnlockCursor(int lockState, bool cursorVisible)
        {
            if (_curLockState != null)
            {
                // Do through reflection for unity 4 compat
                //Cursor.lockState = CursorLockMode.None;
                //Cursor.visible = true;
                if (_obsoleteCursor)
                    _curLockState.SetValue(null, Convert.ToBoolean(lockState), null);
                else
                    _curLockState.SetValue(null, lockState, null);

                _curVisible.SetValue(null, cursorVisible, null);
            }
        }
    }
}
