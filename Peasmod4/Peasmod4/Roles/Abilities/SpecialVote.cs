using System;
using Il2CppSystem.Collections.Generic;
using System.Linq;
using HarmonyLib;
using Peasmod4.API.Components;
using Peasmod4.API.Events;
using Reactor.Networking.Attributes;

namespace Peasmod4.Roles.Abilities;

[HarmonyPatch]
public class SpecialVote
{
    public static System.Collections.Generic.Dictionary<byte, int> SpecialVotes = new ();
    
    [RegisterEventListener(EventType.GameStart)]
    public static void ResetSpecialVotes(object sender, EventArgs args)
    {
        SpecialVotes.Clear();
    }

    [MethodRpc((uint) CustomRpcCalls.SpecialVote)]
    public static void SetAdditionalVotePower(PlayerControl player, int additionalVotes)
    {
        SpecialVotes.Remove(player.PlayerId);
        SpecialVotes.Add(player.PlayerId, additionalVotes);
    }
    
    public static Dictionary<byte, int> CalculateVotes(MeetingHud __instance)
    {
        Dictionary<byte, int> dictionary = new Dictionary<byte, int>();
        for (int i = 0; i < __instance.playerStates.Length; i++)
        {
            PlayerVoteArea playerVoteArea = __instance.playerStates[i];
            if (playerVoteArea.VotedFor != 252 && playerVoteArea.VotedFor != 255 && playerVoteArea.VotedFor != 254)
            {
                var isSpecialVote = SpecialVotes.TryGetValue(playerVoteArea.TargetPlayerId, out int additionalVote);
                int num;
                var isSet = dictionary.TryGetValue(playerVoteArea.VotedFor, out num);
                
                dictionary[playerVoteArea.VotedFor] = (isSet ? num : 0) + (isSpecialVote ? 1 + additionalVote : 1);
            }
        }

        return dictionary;
    }
    
    [HarmonyPrefix]
    [HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.CheckForEndVoting))]
    public static bool CalculateVotesPatch(MeetingHud __instance)
    {
        if (__instance.playerStates.All(ps => ps.AmDead || ps.DidVote))
        {
            Dictionary<byte, int> self = CalculateVotes(__instance);
            bool tie;
            KeyValuePair<byte, int> max = self.MaxPair(out tie);
            GameData.PlayerInfo exiled = GameData.Instance.AllPlayers.ToArray().FirstOrDefault(v => !tie && v.PlayerId == max.Key);
            MeetingHud.VoterState[] array = new MeetingHud.VoterState[__instance.playerStates.Length];
            for (int i = 0; i < __instance.playerStates.Length; i++)
            {
                PlayerVoteArea playerVoteArea = __instance.playerStates[i];
                array[i] = new MeetingHud.VoterState
                {
                    VoterId = playerVoteArea.TargetPlayerId,
                    VotedForId = playerVoteArea.VotedFor
                };
            }
            __instance.RpcVotingComplete(array, exiled, tie);
        }
        
        return false;
    }
}