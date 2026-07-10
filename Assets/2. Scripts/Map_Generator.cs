using System.Collections;
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

    [Header("Reference Image Map")]
    [SerializeField] private bool useReferenceImageMap = true;
    [SerializeField] private string referenceMapResourcePath = "Sprites/Map/ReferenceMap";
    [SerializeField] private float referenceMapPixelsPerUnit = 16f;
    [SerializeField] private float waterPixelRatio = 0.18f;
    [SerializeField] private float treePixelRatio = 0.34f;
    [SerializeField] private float rockPixelRatio = 0.12f;

    [Header("Map Size")]
    public int mapWidth = 200;
    public int mapHeight = 200;

    [Header("Exit Hole")]
    [SerializeField] private bool createRandomEdgeHole = true;

    private TileBase collisionTile;
    private Texture2D referenceMapTexture;
    private GameObject exitMarker;
    private float referenceMapWorldWidth;
    private float referenceMapWorldHeight;

    public Vector3 ExitApproachPointWorld { get; private set; }
    public bool HasExitHole { get; private set; }
    public float MapMinX => 0f;
    public float MapMinY => 0f;
    public float MapMaxX => referenceMapWorldWidth > 0f ? referenceMapWorldWidth : mapWidth;
    public float MapMaxY => referenceMapWorldHeight > 0f ? referenceMapWorldHeight : mapHeight;

    void Start()
    {
        GenerateMap();
        StartCoroutine(EnsureSceneStartsInsideMapNextFrame());
    }

    void GenerateMap()
    {
        tilemapFloor.ClearAllTiles();
        tilemapWall.ClearAllTiles();
        ApplyTilemapSorting();

        if (useReferenceImageMap && TryCreateReferenceImageMap())
        {
            GenerateImageBasedCollision();
        }
        else
        {
            GenerateFallbackTileMap();
        }

        GenerateBoundaryWalls();

        if (createRandomEdgeHole)
        {
            CarveRandomEdgeHole();
        }
    }

    private bool TryCreateReferenceImageMap()
    {
        referenceMapTexture = Resources.Load<Texture2D>(referenceMapResourcePath);
        if (referenceMapTexture == null)
        {
            Debug.LogWarning($"맵 이미지 에셋을 찾지 못했습니다: Resources/{referenceMapResourcePath}");
            return false;
        }

        ApplyReferenceMapWorldSize();

        GameObject oldMap = GameObject.Find("ReferenceMap_Background");
        if (oldMap != null)
        {
            Destroy(oldMap);
        }

        // 맵 이미지를 16px = 1타일 기준으로 배치합니다.
        // 이미지 폭을 200칸에 강제로 맞추면 화면이 뭉개지고 캐릭터가 너무 작아 보여서, 픽셀아트 원본 비율을 우선합니다.
        float pixelsPerUnit = Mathf.Max(1f, referenceMapPixelsPerUnit);
        referenceMapWorldWidth = referenceMapTexture.width / pixelsPerUnit;
        referenceMapWorldHeight = referenceMapTexture.height / pixelsPerUnit;
        Sprite mapSprite = Sprite.Create(
            referenceMapTexture,
            new Rect(0f, 0f, referenceMapTexture.width, referenceMapTexture.height),
            new Vector2(0.5f, 0.5f),
            pixelsPerUnit);

        GameObject mapObject = new GameObject("ReferenceMap_Background");
        mapObject.transform.position = new Vector3(referenceMapWorldWidth * 0.5f, referenceMapWorldHeight * 0.5f, 0.5f);

        SpriteRenderer renderer = mapObject.AddComponent<SpriteRenderer>();
        renderer.sprite = mapSprite;
        renderer.sortingOrder = -5;
        return true;
    }

    private void ApplyReferenceMapWorldSize()
    {
        // 충돌 판정도 배경 이미지와 같은 타일 크기로 맞춰야 물/나무/돌 위치가 화면과 어긋나지 않습니다.
        float pixelsPerUnit = Mathf.Max(1f, referenceMapPixelsPerUnit);
        mapWidth = Mathf.Max(1, Mathf.CeilToInt(referenceMapTexture.width / pixelsPerUnit));
        mapHeight = Mathf.Max(1, Mathf.CeilToInt(referenceMapTexture.height / pixelsPerUnit));
    }

    private IEnumerator EnsureSceneStartsInsideMapNextFrame()
    {
        // 플레이어가 저장된 예전 좌표로 복원되는 작업이 끝난 뒤 검사해야 카메라가 빈 공간을 보는 문제를 막을 수 있습니다.
        yield return null;
        MovePlayerAndCameraInsideMapIfNeeded();
    }

    private void MovePlayerAndCameraInsideMapIfNeeded()
    {
        character_move player = FindObjectOfType<character_move>();
        Vector3 safePosition = FindSafeCenterWorldPosition();

        if (player != null)
        {
            if (!IsWalkableWorld(player.transform.position))
            {
                player.transform.position = safePosition;
                player.SaveCurrentPosition();
            }

            PlayerAttack playerAttack = player.GetComponent<PlayerAttack>();
            if (playerAttack != null)
            {
                playerAttack.RefreshPistolPickupNearPlayer();
            }

            MoveCameraTo(player.transform.position);
            return;
        }

        MoveCameraTo(safePosition);
    }

    private Vector3 FindSafeCenterWorldPosition()
    {
        Vector3Int centerCell = new Vector3Int(mapWidth / 2, mapHeight / 2, 0);
        if (IsWalkableCell(centerCell))
        {
            return tilemapFloor.GetCellCenterWorld(centerCell);
        }

        int maxRadius = Mathf.Max(mapWidth, mapHeight);
        for (int radius = 1; radius <= maxRadius; radius++)
        {
            for (int x = centerCell.x - radius; x <= centerCell.x + radius; x++)
            {
                for (int y = centerCell.y - radius; y <= centerCell.y + radius; y++)
                {
                    Vector3Int candidate = new Vector3Int(x, y, 0);
                    if (IsWalkableCell(candidate))
                    {
                        return tilemapFloor.GetCellCenterWorld(candidate);
                    }
                }
            }
        }

        // 혹시 이미지 판정이 너무 강해서 모든 칸이 막혔을 때도 화면은 맵 중앙을 보도록 합니다.
        return new Vector3(mapWidth * 0.5f, mapHeight * 0.5f, 0f);
    }

    private void MoveCameraTo(Vector3 targetPosition)
    {
        Camera mainCamera = Camera.main;
        if (mainCamera == null)
        {
            return;
        }

        // 카메라 추적 스크립트가 다음 프레임에 다시 따라가더라도, 시작 순간 빈 화면이 보이지 않도록 즉시 위치를 맞춥니다.
        mainCamera.transform.position = new Vector3(targetPosition.x, targetPosition.y, mainCamera.transform.position.z);
    }

    private void GenerateImageBasedCollision()
    {
        collisionTile = RuntimeTileFactory.CreateTransparentCollisionTile();

        for (int x = 0; x < mapWidth; x++)
        {
            for (int y = 0; y < mapHeight; y++)
            {
                if (IsObstacleCellFromImage(x, y))
                {
                    tilemapWall.SetTile(new Vector3Int(x, y, 0), collisionTile);
                }
            }
        }
    }

    private bool IsObstacleCellFromImage(int cellX, int cellY)
    {
        int textureMinX = Mathf.FloorToInt(cellX * referenceMapTexture.width / (float)mapWidth);
        int textureMaxX = Mathf.CeilToInt((cellX + 1) * referenceMapTexture.width / (float)mapWidth);
        int textureMinY = Mathf.FloorToInt(cellY * referenceMapTexture.height / (float)mapHeight);
        int textureMaxY = Mathf.CeilToInt((cellY + 1) * referenceMapTexture.height / (float)mapHeight);

        int total = 0;
        int water = 0;
        int tree = 0;
        int rock = 0;
        int bridge = 0;

        for (int px = textureMinX; px < textureMaxX; px += 2)
        {
            for (int py = textureMinY; py < textureMaxY; py += 2)
            {
                Color32 color = referenceMapTexture.GetPixel(px, py);
                total++;

                if (IsFlowerPixel(color))
                {
                    continue;
                }

                if (IsBridgePixel(color))
                {
                    bridge++;
                }
                else if (IsWaterPixel(color))
                {
                    water++;
                }
                else if (IsTreePixel(color))
                {
                    tree++;
                }
                else if (IsRockPixel(color))
                {
                    rock++;
                }
            }
        }

        if (total == 0)
        {
            return false;
        }

        // 다리는 물 위에 있어서 같은 칸 안에 물 픽셀도 같이 잡힙니다.
        // 다리색이 충분히 있으면 물 판정보다 우선해서 통행 가능하게 둡니다.
        if (bridge / (float)total >= 0.08f && water / (float)total >= 0.08f)
        {
            return false;
        }

        // 물/나무/돌이 일정 비율 이상 들어간 셀만 막습니다.
        // 이렇게 해야 꽃, 길 장식, 작은 잔디 무늬까지 전부 벽이 되는 일을 피할 수 있습니다.
        return water / (float)total >= waterPixelRatio
            || tree / (float)total >= treePixelRatio
            || rock / (float)total >= rockPixelRatio;
    }

    private bool IsWaterPixel(Color32 color)
    {
        bool waterBody = color.b > 150 && color.g > 120 && color.r < 100;
        bool waterEdge = color.b > 115 && color.g > 85 && color.r < 75 && color.b >= color.g - 10;
        return waterBody || waterEdge;
    }

    private bool IsBridgePixel(Color32 color)
    {
        // 나무 다리의 갈색/주황색 계열은 물 위에 있어도 이동 가능한 길로 취급합니다.
        return color.r > 100 && color.r < 235
            && color.g > 45 && color.g < 175
            && color.b < 120
            && color.r > color.g + 20;
    }

    private bool IsTreePixel(Color32 color)
    {
        bool darkLeaf = color.g > 45 && color.g < 125 && color.r < 55 && color.b < 105;
        bool deepLeafShadow = color.g > 35 && color.g < 95 && color.r < 45 && color.b < 85;
        return darkLeaf || deepLeafShadow;
    }

    private bool IsRockPixel(Color32 color)
    {
        bool purpleRock = color.r > 80 && color.r < 180 && color.g > 65 && color.g < 155 && color.b > 110 && color.b > color.r + 5;
        bool grayRock = color.r > 95 && color.r < 175
            && color.g > 85 && color.g < 165
            && color.b > 95 && color.b < 190
            && color.r + color.g + color.b < 485
            && Mathf.Abs(color.r - color.g) < 35
            && Mathf.Abs(color.b - color.r) < 55;
        return purpleRock || grayRock;
    }

    private bool IsFlowerPixel(Color32 color)
    {
        // 꽃은 밝은 흰색/노란색/분홍색 픽셀이 많아 돌 판정과 겹칠 수 있으므로 먼저 이동 가능 장식으로 제외합니다.
        bool whiteFlower = color.r > 205 && color.g > 205 && color.b > 185;
        bool yellowFlower = color.r > 180 && color.g > 135 && color.b < 95;
        bool pinkFlower = color.r > 175 && color.g < 135 && color.b > 105 && color.r > color.b + 20;
        return whiteFlower || yellowFlower || pinkFlower;
    }

    private void GenerateFallbackTileMap()
    {
        // 이미지 에셋을 못 읽었을 때만 기존 단순 타일맵을 사용합니다.
        referenceMapWorldWidth = mapWidth;
        referenceMapWorldHeight = mapHeight;

        for (int x = 0; x < mapWidth; x++)
        {
            for (int y = 0; y < mapHeight; y++)
            {
                tilemapFloor.SetTile(new Vector3Int(x, y, 0), floorTile);
            }
        }
    }

    private void GenerateBoundaryWalls()
    {
        TileBase boundaryTile = collisionTile != null ? collisionTile : wallTile;

        for (int x = -1; x <= mapWidth; x++)
        {
            tilemapWall.SetTile(new Vector3Int(x, -1, 0), boundaryTile);
            tilemapWall.SetTile(new Vector3Int(x, mapHeight, 0), boundaryTile);
        }

        for (int y = 0; y < mapHeight; y++)
        {
            tilemapWall.SetTile(new Vector3Int(-1, y, 0), boundaryTile);
            tilemapWall.SetTile(new Vector3Int(mapWidth, y, 0), boundaryTile);
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
        CreateExitMarker();
    }

    private void CreateExitMarker()
    {
        if (exitMarker != null)
        {
            Destroy(exitMarker);
        }

        // 임시 탈출구 표시입니다. 문 스프라이트를 찾기 전까지 검은 네모로 위치를 확실히 보여줍니다.
        exitMarker = new GameObject("Exit_Marker");
        exitMarker.transform.position = ClampWorldPointInsideVisibleMap(ExitApproachPointWorld, 0.5f);
        exitMarker.transform.localScale = new Vector3(0.9f, 0.9f, 1f);

        SpriteRenderer renderer = exitMarker.AddComponent<SpriteRenderer>();
        renderer.sprite = RuntimeSpriteFactory.CreateSquareSprite(Color.black);
        renderer.sortingOrder = 15;
    }

    private Vector3 ClampWorldPointInsideVisibleMap(Vector3 worldPoint, float margin)
    {
        // 올림 처리된 마지막 타일 중앙은 실제 이미지 밖으로 나갈 수 있어, 표시용 오브젝트는 보이는 맵 안쪽으로 조입니다.
        float minX = MapMinX + margin;
        float maxX = MapMaxX - margin;
        float minY = MapMinY + margin;
        float maxY = MapMaxY - margin;
        float clampedX = minX > maxX ? (MapMinX + MapMaxX) * 0.5f : Mathf.Clamp(worldPoint.x, minX, maxX);
        float clampedY = minY > maxY ? (MapMinY + MapMaxY) * 0.5f : Mathf.Clamp(worldPoint.y, minY, maxY);
        return new Vector3(clampedX, clampedY, 0f);
    }

    private void ApplyTilemapSorting()
    {
        TilemapRenderer floorRenderer = tilemapFloor.GetComponent<TilemapRenderer>();
        TilemapRenderer wallRenderer = tilemapWall.GetComponent<TilemapRenderer>();

        if (floorRenderer != null)
        {
            floorRenderer.sortingOrder = -10;
            floorRenderer.enabled = !useReferenceImageMap;
        }

        if (wallRenderer != null)
        {
            // 충돌 타일은 투명하므로 렌더러는 꺼도 TilemapCollider2D는 그대로 작동합니다.
            wallRenderer.enabled = false;
        }
    }

    public bool IsWalkableWorld(Vector3 worldPosition)
    {
        Vector3Int cell = tilemapFloor.WorldToCell(worldPosition);
        return IsWalkableCell(cell);
    }

    public bool IsWalkableCell(Vector3Int cell)
    {
        // 맵 밖과 wall 타일맵에 찍힌 물/나무/돌/외곽벽은 이동 불가로 취급합니다.
        if (cell.x < 0 || cell.x >= mapWidth || cell.y < 0 || cell.y >= mapHeight)
        {
            return false;
        }

        return !tilemapWall.HasTile(cell);
    }

    private static class RuntimeTileFactory
    {
        private static Tile transparentCollisionTile;

        public static TileBase CreateTransparentCollisionTile()
        {
            if (transparentCollisionTile != null)
            {
                return transparentCollisionTile;
            }

            Texture2D texture = new Texture2D(1, 1, TextureFormat.RGBA32, false);
            texture.SetPixel(0, 0, Color.clear);
            texture.Apply();

            transparentCollisionTile = ScriptableObject.CreateInstance<Tile>();
            transparentCollisionTile.sprite = Sprite.Create(texture, new Rect(0f, 0f, 1f, 1f), new Vector2(0.5f, 0.5f), 1f);
            transparentCollisionTile.colliderType = Tile.ColliderType.Grid;
            return transparentCollisionTile;
        }
    }

    private static class RuntimeSpriteFactory
    {
        private static Sprite blackSquareSprite;

        public static Sprite CreateSquareSprite(Color color)
        {
            if (blackSquareSprite != null)
            {
                return blackSquareSprite;
            }

            Texture2D texture = new Texture2D(16, 16, TextureFormat.RGBA32, false);
            texture.filterMode = FilterMode.Point;
            for (int x = 0; x < texture.width; x++)
            {
                for (int y = 0; y < texture.height; y++)
                {
                    texture.SetPixel(x, y, color);
                }
            }

            texture.Apply();
            blackSquareSprite = Sprite.Create(texture, new Rect(0f, 0f, texture.width, texture.height), new Vector2(0.5f, 0.5f), 16f);
            return blackSquareSprite;
        }
    }
}
