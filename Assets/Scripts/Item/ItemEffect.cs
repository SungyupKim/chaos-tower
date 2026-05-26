using UnityEngine;

public enum ItemType
{
    ElementPowerUp,
    ElementChange,
    TrajectoryChange
}

[System.Serializable]
public struct ItemEffect
{
    public ItemType type;
    public Element element;
    public float powerBonus;

    public static ItemEffect CreatePowerUp(Element element, float bonus = 0.25f)
    {
        return new ItemEffect
        {
            type = ItemType.ElementPowerUp,
            element = element,
            powerBonus = bonus
        };
    }

    public static ItemEffect CreateElementChange(Element element)
    {
        return new ItemEffect
        {
            type = ItemType.ElementChange,
            element = element
        };
    }

    public static ItemEffect CreateTrajectoryChange()
    {
        return new ItemEffect
        {
            type = ItemType.TrajectoryChange
        };
    }

    public void Apply(Tower tower)
    {
        switch (type)
        {
            case ItemType.ElementPowerUp:
                if (tower.CurrentElement == element)
                    tower.BoostDamage(powerBonus);
                else
                    tower.SetElement(element);
                break;

            case ItemType.ElementChange:
                tower.SetElement(element);
                break;

            case ItemType.TrajectoryChange:
                tower.RandomizeTrajectory();
                break;
        }
    }
}
