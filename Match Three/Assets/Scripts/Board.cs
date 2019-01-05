using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Board : MonoBehaviour
{
    [SerializeField] private int width;
    [SerializeField] private int height;
    [SerializeField] private int borderSize;
    [SerializeField] private GameObject tilePrefab;
    [SerializeField] private GameObject[] gamePiecePrefabs;
    [SerializeField] private float swapTime = 0.5f;

    private Tile[,] allTiles;
    private GamePiece[,] allGamePieces;

    private Tile clickedTile;
    private Tile targetTile;

    private void Start()
    {
        allTiles = new Tile[width, height];
        allGamePieces = new GamePiece[width, height];

        SetupTiles();
        SetupCamera();
        FillRandom();
        HighlightMatches();
    }

    private void SetupTiles()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                GameObject tile = Instantiate(tilePrefab, new Vector3(x, y, 0), Quaternion.identity) as GameObject;
                tile.name = "Tile (" + x + "," + y + ")";

                allTiles[x, y] = tile.GetComponent<Tile>();

                tile.transform.parent = transform;

                allTiles[x, y].Init(x, y, this);
            }
        }
    }

    private void SetupCamera()
    {
        Camera.main.transform.position = new Vector3((width - 1) / 2f, (height - 1) / 2f, -10f);

        float aspectRatio = (float)Screen.width / (float)Screen.height;
        float verticalSize = height / 2f + borderSize;
        float horizontalSize = (width / 2f + borderSize) / aspectRatio;

        Camera.main.orthographicSize = (verticalSize > horizontalSize) ? verticalSize : horizontalSize;
    }

    private GameObject GetRandomGamePiece()
    {
        int randomIndex = Random.Range(0, gamePiecePrefabs.Length);

        if (gamePiecePrefabs[randomIndex] == null)
        {
            Debug.LogWarning("BOARD: " + randomIndex + " doesn not contain a valid GamePiece prefab!");
        }

        return gamePiecePrefabs[randomIndex];
    }

    public void PlaceGamePiece(GamePiece gamePiece, int x, int y)
    {
        if (gamePiece == null)
        {
            Debug.LogWarning("BOARD: Invalid GamePiece!");
            return;
        }

        gamePiece.transform.position = new Vector3(x, y, 0);
        gamePiece.transform.rotation = Quaternion.identity;
        if (IsWithinBounds(x, y))
        {
            allGamePieces[x, y] = gamePiece;
        }
        gamePiece.SetCoord(x, y);
    }

    private bool IsWithinBounds(int x, int y)
    {
        return (x >= 0 && x < width && y >= 0 && y < height);
    }

    private void FillRandom()
    {
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                GameObject randomPiece = Instantiate(GetRandomGamePiece(), Vector3.zero, Quaternion.identity) as GameObject;

                if (randomPiece != null)
                {
                    randomPiece.GetComponent<GamePiece>().Init(this);
                    PlaceGamePiece(randomPiece.GetComponent<GamePiece>(), i, j);
                    randomPiece.transform.parent = transform;
                }
            }
        }
    }

    public void ClickTile(Tile tile)
    {
        if (clickedTile == null)
        {
            clickedTile = tile;
        }
    }

    public void DragToTile(Tile tile)
    {
        if (clickedTile != null && IsNextTo(tile, clickedTile))
        {
            targetTile = tile;
        }
    }

    public void ReleaseTile()
    {
        if (clickedTile != null && targetTile != null)
        {
            SwitchTiles(clickedTile, targetTile);

            clickedTile = null;
            targetTile = null;
        }
    }

    private void SwitchTiles(Tile clickedTile, Tile targetTile)
    {
        GamePiece clickedPiece = allGamePieces[clickedTile.xIndex, clickedTile.yIndex];
        GamePiece targetPiece = allGamePieces[targetTile.xIndex, targetTile.yIndex];

        clickedPiece.Move(targetTile.xIndex, targetTile.yIndex, swapTime);
        targetPiece.Move(clickedTile.xIndex, clickedTile.yIndex, swapTime);
    }

    private bool IsNextTo(Tile startTile, Tile endTile)
    {
        if (Mathf.Abs(startTile.xIndex - endTile.xIndex) == 1 && startTile.yIndex == endTile.yIndex)
        {
            return true;
        }

        if (Mathf.Abs(startTile.yIndex - endTile.yIndex) == 1 && startTile.xIndex == endTile.xIndex)
        {
            return true;
        }

        return false;
    }

    private List<GamePiece> FindMatches(int startX, int startY, Vector2 searchDirection, int minLenght = 3)
    {
        List<GamePiece> matches = new List<GamePiece>();
        GamePiece startPiece = null;

        if (IsWithinBounds(startX, startY))
        {
            startPiece = allGamePieces[startX, startY];
        }

        if (startPiece != null)
        {
            matches.Add(startPiece);
        }
        else
        {
            return null;
        }

        int nextX;
        int nextY;

        int maxValue = (width > height) ? width : height;

        for (int i = 1; i < maxValue - 1; i++)
        {
            nextX = startX + (int)Mathf.Clamp(searchDirection.x, -1, 1) * i;
            nextY = startY + (int)Mathf.Clamp(searchDirection.y, -1, 1) * i;

            if (!IsWithinBounds(nextX, nextY))
            {
                break;
            }

            GamePiece nextPiece = allGamePieces[nextX, nextY];

            if (nextPiece.matchValue == startPiece.matchValue && !matches.Contains(nextPiece))
            {
                matches.Add(nextPiece);
            }
            else
            {
                break;
            }
        }

        if (matches.Count >= minLenght)
        {
            return matches;
        }

        return null;
    }

    private List<GamePiece> FindVerticalMatches(int startX, int startY, int minLenght = 3)
    {
        List<GamePiece> upwardMatches = FindMatches(startX, startY, new Vector2(0, 1), 2);
        List<GamePiece> downwardMatches = FindMatches(startX, startY, new Vector2(0, -1), 2);

        if (upwardMatches == null)
        {
            upwardMatches = new List<GamePiece>();
        }

        if (downwardMatches == null)
        {
            downwardMatches = new List<GamePiece>();
        }

        var combinedMatches = upwardMatches.Union(downwardMatches).ToList();

        return (combinedMatches.Count >= minLenght) ? combinedMatches : null;
    }

    private List<GamePiece> FindHorizontalMatches(int startX, int startY, int minLenght = 3)
    {
        List<GamePiece> rightMatches = FindMatches(startX, startY, new Vector2(1, 0), 2);
        List<GamePiece> leftMatches = FindMatches(startX, startY, new Vector2(-1, 0), 2);

        if (rightMatches == null)
        {
            rightMatches = new List<GamePiece>();
        }

        if (leftMatches == null)

        {
            leftMatches = new List<GamePiece>();
        }

        var combinedMatches = rightMatches.Union(leftMatches).ToList();

        return (combinedMatches.Count >= minLenght) ? combinedMatches : null;
    }

    void HighlightMatches()
    {
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < width; j++)
            {
                SpriteRenderer spriteRenderer = allTiles[i, j].GetComponent<SpriteRenderer>();
                spriteRenderer.color = new Color(spriteRenderer.color.r, spriteRenderer.color.g, spriteRenderer.color.b, 0);

                List<GamePiece> horizontalMatches = FindHorizontalMatches(i, j, 3);
                List<GamePiece> vertialMatches = FindVerticalMatches(i, j, 3);

                if (horizontalMatches == null)
                {
                    horizontalMatches = new List<GamePiece>();
                }

                if (vertialMatches == null)
                {
                    vertialMatches = new List<GamePiece>();
                }

                var combinedMatches = horizontalMatches.Union(vertialMatches).ToList();

                if (combinedMatches.Count > 0)
                {
                    foreach(GamePiece piece in combinedMatches)
                    {
                        spriteRenderer = allTiles[piece.xIndex, piece.yIndex].GetComponent<SpriteRenderer>();
                        spriteRenderer.color = piece.GetComponent<SpriteRenderer>().color;
                    }
                }
            }
        }
    }
}