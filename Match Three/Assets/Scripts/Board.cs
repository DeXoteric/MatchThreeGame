using UnityEngine;

public class Board : MonoBehaviour
{
    [SerializeField] int width;
    [SerializeField] int height;

    [SerializeField] GameObject tilePrefab;

    Tile[,] allTiles;

    private void Start()
    {
        allTiles = new Tile[width, height];

        SetupTiles();
    }

    void SetupTiles()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                GameObject tile = Instantiate(tilePrefab, new Vector3(x, y, 0), Quaternion.identity) as GameObject;
                tile.name = "Tile (" + x + "," + y + ")";

                allTiles[x, y] = tile.GetComponent<Tile>();

                tile.transform.parent = transform;
            }
        }
    }
}