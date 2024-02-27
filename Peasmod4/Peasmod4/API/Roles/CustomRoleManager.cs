using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using AmongUs.GameOptions;
using Reactor.Localization.Utilities;
using Reactor.Utilities.Extensions;
using UnityEngine;

namespace Peasmod4.API.Roles;

public class CustomRoleManager
{
    public static List<CustomRole> Roles = new ();
    
    internal static int GetId() => Roles.Count;

    internal static RoleBehaviour ToRoleBehaviour(CustomRole customRole)
    {
        if (GameObject.Find($"{customRole.Name}-Role"))
        {
            return GameObject.Find($"{customRole.Name}-Role").GetComponent<ModRole>();
        }

        var roleObject = new GameObject($"{customRole.Name}-Role");
        roleObject.DontDestroy();

        var role = roleObject.AddComponent<ModRole>();
        role.StringName = CustomStringName.CreateAndRegister(customRole.Name);
        role.BlurbName = CustomStringName.CreateAndRegister(customRole.Description);
        role.BlurbNameLong = CustomStringName.CreateAndRegister(customRole.LongDescription);
        role.BlurbNameMed = CustomStringName.CreateAndRegister(customRole.TaskHint ?? "");
        role.Role = (RoleTypes) (8 + customRole.Id);
        role.NameColor = customRole.Color;
            
        var abilityButtonSettings = ScriptableObject.CreateInstance<AbilityButtonSettings>();
        abilityButtonSettings.Image = customRole.Icon;
        abilityButtonSettings.Text = CustomStringName.CreateAndRegister("Please work");
        abilityButtonSettings.FontMaterial = Material.GetDefaultMaterial();
        role.Ability = abilityButtonSettings;

        role.TeamType = customRole.Team switch
        {
            Enums.Team.Alone => (RoleTeamTypes) 3,
            Enums.Team.Role => (RoleTeamTypes) 2,
            Enums.Team.Crewmate => RoleTeamTypes.Crewmate,
            Enums.Team.Impostor => RoleTeamTypes.Impostor,
            _ => RoleTeamTypes.Crewmate
        };
        role.MaxCount = customRole.MaxCount;
        role.TasksCountTowardProgress = customRole.HasToDoTasks;
        role.CanVent = customRole.CanVent();
        role.CanUseKillButton = customRole.CanKill();
        
        PeasmodPlugin.Logger.LogInfo($"Created RoleBehaviour for Role {customRole.Name}");
            
        return role;
    }

    internal static bool InjectedRoleBehaviours;
    internal static void InjectRoleBehaviours()
    {
        if (InjectedRoleBehaviours)
            return;
        
        var list = RoleManager.Instance.AllRoles.ToList();
        foreach (var role in Roles)
        {
            list.Add(role.RoleBehaviour);
        }
        RoleManager.Instance.AllRoles = list.ToArray();

        InjectedRoleBehaviours = true;
    }

    public static T GetRole<T>() where T : CustomRole
    {
        return (T) Roles.First(role => role.GetType() == typeof(T));
    }

    public static CustomRole GetRole(RoleTypes roleType)
    {
        return Roles.Find(role => role.RoleBehaviour.Role == roleType);
    }

    public static List<PlayerControl> GetRoleMembers(CustomRole role)
    {
        return PlayerControl.AllPlayerControls.WrapToSystem().FindAll(player => player.IsCustomRole(role));
    }
}