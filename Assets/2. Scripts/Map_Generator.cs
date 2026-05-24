using UnityEngine;
using UnityEngine.Tilemaps;

public class Map_Generator : MonoBehaviour
{
    [Header("Tilemaps")]
    public Tilemap tilemapFloor;
    public Tilemap tilemapWall;

    [Header("Tiles")]
    public TileBase floorTile;
    public TileBase wallTile;

    [Header("Map Size")]
    public int mapWidth = 200;
    public int mapHeight = 200;

    void Start()
    {
        GenerateMap();
    }

    void GenerateMap()
    {
        // 바닥 생성
        for (int x = 0; x < mapWidth; x++)
            for (int y = 0; y < mapHeight; y++)
                tilemapFloor.SetTile(new Vector3Int(x, y, 0), floorTile);

        // 경계벽 생성
        for (int x = -1; x <= mapWidth; x++)
        {
            tilemapWall.SetTile(new Vector3Int(x, -1, 0), wallTile);
            tilemapWall.SetTile(new Vector3Int(x, mapHeight, 0), wallTile);
        }
        for (int y = 0; y < mapHeight; y++)
        {
            tilemapWall.SetTile(new Vector3Int(-1, y, 0), wallTile);
            tilemapWall.SetTile(new Vector3Int(mapWidth, y, 0), wallTile);
        }
    }
}