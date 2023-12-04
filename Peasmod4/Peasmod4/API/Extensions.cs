using System.Collections.Generic;
using System.Linq;
using AmongUs.GameOptions;
using BepInEx.Unity.IL2CPP.Utils.Collections;
using UnityEngine;

namespace Peasmod4.API;

public static class Extensions
{
    public static PlayerControl GetPlayer(this byte id)
    {
        return PlayerControl.AllPlayerControls.ToArray().ToList().Find(player => player.PlayerId == id);
    }

    public static bool IsLocal(this GameData.PlayerInfo player) => IsLocal(player.Object);
    
    public static bool IsLocal(this PlayerControl player)
    {
        return PlayerControl.LocalPlayer.PlayerId == player.PlayerId;
    }

    public static string GetTranslation(this StringNames stringName)
    {
        return TranslationController.Instance.GetString(stringName);
    }
    
    /// <summary>
    /// Gets the text color from a <see cref="Color"/>
    /// </summary>
    public static string GetTextColor(this Color color)
    {
        var r = Mathf.RoundToInt(color.r * 255f).ToString("X2");
            
        var g = Mathf.RoundToInt(color.g * 255f).ToString("X2");
            
        var b = Mathf.RoundToInt(color.b * 255f).ToString("X2");
            
        var a = Mathf.RoundToInt(color.a * 255f).ToString("X2");

        return $"<color=#{r}{g}{b}{a}>";
    }

    public static List<T> WrapToSystem<T>(this Il2CppSystem.Collections.Generic.List<T> list)
    {
        var newList = new List<T>();
        foreach (var item in list)
        {
            newList.Add(item);
        }

        return newList;
    }
    
    public static Il2CppSystem.Collections.Generic.List<T> WrapToIl2Cpp<T>(this List<T> list)
    {
        var newList = new Il2CppSystem.Collections.Generic.List<T>();
        foreach (var item in list)
        {
            newList.Add(item);
        }

        return newList;
    }
}