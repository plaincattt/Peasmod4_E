using System;
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

    public static PlayerControl FindNearestPlayer(this PlayerControl player, Predicate<PlayerControl> selector, float distance = 3f, bool ignoreColliders = false)
    {
        var myPos = player.GetTruePosition();
        
        var outputList = new List<PlayerControl>();
        foreach (var target in PlayerControl.AllPlayerControls)
        {
            if (selector.Invoke(target))
            {
                var vector = target.GetTruePosition() - myPos;
                var magnitude = vector.magnitude;
                if (magnitude <= distance && (ignoreColliders || !PhysicsHelpers.AnyNonTriggersBetween(myPos, vector.normalized, magnitude, Constants.ShipAndObjectsMask)))
                {
                    outputList.Add(target);
                }
            }
        }
        
        outputList.Sort(delegate(PlayerControl a, PlayerControl b)
        {
            float magnitude2 = (a.GetTruePosition() - myPos).magnitude;
            float magnitude3 = (b.GetTruePosition() - myPos).magnitude;
            if (magnitude2 > magnitude3)
            {
                return 1;
            }
            if (magnitude2 < magnitude3)
            {
                return -1;
            }
            return 0;
        });
        
        if (outputList.Count == 0)
            return null;
        
        return outputList.First();
    }

    public static GameObject FindNearestObject(this PlayerControl player, Predicate<GameObject> selector,
        float distance = 3f, bool ignoreColliders = false)
    {
        GameObject result = null;
        var biggestDistance = distance;
        var myPos = player.GetTruePosition();
        foreach (var collider in Physics2D.OverlapCircleAll(myPos, biggestDistance))
        {
            var obj = collider.gameObject;
            if ((obj.GetComponent<SpriteRenderer>() != null || obj.GetComponentInChildren<SpriteRenderer>() != null) && selector.Invoke(obj))
            {
                Vector2 vector = new Vector2(obj.transform.position.x, obj.transform.position.y) - myPos;
                float magnitude = vector.magnitude;
                if (magnitude <= biggestDistance && (ignoreColliders || !PhysicsHelpers.AnyNonTriggersBetween(myPos, vector.normalized, magnitude, Constants.ShipAndObjectsMask)))
                {
                    result = obj;
                    biggestDistance = magnitude;
                }
            }
        }

        return result;
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