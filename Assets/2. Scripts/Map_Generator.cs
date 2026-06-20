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

    [Header("Exit Hole")]
    [SerializeField] private bool createRandomEdgeHole = true;

    public Vector3 ExitApproachPointWorld { get; private set; }
    public bool HasExitHole { get; private set; }

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

        if (createRandomEdgeHole)
        {
            CarveRandomEdgeHole();
        }
    }

    private void CarveRandomEdgeHole()
    {
        int side = Random.Range(0, 4);
        Vector3Int holeCell;
        Vector3Int approachCell;

        switch (side)
        {
            case 0:
                holeCell = new Vector3Int(Random.Range(0, mapWidth), -1, 0);
                approachCell = new Vector3Int(holeCell.x, 0, 0);
                break;
            case 1:
                holeCell = new Vector3Int(Random.Range(0, mapWidth), mapHeight, 0);
                approachCell = new Vector3Int(holeCell.x, mapHeight - 1, 0);
                break;
            case 2:
                holeCell = new Vector3Int(-1, Random.Range(0, mapHeight), 0);
                approachCell = new Vector3Int(0, holeCell.y, 0);
                break;
            default:
                holeCell = new Vector3Int(mapWidth, Random.Range(0, mapHeight), 0);
                approachCell = new Vector3Int(mapWidth - 1, holeCell.y, 0);
                break;
        }

        tilemapWall.SetTile(holeCell, null);
        ExitApproachPointWorld = tilemapFloor.GetCellCenterWorld(approachCell);
        HasExitHole = true;
    }
}
