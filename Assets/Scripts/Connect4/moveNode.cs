using UnityEngine;
using System.Collections.Generic;

public class moveNode{
    public Connect4State connect4State;

    public moveNode prevNode;
    public int move;

    public float score;

    public int visitCount;

    public List<moveNode> children;
    public List<int> untriedMoves;

    public moveNode(Connect4State connect4State, moveNode prevNode, int move, List<int> untriedMoves)
    {
        this.connect4State = connect4State;
        this.prevNode = prevNode;
        this.move = move;
        this.untriedMoves = untriedMoves;
        score = 0.0f;
        visitCount = 0;
        children = new List<moveNode>();
    }

}