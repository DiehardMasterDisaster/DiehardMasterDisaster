namespace DiehardMasterDisaster;

public class DiehardOptions : OptionInterface
{
    public static Configurable<int> MaxAmmoSmall;
    public static Configurable<int> MaxAmmoLarge;
    public static Configurable<int> MaxAmmoShells;
    public static Configurable<int> MaxAmmoSpecial;
    
    public DiehardOptions()
    {
        MaxAmmoSmall = new Configurable<int>(this, nameof(MaxAmmoSmall), 50, new ConfigurableInfo("Max Small Ammo"));
        MaxAmmoLarge = new Configurable<int>(this, nameof(MaxAmmoLarge), 50, new ConfigurableInfo("Max Large Ammo"));
        MaxAmmoShells = new Configurable<int>(this, nameof(MaxAmmoShells), 30, new ConfigurableInfo("Max Shells"));
        MaxAmmoSpecial = new Configurable<int>(this, nameof(MaxAmmoSpecial), 8, new ConfigurableInfo("Max Special Ammo"));
    }
}