
using UnityEngine;

namespace ConfigurationManager.UI
{
    /// <summary>
    /// Centralized management for all GUIStyles to prevent repeated creation
    /// in the OnGUI loop, thereby reducing GC overhead and improving performance.
    /// </summary>
    internal static class StyleManager
    {
        public static GUISkin CustomSkin { get; private set; }
        public static GUIStyle WindowStyle { get; private set; }
        public static GUIStyle HeaderStyle { get; private set; }
        public static GUIStyle TooltipStyle { get; private set; }
        public static GUIStyle ResizeHandleStyle { get; private set; }
        public static GUIStyle DropdownBackgroundStyle { get; private set; }


        // This can only call in OnGUI
        public static void CreateStyles(ManagerSettings settings)
        {
            if (CustomSkin != null) return;
            Reinitialize(settings);
        }

        public static void CleanCache()
        {
            Object.Destroy(CustomSkin);
            CustomSkin = null;
        }

        private static void Reinitialize(ManagerSettings settings)
        {
            int fontSize = settings.FontSize.Value;
            

            var windowBackground = new Texture2D(1, 1, TextureFormat.ARGB32, false);
            windowBackground.SetPixel(0, 0, settings.BackgroundColor.Value);
            windowBackground.Apply();

            var tooltipBg = new Texture2D(1, 1, TextureFormat.ARGB32, false);
            tooltipBg.SetPixel(0, 0, Color.black);
            tooltipBg.Apply();

            WindowStyle = new GUIStyle
            { 
                normal = new GUIStyleState { background = windowBackground } 
            };

            HeaderStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.UpperCenter,
                wordWrap = true,
                stretchWidth = true,
                fontSize = fontSize + 2
            };

            TooltipStyle = new GUIStyle
            {
                normal = new GUIStyleState { textColor = Color.white, background = tooltipBg },
                wordWrap = true,
                alignment = TextAnchor.MiddleCenter
            };

            ResizeHandleStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 30
            };

            DropdownBackgroundStyle = new GUIStyle
            {
                normal = new GUIStyleState { background = windowBackground }
            };

            CustomSkin = Object.Instantiate(GUI.skin);
            UpdateCustomSkinFontSize(fontSize);
        }

        private static void UpdateCustomSkinFontSize(int fontSize)
        {
            CustomSkin.label.fontSize = fontSize;
            CustomSkin.button.fontSize = fontSize;
            CustomSkin.textField.fontSize = fontSize;
            CustomSkin.toggle.fontSize = fontSize;
            CustomSkin.box.fontSize = fontSize;
            CustomSkin.window.fontSize = fontSize;
        }
    }
}
