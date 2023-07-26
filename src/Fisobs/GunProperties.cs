using Fisobs.Properties;

namespace DiehardMasterDisaster.Fisobs;

public class GunProperties : ItemProperties
{
    public override void ScavCollectScore(Scavenger scav, ref int score)
    {
        score = 0;
    }

    public override void ScavWeaponPickupScore(Scavenger scav, ref int score)
    {
        score = 0;
    }

    public override void ScavWeaponUseScore(Scavenger scav, ref int score)
    {
        score = 0;
    }

    public override void LethalWeapon(Scavenger scav, ref bool isLethal)
    {
        isLethal = true;
    }

    public override void Grabability(Player player, ref Player.ObjectGrabability grabability)
    {
        grabability = Player.ObjectGrabability.TwoHands;
    }
}