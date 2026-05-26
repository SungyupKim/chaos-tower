using UnityEngine;

public enum Element
{
    Fire,
    Earth,
    Lightning,
    Water
}

public static class ElementSystem
{
    // 순환형 상성: Fire → Earth → Lightning → Water → Fire
    // 각 속성은 다음 속성에 강하고, 이전 속성에 약함
    private static readonly Element[] Cycle = { Element.Fire, Element.Earth, Element.Lightning, Element.Water };

    public static float GetDamageMultiplier(Element attacker, Element defender)
    {
        if (attacker == defender) return 1f;

        int attackerIndex = (int)attacker;
        int defenderIndex = (int)defender;
        int next = (attackerIndex + 1) % 4;

        if (next == defenderIndex) return 2f;

        int prev = (attackerIndex + 3) % 4;
        if (prev == defenderIndex) return 0.5f;

        return 1f;
    }

    public static Element GetStrongAgainst(Element element)
    {
        return Cycle[((int)element + 1) % 4];
    }

    public static Element GetWeakAgainst(Element element)
    {
        return Cycle[((int)element + 3) % 4];
    }

    public static Color GetElementColor(Element element)
    {
        return element switch
        {
            Element.Fire => new Color(1f, 0.3f, 0.1f),
            Element.Earth => new Color(0.6f, 0.4f, 0.2f),
            Element.Lightning => new Color(1f, 1f, 0.2f),
            Element.Water => new Color(0.2f, 0.5f, 1f),
            _ => Color.white
        };
    }
}
