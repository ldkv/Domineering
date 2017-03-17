using UnityEngine;
using System.Collections.Generic;

// Color status of a tile
public enum TileStatus
{
    HORIZONTAL = -1,
    EMPTY = 0,
    VERTICAL = 1
};

public class AI : MonoBehaviour
{
    private static int SIZE = 8;
    const int INFINITY = 1000000;
    int maxDepth = 4;
    
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
        maxDepth = difficulty;
        SIZE = N;
        logicBoard = new TileStatus[N * N];
        for (int i = 0; i < N; i++)
            for (int j = 0; j < N; j++)
                logicBoard[i * N + j] = TileStatus.EMPTY;
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
                    return beta;
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