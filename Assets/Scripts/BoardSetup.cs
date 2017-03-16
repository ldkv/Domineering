using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardSetup : MonoBehaviour
{
    // Graphic objects
    public GameObject tilePrefab;
    public GameObject lastMovePrefab;
    // Sprites
    public Sprite[] tileSprites;        // Color tile sprites - for changing tile color

    private List<GameObject> tilesBoard;// List of all tiles
    private GameObject lastMove;
    private int SIZE;

    // Create the game board
    public void GenerateBoard(int N)
    {
        SIZE = N;
        float sizeTile = tilePrefab.GetComponent<SpriteRenderer>().bounds.size.x;
        
        // Initialise global variables
        tilesBoard = new List<GameObject>();

        // Calculate coordinate of the first tile at position (0,0) (bottom-left corner)
        float startX = (1 - N) / 2.0f * sizeTile;
        float posY = (N - 1) / 2.0f * sizeTile;

        // start instantiating board tiles
        for (int i = 0; i < N; i++)
        {
            float posX = startX;
            Vector3 tilePos = Vector3.zero;
            GameObject instance;
            for (int j = 0; j < N; j++)
            {
                tilePos = new Vector3(posX, posY, 0f);
                // Generate tile board
                instance = Instantiate(tilePrefab, tilePos, Quaternion.identity) as GameObject;
                instance.transform.SetParent(this.transform);
                tilesBoard.Add(instance);
                // increment horizontal position
                posX += sizeTile;   // increase x by tile size to move to next column position
            }
            // increase y by tile size to move to next row position
            posY -= sizeTile;
        }
        // Create lastMove indicator
        lastMove = Instantiate(lastMovePrefab, Vector3.zero, Quaternion.identity) as GameObject;
        HidelastMove();
    }

    // Change tile color to the chosen one
    public void SetTilesColor(int id1, int id2, int color)
    {
        tilesBoard[id1].GetComponent<SpriteRenderer>().sprite = tileSprites[color];
        tilesBoard[id2].GetComponent<SpriteRenderer>().sprite = tileSprites[color];
    }

    // Return the column where the mouse clicked
    public int TileChosen(GameObject tile)
    {
        int index = tilesBoard.FindIndex(x => x == tile);
        return index;
    }

    // Positioning the lastMove to the chosen tile
    public void PlacelastMove(int index)
    {
        Vector3 lastMovePos = new Vector3(tilesBoard[index].transform.position.x, tilesBoard[index].transform.position.y, 0f);
        lastMove.transform.position = lastMovePos;
    }

    // Hide the lastMove
    public void HidelastMove()
    {
        lastMove.transform.position = new Vector3(-100, -100, -100);
    }
}
