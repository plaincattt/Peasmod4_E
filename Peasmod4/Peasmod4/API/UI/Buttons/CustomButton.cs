using System;
using Peasmod4.API.Events;
using Reactor.Utilities.Extensions;
using UnityEngine;
using UnityEngine.Events;

namespace Peasmod4.API.UI.Buttons;

public class CustomButton
{
    public ActionButton Button { get; internal set; }
    public string ObjectName;
    
    public Action OnClick;
    public string Text;
    public Sprite Image;
    public Predicate<PlayerControl> CouldUse;
    public Predicate<PlayerControl> CanUse;
    public CustomButtonOptions Options;
    
    public float Cooldown;
    public int UsesLeft;
    public PlayerControl Target;
    public bool Enabled = true;

    public bool IsEffectActive { get; private set; }
    public bool IsHudActive { get; private set; }

    private bool _hasBeenCreated;

    public CustomButton(string objectName, Action onClick, string text, Sprite image, Predicate<PlayerControl> couldUse,
        Predicate<PlayerControl> canUse) => new CustomButton(objectName, onClick, text, image, couldUse, canUse, new CustomButtonOptions());
    
    public CustomButton(string objectName, Action onClick, string text, Sprite image, Predicate<PlayerControl> couldUse, Predicate<PlayerControl> canUse, CustomButtonOptions options)
    {
        PeasmodPlugin.Logger.LogInfo("CustomButton#Constructor");
        ObjectName = objectName;
        OnClick = onClick;
        Text = text;
        Image = image;
        CouldUse = couldUse;
        CanUse = canUse;
        Options = options;
        
        CustomButtonManager.AllButtons.Add(this);
        
        GameEventManager.GameStartEventHandler += Start;
        HudEventManager.HudUpdateEventHandler += Update;
        HudEventManager.HudSetActiveEventHandler += OnHudSetActive;
        
        if (ShipStatus.Instance != null)
            Start(null, EventArgs.Empty);
    }
    
    public void Start(object sender, EventArgs args)
    {
        if (_hasBeenCreated)
        {
            Dispose();
            return;
        }

        _hasBeenCreated = true;
        
        PeasmodPlugin.Logger.LogInfo("CustomButton#Start");
        Button = GameObject.Instantiate(HudManager.Instance.AbilityButton, HudManager.Instance.AbilityButton.transform.parent);
        Button.gameObject.SetActive(CouldBeUsed());
        Button.gameObject.name = ObjectName + "-CustomButton";
        Button.buttonLabelText.GetComponent<TextTranslatorTMP>().Destroy();
        Button.buttonLabelText.text = Text;
        Button.graphic.sprite = Image;
        
        Cooldown = Options.MaxCooldown;
        if (Cooldown != 0f)
            Button?.SetCoolDown(10f, Cooldown);
        
        UsesLeft = Options.MaxUses;
        if (Options.InfinitelyUsable)
            Button.SetInfiniteUses();
        else
            Button.SetUsesRemaining(Options.MaxUses);
        
        var buttonComponent = Button.GetComponent<PassiveButton>();
        buttonComponent.OnClick.RemoveAllListeners();
        buttonComponent.OnClick.AddListener((UnityAction) listener);

        void listener()
        {
            if (CanBeUsed() && Button.gameObject.active &&
                PlayerControl.LocalPlayer.moveable)
            {
                OnClick();
                Cooldown = Options.MaxCooldown;

                if (!Options.InfinitelyUsable)
                {
                    UsesLeft--;
                    Button.SetUsesRemaining(UsesLeft);
                }
                
                if (Options.HasEffect)
                {
                    Cooldown = Options.EffectDuration;
                    IsEffectActive = true;
                }
            }
        }
    }

    public void Update(object sender, HudEventManager.HudUpdateEventArgs args)
    {
        if (Button == null || Button.gameObject == null)
            return;

        if (CouldBeUsed() && PlayerControl.LocalPlayer.IsKillTimerEnabled)
        {
            if (Cooldown > 0f)
            {
                Cooldown -= Time.deltaTime;
                Button.SetCoolDown(Cooldown, IsEffectActive ? Options.EffectDuration : Options.MaxCooldown);
            }
            else
                Button.graphic.material.SetFloat("_Percent", 0f);
            
            Button.cooldownTimerText.color = IsEffectActive ? Color.green : Color.white;
            if (Cooldown <= 0f && IsEffectActive)
            {
                IsEffectActive = false;
                Options.OnEffectEnded();
                Cooldown = Options.MaxCooldown;
            }
        }
        
        if (CanBeUsed())
            Button.SetEnabled();
        else
            Button.SetDisabled();
    }

    public void OnHudSetActive(object sender, HudEventManager.HudSetActiveEventArgs args)
    {
        IsHudActive = args.Active;
        Button?.gameObject.SetActive(CouldBeUsed() && IsHudActive);
    }

    public bool CouldBeUsed()
    {
        if (PlayerControl.LocalPlayer == null) 
            return false;
            
        if (PlayerControl.LocalPlayer.Data == null) 
            return false;
            
        if (MeetingHud.Instance != null) 
            return false;

        if (!Enabled)
            return false;

        return CouldUse.Invoke(PlayerControl.LocalPlayer);
    }

    public bool CanBeUsed()
    {
        if (!CouldBeUsed())
            return false;
        
        if (PlayerControl.LocalPlayer == null) 
            return false;
            
        if (PlayerControl.LocalPlayer.Data == null) 
            return false;
            
        if (!PlayerControl.LocalPlayer.CanMove || PlayerControl.LocalPlayer.inVent) 
            return false;

        if (!Options.InfinitelyUsable && UsesLeft <= 0)
            return false;
        
        if (Options._TargetType != CustomButtonOptions.TargetType.None && Target == null)
            return false;
        
        if (Cooldown > 0f)
            return false;
        
        return CanUse.Invoke(PlayerControl.LocalPlayer);
    }

    public void Dispose()
    {
        PeasmodPlugin.Logger.LogInfo("CustomButton#Dispose");
        GameEventManager.GameStartEventHandler -= Start;
        HudEventManager.HudUpdateEventHandler -= Update;
        Button.gameObject.Destroy();
        Button = null;
        Options = null;
        CustomButtonManager.AllButtons.Remove(this);
    }

    public class CustomButtonOptions
    {
        public float MaxCooldown;
        public bool HasEffect;
        public float EffectDuration;
        public Action OnEffectEnded;
        public bool InfinitelyUsable;
        public int MaxUses;
        public TargetType _TargetType;

        public CustomButtonOptions(float maxCooldown = 0f, bool hasEffect = false, float effectDuration = 0f,
            Action onEffectEnded = null, bool infinitelyUsable = true, int maxUses = 0, TargetType targetType = TargetType.None)
        {
            MaxCooldown = maxCooldown;
            HasEffect = hasEffect;
            EffectDuration = effectDuration;
            OnEffectEnded = onEffectEnded;
            InfinitelyUsable = infinitelyUsable;
            MaxUses = maxUses;
            _TargetType = targetType;
        }

        public enum TargetType
        {
            None,
            Player
        }
    }
}