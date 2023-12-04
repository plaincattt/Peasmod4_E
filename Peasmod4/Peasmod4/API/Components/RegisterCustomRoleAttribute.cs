using System;
using System.Reflection;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;
using Peasmod4.API.Roles;

namespace Peasmod4.API.Components;

[AttributeUsage(AttributeTargets.Class)]
public class RegisterCustomRoleAttribute : Attribute
{
    public static void Register(Assembly assembly)
    {
        foreach (var type in assembly.GetTypes())
        {
            var attribute = type.GetCustomAttribute<RegisterCustomRoleAttribute>(); 

            if (attribute != null)
            {
                if (!type.IsSubclassOf(typeof(CustomRole)))
                {
                    throw new InvalidOperationException($"Type {type.FullDescription()} must extend {nameof(CustomRole)}.");
                }

                PeasmodPlugin.Logger.LogInfo($"Registered role {type.Name} from {assembly.GetName().Name}");

                try
                {
                    //Activator.CreateInstance(type);
                    Activator.CreateInstance(type, assembly);
                }
                catch (Exception e)
                {
                    PeasmodPlugin.Logger.LogWarning(e);
                }
            }
        }
    }

    public static void Load()
    {
        IL2CPPChainloader.Instance.PluginLoad += (_, assembly, _) => Register(assembly);
    }
}