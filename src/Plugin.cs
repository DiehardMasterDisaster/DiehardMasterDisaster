using System;
using System.Security;
using System.Security.Permissions;
using BepInEx;
using DiehardMasterDisaster.CoolnessSystem;
using DiehardMasterDisaster.Fisobs;
using DiehardMasterDisaster.GunStuff;
using DiehardMasterDisaster.HUD;
using DiehardMasterDisaster.PlayerStuff;
using DiehardMasterDisaster.SaveData;
using DiehardMasterDisaster.ScoreScreen;
using Fisobs.Core;
using UnityEngine;

[module: UnverifiableCode]
#pragma warning disable CS0618 // Type or member is obsolete
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
#pragma warning restore CS0618 // Type or member is obsolete

namespace DiehardMasterDisaster;

[BepInPlugin(MOD_ID, "DiehardMasterDisaster", "1.0.0")]
public class Plugin : BaseUnityPlugin
{
    public const string MOD_ID = "diehard.master.disaster";

    public bool IsInit;
    public bool IsPreInit;
    public bool IsPostInit;

    public static DiehardOptions Options;

    private void OnEnable()
    {
        On.RainWorld.PreModsInit += RainWorld_PreModsInit;
        On.RainWorld.OnModsInit += RainWorld_OnModsInit;
        On.RainWorld.PostModsInit += RainWorld_PostModsInit;
    }

    private void RainWorld_PreModsInit(On.RainWorld.orig_PreModsInit orig, RainWorld self)
    {
        orig(self);
  
        try
        {
            if (IsPreInit) return;
            IsPreInit = true;
        }
        catch (Exception ex)
        {
            Debug.LogException(ex);
        }
    }

    private void RainWorld_OnModsInit(On.RainWorld.orig_OnModsInit orig, RainWorld self)
    {
        orig(self);

        try
        {
            MachineConnector.SetRegisteredOI(MOD_ID, Options = new DiehardOptions());
            
            if (IsInit) return;
            IsInit = true;

            DiehardEnums.Init();

            Futile.atlasManager.LoadAtlas("atlases/DMDGuns");
            Futile.atlasManager.LoadAtlas("atlases/DMDSlugcat");
            
            //-- TODO: Put all this crap in an atlas
            Futile.atlasManager.LoadImage("sprites/WeaponsHUDCategoryBackground");
            Futile.atlasManager.LoadImage("sprites/WeaponsHUDWeaponBackground");

            Futile.atlasManager.LoadImage("sprites/WeaponsHUDIconDMDMinigun");
            Futile.atlasManager.LoadImage("sprites/WeaponsHUDIconDMDGrenadeLauncherGun");
            Futile.atlasManager.LoadImage("sprites/WeaponsHUDIconDMDShotgun");
            Futile.atlasManager.LoadImage("sprites/WeaponsHUDIconDMDBFGGun");
            Futile.atlasManager.LoadImage("sprites/WeaponsHUDIconDMDDerringerGun");
            Futile.atlasManager.LoadImage("sprites/WeaponsHUDIconDMDAK47Gun");

            foreach (var ammoType in ExtEnumBase.GetNames(typeof(DiehardEnums.AmmoType)))
            {
                Futile.atlasManager.LoadImage($"sprites/WeaponsHUDCategoryIcon{ammoType}");
            }

            for (var i = 0; i <= 4; i++)
            {
                Futile.atlasManager.LoadImage($"sprites/DMDCoolness{i}");
            }

            GunHooks.Apply();
            PlayerGraphicsHooks.Apply();
            HUDHooks.Apply();
            SaveDataHooks.Apply();
            CoolnessHooks.Apply();
            ScoreScreenHooks.Apply();
            ScoreTrackingHooks.Apply();

            CustomFont.Load();
            
            Content.Register(
                new GunFisob(DiehardEnums.AbstractObject.DMDAK47Gun),
                new GunFisob(DiehardEnums.AbstractObject.DMDBFGGun),
                new GunFisob(DiehardEnums.AbstractObject.DMDDerringerGun),
                new GunFisob(DiehardEnums.AbstractObject.DMDMinigun),
                new GunFisob(DiehardEnums.AbstractObject.DMDShotgun),
                new GunFisob(DiehardEnums.AbstractObject.DMDGrenadeLauncherGun),

                new ProjectileFisob(DiehardEnums.AbstractObject.DMDBFGOrb),
                new ProjectileFisob(DiehardEnums.AbstractObject.DMDPipe)
                );
        }
        catch (Exception ex)
        {
            Debug.LogException(ex);
        }
    }

    private void RainWorld_PostModsInit(On.RainWorld.orig_PostModsInit orig, RainWorld self)
    {
        orig(self);

        try
        {
            if (IsPostInit) return;
            IsPostInit = true;
        }
        catch (Exception ex)
        {
            Debug.LogException(ex);
        }
    }
}