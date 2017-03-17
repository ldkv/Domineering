using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    // Game board
    private BoardSetup graphicBoard;
    private AI logicBoard;
    private const int noColor = 1;
    private TileStatus currentTurn; // Variable to track current turn and current color
    const int INFINITY = 1000000;
    bool checkAITurn = true;        // true => check P1 is AI or not, false => check P2

    // UI
    public Slider sizeBoard;
    public Slider difficultyAI;
    public Dropdown methodAI;
    public Dropdown P1Dropdown;
    public Dropdown P2Dropdown;
    public Dropdown PieceDropdown;
    public Button bStart;
    public Text gameOverText;
    public Text timerText;

    // UI variables
    private static int N = 4;
    private static int depthAI = 3;
    private static int typeAI = 0;
    private static int p1Choice = 0;
    private static int p2Choice = 0;
    private static int pieceChoice = 0;

    // Use this for initialization
    void Start()
    {
        graphicBoard = GetComponent<BoardSetup>();
        logicBoard = GetComponent<AI>();
        UpdatePanelData();
        currentTurn = TileStatus.EMPTY;
        gameOverText.text = "";
    }

    // Sauvegarder les paramètres rentrés
    public void GetPanelData()
    {
        N = (int)sizeBoard.value;
        depthAI = (int)difficultyAI.value;
        typeAI = methodAI.value;
        p1Choice = P1Dropdown.value;
        p2Choice = P2Dropdown.value;
        pieceChoice = PieceDropdown.value;
        //logicBoard.initKillerList(depthAI);
    }

    // Sauvegarder les paramètres rentrés
    public void UpdatePanelData()
    {
        // Sauvegarder les données
        int tN = N;
        int tdepthAI = depthAI;
        int ttypeAI = typeAI;
        int tp1Choice = p1Choice;
        int tp2Choice = p2Choice;
        int tpieceChoice = pieceChoice;
        // Mise à jour de l'UI
        sizeBoard.value = tN;
        difficultyAI.value = tdepthAI;
        methodAI.value = ttypeAI;
        P1Dropdown.value = tp1Choice;
        P2Dropdown.value = tp2Choice;
        PieceDropdown.value = tpieceChoice;
    }

    public void StartGame()
    {
        GetPanelData();
        // Generate the graphic board
        graphicBoard.GenerateBoard(N);
        // Generate the logic board
        logicBoard.InitLogicBoard(N, depthAI);
        currentTurn = (TileStatus)(pieceChoice * 2 - 1);
        checkAITurn = true;
        bStart.interactable = false;
    }

    public void Restart()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    void GameOver()
    {
        currentTurn = (TileStatus)(-(int)currentTurn);
        gameOverText.text = currentTurn + " GAGNE !!!";
        currentTurn = TileStatus.EMPTY;
    }

    int moveAIMethods()
    {
        int chosenMove = -1;
        switch (typeAI)
        {
            case 0:
                logicBoard.NegaMax(currentTurn, depthAI, out chosenMove);
                //logicBoard.MiniMax(currentTurn, depthAI, out chosenMose);
                break;
            case 1:
                logicBoard.abNegaMax(currentTurn, depthAI, -INFINITY, INFINITY, out chosenMove);
                break;
            case 2:
                //logicBoard.initKillerList(depthAI);
                logicBoard.abNegaMax_HeurTueur(currentTurn, depthAI, -INFINITY, INFINITY, out chosenMove);
                break;
            case 3:
                logicBoard.abNegaMax_HeurHistory(currentTurn, depthAI, -INFINITY, INFINITY, out chosenMove);
                break;
            default:
                break;
        }
        return chosenMove;
    }

    int lastIndex = -1;
    // Update is called once per frame
    void Update()
    {
        if (currentTurn == TileStatus.EMPTY)
            return;
        if (AITurn())
        {
            float timeStart = Time.realtimeSinceStartup;
            int chosenTile = moveAIMethods();
            float timeExec = Time.realtimeSinceStartup - timeStart;
            timeExec *= 1000;
            timerText.text = timeExec.ToString("0.000") + "ms";
            if (chosenTile == -1) // AI knows that it loses -> choose random move
            {
                GameOver();
                return;
            }
            else
            {
                int nextIndex = logicBoard.getNextIndex(chosenTile, currentTurn);
                UpdateTiles(chosenTile, nextIndex);
                graphicBoard.PlacelastMove(chosenTile);
            }
            checkAITurn = !checkAITurn;
            currentTurn = currentTurn == TileStatus.VERTICAL ? TileStatus.HORIZONTAL : TileStatus.VERTICAL;
        }
        else
        {
            if (logicBoard.possibleMoves(currentTurn).Count == 0)
            {
                GameOver();
                return;
            }
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
                    checkAITurn = !checkAITurn;
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
        if (checkAITurn)
            return (p1Choice == 1);
        return (p2Choice == 1);
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
