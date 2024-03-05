using System;
using System.Reflection;
using Peasmod4.API.Components;
using Peasmod4.API.Events;
using Peasmod4.API.Roles;
using Peasmod4.API.UI.Buttons;
using Peasmod4.API.UI.EndGame;
using Peasmod4.API.UI.Options;
using Peasmod4.Resources;
using Peasmod4.Roles.Abilities;
using Reactor.Networking.Rpc;
using UnityEngine;

namespace Peasmod4;

#if !API && DEV
[RegisterCustomRole]
#endif
public class TestRole : CustomRole
{
    public override string Name => "Peasplayer";
    public override string Description => "Incredible Person";
    public override string LongDescription => "An incredible Person that is currently rewriting Peasmod";
    public override string TaskHint => LongDescription;
    public override Color Color => Color.green;
    public override Enums.Visibility Visibility => Enums.Visibility.Everyone;
    public override Enums.Team Team => Enums.Team.Crewmate;
    public override bool HasToDoTasks => true;
    public override bool CanVent() => true;

    public CustomButton Button;
    public CustomToggleOption TestOption = new CustomToggleOption("TestOption", "Test123", false);
    public CustomNumberOption TestOption4 = new CustomNumberOption("TestOption4", "Test3214", 2.3f, NumberSuffixes.Multiplier, 0.3f, new FloatRange(0f, 3f), true);
    public CustomNumberOption TestOption5 = new CustomNumberOption("TestOption5", "Test3215", 2f, NumberSuffixes.None, 1, new FloatRange(0, 7), true);
    public CustomStringOption TestOption6 = new CustomStringOption("TestOption6", "test", "hallo", "mitte", "tschüs!");
    public CustomRoleOption CustomRoleOption1;
    public CustomEndGameManager.CustomEndReason CustomEndReason;

    public bool test = true;
    
    public void Start(object sender, EventArgs args)
    {
        PeasmodPlugin.Logger.LogInfo("test2");
        CustomEndReason = CustomEndGameManager.RegisterCustomEndReason("Peasplayer wins (obviously)", Color, false, false);
        Button = new CustomButton("Peasplayer", () =>
            {
                PeasmodPlugin.Logger.LogInfo("test");
                CustomEndReason.Trigger();
                //Rpc<TurnInvisible.RpcTurnInvisible>.Instance.Send(new TurnInvisible.RpcTurnInvisible.Data(PlayerControl.LocalPlayer, test));
                //test = !test;
                /*var player = PlayerControl.LocalPlayer;
                GameData.PlayerInfo data = player.Data;
                //GameManager.Instance.RpcEndGame(GameOverReason.HumansDisconnect, false);
                bool flag2 = (GameOptionsManager.Instance.CurrentGameOptions.GetBool(BoolOptionNames.GhostsDoTasks) || !data.IsDead) && (!AmongUsClient.Instance || !AmongUsClient.Instance.IsGameOver) && player.CanMove;
                Vector2 truePosition = player.GetTruePosition();
                int num2 = Physics2D.OverlapCircleNonAlloc(truePosition, player.MaxReportDistance, player.hitBuffer, Constants.Usables);
                IUsable usable = null;
                float num3 = float.MaxValue;
                bool flag3 = false;
                List<Vent> list = new List<Vent>();
                for (int i = 0; i < num2; i++)
                {
                    Collider2D collider2D = player.hitBuffer[i];
                    Il2CppReferenceArray<IUsable> array;
                    if (!player.cache.TryGetValue(collider2D, out array))
                    {
                        array = (player.cache[collider2D] = collider2D.GetComponents<IUsable>().ToArray());
                    }

                    PeasmodPlugin.Logger.LogInfo("test1: " + collider2D.gameObject.name);
                    if (array != null && (flag2 || player.inVent))
                    {
                        PeasmodPlugin.Logger.LogInfo("test2");
                        foreach (IUsable usable2 in array)
                        {
                            bool flag4;
                            bool flag5;
                            float num4 = usable2.CanUse(data, out flag4, out flag5);
                            PeasmodPlugin.Logger.LogInfo("test3: " + flag4 + " " + flag5 + " " + num4);
                            if (flag4 || flag5)
                            {
                                PeasmodPlugin.Logger.LogInfo("test4");
                                player.newItemsInRange.Add(usable2);
                            }
                            if (flag4 && num4 < num3)
                            {
                                PeasmodPlugin.Logger.LogInfo("test5");
                                if (usable2 is Vent)
                                {
                                    PeasmodPlugin.Logger.LogInfo("test6");
                                    list.Add(usable2.Cast<Vent>());
                                }
                                else
                                {
                                    PeasmodPlugin.Logger.LogInfo("test7");
                                    num3 = num4;
                                    usable = usable2;
                                }
                            }
                        }
                    }
                    if (flag2 && !data.IsDead && !flag3 && collider2D.tag == "DeadBody")
                    {
                        DeadBody component = collider2D.GetComponent<DeadBody>();
                        if (component.enabled && Vector2.Distance(truePosition, component.TruePosition) <= player.MaxReportDistance && !PhysicsHelpers.AnythingBetween(truePosition, component.TruePosition, Constants.ShipAndObjectsMask, false))
                        {
                            flag3 = true;
                        }
                    }
                }
                PeasmodPlugin.Logger.LogInfo("test8: " + (usable == null));
                */
            }, "Hallo", ResourceManager.PlaceholderButton, 
            player => player.IsCustomRole(this), player => player.IsCustomRole(this), new CustomButton.CustomButtonOptions(maxCooldown: 0f, true, 3f,
                () =>
                {
                    PeasmodPlugin.Logger.LogInfo("test123333");
                }, false, 3));
    }

    public void OnHudUpdate(object sender, HudEventManager.HudUpdateEventArgs args)
    {
        //PeasmodPlugin.Logger.LogInfo("test123333");
    }

    public override bool DidWin(GameOverReason gameOverReason, PlayerControl player, ref bool overrides)
    {
        return (GameManager.Instance.DidHumansWin(gameOverReason) || CustomEndReason.EndReason == gameOverReason) && player.IsCustomRole(this);
    }

    public override bool CanKill(PlayerControl victim = null)
    {
        return true;
    }

    public TestRole(Assembly assembly) : base(assembly)
    {
        GameEventManager.GameStartEventHandler += Start;
        HudEventManager.HudUpdateEventHandler += OnHudUpdate;
        
        CustomRoleOption1 = new CustomRoleOption(this, true, new CustomToggleOption("TestOptionForRole", "Test1234555555", false));
    }
}