using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    // Game board
    private BoardSetup graphicBoard;
    private AI logicBoard;
    public int N = 5;
    private const int bWins = 1000000;      // To check win condition based on scoreboard
    private const int aWins = -bWins;
    private const int noColor = 1;
    private TileStatus currentTurn;         // Variable to track current turn and current color
    private TileStatus AIColor;             // color controlled by AI
    public int AIDifficulty = 3;
    const int INFINITY = 1000000;

    // Variables to track moves
    private int chosenTile;
    private bool first_AI_Move = true;
    
    // Use this for initialization
    void Start()
    {
        graphicBoard = GetComponent<BoardSetup>();
        logicBoard = GetComponent<AI>();

        // Generate the graphic board
        graphicBoard.GenerateBoard(N);
        // Generate the logic board
        logicBoard.InitLogicBoard(N);

        if (currentTurn == TileStatus.EMPTY)
            currentTurn = UnityEngine.Random.Range(0, 2) > 0 ? TileStatus.VERTICAL : TileStatus.HORIZONTAL;
        AIColor = currentTurn;
    }

    int lastIndex = -1;
    // Update is called once per frame
    void Update()
    {
        if (AITurn())
        {
            bool turn = (currentTurn == TileStatus.VERTICAL) ? true : false;
            logicBoard.abNegaMax(currentTurn, AIDifficulty, -INFINITY, INFINITY, out chosenTile);
            if (chosenTile == -1) // AI knows that it loses -> choose random move
            {
                //chosenTile = logicBoard.FindMostFilledColumn();
                Debug.Log("AI DESPERATE MOVE!!!");
            }
            else
            {
                int nextIndex = logicBoard.getNextIndex(chosenTile, currentTurn);
                UpdateTiles(chosenTile, nextIndex);
                graphicBoard.PlacelastMove(chosenTile);
            }
            currentTurn = currentTurn == TileStatus.VERTICAL ? TileStatus.HORIZONTAL : TileStatus.VERTICAL;
        }
        else
        {
            if (lastIndex != -1)
            {
                int nextLastIndex = logicBoard.getNextIndex(lastIndex, currentTurn);
                graphicBoard.SetTilesColor(lastIndex, nextLastIndex, noColor);
            }
            int index = GetTileSelected();
            int nextIndex = logicBoard.getNextIndex(index, currentTurn);
            // Hovering effect
            if (index >= 0 && logicBoard.Movable(index, nextIndex))
            {
                int choosingColor = ((int)currentTurn + 1) / 2 + 3;
                graphicBoard.SetTilesColor(index, nextIndex, choosingColor);
                lastIndex = index;
            }
            // If player has chosen
            if (Input.GetMouseButtonDown(0))
            {
                if (index >= 0 && logicBoard.Movable(index, nextIndex))
                {
                    UpdateTiles(index, nextIndex);
                    graphicBoard.PlacelastMove(index);
                    currentTurn = currentTurn == TileStatus.VERTICAL ? TileStatus.HORIZONTAL : TileStatus.VERTICAL;
                    lastIndex = -1;
                }
            }
        }
    }
    
    //
    void UpdateTiles(int index, int nextIndex)
    {
        graphicBoard.SetTilesColor(index, nextIndex, (int)currentTurn + 1);
        logicBoard.SetTileValue(index, currentTurn);
        logicBoard.SetTileValue(nextIndex, currentTurn);
    }

    // Check if current turn is AI's turn
    private bool AITurn()
    {
        return (AIDifficulty > 0) && (currentTurn == AIColor);
    }

    // Check if mouse input is valid - return -1 if not
    private int GetTileSelected()
    {
        // Specify the ray to be casted from the position of the mouse
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit2D hitInfo = Physics2D.Raycast(ray.origin, Vector2.zero, 0f);
        // Raycast and verify that it collided
        if (hitInfo)
            return graphicBoard.TileChosen(hitInfo.collider.gameObject);
        return -1;
    }
}
