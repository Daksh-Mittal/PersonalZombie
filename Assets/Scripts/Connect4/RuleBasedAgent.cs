// using UnityEngine;
// using System.Collections.Generic;


// public class RuleBasedAgent : Agent
// {
//     public override int GetMove(Connect4State state)
//     {
//         List<int> moves = state.GetPossibleMoves();

//         // just in case there are no moves available
//         if (moves.Count == 0)
//             return 0;

//         // first check if i can win right now
//         // no point doing anything else if i can just win
//         int winMove = CheckIfICanWin(state, moves, playerIdx);
//         if (winMove != -1)
//             return winMove;

//         // if i cant win, check if the opponent can win next turn
//         // if they can i need to block them
//         int opponentIdx = (playerIdx == 0) ? 1 : 0;
//         int blockMove = CheckIfICanWin(state, moves, opponentIdx);
//         if (blockMove != -1)
//             return blockMove;

//         // check if i can make a fork
//         // a fork is when i have two ways to win at the same time
//         // the opponent cant block both so i will win
//         int forkMove = TryToFork(state, moves);
//         if (forkMove != -1)
//             return forkMove;

//         // try to get 3 in a row
//         int threeMove = TryToBuild(state, moves, 3);
//         if (threeMove != -1)
//             return threeMove;

//         // if i cant get 3 in a row at least try for 2
//         int twoMove = TryToBuild(state, moves, 2);
//         if (twoMove != -1)
//             return twoMove;

//         // i read online that centre columns are better in connect4
//         // so if nothing else works just go for the middle
//         int[] goodColumns = { 3, 2, 4, 1, 5, 0, 6 };
//         foreach (int col in goodColumns)
//         {
//             if (moves.Contains(col))
//                 return col;
//         }

//         // honestly shouldnt reach here but just in case
//         return moves[Random.Range(0, moves.Count)];
//     }

//     // this checks if a player can win in one move
//     // i just try every column and see if it wins
//     private int CheckIfICanWin(Connect4State state, List<int> moves, int whoIsPlaying)
//     {
//         foreach (int col in moves)
//         {
//             // make a copy so i dont mess up the real game
//             Connect4State testState = state.Clone();
//             testState.MakeMove(col);

//             Connect4State.Result result = testState.GetResult();

//             // check if red won
//             if (whoIsPlaying == 1 && result == Connect4State.Result.RedWin)
//                 return col;

//             // check if yellow won
//             if (whoIsPlaying == 0 && result == Connect4State.Result.YellowWin)
//                 return col;
//         }

//         // no winning move found
//         return -1;
//     }

//     // this tries to find a fork move
//     // basically i play a move and then count how many ways i can win after that
//     // if i have 2 or more ways to win its a fork
//     private int TryToFork(Connect4State state, List<int> moves)
//     {
//         foreach (int col in moves)
//         {
//             Connect4State testState = state.Clone();
//             testState.MakeMove(col);

//             // now count how many winning moves i have from here
//             List<int> nextMoves = testState.GetPossibleMoves();
//             int waysToWin = 0;

//             foreach (int nextCol in nextMoves)
//             {
//                 Connect4State testState2 = testState.Clone();
//                 testState2.MakeMove(nextCol);

//                 Connect4State.Result result = testState2.GetResult();

//                 if (playerIdx == 1 && result == Connect4State.Result.RedWin)
//                     waysToWin++;

//                 if (playerIdx == 0 && result == Connect4State.Result.YellowWin)
//                     waysToWin++;
//             }

//             // if i have 2 or more winning moves its a fork
//             if (waysToWin >= 2)
//                 return col;
//         }

//         // couldnt find a fork
//         return -1;
//     }

//     // this tries to build sequences of discs
//     // i score each move by counting how many threats i have after playing it
//     // then pick the move with the best score
//     // i didnt end up using targetLength in the scoring but i left it in
//     // because it makes it clearer what the method is trying to do
//     private int TryToBuild(Connect4State state, List<int> moves, int targetLength)
//     {
//         int bestCol   = -1;
//         int bestScore = 0;

//         foreach (int col in moves)
//         {
//             Connect4State testState = state.Clone();
//             testState.MakeMove(col);

//             // score this move
//             int score = HowManyThreatsDoIHave(testState);

//             if (score > bestScore)
//             {
//                 bestScore = score;
//                 bestCol   = col;
//             }
//         }

//         return bestCol;
//     }

//     // counts how many columns would make me win right now
//     // more threats = better position
//     private int HowManyThreatsDoIHave(Connect4State state)
//     {
//         int threats = 0;
//         List<int> moves = state.GetPossibleMoves();

//         foreach (int col in moves)
//         {
//             Connect4State testState = state.Clone();
//             testState.MakeMove(col);

//             Connect4State.Result result = testState.GetResult();

//             if (playerIdx == 1 && result == Connect4State.Result.RedWin)
//                 threats++;

//             if (playerIdx == 0 && result == Connect4State.Result.YellowWin)
//                 threats++;
//         }

//         return threats;
//     }
// }