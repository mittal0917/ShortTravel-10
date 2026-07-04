using UnityEngine;

public static class WeaponSpriteLibrary
{
    private const string WeaponSheetPath = "Sprites/Weapons/TheUltimateWeaponsPack_Sheet";
    private const float PixelsPerUnit = 32f;

    public static Sprite CreatePistolSprite()
    {
        return CreateSpriteFromSheet(new Rect(96f, 470f, 24f, 10f), new Vector2(0.5f, 0.5f));
    }

    public static Sprite CreateBatSprite()
    {
        return CreateSpriteFromSheet(new Rect(240f, 225f, 43f, 15f), new Vector2(0.5f, 0.5f));
    }

    public static Sprite CreateBulletSprite()
    {
        return CreateSpriteFromSheet(new Rect(0f, 19f, 38f, 5f), new Vector2(0.5f, 0.5f));
    }

    private static Sprite CreateSpriteFromSheet(Rect rect, Vector2 pivot)
    {
        Texture2D texture = Resources.Load<Texture2D>(WeaponSheetPath);
        if (texture == null)
        {
            return null;
        }

        texture.filterMode = FilterMode.Point;
        return Sprite.Create(texture, rect, pivot, PixelsPerUnit);
    }
}
