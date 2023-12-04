using System;
using System.Collections.Generic;
using BepInEx.Unity.IL2CPP;
using UnityEngine;

namespace Peasmod4.API.Roles;

public abstract class CustomRole
{
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
    public abstract string TaskText { get; }

    /// <summary>
    /// The color of the Role. Will displayed at the intro, name, task list, game end
    /// </summary>
    public abstract Color Color { get; }
    
    public virtual Sprite Icon { get; } = Utility.CreateSprite("Peasmod4.Placeholder.png");
    
    /// <summary>
    /// Who can see the identity of the player with the Role
    /// </summary>
    public abstract Enums.Visibility Visibility { get; }

    /// <summary>
    /// Who the player with the Role is in a team
    /// </summary>
    public abstract Enums.Team Team { get; }

    public virtual Enums.SourceType SourceType
    {
        get
        {
            switch (Team)
            {
                case Enums.Team.Impostor:
                    return Enums.SourceType.Impostor;
                default:
                    return Enums.SourceType.Crewmate;
            } 
        }
    }

    /// <summary>
    /// Whether the player should get tasks
    /// </summary>
    public virtual bool AssignTasks { get; set; } = true;
        
    public abstract bool HasToDoTasks { get; }

    /// <summary>
    /// How many player should get the Role
    /// </summary>
    public virtual int Count { get; set; } = 0;
        
    public virtual int MaxCount { get; set; } = 15;
        
    public virtual int Chance { get; set; } = 100;

    public virtual float KillDistance { get; set; } =
        Mathf.Clamp(GameManager.Instance?.LogicOptions?.GetKillDistance() ?? 1.8f, 0, 2);

    /// <summary>
    /// If a member of the role should be able to kill that player / in general
    /// </summary>
    public virtual bool CanKill(PlayerControl victim = null)
    {
        return false;
    }

    /// <summary>
    /// If a member of the role should be able to use vents
    /// </summary>
    public virtual bool CanVent { get; } = false;

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
            case Enums.Visibility.Custom: return IsRoleVisible(playerWithRole, perspective);
            default: throw new NotImplementedException("Unknown Visibility");
        }
    }

    public virtual bool IsRoleVisible(PlayerControl playerWithRole, PlayerControl perspective)
    {
        return false;
    }
    
    public CustomRole(BasePlugin plugin)
    {
        Id = CustomRoleManager.GetId();
        RoleBehaviour = CustomRoleManager.ToRoleBehaviour(this);
        CustomRoleManager.Roles.Add(this);
    }
}