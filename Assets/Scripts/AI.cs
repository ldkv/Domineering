using UnityEngine;
using System.Collections.Generic;

// Color status of a tile
public enum TileStatus
{
    HORIZONTAL = -1,
    EMPTY = 0,
    VERTICAL = 1
};

public struct transpoTable
{
    public bool exactScore;
};

public class AI : MonoBehaviour
{
    private static int SIZE = 8;
    const int INFINITY = 1000000;
    int maxDepth = 4;
    List<int> killer;
    
    private TileStatus[] logicBoard = null;
    
    public void SetTileValue(int index, TileStatus val)
    {
        logicBoard[index] = val;
    }
    public TileStatus GetTileValue(int index)
    {
        return logicBoard[index];
    }

    // Initialization of status grid of the board
    // ATTENTION: board initialized with (0,0) at top left corner
    //            and (N, N) at bottom right corner
    public void InitLogicBoard(int N, int difficulty)
    {
        SIZE = N;
        maxDepth = difficulty;
        initKillerList(difficulty);
        logicBoard = new TileStatus[N * N];
        for (int i = 0; i < N; i++)
            for (int j = 0; j < N; j++)
                logicBoard[i * N + j] = TileStatus.EMPTY;
        initHistoryTable();
    }

    // Retourner l'indice du deuxième carreau (horizontal ou vertical)
    public int getNextIndex(int index, TileStatus turn)
    {
        if (index < 0)
            return -1;
        if (turn == TileStatus.VERTICAL)
        {
            if (index / SIZE >= SIZE - 1)
                return index - SIZE;
            return index + SIZE;
        }
        if (index % SIZE >= SIZE - 1)
            return index - 1;
        return index + 1;
    }

    // Test and return the row if the column is movable
    public bool Movable(int id1, int id2)
    {
        return logicBoard[id1] == TileStatus.EMPTY && logicBoard[id2] == TileStatus.EMPTY;    
    }

    public List<int> possibleMoves(TileStatus turn)
    {
        List<int> moves = new List<int>();
        int i, j;
        //count++;
        if (turn == TileStatus.VERTICAL)
        {
            for (i = 0; i < SIZE - 1; i++)
                for (j = 0; j < SIZE; j++)
                    if (logicBoard[i * SIZE + j] == TileStatus.EMPTY && logicBoard[i * SIZE + j + SIZE] == TileStatus.EMPTY)
                        moves.Add(i * SIZE + j);
        }
        else
            for (i = 0; i < SIZE; i++)
                for (j = 0; j < SIZE - 1; j++)
                    if (logicBoard[i * SIZE + j] == TileStatus.EMPTY && logicBoard[i * SIZE + j + 1] == TileStatus.EMPTY)
                        moves.Add(i * SIZE + j);
        return moves;
    }

    int boardEvaluation(TileStatus turn)
    {
        List<int> vertiMoves = possibleMoves(turn);
        List<int> horiMoves = possibleMoves((TileStatus)(-(int)turn));
        return vertiMoves.Count - horiMoves.Count;
    }

    public int NegaMax(TileStatus turn, int depth, out int move)
    {
        move = -1;
        //if (depth == maxDepth)
            //count = 0;
        // Condition d'arrêt
        if (depth == 0)
            return boardEvaluation(turn);
        int eval = -INFINITY;
        List<int> moves = possibleMoves(turn);

        int bestMove = -1;
        foreach (int m in moves)
        {
            int nextIndex = getNextIndex(m, turn);
            // Jouer le coup
            logicBoard[m] = turn;
            logicBoard[nextIndex] = turn;
            int e = -NegaMax((TileStatus)(-(int)turn), depth - 1, out bestMove);
            // Déjouer le coup
            logicBoard[m] = TileStatus.EMPTY;
            logicBoard[nextIndex] = TileStatus.EMPTY;
            if (e >= eval)
            {
                eval = e;
                move = m;
            }
        }
       // if (depth == maxDepth)
        //{
         //   Debug.Log("HERE" + count);
           // count = 0;
        //}
        return eval;
    }

    public int abNegaMax(TileStatus turn, int depth, int alpha, int beta, out int move)
    {
        move = -1;
        // Condition d'arrêt
        if (depth == 0)
            return boardEvaluation(turn);
        List<int> moves = possibleMoves(turn);
        int bestMove = -1;
        if (moves.Count > 0)
            move = moves[0];
        foreach (int m in moves)
        {
            int nextIndex = getNextIndex(m, turn);
            // Jouer le coup
            logicBoard[m] = turn;
            logicBoard[nextIndex] = turn;
            int e = -abNegaMax((TileStatus)(-(int)turn), depth - 1, -beta, -alpha, out bestMove);
            // Déjouer le coup
            logicBoard[m] = TileStatus.EMPTY;
            logicBoard[nextIndex] = TileStatus.EMPTY;
            if (e > alpha)
            {
                alpha = e;
                move = m;
                if (alpha >= beta)
                {
                    move = m;
                    return beta;
                }
            }
        }
        return alpha;
    }

    public void initKillerList(int difficulty)
    {
        maxDepth = difficulty;
        killer = new List<int>();
        for (int i = 0; i < difficulty; i++)
            killer.Add(-1);
        Debug.Log("Reset killer Count = " + killer.Count);
    }

    //List<int> history;

    public int abNegaMax_HeurTueur(TileStatus turn, int depth, int alpha, int beta, out int move)
    {
        move = -1;
        // Condition d'arrêt
        if (depth == 0)
            return boardEvaluation(turn);
        List<int> moves = possibleMoves(turn);
        int index = moves.FindIndex(m => m == killer[maxDepth - depth]);
        if (index >= 0)
        {
            int temp = moves[index];
            moves[index] = moves[0];
            moves[0] = temp;
            //Debug.Log("YES");
        }
        int bestMove = -1;
        if (moves.Count > 0)
            move = moves[0];
        foreach (int m in moves)
        {
            int nextIndex = getNextIndex(m, turn);
            // Jouer le coup
            logicBoard[m] = turn;
            logicBoard[nextIndex] = turn;
            int e = -abNegaMax_HeurTueur((TileStatus)(-(int)turn), depth - 1, -beta, -alpha, out bestMove);
            // Déjouer le coup
            logicBoard[m] = TileStatus.EMPTY;
            logicBoard[nextIndex] = TileStatus.EMPTY;
            if (e > alpha)
            {
                alpha = e;
                move = m;
                //killer[maxDepth - depth] = m;
                if (alpha >= beta)
                {
                    killer[maxDepth - depth] = m;
                    move = m;
                    return beta;
                }
            }
        }
        return alpha;
    }

    Dictionary<int, int> history;
    public void initHistoryTable()
    {
        history = new Dictionary<int, int>();
        for (int i = 0; i < logicBoard.Length; i++)
        {
            history.Add(i, 0);
        }
    }

    public int abNegaMax_HeurHistory(TileStatus turn, int depth, int alpha, int beta, out int move)
    {
        move = -1;
        // Condition d'arrêt
        if (depth == 0)
            return boardEvaluation(turn);
        List<int> moves = possibleMoves(turn);
        List<int> rating = new List<int>();
        foreach (int m in moves)
        {
            rating.Add(history[m]);
        }
        int nbMoves = rating.Count;
        
        for (int i = 0; i < nbMoves - 1; i++)
            for (int j = i + 1; j < nbMoves; j++)
                if (rating[j] > rating[i])
                {
                    int r = rating[j];
                    rating[j] = rating[i];
                    rating[i] = r;
                    int m = moves[j];
                    moves[j] = moves[i];
                    moves[i] = m;
                }

        int bestMove = -1;
        if (moves.Count > 0)
            move = moves[0];
        foreach (int m in moves)
        {
            int nextIndex = getNextIndex(m, turn);
            // Jouer le coup
            logicBoard[m] = turn;
            logicBoard[nextIndex] = turn;
            int e = -abNegaMax_HeurHistory((TileStatus)(-(int)turn), depth - 1, -beta, -alpha, out bestMove);
            // Déjouer le coup
            logicBoard[m] = TileStatus.EMPTY;
            logicBoard[nextIndex] = TileStatus.EMPTY;
            if (e > alpha)
            {
                alpha = e;
                move = m;
                //killer[maxDepth - depth] = m;
                if (alpha >= beta)
                {
                    history[m] += (int)Mathf.Pow(4, depth);
                    move = m;
                    return beta;
                }
            }
        }
        return alpha;
    }


    /*public int MiniMax(TileStatus turn, int depth, out int move)
    {
        move = -1;
        // Condition d'arrêt
        if (depth == 0)
            return boardEvaluation(turn);
        int eval = turn == TileStatus.VERTICAL ? -INFINITY : INFINITY;
        List<int> moves = possibleMoves(turn);
        if (moves.Count > 0)
            move = moves[0];
        int bestMove = -1;
        foreach (int m in moves)
        {
            int nextIndex = getNextIndex(m, turn);
            // Jouer le coup
            logicBoard[m] = turn;
            logicBoard[nextIndex] = turn;
            int e = MiniMax((TileStatus)(-(int)turn), depth - 1, out bestMove);
            // Déjouer le coup
            logicBoard[m] = TileStatus.EMPTY;
            logicBoard[nextIndex] = TileStatus.EMPTY;
            if ((turn == TileStatus.VERTICAL && e >= eval)
                || (turn == TileStatus.HORIZONTAL && e <= eval))
            {
                eval = e;
                move = m;
            }
        }
        return eval;
    }*/
}