using System;
using System.ComponentModel;
using System.Reflection;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;
using Peasmod4.API.Events;

namespace Peasmod4.API.Components;

[AttributeUsage(AttributeTargets.Method)]
public class RegisterEventListenerAttribute : Attribute
{
    private EventType EventType;

    public RegisterEventListenerAttribute(EventType eventType) => EventType = eventType;
    
    public static void Register(Assembly assembly)
    {
        foreach (var type in assembly.GetTypes())
        {
            foreach (var method in type.GetMethods())
            {
                var attribute = method.GetCustomAttribute<RegisterEventListenerAttribute>();
                if (attribute != null)
                {
                    if (!method.IsStatic)
                    {
                        throw new InvalidOperationException($"Method {method.FullDescription()} must be static in order to use the attribute.");
                    }
                    
                    if (method.GetParameters().Length != 2)
                    {
                        throw new InvalidOperationException($"Method {method.FullDescription()} must have parameters sender and args.");
                    }
                    
                    var senderParameter = method.GetParameters()[0];
                    var args = method.GetParameters()[1];
                    switch (attribute.EventType)
                    {
                        case EventType.GameStart:
                            if (senderParameter.ParameterType != typeof(object) || args.ParameterType != typeof(EventArgs))
                            {
                                throw new InvalidOperationException($"Method {method.FullDescription()} must have parameters sender of type object and args of type EventArgs.");
                            }
                            GameEventManager.GameStartEventHandler += (sender, eventArgs) => method.Invoke(null, new[] { sender, eventArgs });
                            break;
                        case EventType.GameJoined:
                            if (senderParameter.ParameterType != typeof(object) || args.ParameterType != typeof(GameEventManager.GameJoinedEventArgs))
                            {
                                throw new InvalidOperationException($"Method {method.FullDescription()} must have parameters sender of type object and args of type GameJoinedEventArgs.");
                            }
                            GameEventManager.GameJoinedEventHandler += (sender, eventArgs) => method.Invoke(null, new[] { sender, eventArgs });
                            break;
                        case EventType.GameEnd:
                            if (senderParameter.ParameterType != typeof(object) || args.ParameterType != typeof(GameEventManager.GameEndEventArgs))
                            {
                                throw new InvalidOperationException($"Method {method.FullDescription()} must have parameters sender of type object and args of type GameEndEventArgs.");
                            }
                            GameEventManager.GameEndEventHandler += (sender, eventArgs) => method.Invoke(null, new[] { sender, eventArgs });
                            break;
                        case EventType.HudUpdate:
                            if (senderParameter.ParameterType != typeof(object) || args.ParameterType != typeof(HudEventManager.HudUpdateEventArgs))
                            {
                                throw new InvalidOperationException($"Method {method.FullDescription()} must have parameters sender of type object and args of type HudUpdateEventArgs.");
                            }
                            HudEventManager.HudUpdateEventHandler += (sender, eventArgs) => method.Invoke(null, new[] { sender, eventArgs });
                            break;
                        case EventType.HudSetActive:
                            if (senderParameter.ParameterType != typeof(object) || args.ParameterType != typeof(HudEventManager.HudSetActiveEventArgs))
                            {
                                throw new InvalidOperationException($"Method {method.FullDescription()} must have parameters sender of type object and args of type HudSetActiveEventArgs.");
                            }
                            HudEventManager.HudSetActiveEventHandler += (sender, eventArgs) => method.Invoke(null, new[] { sender, eventArgs });
                            break;
                        case EventType.MeetingEnd:
                            if (senderParameter.ParameterType != typeof(object) || args.ParameterType != typeof(MeetingEventManager.MeetingEndEventArgs))
                            {
                                throw new InvalidOperationException($"Method {method.FullDescription()} must have parameters sender of type object and args of type MeetingEndEventArgs.");
                            }
                            MeetingEventManager.MeetingEndEventHandler += (sender, eventArgs) => method.Invoke(null, new[] { sender, eventArgs });
                            break;
                        case EventType.PlayerDied:
                            if (senderParameter.ParameterType != typeof(object) || args.ParameterType != typeof(PlayerEventManager.PlayerDiedEventArgs))
                            {
                                throw new InvalidOperationException($"Method {method.FullDescription()} must have parameters sender of type object and args of type PlayerDiedEventArgs.");
                            }
                            PlayerEventManager.PlayerDiedEventHandler += (sender, eventArgs) => method.Invoke(null, new[] { sender, eventArgs });
                            break;
                        case EventType.PlayerExiled:
                            if (senderParameter.ParameterType != typeof(object) || args.ParameterType != typeof(PlayerEventManager.PlayerExiledEventArgs))
                            {
                                throw new InvalidOperationException($"Method {method.FullDescription()} must have parameters sender of type object and args of type PlayerExiledEventArgs.");
                            }
                            PlayerEventManager.PlayerExiledEventHandler += (sender, eventArgs) => method.Invoke(null, new[] { sender, eventArgs });
                            break;
                        case EventType.PlayerMurdered:
                            if (senderParameter.ParameterType != typeof(object) || args.ParameterType != typeof(PlayerEventManager.PlayerMurderedEventArgs))
                            {
                                throw new InvalidOperationException($"Method {method.FullDescription()} must have parameters sender of type object and args of type PlayerMurderedEventArgs.");
                            }
                            PlayerEventManager.PlayerMurderedEventHandler += (sender, eventArgs) => method.Invoke(null, new[] { sender, eventArgs });
                            break;
                        default:
                            throw new InvalidEnumArgumentException(
                                $"Method {method.FullDescription()} must have EventType parameter.");
                    }
                }
            }
        }
    }

    public static void Load()
    {
        IL2CPPChainloader.Instance.PluginLoad += (_, assembly, _) => Register(assembly);
    }
}