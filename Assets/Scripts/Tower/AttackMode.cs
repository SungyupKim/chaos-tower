using UnityEngine;
using System.Collections.Generic;

public enum Rarity { Common, Uncommon, Rare, Legendary }

public enum AttackModeType
{
    Standard,
    Multishot,
    Burst,
    Explosion,
    Chain,
    OrbitalStrike
}

[System.Serializable]
public class AttackMode
{
    public AttackModeType type;
    public string displayName;
    public string description;
    public Rarity rarity;
    public BeamShape beamShape;
    public float fireRateMultiplier = 1f;
    public float damageMultiplier = 1f;
    public int targetCount = 1;
    public float aoeRadius = 0f;
    public float chainRange = 0f;
    public int burstCount = 3;

    public static AttackMode GetStandard()
    {
        return new AttackMode
        {
            type = AttackModeType.Standard,
            displayName = "Basic Beam",
            description = "Standard single-target beam",
            rarity = Rarity.Common,
            beamShape = BeamShape.Clean,
            fireRateMultiplier = 1f,
            damageMultiplier = 1f,
            targetCount = 1,
        };
    }

    public static AttackMode RollGacha()
    {
        int roll = Random.Range(0, 100);
        Rarity rarity;
        if (roll < 2)       rarity = Rarity.Legendary;
        else if (roll < 12) rarity = Rarity.Rare;
        else if (roll < 37) rarity = Rarity.Uncommon;
        else                rarity = Rarity.Common;

        var pool = GetPool(rarity);
        return pool[Random.Range(0, pool.Count)];
    }

    private static List<AttackMode> GetPool(Rarity rarity)
    {
        switch (rarity)
        {
            case Rarity.Common:
                return new List<AttackMode>
                {
                    new AttackMode { type=AttackModeType.Standard,  displayName="Power Beam",   description="Damage +35%",              rarity=Rarity.Common,   beamShape=BeamShape.Clean,   fireRateMultiplier=1f,    damageMultiplier=1.35f, targetCount=1 },
                    new AttackMode { type=AttackModeType.Burst,     displayName="Burst x3",     description="3-shot burst, slow reload", rarity=Rarity.Common,   beamShape=BeamShape.Flicker, fireRateMultiplier=0.55f, damageMultiplier=0.8f,  targetCount=1, burstCount=3 },
                    new AttackMode { type=AttackModeType.Standard,  displayName="Rapid Beam",   description="Speed +70%, Dmg -25%",     rarity=Rarity.Common,   beamShape=BeamShape.Flicker, fireRateMultiplier=1.7f,  damageMultiplier=0.75f, targetCount=1 },
                };
            case Rarity.Uncommon:
                return new List<AttackMode>
                {
                    new AttackMode { type=AttackModeType.Multishot, displayName="Dual Shot",    description="Hits 2 nearest enemies",   rarity=Rarity.Uncommon, beamShape=BeamShape.Clean,   fireRateMultiplier=0.7f,  damageMultiplier=0.8f,  targetCount=2 },
                    new AttackMode { type=AttackModeType.Burst,     displayName="Burst x5",     description="5-shot burst",             rarity=Rarity.Uncommon, beamShape=BeamShape.Pulse,   fireRateMultiplier=0.45f, damageMultiplier=0.9f,  targetCount=1, burstCount=5 },
                };
            case Rarity.Rare:
                return new List<AttackMode>
                {
                    new AttackMode { type=AttackModeType.Explosion, displayName="Blast Beam",   description="AoE r=1.5 on hit",         rarity=Rarity.Rare,     beamShape=BeamShape.Crackle, fireRateMultiplier=0.65f, damageMultiplier=1.5f,  targetCount=1, aoeRadius=1.5f },
                    new AttackMode { type=AttackModeType.Chain,     displayName="Chain Bolt",   description="Chains 60% dmg to near",   rarity=Rarity.Rare,     beamShape=BeamShape.Crackle, fireRateMultiplier=0.85f, damageMultiplier=1.2f,  targetCount=1, chainRange=2.8f },
                    new AttackMode { type=AttackModeType.Multishot, displayName="Triple Shot",  description="Hits 3 nearest enemies",   rarity=Rarity.Rare,     beamShape=BeamShape.Pulse,   fireRateMultiplier=0.55f, damageMultiplier=0.75f, targetCount=3 },
                };
            case Rarity.Legendary:
            default:
                return new List<AttackMode>
                {
                    new AttackMode { type=AttackModeType.OrbitalStrike, displayName="Orbital Strike", description="Massive dmg, very slow", rarity=Rarity.Legendary, beamShape=BeamShape.Pulse,   fireRateMultiplier=0.2f,  damageMultiplier=6f,   targetCount=1 },
                    new AttackMode { type=AttackModeType.Explosion,     displayName="Nova Blast",     description="AoE r=3.0 mega blast",  rarity=Rarity.Legendary, beamShape=BeamShape.Crackle, fireRateMultiplier=0.3f,  damageMultiplier=2.5f, targetCount=1, aoeRadius=3.0f },
                };
        }
    }

    public Color GetRarityColor()
    {
        return rarity switch
        {
            Rarity.Common    => new Color(0.85f, 0.85f, 0.85f),
            Rarity.Uncommon  => new Color(0.3f, 1f, 0.4f),
            Rarity.Rare      => new Color(0.4f, 0.6f, 1f),
            Rarity.Legendary => new Color(1f, 0.75f, 0.1f),
            _                => Color.white
        };
    }

    public string GetRarityTag()
    {
        return rarity switch
        {
            Rarity.Common    => "[C]",
            Rarity.Uncommon  => "[U]",
            Rarity.Rare      => "[R]",
            Rarity.Legendary => "[L]",
            _                => ""
        };
    }
}
