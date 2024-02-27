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
    public string Text { get; private set; }
    public Sprite Image { get; private set; }
    public Predicate<PlayerControl> CouldUse;
    public Predicate<PlayerControl> CanUse;
    public CustomButtonOptions Options;
    
    public float Cooldown { get; private set; }
    public int UsesLeft { get; private set; }
    public PlayerControl PlayerTarget;
    public GameObject ObjectTarget;
    public bool Enabled = true;
    public bool Visible = true;

    public bool IsEffectActive { get; private set; }
    public bool IsHudActive { get; private set; } = true;

    private bool _hasBeenCreated;

    public CustomButton(string objectName, Action onClick, string text, Sprite image, Predicate<PlayerControl> couldUse, Predicate<PlayerControl> canUse, CustomButtonOptions options = null)
    {
        PeasmodPlugin.Logger.LogInfo("CustomButton#Constructor");
        ObjectName = objectName;
        OnClick = onClick;
        Text = text;
        Image = image;
        CouldUse = couldUse;
        CanUse = canUse;
        Options = options ?? new CustomButtonOptions();
        
        CustomButtonManager.AllButtons.Add(this);
        
        HudEventManager.HudUpdateEventHandler += Update;
        HudEventManager.HudSetActiveEventHandler += OnHudSetActive;
        
        if (ShipStatus.Instance != null && Patches.LeftBottomParent != null)
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
        Button = GameObject.Instantiate(HudManager.Instance.AbilityButton, Options.OnLeftSide ? Patches.LeftBottomParent.transform : HudManager.Instance.AbilityButton.transform.parent);
        Button.gameObject.SetActive(CouldBeUsed());
        Button.gameObject.name = ObjectName + "-CustomButton";
        Button.buttonLabelText.GetComponent<TextTranslatorTMP>().Destroy();
        Button.buttonLabelText.text = Text;
        Button.buttonLabelText.fontSharedMaterial = HudManager.Instance.ReportButton.buttonLabelText.fontSharedMaterial;
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

                PlayerTarget = null;
                ObjectTarget = null;
                
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
        
        Button.gameObject.SetActive(CouldBeUsed() && IsHudActive && Visible);

        if (CouldBeUsed() && PlayerControl.LocalPlayer.IsKillTimerEnabled)
        {
            if (Cooldown > 0f)
            {
                Cooldown -= Time.deltaTime;
                Button.SetCoolDown(Cooldown, IsEffectActive ? Options.EffectDuration : Options.MaxCooldown);
            }
            else
                Button.graphic.material.SetFloat("_Percent", 0f);

            if (Options._TargetType == CustomButtonOptions.TargetType.Player)
            {
                var newTarget = Options.PlayerTargetSelector.Invoke();
                if (PlayerTarget && PlayerTarget != newTarget)
                {
                    PlayerTarget.cosmetics.SetOutline(false, new Il2CppSystem.Nullable<Color>(Options.TargetOutline));
                }
                
                PlayerTarget = newTarget;
                
                if (PlayerTarget)
                {
                    newTarget.cosmetics.SetOutline(true, new Il2CppSystem.Nullable<Color>(Options.TargetOutline));
                }
            }
            else if (Options._TargetType == CustomButtonOptions.TargetType.Object)
            {
                var newTarget = Options.ObjectTargetSelector.Invoke();
                if (ObjectTarget && ObjectTarget != newTarget)
                {
                    var image = ObjectTarget.GetComponent<SpriteRenderer>();
                    if (!image && ObjectTarget.transform.FindChild("Sprite") != null)
                        image = ObjectTarget.transform.FindChild("Sprite").GetComponent<SpriteRenderer>();
                    if (!image)
                        image = ObjectTarget.GetComponentInChildren<SpriteRenderer>();
                    
                    if (image)
                    {
                        image.material.SetFloat("_Outline", 0);
                    }
                }
                
                ObjectTarget = newTarget;

                if (ObjectTarget)
                {
                    var image = ObjectTarget.GetComponent<SpriteRenderer>();
                    if (!image && ObjectTarget.transform.FindChild("Sprite") != null)
                        image = ObjectTarget.transform.FindChild("Sprite").GetComponent<SpriteRenderer>();
                    if (!image)
                        image = ObjectTarget.GetComponentInChildren<SpriteRenderer>();
                    
                    if (image)
                    {
                        image.material.SetFloat("_Outline", 1);
                        image.material.SetColor("_OutlineColor", Options.TargetOutline);
                    }
                }
            }
            
            Button.cooldownTimerText.color = IsEffectActive ? Color.green : Color.white;
            if (Cooldown <= 0f && IsEffectActive)
            {
                IsEffectActive = false;
                Options.OnEffectEnded();
                Cooldown = Options.MaxCooldown;
            }
        }
        
        if (CanBeUsed(true))
            Button.SetEnabled();
        else
            Button.SetDisabled();
    }

    public void OnHudSetActive(object sender, HudEventManager.HudSetActiveEventArgs args)
    {
        if (Button == null || Button.gameObject == null)
            return;
        
        IsHudActive = args.Active;
        Button.gameObject.SetActive(CouldBeUsed() && IsHudActive);
    }

    public bool CouldBeUsed()
    {
        if (PlayerControl.LocalPlayer == null) 
            return false;
            
        if (PlayerControl.LocalPlayer.Data == null) 
            return false;
            
        if (MeetingHud.Instance != null) 
            return false;

        if (!Visible)
            return false;

        return CouldUse.Invoke(PlayerControl.LocalPlayer);
    }

    public bool CanBeUsed(bool ignoreCooldown = false)
    {
        if (!CouldBeUsed())
            return false;
        
        if (!Enabled)
            return false;
        
        if (PlayerControl.LocalPlayer == null) 
            return false;

        if (PlayerControl.LocalPlayer.Data == null)
            return false;
            
        if (!PlayerControl.LocalPlayer.CanMove || PlayerControl.LocalPlayer.inVent) 
            return false;

        if (!Options.InfinitelyUsable && UsesLeft <= 0)
            return false;
        
        if (Options._TargetType == CustomButtonOptions.TargetType.Player && PlayerTarget == null)
            return false;
        
        if (Options._TargetType == CustomButtonOptions.TargetType.Object && ObjectTarget == null)
            return false;
        
        if (Cooldown > 0f && !ignoreCooldown)
            return false;
        
        return CanUse.Invoke(PlayerControl.LocalPlayer);
    }

    public void SetText(string text)
    {
        Text = text;
        Button.buttonLabelText.text = Text;
    }

    public void SetImage(Sprite sprite)
    {
        Image = sprite;
        Button.graphic.sprite = Image;
    }
    
    public void SetCooldown(float cooldown, float maxCooldown = Single.NaN)
    {
        Cooldown = cooldown;
        if (!float.IsNaN(maxCooldown))
            Options.MaxCooldown = maxCooldown;
        Button.SetCoolDown(cooldown, Options.MaxCooldown);
    }

    public void SetUsesLeft(int usesLeft)
    {
        Options.InfinitelyUsable = false;
        UsesLeft = usesLeft;
        Button.SetUsesRemaining(UsesLeft);
    }

    public void SetInfiniteUses()
    {
        Options.InfinitelyUsable = true;
        Button.SetInfiniteUses();
    }

    public void Dispose()
    {
        PeasmodPlugin.Logger.LogInfo("CustomButton#Dispose");
        GameEventManager.GameStartEventHandler -= Start;
        HudEventManager.HudUpdateEventHandler -= Update;
        if (Button != null && Button.gameObject != null)
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
        public Color TargetOutline;
        public Func<PlayerControl> PlayerTargetSelector;
        public Func<GameObject> ObjectTargetSelector;
        public bool OnLeftSide;

        public CustomButtonOptions(float maxCooldown = 0f, bool hasEffect = false, float effectDuration = 0f,
            Action onEffectEnded = null, bool infinitelyUsable = true, int maxUses = 0,
            TargetType targetType = TargetType.None, Color targetOutline = default,
            Func<PlayerControl> playerTargetSelector = null, Func<GameObject> objectTargetSelector = null,
            bool onLeftSide = true)
        {
            MaxCooldown = maxCooldown;
            HasEffect = hasEffect;
            EffectDuration = effectDuration;
            OnEffectEnded = onEffectEnded;
            InfinitelyUsable = infinitelyUsable;
            MaxUses = maxUses;
            _TargetType = targetType;
            TargetOutline = targetOutline;
            PlayerTargetSelector = playerTargetSelector;
            ObjectTargetSelector = objectTargetSelector;
            OnLeftSide = onLeftSide;
        }

        public enum TargetType
        {
            None,
            Player,
            Object
        }
    }
}