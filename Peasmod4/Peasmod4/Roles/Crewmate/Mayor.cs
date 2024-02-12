using System;
using System.Reflection;
using HarmonyLib;
using Peasmod4.API;
using Peasmod4.API.Components;
using Peasmod4.API.Events;
using Peasmod4.API.Roles;
using Peasmod4.API.UI.Options;
using Peasmod4.Resources;
using Peasmod4.Roles.Abilities;
using UnityEngine;
using UnityEngine.UI;
using IEnumerator = Il2CppSystem.Collections.IEnumerator;
using Object = UnityEngine.Object;

namespace Peasmod4.Roles.Crewmate;

[HarmonyPatch]
#if !API
[RegisterCustomRole]
#endif
public class Mayor : CustomRole
{
    public Mayor(Assembly assembly) : base(assembly)
    {
        DoubleVotesAmount = new CustomNumberOption("MayorDoubleVoteAmount", "Double-votes amount", 0f,
            NumberSuffixes.None, 1f, new FloatRange(0, 100), false, true);
        RoleOption = new CustomRoleOption(this, true, DoubleVotesAmount);
    }

    public override string Name => "Mayor";
    public override string Description => "Your vote counts twice";
    public override string LongDescription => "";
    public override string TaskHint => "Your vote counts twice. Use it wisely!";
    public override Color Color => new Color(17f / 255f, 49f / 255f, 255f / 255f);
    public override Enums.Visibility Visibility => Enums.Visibility.NoOne;
    public override Enums.Team Team => Enums.Team.Crewmate;
    public override bool HasToDoTasks => true;

    public CustomRoleOption RoleOption;
    public CustomNumberOption DoubleVotesAmount;

    public int DoubleVotesLeft;

    public override void OnRoleAssigned(PlayerControl player)
    {
        if (!player.IsLocal())
            return;
        
        DoubleVotesLeft = DoubleVotesAmount.Value == 0 ? -1 : (int) DoubleVotesAmount.Value;
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(PlayerVoteArea), nameof(PlayerVoteArea.Start))]
    public static void AddMayorButton(PlayerVoteArea __instance)
    {
        if (!PlayerControl.LocalPlayer.IsCustomRole<Mayor>())
            return;

        if (CustomRoleManager.GetRole<Mayor>().DoubleVotesLeft == 0)
            return;
        
        var voteTwiceButton = Object.Instantiate(__instance.ConfirmButton, __instance.ConfirmButton.transform.parent, true);
        voteTwiceButton.name = "VoteTwiceButton";
        voteTwiceButton.transform.FindChild("ControllerHighlight").GetComponent<SpriteRenderer>().color = CustomRoleManager.GetRole<Mayor>().Color;
        voteTwiceButton.GetComponent<SpriteRenderer>().sprite = ResourceManager.VoteTwiceButton;
        
        var button = voteTwiceButton.GetComponent<PassiveButton>();
        button.OnClick = new Button.ButtonClickedEvent();
        button.AddOnClickListeners(new System.Action(() =>
        {
            __instance.VoteForMe();
            __instance.Cancel();
            SpecialVote.SetAdditionalVotePower(PlayerControl.LocalPlayer, 1);
            CustomRoleManager.GetRole<Mayor>().DoubleVotesLeft--;
        }));
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(PlayerVoteArea), nameof(PlayerVoteArea.Select))]
    public static bool ShowMayorButton(PlayerVoteArea __instance)
    {
        if (!PlayerControl.LocalPlayer.IsCustomRole<Mayor>())
            return true;

        if (PlayerControl.LocalPlayer.Data.IsDead)
        {
            return false;
        }
        if (__instance.AmDead)
        {
            return false;
        }
        if (!__instance.Parent)
        {
            return false;
        }
        
        if (!__instance.voteComplete && __instance.Parent.Select(__instance.TargetPlayerId))
        {
            var voteTwiceButton = __instance.Buttons.transform.FindChild("VoteTwiceButton");

            __instance.Buttons.SetActive(true);
            float startPos = __instance.AnimateButtonsFromLeft ? 0.2f : 1.95f;
            var effects = new System.Collections.Generic.List<IEnumerator>()
            {
                Effects.Lerp(0.25f, new System.Action<float>(t =>
                {
                    __instance.CancelButton.transform.localPosition = Vector2.Lerp(Vector2.right * startPos, Vector2.right * 1.3f, Effects.ExpOut(t));
                })),
                Effects.Lerp(0.35f, new System.Action<float>(t =>
                {
                    __instance.ConfirmButton.transform.localPosition = Vector2.Lerp(Vector2.right * startPos, Vector2.right * 0.65f, Effects.ExpOut(t));
                }))
            };
            if (voteTwiceButton != null)
            {
                effects.Add(Effects.Lerp(0.45f, new System.Action<float>(t =>
                {
                    voteTwiceButton.transform.localPosition = Vector2.Lerp(Vector2.right * startPos, Vector2.right * 0f, Effects.ExpOut(t));
                })));
            }
            __instance.StartCoroutine(Effects.All(effects.ToArray()));
            
            var selectableElements = new System.Collections.Generic.List<UiElement>
            {
                __instance.CancelButton,
                __instance.ConfirmButton
            };
            if (voteTwiceButton != null)
            {
                selectableElements.Add(voteTwiceButton.GetComponent<UiElement>());
            }
            ControllerManager.Instance.OpenOverlayMenu(__instance.name, __instance.CancelButton, __instance.ConfirmButton, selectableElements.WrapToIl2Cpp(), false);
            return false;
        }
        return false;
    }
}