using UnityEngine;

public static class SupplyVisuals
{
    private const string SupplySpritePath = "Sprites/Items/SupplyCrate";
    private static Sprite supplySprite;

    public static Sprite GetSupplySprite()
    {
        if (supplySprite != null)
        {
            return supplySprite;
        }

        Texture2D texture = Resources.Load<Texture2D>(SupplySpritePath);
        if (texture == null)
        {
            Debug.LogWarning($"Supply sprite not found: Resources/{SupplySpritePath}");
            return null;
        }

        texture.filterMode = FilterMode.Point;
        supplySprite = Sprite.Create(texture, new Rect(0f, 0f, texture.width, texture.height), new Vector2(0.5f, 0.5f), 32f);
        return supplySprite;
    }
}
