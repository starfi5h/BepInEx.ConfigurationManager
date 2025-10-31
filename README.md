# Configuration Manager 

A forked version of BepInEx's in-game configuration manager, allowing you to easily change mod configurations directly within the game.  
The default hotkey to open or close the menu is `Ctrl+F1`. This can be changed in the config manager itself.  
Note: Some mods still require restarting the game to apply the changes.  
  
这是 BepInEx 游戏内配置管理器的一个分支版本，允许您直接在游戏内轻松更改模组配置。  
打开或关闭菜单的默认热键是`Ctrl+F1`。这可以在配置管理器本身中更改。  
注意：部分模组仍需重启游戏，变更后的配置才能生效。  

## Changes from the original

![EN](https://raw.githubusercontent.com/starfi5h/BepInEx.ConfigurationManager/refs/heads/main/img/Demo_EN.png)   
   
This fork version of [BepInEx.ConfigurationManager](https://github.com/BepInEx/BepInEx.ConfigurationManager) is for use with Dyson Sphere Program. It was forked from version `17.0` to make it works with BepInEx v5.4.17.  
The changes from the original version are as follows:  
- Window is draggable and resizable
- Background colors are customizable
- Text size and color are customizable
- Change the close behavior from clicking outside of window to top-left close button and hotkey
- Configuration Manager settings are changeable within the manager itself

UI layout changes:  
- English and Chinese localization switch
- Button to open BepInEx log and Unity log in Debug mode  
- Left and right buttons beside plugin name to reload the config and open the config file  

## 与原版的区别

![CN](https://raw.githubusercontent.com/starfi5h/BepInEx.ConfigurationManager/refs/heads/main/img/Demo_CN.png)   

这个 [BepInEx.ConfigurationManager](https://github.com/BepInEx/BepInEx.ConfigurationManager) 的分支版本是供戴森球计划使用的。它是从版本`17.0`分支出来的，目的是使其能与BepInEx v5.4.17配合工作。  

与原版相比，更改如下：
- **窗口可拖动和调整大小**
- **背景颜色可自定义**
- **文本大小和颜色可自定义**
- **关闭行为**从点击窗口外部更改为**左上角的关闭按钮和热键**
- **配置管理器设置**可在管理器**内部更改**

UI 布局更改：  
- **英汉本地化切换**
- **按钮**：在调试模式下，可打开 BepInEx 日志和 Unity 日志
- **插件名称旁边**的左右**按钮**：用于**重新加载配置**和**打开配置文件**

----

# Original README

## Plugin / mod configuration manager for BepInEx 5
An easy way to let user configure how a plugin behaves without the need to make your own GUI. The user can change any of the settings you expose, even keyboard shortcuts.

The configuration manager can be accessed in-game by pressing the hotkey. Hover over the setting names to see their descriptions, if any.

Note: The .xml file is useful for plugin developers when referencing ConfigurationManager.dll in your plugin, it will provide descriptions for types and methods to your IDE. Users can ignore it.

## How to make my mod compatible?
ConfigurationManager will automatically display all settings from your plugin's `Config`. All metadata (e.g. description, value range) will be used by ConfigurationManager to display the settings to the user.

In most cases you don't have to reference ConfigurationManager.dll or do anything special with your settings. Simply make sure to add as much metadata as possible (doing so will help all users, even if they use the config files directly). Always add descriptive section and key names, descriptions, and acceptable value lists or ranges (wherever applicable).

### How to make my setting into a slider?
Specify `AcceptableValueRange` when creating your setting. If the range is 0f - 1f or 0 - 100 the slider will be shown as % (this can be overridden below).
```c#
CaptureWidth = Config.AddSetting("Section", "Key", 1, new ConfigDescription("Description", new AcceptableValueRange<int>(0, 100)));
```

### How to make my setting into a drop-down list?
Specify `AcceptableValueList` when creating your setting. If you use an enum you don't need to specify AcceptableValueList, all of the enum values will be shown. If you want to hide some values, you will have to use the attribute.

Note: You can add `System.ComponentModel.DescriptionAttribute` to your enum's items to override their displayed names. For example:
```c#
public enum MyEnum
{
    // Entry1 will be shown in the combo box as Entry1
    Entry1,
    [Description("Entry2 will be shown in the combo box as this string")]
    Entry2
}
```

### How to allow user to change my keyboard shorcuts / How to easily check for key presses?
Add a setting of type KeyboardShortcut. Use the value of this setting to check for inputs (recommend using IsDown) inside of your Update method.

The KeyboardShortcut class supports modifier keys - Shift, Control and Alt. They are properly handled, preventing common problems like K+Shift+Control triggering K+Shift when it shouldn't have.
```c#
private ConfigEntry<KeyboardShortcut> ShowCounter { get; set; }

public Constructor()
{
    ShowCounter = Config.AddSetting("Hotkeys", "Show FPS counter", new KeyboardShortcut(KeyCode.U, KeyCode.LeftShift));
}

private void Update()
{
    if (ShowCounter.Value.IsDown())
    {
        // Handle the key press
    }
}
```

## Overriding default Configuration Manager behavior
You can change how a setting is shown inside the configuration manager window by passing an instance of a special class as a tag of the setting. The special class code can be downloaded [here](ConfigurationManagerAttributes.cs). Simply download the .cs file and drag it into your project.
- You do not have to reference ConfigurationManager.dll for this to work.
- The class will work as long as name of the class and declarations of its fields remain unchanged. 
- Avoid making the class public to prevent conflicts with other plugins. If you want to share it between your plugins either give each a copy, or move it to your custom namespace.
- If the ConfigurationManager plugin is not installed in the game, this class will be safely ignored and your plugin will work as normal.

Here's an example of overriding order of settings and marking one of the settings as advanced:
```c#
// Override IsAdvanced and Order
Config.AddSetting("X", "1", 1, new ConfigDescription("", null, new ConfigurationManagerAttributes { IsAdvanced = true, Order = 3 }));
// Override only Order, IsAdvanced stays as the default value assigned by ConfigManager
Config.AddSetting("X", "2", 2, new ConfigDescription("", null, new ConfigurationManagerAttributes { Order = 1 }));
Config.AddSetting("X", "3", 3, new ConfigDescription("", null, new ConfigurationManagerAttributes { Order = 2 }));
```

### How to make a custom editor for my setting?
If you are using a setting type that is not supported by ConfigurationManager, you can add a drawer Action for it. The Action will be executed inside OnGUI, use GUILayout to draw your setting as shown in the example below.

To use a custom seting drawer for an individual setting, use the `CustomDrawer` field in the attribute class. See above for more info on the attribute class.
```c#
void Start()
{
    // Add the drawer as a tag to this setting.
    Config.AddSetting("Section", "Key", "Some value" 
        new ConfigDescription("Desc", null, new ConfigurationManagerAttributes{ CustomDrawer = MyDrawer });
}

static void MyDrawer(BepInEx.Configuration.ConfigEntryBase entry)
{
    // Make sure to use GUILayout.ExpandWidth(true) to use all available space
    GUILayout.Label(entry.BoxedValue, GUILayout.ExpandWidth(true));
}
```
#### Add a custom editor globally
You can specify a drawer for all settings of a setting type. Do this by using `ConfigurationManager.RegisterCustomSettingDrawer(Type, Action<SettingEntryBase>)`.

**Warning:** This requires you to reference ConfigurationManager.dll in your project and is not recommended unless you are sure all users will have it installed. It's usually better to use the above method to add the custom drawer to each setting individually instead.
```c#
void Start()
{
    ConfigurationManager.RegisterCustomSettingDrawer(typeof(MyType), CustomDrawer);
}

static void CustomDrawer(SettingEntryBase entry)
{
    GUILayout.Label((MyType)entry.Get(), GUILayout.ExpandWidth(true));
}
```
