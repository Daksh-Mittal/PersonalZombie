using UnityEngine;
using System.Linq;

public abstract class Agent : MonoBehaviour
{
    protected int playerIdx;

    public void setPlayerIdx(int _playerIdx)
    {
        playerIdx = _playerIdx;
    }
    public abstract Vector2 GetMove();

    public int argMin(float[] arr)
    {
        return Enumerable.Range(0, arr.Length).Aggregate((a, b) => (arr[a] < arr[b]) ? a : b);
    }

    public int argMax(float[] arr)
    {
        return Enumerable.Range(0, arr.Length).Aggregate((a, b) => (arr[a] > arr[b]) ? a : b);
    }
}