using System;
using System.Collections.Generic;
using System.Linq;
using DiehardMasterDisaster.Fisobs;
using DiehardMasterDisaster.GunStuff;
using HUD;
using UnityEngine;

namespace DiehardMasterDisaster.HUD;

public class WeaponsHUD : HudPart
{
    public readonly List<WeaponCategory> Categories;

    public Player Owner;

    private const int FadeOutDuration = 40;
    private const int FadeInDuration = 5;
    public const int DefaultHudDisplayTime = 120;

    private const float CategoryWidth = 40;
    private const float CategoryHeight = 50;
    private const float WeaponWidth = 70;
    private const float AmmoMeterPosX = 5;
    private const float AmmoMeterPosY = 5;
    private const float AmmoMeterWidth = 3;
    private const float AmmoMeterHeight = 40;
    private const float AmmoTypePosX = 10;
    private const float AmmoTypePosY = 5;
    
    //-- TODO: Half Life-like colors meant for player one, should get slugcat color or something for other jolly players
    private static readonly Color UIColor = new Color(1, 177 / 255f, 62 / 255f);
    private static readonly Color IconColor = new Color(1, 152 / 255f, 0 / 255f);
    private static readonly Color BarBackgroundColor = Color.red;
    private static readonly Color BarForegroundColor = Color.green;
    
    //-- TODO: also, should change it so it reduces the container size if it goes past the top of the screen in order to accomodate many HUDs
    private readonly FContainer container = new();

    private Vector2 pos;
    private Vector2 lastPos;
    private float fade;
    private float lastFade;
    private int show;

    //-- TODO: 4 is a hardcoded amount of categories, could change this to be dynamic
    //-- TODO: should be changed to only consider DMD players before the current one so it doesn't offset when not needed
    private int YOffset => Owner.playerState.playerNumber * (int)CategoryHeight * 4;

    public WeaponsHUD(global::HUD.HUD hud, Player owner) : base(hud)
    {
        Owner = owner;
        hud.fContainers[1].AddChild(container);

        Categories = new List<WeaponCategory>
        {
            new(this, DiehardEnums.AmmoType.Small, 0),
            new(this, DiehardEnums.AmmoType.Large, 1),
            new(this, DiehardEnums.AmmoType.Shells, 2),
            new(this, DiehardEnums.AmmoType.Special, 3)
        };
    }

    public void Show(int duration = DefaultHudDisplayTime)
    {
        show = Math.Max(show, duration);
    }

    public override void Update()
    {
        lastFade = fade;
        lastPos = pos;

        if (show > 0)
        {
            fade = Mathf.Min(fade + 1f / FadeInDuration, 1);
            show--;
        }
        else if (fade > 0)
        {
            fade = Mathf.Max(fade - 1f / FadeOutDuration, 0);
        }

        fade = Mathf.Max(fade, hud.karmaMeter.fade);
        pos = hud.karmaMeter.pos + new Vector2(-30, 50);

        var equippedGun = Owner.grasps.FirstOrDefault(x => x?.grabbed is Gun)?.grabbed as Gun;
        foreach (var category in Categories)
        {
            foreach (var display in category.WeaponDisplays)
            {
                display.Active = equippedGun != null && equippedGun.abstractGun == display.Weapon;
            }
        }

        foreach (var category in Categories)
        {
            category.Update();
        }
    }

    public override void Draw(float timeStacker)
    {
        container.alpha = Mathf.Lerp(lastFade, fade, timeStacker);
        container.SetPosition(Vector2.Lerp(lastPos, pos, timeStacker));

        foreach (var category in Categories)
        {
            category.GrafUpdate(timeStacker);
        }
    }

    public override void ClearSprites()
    {
        container.RemoveAllChildren();
        container.RemoveFromContainer();
    }

    public AbstractGun GetRelativeWeapon(AbstractGun gun, int relativeIndex)
    {
        var orderedGuns = OrderedGuns;
        if (orderedGuns.Count == 0) return null;

        if (gun == null)
        {
            return relativeIndex < 0 ? orderedGuns.LastOrDefault() : orderedGuns.FirstOrDefault();
        }

        if (relativeIndex < 0)
        {
            relativeIndex = Math.Abs(relativeIndex);
            orderedGuns.Reverse();
        }

        return orderedGuns[(orderedGuns.IndexOf(gun) + relativeIndex) % orderedGuns.Count];
    }

    private List<AbstractGun> OrderedGuns => (from category in Categories from display in category.WeaponDisplays select display.Weapon).ToList();

    public class WeaponCategory
    {
        private readonly DiehardEnums.AmmoType AmmoType;

        private readonly FSprite BackgroundSprite;
        private readonly FSprite TypeSprite;
        private readonly FSprite AmmoMeterBackground;
        private readonly FSprite AmmoMeterBar;
        public readonly List<WeaponDisplay> WeaponDisplays = new();
        
        private readonly WeaponsHUD owner;
        private readonly int index;

        public int BaseX => 0;
        public int BaseY => (int)CategoryHeight * index + owner.YOffset;
        public FContainer container => owner.container;
        private List<AbstractGun> PlayerWeapons => owner.Owner?.GetDMD().StoredGuns;

        public WeaponCategory(WeaponsHUD owner, DiehardEnums.AmmoType ammoType, int index)
        {
            AmmoType = ammoType;
            this.owner = owner;
            this.index = index;

            BackgroundSprite = new FSprite("sprites/WeaponsHUDCategoryBackground")
            {
                anchorX = 0,
                anchorY = 0,
                x = BaseX,
                y = BaseY,
                color = UIColor
            };
            TypeSprite = new FSprite($"sprites/WeaponsHUDCategoryIcon{AmmoType.value}")
            {
                anchorX = 0,
                anchorY = 0,
                x = BaseX + AmmoTypePosX,
                y = BaseY + AmmoTypePosY,
                color = IconColor
            };
            AmmoMeterBackground = new FSprite("pixel")
            {
                anchorX = 0,
                anchorY = 0,
                x = BaseX + AmmoMeterPosX,
                y = BaseY + AmmoMeterPosY,
                scaleX = AmmoMeterWidth,
                scaleY = AmmoMeterHeight,
                color = BarBackgroundColor
            };
            AmmoMeterBar = new FSprite("pixel")
            {
                anchorX = 0,
                anchorY = 0,
                x = BaseX + AmmoMeterPosX,
                y = BaseY + AmmoMeterPosY,
                scaleX = AmmoMeterWidth,
                scaleY = 0,
                color = BarForegroundColor
            };

            AddToContainer();
        }

        public void Update()
        {
            AmmoMeterBar.scaleY = AmmoMeterHeight / AmmoType.AmmoStorage * owner.Owner.GetDMD().StoredAmmo[AmmoType];
            
            foreach (var display in WeaponDisplays.ToList())
            {
                if (!PlayerWeapons.Any(x => x == display.Weapon))
                {
                    display.RemoveFromContainer();
                    WeaponDisplays.Remove(display);
                    foreach (var otherDisplay in WeaponDisplays)
                    {
                        if (otherDisplay.Weapon.AmmoType == display.Weapon.AmmoType && otherDisplay.Index > display.Index)
                        {
                            otherDisplay.Index--;
                            otherDisplay.UpdatePositions();
                        }
                    }
                }
            }
            
            foreach (var weapon in PlayerWeapons)
            {
                if (weapon.AmmoType == AmmoType && !WeaponDisplays.Any(x => x.Weapon == weapon))
                {
                    WeaponDisplays.Add(new WeaponDisplay(this, weapon, WeaponDisplays.Count(x => x.Weapon.AmmoType == weapon.AmmoType)));
                }
            }

            foreach (var display in WeaponDisplays)
            {
                display.Update();
            }
        }

        public void GrafUpdate(float timeStacker)
        {
            foreach (var display in WeaponDisplays)
            {
                display.GrafUpdate(timeStacker);
            }
        }

        public void AddToContainer()
        {
            container.AddChild(BackgroundSprite);
            container.AddChild(TypeSprite);
            container.AddChild(AmmoMeterBackground);
            container.AddChild(AmmoMeterBar);
        }
    }

    public class WeaponDisplay
    {
        public readonly AbstractGun Weapon;
        public FContainer container => owner.container;
        public int Index;
        public bool Active;
        
        private readonly WeaponCategory owner;
        private readonly FSprite BackgroundSprite;
        private readonly FSprite WeaponSprite;
        private readonly FSprite AmmoMeterBackground;
        private readonly FSprite AmmoMeterBar;

        private float blink;
        private float lastBlink;
        private int age;

        private int BaseY => owner.BaseY;
        private int BaseX => (int)CategoryWidth + (int)WeaponWidth * Index;

        public WeaponDisplay(WeaponCategory owner, AbstractGun weapon, int index)
        {
            this.owner = owner;
            Index = index;
            Weapon = weapon;

            BackgroundSprite = new FSprite("sprites/WeaponsHUDWeaponBackground")
            {
                anchorX = 0,
                anchorY = 0,
                x = BaseX,
                y = BaseY,
                color = UIColor
            };
            WeaponSprite = new FSprite($"sprites/WeaponsHUDIcon{weapon.type.value}")
            {
                anchorX = 0,
                anchorY = 0,
                x = BaseX + AmmoTypePosX,
                y = BaseY + AmmoTypePosY,
                color = IconColor
            };
            AmmoMeterBackground = new FSprite("pixel")
            {
                anchorX = 0,
                anchorY = 0,
                x = BaseX + AmmoMeterPosX,
                y = BaseY + AmmoMeterPosY,
                scaleX = AmmoMeterWidth,
                scaleY = AmmoMeterHeight,
                color = BarBackgroundColor
            };
            AmmoMeterBar = new FSprite("pixel")
            {
                anchorX = 0,
                anchorY = 0,
                x = BaseX + AmmoMeterPosX,
                y = BaseY + AmmoMeterPosY,
                scaleX = AmmoMeterWidth,
                scaleY = 0,
                color = BarForegroundColor
            };
            
            AddToContainer();
        }
        
        public void UpdatePositions()
        {
            BackgroundSprite.x = BaseX;
            BackgroundSprite.y = BaseY;

            WeaponSprite.x = BaseX + AmmoTypePosX;
            WeaponSprite.y = BaseY + AmmoTypePosY;
            
            AmmoMeterBackground.x = BaseX + AmmoMeterPosX;
            AmmoMeterBackground.y = BaseY + AmmoMeterPosY;
            
            AmmoMeterBar.x = BaseX + AmmoMeterPosX;
            AmmoMeterBar.y = BaseY + AmmoMeterPosY;
        }

        public void Update()
        {
            lastBlink = blink;
            age++;

            AmmoMeterBar.scaleY = AmmoMeterHeight / Weapon.ClipSize * Weapon.CurrentAmmo;

            if (Active)
            {
                blink = Mathf.PingPong(age / 30f, 0.5f);
            }
            else
            {
                blink = 0;
            }
        }

        public void GrafUpdate(float timeStacker)
        {
            var currentBlink = Mathf.Lerp(lastBlink, blink, timeStacker);
            if (currentBlink == 0)
            {
                BackgroundSprite.color = UIColor;
                WeaponSprite.color = IconColor;
            }
            else
            {
                BackgroundSprite.color = Color.Lerp(UIColor, Color.white, currentBlink);
                WeaponSprite.color = Color.Lerp(IconColor, Color.white, currentBlink);
            }
        }

        public void AddToContainer()
        {
            container.AddChild(BackgroundSprite);
            container.AddChild(WeaponSprite);
            container.AddChild(AmmoMeterBackground);
            container.AddChild(AmmoMeterBar);
        }

        public void RemoveFromContainer()
        {
            BackgroundSprite.RemoveFromContainer();
            WeaponSprite.RemoveFromContainer();
            AmmoMeterBackground.RemoveFromContainer();
            AmmoMeterBar.RemoveFromContainer();
        }
    }
}