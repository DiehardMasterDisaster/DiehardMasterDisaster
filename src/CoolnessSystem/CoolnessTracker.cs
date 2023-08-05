using Menu;

namespace DiehardMasterDisaster.CoolnessSystem;

public static class CoolnessTracker
{
    private const float DefaultScore = 5;
    
    public static float CurrentCoolness;

    private static int[] KillScores = new int[ExtEnum<MultiplayerUnlocks.SandboxUnlockID>.values.Count];

    public static void Reset()
    {
        CurrentCoolness = 0;

        KillScores = new int[ExtEnum<MultiplayerUnlocks.SandboxUnlockID>.values.Count];
        for (var i = 0; i < KillScores.Length; i++)
        {
            KillScores[i] = 1;
        }
        SandboxSettingsInterface.DefaultKillScores(ref KillScores);
        KillScores[(int)MultiplayerUnlocks.SandboxUnlockID.Slugcat] = 1;
    }

    public static void AddKill(Creature creature)
    {
        MultiplayerUnlocks.SandboxUnlockID id;
        if (creature is Centipede centi)
        {
            if (centi.Red)
            {
                id = MultiplayerUnlocks.SandboxUnlockID.RedCentipede;
            }
            else
            {
                id = centi.size switch
                {
                    < 0.255f => MultiplayerUnlocks.SandboxUnlockID.SmallCentipede,
                    < 0.6f => MultiplayerUnlocks.SandboxUnlockID.MediumCentipede,
                    _ => MultiplayerUnlocks.SandboxUnlockID.BigCentipede
                };
            }
        }
        else
        {
            id = new MultiplayerUnlocks.SandboxUnlockID(creature.Template.type.ToString());
        }

        var score = DefaultScore;
        if (id.index >= 0 && id.index < KillScores.Length)
        {
            score = KillScores[id.index];
        }

        CurrentCoolness += score;
    }
}