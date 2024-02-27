using System.Collections.Generic;
using AmongUs.GameOptions;
using Peasmod4.API.Events;
using Peasmod4.API.Networking;
using Reactor.Networking.Rpc;

namespace Peasmod4.API.Roles;

public static class Extensions
{
    public static void RpcSetVanillaRole(this PlayerControl player, RoleTypes roleTypes)
    {
        Rpc<RpcSetCustomRole>.Instance.Send(new RpcSetCustomRole.Data(player, roleTypes));
    }
    
    public static void RpcSetCustomRole(this GameData.PlayerInfo player, CustomRole baseRole) => RpcSetCustomRole(player.Object, baseRole);

    public static void RpcSetCustomRole(this PlayerControl player, CustomRole baseRole)
    {
        Rpc<RpcSetCustomRole>.Instance.Send(new RpcSetCustomRole.Data(player, baseRole.RoleBehaviour.Role));
    }

    public static CustomRole GetCustomRole(this GameData.PlayerInfo player) => GetCustomRole(player.Object);
    
    public static CustomRole GetCustomRole(this PlayerControl player)
    {
        if (player == null)
            return null;
        
        if (player.Data.Role == null)
            return null;
        
        return CustomRoleManager.Roles.Find(role => role.RoleBehaviour.Role == player.Data?.Role.Role);
    }

    public static bool IsCustomRole(this GameData.PlayerInfo player) => IsCustomRole(player.Object);
    
    public static bool IsCustomRole(this PlayerControl player)
    {
        if (player == null)
            return false;
        
        return GetCustomRole(player) != null;
    }
    
    public static bool IsCustomRole(this GameData.PlayerInfo player, CustomRole role) => IsCustomRole(player.Object, role);
    
    public static bool IsCustomRole(this PlayerControl player, CustomRole role)
    {
        return GetCustomRole(player) == role;
    }

    public static bool IsCustomRole<T>(this PlayerControl player) where T : CustomRole
    {
        return player.IsCustomRole(CustomRoleManager.GetRole<T>());
    }

    public static bool IsCustomRole<T>(this GameData.PlayerInfo player) where T : CustomRole => IsCustomRole<T>(player.Object);

    public static bool IsVisibleTo(this PlayerControl source, PlayerControl otherPlayer)
    {
        if (otherPlayer == null || source == null || otherPlayer.Data == null || source.Data == null)
            return false;
        
        if (otherPlayer.Data.IsDead && PeasmodPlugin.ShowRolesToDead.Value)
            return true;
        
        if (source.IsCustomRole())
        {
            if (source.GetCustomRole().IsVisibleTo(source, otherPlayer))
                return true;
        }
        else
        {
            if (source.Data.Role.TeamType == RoleTeamTypes.Impostor &&
                otherPlayer.Data.Role.TeamType == RoleTeamTypes.Impostor)
                return true;
        }

        return false;
    }

    public static void SetNameFromRole(this PlayerControl player)
    {
        var role = player.Data.Role;
        player.cosmetics.nameText.text = $"{player.name}\n<size=80%>{role.NiceName}</size>";
        player.cosmetics.nameText.color = role.NameColor;
    }
    
    public static void SetNameFromRole(this PlayerVoteArea voteArea, PlayerControl player)
    {
        var role = player.Data.Role;
        voteArea.NameText.text = $"{player.name}\n<size=80%>{role.NiceName}</size>";
        voteArea.NameText.color = role.NameColor;
    }
    
    public static void SetRoleAfterIntro(this PlayerControl player, RoleTypes role)
    {
        player.roleAssigned = true;
        player.RemainingEmergencies = GameManager.Instance.LogicOptions.GetNumEmergencyMeetings();
        DestroyableSingleton<RoleManager>.Instance.SetRole(player, role);
        player.Data.Role.SpawnTaskHeader(player);
        player.MyPhysics.SetBodyType(player.BodyType);
        if (player.AmOwner)
        {
            if (player.Data.Role.IsImpostor)
            {
                StatsManager.Instance.IncrementStat(StringNames.StatsGamesImpostor);
                StatsManager.Instance.ResetStat(StringNames.StatsCrewmateStreak);
            }
            else
            {
                StatsManager.Instance.IncrementStat(StringNames.StatsGamesCrewmate);
                StatsManager.Instance.IncrementStat(StringNames.StatsCrewmateStreak);
            }
            DestroyableSingleton<HudManager>.Instance.MapButton.gameObject.SetActive(true);
            DestroyableSingleton<HudManager>.Instance.ReportButton.gameObject.SetActive(true);
            DestroyableSingleton<HudManager>.Instance.UseButton.gameObject.SetActive(true);
        }
        foreach (var playerControl in PlayerControl.AllPlayerControls.WrapToSystem())
        {
            PlayerNameColor.Set(playerControl);
        };
    }

    public static List<PlayerControl> GetMembers(this CustomRole customRole) =>
        CustomRoleManager.GetRoleMembers(customRole);
}