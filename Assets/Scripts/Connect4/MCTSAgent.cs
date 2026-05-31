using UnityEngine;
using System.Collections.Generic;


public class MCTSAgent : Agent
{
    public int totalSims = 2500;

    private CSVLogger logger;

    void Start()
    {
        logger = new CSVLogger("MCTS_log.csv");
    }

    public override int GetMove(Connect4State state)
    {
        List<int> moves = state.GetPossibleMoves();

        //Local vairable to keep track of the number of simulations remaining
        int simsRemaining = totalSims;

        //Setting the root node of the tree
        moveNode root = new moveNode(state.Clone(), null, -1, state.GetPossibleMoves());

        //Main while loop to run all the simulations
        while(simsRemaining > 0)
        {
            moveNode currNode = root;
 
            //Selection part
            //Moving down the tree until we reach a leaf node which is a node with no children and no untried moves
            while(currNode.untriedMoves.Count == 0 && currNode.children.Count > 0)
            {
                float bestUCB1 = float.NegativeInfinity;
                moveNode bestNode = null;
 
                foreach(moveNode child in currNode.children)
                {
                    float ucb1 = UCB1(child);
 
                    if(ucb1 > bestUCB1)
                    {
                        bestUCB1 = ucb1;
                        bestNode = child;
                    }
                }
 
                currNode = bestNode;
            }
 
            //Expansion part
            //If the game is still going and the curret node has untried moves
            if (currNode.connect4State.GetResult() == Connect4State.Result.Undecided && currNode.untriedMoves.Count > 0){
 
                //expand the current leaf node, getting one of its children and deleting it frmo untried moves
                int nextMove = currNode.untriedMoves[0];
                currNode.untriedMoves.Remove(nextMove);
 
                //Creating a new node for the move, making the move, and adding the new node as a child of the current node
                Connect4State newState = currNode.connect4State.Clone();
                newState.MakeMove(nextMove);
                moveNode moveNode = new moveNode(newState, currNode, nextMove, newState.GetPossibleMoves());
                currNode.children.Add(moveNode);
                currNode = moveNode;
            }
 
            //Simulating the child node and decrementing the simulation budget
            float score = simulate(currNode);
            --simsRemaining;
 
            //Backpropagation part
            moveNode curr = currNode;
            while(curr != null)
            {
                curr.visitCount++;
                curr.score += score;
 
                score = -score;
 
                curr = curr.prevNode;
            }
        }
        //Getting the best node/move from the root node 
        moveNode bestChild = null;
        int maxVisits = -1;
 
        foreach(moveNode child in root.children)
        {
            if(child.visitCount > maxVisits)
            {
                maxVisits = child.visitCount;
                bestChild = child;
            }
        }

        //logger.Log(bestChild.move, bestChild.score / bestChild.visitCount, bestChild.visitCount);

        return bestChild.move;
    }


    //Funtction so simulate the game from the current node until the end of the game
    public float simulate(moveNode node)
    {
        Connect4State cloneState = node.connect4State.Clone();

        //While the game is still going, keep making random moves
        while(cloneState.GetResult() == Connect4State.Result.Undecided)
            {
                //Getting the list of moves to make from the current state
                List<int> randomMoves = cloneState.GetPossibleMoves();
                //Choosing one randomly
                int randomMove = randomMoves[Random.Range(0, randomMoves.Count)];
                //Making that move on the clone state
                cloneState.MakeMove(randomMove);
            }

        Connect4State.Result result = cloneState.GetResult();
        if (result == Connect4State.Result.Draw) return 0.0f; //Checking for draw

        //Finding the player who made the last move that let to the win
        int playerWhoMoved = (node.connect4State.GetPlayerTurn() == 0) ? 1 : 0;

        if(result == Connect4State.Result.YellowWin)
            return (playerWhoMoved == 0) ? 1f : -1f;
        else if(result == Connect4State.Result.RedWin)
            return (playerWhoMoved == 1) ? 1f : -1f;
        return 0f;

    }

    //Function to calculate the UCB1 value for a node
    public float UCB1(moveNode node)
    {
        if(node.visitCount == 0)
        {
            return float.MaxValue;
        }
        float c = 1.414f;

        return (node.score / node.visitCount) + c * Mathf.Sqrt(Mathf.Log(node.prevNode.visitCount) / node.visitCount);
    }

}
