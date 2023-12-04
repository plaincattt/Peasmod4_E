using HarmonyLib;

namespace Peasmod4.API.UI.Buttons;

[HarmonyPatch]
public class Patches
{
    [HarmonyPostfix]
    [HarmonyPatch(typeof(HudManager), nameof(HudManager.CoShowIntro))]
    public static void SetCooldownPatch(HudManager __instance)
    {
        CustomButtonManager.AllButtons.Do(button => button.Button?.SetCoolDown(10f, button.Options.MaxCooldown));
    }
}