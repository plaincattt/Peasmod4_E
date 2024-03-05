using System;
using HarmonyLib;
using Peasmod4.API.Components;
using UnityEngine;
using EventType = Peasmod4.API.Events.EventType;

namespace Peasmod4.API.UI.Buttons;

[HarmonyPatch]
public class Patches
{
    public static GameObject LeftBottomParent;
    
    [RegisterEventListener(EventType.GameStart)]
    public static void AddLeftButtonParent(object sender, EventArgs args)
    {
        var buttonsParent = HudManager.Instance.gameObject.transform.FindChild("Buttons");
        LeftBottomParent = GameObject.Instantiate(buttonsParent.FindChild("BottomRight").gameObject, buttonsParent);
        LeftBottomParent.name = "BottomLeft";
        LeftBottomParent.transform.DestroyChildren();
        
        var aspectPos = LeftBottomParent.GetComponent<AspectPosition>();
        aspectPos.Alignment = AspectPosition.EdgeAlignments.LeftBottom;
        aspectPos.AdjustPosition();

        var gridArrange = LeftBottomParent.GetComponent<GridArrange>();
        gridArrange.Alignment = GridArrange.StartAlign.Right;
        gridArrange.Start();
        gridArrange.CheckCurrentChildren();
        gridArrange.ArrangeChilds();

        for (int i = 0; i < CustomButtonManager.AllButtons.Count; i++)
        {
            CustomButtonManager.AllButtons[i].Start(null, EventArgs.Empty);
        }
    }
}