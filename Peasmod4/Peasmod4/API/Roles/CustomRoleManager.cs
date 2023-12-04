using System.Collections.Generic;
using System.Linq;
using AmongUs.GameOptions;
using Reactor.Localization.Utilities;
using Reactor.Utilities.Extensions;
using UnityEngine;

namespace Peasmod4.API.Roles;

public class RoleManager
{
    public static List<BaseRole> Roles = new List<BaseRole>();

    internal static int GetId() => Roles.Count;

    internal static RoleBehaviour ToRoleBehaviour(BaseRole baseRole)
    {
        if (GameObject.Find($"{baseRole.Name}-Role"))
        {
            return GameObject.Find($"{baseRole.Name}-Role").GetComponent<RoleBehaviour>();
        }

        var roleObject = new GameObject($"{baseRole.Name}-Role");
        roleObject.DontDestroy();

        var role = roleObject.AddComponent<CrewmateRole>();
        role.StringName = CustomStringName.CreateAndRegister(baseRole.Name);
        role.BlurbName = CustomStringName.CreateAndRegister(baseRole.Description);
        role.BlurbNameLong = CustomStringName.CreateAndRegister(baseRole.LongDescription);
        role.BlurbNameMed = CustomStringName.CreateAndRegister(baseRole.Name);
        role.Role = (RoleTypes) (8 + baseRole.Id);
        role.NameColor = baseRole.Color;
            
        //var abilityButtonSettings = ScriptableObject.CreateInstance<AbilityButtonSettings>();
        //abilityButtonSettings.Image = Utility.CreateSprite("Peasmod4.Placeholder.png");
        //abilityButtonSettings.Text = CustomStringName.CreateAndRegister("baseRole.Name");
        //role.Ability = abilityButtonSettings;

        role.TeamType = baseRole.Team switch
        {
            Enums.Team.Alone => (RoleTeamTypes) 3,
            Enums.Team.Role => (RoleTeamTypes) 2,
            Enums.Team.Crewmate => RoleTeamTypes.Crewmate,
            Enums.Team.Impostor => RoleTeamTypes.Impostor,
            _ => RoleTeamTypes.Crewmate
        };
        role.MaxCount = baseRole.MaxCount;
        role.TasksCountTowardProgress = baseRole.HasToDoTasks;
        role.CanVent = baseRole.CanVent;
        role.CanUseKillButton = baseRole.CanKill();
        
        PeasmodPlugin.Logger.LogInfo($"Created RoleBehaviour for Role {baseRole.Name}");
            
        return role;
    }

    internal static bool InjectedRoleBehaviours;
    internal static void InjectRoleBehaviours()
    {
        if (InjectedRoleBehaviours)
            return;
        
        var list = global::RoleManager.Instance.AllRoles.ToList();
        foreach (var baseRole in Roles)
        {
            list.Add(baseRole.RoleBehaviour);
        }
        global::RoleManager.Instance.AllRoles = list.ToArray();

        InjectedRoleBehaviours = true;
    }
}