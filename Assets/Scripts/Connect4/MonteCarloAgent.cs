// using UnityEngine;
// using System.Collections.Generic;

// public class MonteCarloAgent : Agent
// {
//     public int totalSims = 2500;
//     private CSVLogger logger;

//     void Start()
//     {
//         logger = new CSVLogger("MCS_log.csv");
//     }

//     public override int GetMove(Connect4State state)
//     {


//         //Getting the available moves
//         List<int> moves = state.GetPossibleMoves();

//         //Splitting the simulations budget equally between the moves
//         int moveSims = totalSims / moves.Count;

//         float[] moveScores = new float[moves.Count];

//         for(int i = 0; i < moves.Count; i++)
//         {
//             float totalScore = 0.0f;


//             for(int j = 0; j < moveSims; j++)
//             {
//                 //Getting clones state
//                 Connect4State cloneState = state.Clone();

//                 //Making the current move being assessed
//                 cloneState.MakeMove(moves[i]);

//                 //Make a bunch of random moves until the game ends, keeping track of the score at the end of the game (1 for win, 0 for loss, 0.5 for draw)
//                 while(cloneState.GetResult() == Connect4State.Result.Undecided)
//                 {
//                     //Getting the lits of moves to make from the current state
//                     List<int> randomMoves = cloneState.GetPossibleMoves();

//                     //Choosing one randomly
//                     int randomMove = randomMoves[Random.Range(0, randomMoves.Count)];

//                     //Making that move on the clone state
//                     cloneState.MakeMove(randomMove);
//                 }

//                 //Getting the result of the current simulation and adding the score to the total score for the current move
//                 totalScore += Connect4State.ResultToFloat(cloneState.GetResult());
//             }

//         // Averaging the score for the current move
//         moveScores[i] = totalScore / moveSims; 
//         }

//         // Finding the move with the highest average score
//         int bestMove = 0;
//         for(int i = 1; i < moveScores.Length; i++)
//         {
//             if(moveScores[i] > moveScores[bestMove])
//             {
//                 bestMove = i;
//             }
//         }

//         //logger.Log(moves[argMax(moveScores)], moveScores[argMax(moveScores)]);

//         return (playerIdx == 1) ? moves[argMax(moveScores)] : moves[argMin(moveScores)];
//     }
// }
