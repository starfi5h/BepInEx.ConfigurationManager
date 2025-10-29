// Made by MarC0 / ManlyMarco
// Copyright 2018 GNU General Public License v3.0

using UnityEngine;

namespace ConfigurationManager.UI
{
    /// <summary>
    /// Manages the configuration window rendering and layout
    /// </summary>
    internal partial class ConfigWindow
    {
        private const int WindowId = -68;        
        private static readonly Color AdvancedSettingColor = new Color(1f, 0.95f, 0.67f, 1f);

        private readonly ManagerSettings _settings;
        private readonly PluginSettingsDataManager _dataManager;
        private readonly WindowResizeHandler _resizeHandler;
        private readonly SettingFieldDrawer _fieldDrawer;

        internal Rect SettingWindowRect { get; private set; }
        internal int LeftColumnWidth { get; private set; }
        internal int RightColumnWidth { get; private set; }
        internal static Texture2D TooltipBg { get; private set; }
        internal static Texture2D WindowBackground { get; private set; }


        public ConfigWindow(ManagerSettings settings, PluginSettingsDataManager dataManager)
        {
            _settings = settings;
            _dataManager = dataManager;
            _resizeHandler = new WindowResizeHandler();
            _fieldDrawer = new SettingFieldDrawer(this);
        }

        /// <summary>
        /// Initialize textures and resources
        /// </summary>
        public void Initialize()
        {
            var background = new Texture2D(1, 1, TextureFormat.ARGB32, false);
            background.SetPixel(0, 0, Color.black);
            background.Apply();
            TooltipBg = background;

            var windowBackground = new Texture2D(1, 1, TextureFormat.ARGB32, false);
            windowBackground.SetPixel(0, 0, _settings.BackgroundColor.Value);
            windowBackground.Apply();
            WindowBackground = windowBackground;
        }

        /// <summary>
        /// Called when window is shown
        /// </summary>
        public void OnWindowShown()
        {
            CalculateWindowRect();
        }

        /// <summary>
        /// Save current window position to config
        /// </summary>
        public void SaveWindowPosition()
        {
            _settings.WindowX.Value = (int)SettingWindowRect.x;
            _settings.WindowY.Value = (int)SettingWindowRect.y;
            _settings.WindowWidth.Value = (int)SettingWindowRect.width;
            _settings.WindowHeight.Value = (int)SettingWindowRect.height;
        }

        /// <summary>
        /// Draw the main configuration window
        /// </summary>
        public void DrawWindow()
        {
            GUI.Box(SettingWindowRect, GUIContent.none,
                new GUIStyle { normal = new GUIStyleState { background = WindowBackground } });

            SettingWindowRect = _resizeHandler.HandleWindowResize(SettingWindowRect, out var columnWidthsChanged);

            if (columnWidthsChanged)
            {
                LeftColumnWidth = Mathf.RoundToInt(SettingWindowRect.width / 2.5f);
                RightColumnWidth = (int)SettingWindowRect.width - LeftColumnWidth - 115;
            }

            SettingWindowRect = GUILayout.Window(WindowId, SettingWindowRect,
                DrawSettingsWindow, "Plugin / mod settings");

            // Eat only left mouse click in the window
            bool inWindow = SettingWindowRect.Contains(Event.current.mousePosition);
            bool isClick = Input.GetMouseButton(0) && Input.GetMouseButtonDown(0);
            if (!SettingFieldDrawer.SettingKeyboardShortcut && inWindow && isClick)
                Input.ResetInputAxes();
        }

        private void CalculateWindowRect()
        {
            var width = _settings.WindowWidth.Value > 0 ? _settings.WindowWidth.Value : Mathf.Min(Screen.width, 650);
            var height = _settings.WindowHeight.Value > 0 ? _settings.WindowHeight.Value : (Screen.height < 560 ? Screen.height : Screen.height - 100);
            var offsetX = _settings.WindowX.Value >= 0 ? _settings.WindowX.Value : Mathf.RoundToInt((Screen.width - width) / 2f);
            var offsetY = _settings.WindowY.Value >= 0 ? _settings.WindowY.Value : Mathf.RoundToInt((Screen.height - height) / 2f);
            SettingWindowRect = new Rect(offsetX, offsetY, width, height);

            LeftColumnWidth = Mathf.RoundToInt(SettingWindowRect.width / 2.5f);
            RightColumnWidth = (int)SettingWindowRect.width - LeftColumnWidth - 115;
        }

        private void DrawSettingsWindow(int id)
        {
            // Close button
            GUILayout.BeginArea(new Rect(SettingWindowRect.width - 27f, 1f, 25f, 21f));
            if (GUILayout.Button("X"))
            {
                ConfigurationManager.Instance.DisplayingWindow = false;
            }
            GUILayout.EndArea();

            // Title bar drag
            var titleBarRect = new Rect(0, 0, SettingWindowRect.width, 20);
            GUI.DragWindow(titleBarRect);

            GUILayout.Space(3);
            DrawWindowHeader(); // ConfigWindow.Header.cs

            DrawScrollItems(); // ConfigWindow.Items.cs

            if (!SettingFieldDrawer.DrawCurrentDropdown())
                DrawTooltip(SettingWindowRect);
        }

        private static void DrawTooltip(Rect area)
        {
            if (!string.IsNullOrEmpty(GUI.tooltip))
            {
                var currentEvent = Event.current;

                var style = new GUIStyle
                {
                    normal = new GUIStyleState { textColor = Color.white, background = TooltipBg },
                    wordWrap = true,
                    alignment = TextAnchor.MiddleCenter
                };

                const int width = 400;
                var height = style.CalcHeight(new GUIContent(GUI.tooltip), 400) + 10;

                var x = currentEvent.mousePosition.x + width > area.width
                    ? area.width - width
                    : currentEvent.mousePosition.x;

                var y = currentEvent.mousePosition.y + 25 + height > area.height
                    ? currentEvent.mousePosition.y - height
                    : currentEvent.mousePosition.y + 25;

                GUI.Box(new Rect(x, y, width, height), GUI.tooltip, style);
            }
        }
    }
}
