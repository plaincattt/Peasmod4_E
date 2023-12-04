using System;
using Peasmod4.API.Components;
using Peasmod4.API.Events;
using Reactor.Utilities.Extensions;
using UnityEngine;
using UnityEngine.Events;

namespace Peasmod4.API.UI.Buttons;

public class CustomButton
{
    public KillButton Button { get; internal set; }
    
    public Action OnClick;
    public string Text;
    public Sprite Image;
    public Predicate<PlayerControl> CouldUse;
    public Predicate<PlayerControl> CanUse;
    public CustomButtonOptions Options;
    
    public float Cooldown;
    public PlayerControl Target;

    public CustomButton(Action onClick, string text, Sprite image, Predicate<PlayerControl> couldUse,
        Predicate<PlayerControl> canUse) => new CustomButton(onClick, text, image, couldUse, canUse, new CustomButtonOptions());
    
    public CustomButton(Action onClick, string text, Sprite image, Predicate<PlayerControl> couldUse, Predicate<PlayerControl> canUse, CustomButtonOptions options)
    {
        PeasmodPlugin.Logger.LogInfo("CustomButton#Constructor");
        OnClick = onClick;
        Text = text;
        Image = image;
        CouldUse = couldUse;
        CanUse = canUse;
        Options = options;
        
        CustomButtonManager.AllButtons.Add(this);
        
        GameEventManager.GameStartEventHandler += Start;
        HudEventManager.HudUpdateEventHandler += Update;
        GameEventManager.GameEndEventHandler += Dispose;
        
        if (ShipStatus.Instance != null)
            Start(null, EventArgs.Empty);
    }
    
    public void Start(object sender, EventArgs args)
    {
        PeasmodPlugin.Logger.LogInfo("CustomButton#Start");
        Button = GameObject.Instantiate(HudManager.Instance.KillButton, HudManager.Instance.KillButton.transform.parent);
        Button.gameObject.SetActive(true);
        Button.gameObject.name = Text + "-CustomButton";
        Button.buttonLabelText.GetComponent<TextTranslatorTMP>().Destroy();
        Button.buttonLabelText.text = Text;
        Button.graphic.sprite = Image;
        Cooldown = Options.MaxCooldown;
        
        var buttonComponent = Button.GetComponent<PassiveButton>();
        buttonComponent.OnClick.RemoveAllListeners();
        buttonComponent.OnClick.AddListener((UnityAction) listener);

        void listener()
        {
            if (CanBeUsed() && CouldBeUsed() && Button.gameObject.active &&
                PlayerControl.LocalPlayer.moveable)
            {
                OnClick();
                Cooldown = Options.MaxCooldown;
            }
        }
    }

    public void Update(object sender, HudEventManager.HudUpdateEventArgs args)
    {
        if (Button == null || Button.gameObject == null)
            return;
        
        Button.gameObject.SetActive(CouldBeUsed());

        if (CouldBeUsed())
        {
            if (Options.MaxCooldown > 0f)
            {
                if (Cooldown > 0f)
                    Cooldown -= Time.deltaTime;
                Button.SetCoolDown(Cooldown, Options.MaxCooldown);
            }
        }
        
        if (CanBeUsed())
            Button.SetEnabled();
    }

    public bool CouldBeUsed()
    {
        if (PlayerControl.LocalPlayer == null) 
            return false;
            
        if (PlayerControl.LocalPlayer.Data == null) 
            return false;
            
        //PeasmodPlugin.Logger.LogInfo("Button 1");
        if (MeetingHud.Instance != null) 
            return false;

        //PeasmodPlugin.Logger.LogInfo("Button 2");
        if (MapBehaviour.Instance != null && MapBehaviour.Instance.IsOpen)
            return false;

        //PeasmodPlugin.Logger.LogInfo("Button 3");
        return CouldUse.Invoke(PlayerControl.LocalPlayer);
    }

    public bool CanBeUsed()
    {
        if (PlayerControl.LocalPlayer == null) 
            return false;
            
        if (PlayerControl.LocalPlayer.Data == null) 
            return false;
            
        if (MeetingHud.Instance != null) 
            return false;

        if (Options._TargetType != CustomButtonOptions.TargetType.None && Target == null)
            return false;
        
        if (Cooldown > 0f)
            return false;
        
        return CanUse.Invoke(PlayerControl.LocalPlayer);
    }

    public void Dispose(object sender, EventArgs args)
    {
        PeasmodPlugin.Logger.LogInfo("CustomButton#Dispose");
        GameEventManager.GameStartEventHandler -= Start;
        HudEventManager.HudUpdateEventHandler -= Update;
        GameEventManager.GameEndEventHandler -= Dispose;
        Button.gameObject.Destroy();
        Button = null;
        Options = null;
        CustomButtonManager.AllButtons.Remove(this);
    }

    public class CustomButtonOptions
    {
        public float MaxCooldown = 0f;
        public bool HasEffect;
        public float EffectDuration;
        public Action OnEffectEnded;
        public TargetType _TargetType;

        public CustomButtonOptions(float maxCooldown = 0f, bool hasEffect = false, float effectDuration = 0f,
            Action onEffectEnded = null, TargetType targetType = TargetType.None)
        {
            MaxCooldown = maxCooldown;
            HasEffect = hasEffect;
            EffectDuration = effectDuration;
            OnEffectEnded = onEffectEnded;
            _TargetType = targetType;
        }

        public enum TargetType
        {
            None,
            Player
        }
    }
}