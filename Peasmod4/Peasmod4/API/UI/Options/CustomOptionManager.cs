using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Il2CppSystem.Text;
using UnityEngine;

namespace Peasmod4.API.UI.Options;

public class CustomOptionManager
{
    public static List<CustomOption> CustomOptions = new ();
        
    public static List<CustomRoleOption> CustomRoleOptions = new ();

    public static List<CustomOption> HudVisibleOptions => CustomOptions.FindAll(option => option.HudVisible);
        
    public static List<CustomOption> MenuVisibleOptions => CustomOptions.FindAll(option => option.MenuVisible && !option.AdvancedRoleOption);

    public static ToggleOption ToggleOptionPrefab;
    public static NumberOption NumberOptionPrefab;
    public static StringOption StringOptionPrefab;
    public static RoleOptionSetting RoleOptionPrefab;
    public static GameObject AdvancedRoleSettingsPrefab;
}