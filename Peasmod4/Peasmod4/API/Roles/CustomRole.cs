using System;
using System.Collections.Generic;
using System.Reflection;
using BepInEx.Unity.IL2CPP;
using Peasmod4.API.Events;
using Peasmod4.Resources;
using UnityEngine;

namespace Peasmod4.API.Roles;

public abstract class CustomRole
{
    public Assembly Assembly { get; }
    
    public int Id { get; }

    public RoleBehaviour RoleBehaviour;

    /// <summary>
    /// The name of the Role. Will displayed at the intro, ejection and task list
    /// </summary>
    public abstract string Name { get; }

    /// <summary>
    /// The description of the Role. Will displayed at the intro
    /// </summary>
    public abstract string Description { get; }
        
    public abstract string LongDescription { get; }
        
    /// <summary>
    /// The description of the Role at the task list
    /// </summary>
    public virtual string TaskHint { get; }

    /// <summary>
    /// The color of the Role. Will displayed at the intro, name, task list, game end
    /// </summary>
    public abstract Color Color { get; }

    public virtual Sprite Icon { get; } = ResourceManager.PlaceholderButton;
    
    /// <summary>
    /// Who can see the identity of the player with the Role
    /// </summary>
    public abstract Enums.Visibility Visibility { get; }

    /// <summary>
    /// Who the player with the Role is in a team
    /// </summary>
    public abstract Enums.Team Team { get; }

    public virtual string TeamText
    {
        get
        {
            switch (Team)
            {
                case Enums.Team.Alone:
                case Enums.Team.Role: return "Neutral";
                case Enums.Team.Crewmate: return "Crewmate";
                case Enums.Team.Impostor: return "Impostor";
                default: throw new NotImplementedException("Unknown Visibility");
            }
        }
    }

    public virtual bool HasToDoTasks => Team == Enums.Team.Crewmate;

    public int Count;
    
    public int Chance;
    
    /// <summary>
    /// How many players can possibly get the Role
    /// </summary>
    public virtual int MaxCount { get; set; } = 15;

    public virtual float KillDistance { get; set; } =
        Mathf.Clamp(GameManager.Instance?.LogicOptions?.GetKillDistance() ?? 1.8f, 0, 2);

    /// <summary>
    /// If a member of the role should be able to kill that player / in general
    /// </summary>
    public virtual bool CanKill(PlayerControl victim = null) => Team == Enums.Team.Impostor && (!victim || !victim.Data.Role.IsImpostor);

    /// <summary>
    /// If a member of the role should be able to use vents
    /// </summary>
    public virtual bool CanVent() => Team == Enums.Team.Impostor;

    public bool IsVisibleTo(PlayerControl playerWithRole, PlayerControl perspective)
    {
        if (playerWithRole.PlayerId == perspective.PlayerId)
            return true;

        if (playerWithRole.Data.IsDead)
            return true;
            
        switch (Visibility)
        {
            case Enums.Visibility.Role: return perspective.IsCustomRole(this);
            case Enums.Visibility.Impostor: return perspective.Data.Role.IsImpostor;
            case Enums.Visibility.Everyone: return true;
            case Enums.Visibility.NoOne: return false;
            case Enums.Visibility.Custom: return IsRoleVisibleCustom(playerWithRole, perspective);
            default: throw new NotImplementedException("Unknown Visibility");
        }
    }

    public virtual bool IsRoleVisibleCustom(PlayerControl playerWithRole, PlayerControl perspective)
    {
        return false;
    }

    public virtual bool DidWin(GameOverReason gameOverReason, PlayerControl player, ref bool overrides)
    {
        switch (Team)
        {
            case Enums.Team.Crewmate:
                return GameManager.Instance.DidHumansWin(gameOverReason) && player.Data.Role.TeamType == RoleTeamTypes.Crewmate;
            case Enums.Team.Impostor:
                return GameManager.Instance.DidImpostorsWin(gameOverReason) && player.Data.Role.TeamType == RoleTeamTypes.Impostor;
            default:
                return false;
        }
    }

    private void _ShowButtons(object sender, HudEventManager.HudSetActiveEventArgs args)
    {
        
    }

    public virtual void OnRoleAssigned(PlayerControl player) { }
    
    public CustomRole(Assembly assembly)
    {
        HudEventManager.HudSetActiveEventHandler += _ShowButtons;
        
        Assembly = assembly;
        Id = CustomRoleManager.GetId();
        PeasmodPlugin.Logger.LogInfo(Id);
        RoleBehaviour = CustomRoleManager.ToRoleBehaviour(this);
        CustomRoleManager.Roles.Add(this);
    }
}