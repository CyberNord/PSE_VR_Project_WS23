using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Random = UnityEngine.Random;

public class ArrayManager : MonoBehaviour
{
    public static ArrayManager Instance;

    // public GameObject[] vBalls;
    public GameObject[] sockets;
    public TMP_Text arrayText;

    [Header("Swap velocity")] public float moveSpeed = 5.0f;

    private int[] _arrVal;
    
    private Queue<Tuple<int, int>> _swapQueue = new();
    private bool _isSwapping;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        InitializeArray();
    }

    // Updates the Array Value of TMP_Text arrayText on a specific index to an specific value 
    public void UpdateArrayValue(int value, int index)
    {
        _arrVal[index] = value;
        string text = "Current Array \n \n";
        foreach (var t in _arrVal)
        {
            text += "[" + t + "] ";
        }

        arrayText.text = text;
    }
    
    // Updates the Array Value of TMP_Text arrayText by going through every socket
    private void UpdateArray()
    {
        // Update sockets
        for (int i = 0; i < sockets.Length; i++)
        {
            Socket valueSocket = sockets[i].GetComponent<Socket>();
            if (valueSocket.ContainsBall())
            {
                {
                    _arrVal[i] = valueSocket.GetVBall().GetValue();
                }
            }
            else
            {
                _arrVal[i] = -1;
            }
        }

        string text = "Current Array \n \n";
        foreach (var t in _arrVal)
        {
            text += "[" + t + "] ";
        }

        arrayText.text = text;
    }

    void InitializeArray()
    {
        _arrVal = new int[sockets.Length];
        for (int i = 0; i < _arrVal.Length; i++)
        {
            _arrVal[i] = 0;
        }

        arrayText.text = "Current Array \n \n[0] [0] [0]";
    }

    public void TestSwap()
    {
        Debug.Log("testSwap initialized");
        SwapBallPositions(0, 1);
        SwapBallPositions(2, 3);
        SwapBallPositions(4, 5);
        SwapBallPositions(0, 4);
    }

    public void DoBubbleSort()
    {
        List<Tuple<int, int>> swapList = BubbleSortWithSwaps(_arrVal);

        foreach (var t in swapList)
        {
            SwapBallPositions(t.Item1, t.Item2); 
        }
    }
    
    public void ShuffleArray()
    {
        for (int i = 0; i < sockets.Length; i++)
        {
            int randomIndex = Random.Range(0, sockets.Length);
            SwapBallPositions(i, randomIndex);
        }
    }

    private void SwapBallPositions(int indexA, int indexB)
    {
        if (indexA < 0 || indexA >= sockets.Length || indexB < 0 || indexB >= sockets.Length)
        {
            Debug.LogError("Invalid indices for swapping ball positions.");
            return;
        }
        
        // Enqueue the swap request
        _swapQueue.Enqueue(new Tuple<int, int>(indexA, indexB));
        Debug.Log($"Enqueued swap request: {indexA} <-> {indexB}");

        // If not currently swapping, start the coroutine
        if (!_isSwapping)
        {
            StartCoroutine(SequentialSwapCoroutine());
        }
    }
    
    private IEnumerator SequentialSwapCoroutine()
    {
        _isSwapping = true;

        // Process swaps one by one
        while (_swapQueue.Count > 0)
        {
            Tuple<int, int> swapRequest = _swapQueue.Dequeue();
            yield return StartCoroutine(SwapCoroutine(swapRequest.Item1, swapRequest.Item2));
        }

        _isSwapping = false;
    }
    
    private IEnumerator SwapCoroutine(int indexA, int indexB)
    {
        yield return StartCoroutine(MoveBalls(indexA, indexB));

        // Swap sockets 
        (sockets[indexA], sockets[indexB]) = (sockets[indexB], sockets[indexA]);

        UpdateArray(); // Update components after swapping positions
    }

    private IEnumerator MoveBalls(int indexA, int indexB)
    {
        float upA = 0.3f;
        float upB = 0.5f;

        GameObject socketA = sockets[indexA];
        GameObject socketB = sockets[indexB];

        Vector3 originPositionA = socketA.transform.position;
        Vector3 originPositionB = socketB.transform.position;

        Vector3 upPosA = originPositionA + new Vector3(0, upA, 0);
        Vector3 upPosB = originPositionB + new Vector3(0, upB, 0);

        // Move the balls up
        float t = 0f;

        while (t < 1f)
        {
            t += Time.deltaTime * moveSpeed;
            socketA.transform.position = Vector3.Lerp(originPositionA, upPosA, t);
            socketB.transform.position = Vector3.Lerp(originPositionB, upPosB, t);
            yield return new WaitForEndOfFrame();
        }

        Vector3 sidePosA = new Vector3(originPositionB.x, upPosA.y, originPositionB.z);
        Vector3 sidePosB = new Vector3(originPositionA.x, upPosB.y, originPositionA.z);

        // Move sidewards
        t = 0f;

        while (t < 1f)
        {
            t += Time.deltaTime * moveSpeed;
            socketA.transform.position = Vector3.Lerp(upPosA, sidePosA, t);
            socketB.transform.position = Vector3.Lerp(upPosB, sidePosB, t);
            yield return new WaitForEndOfFrame();
        }

        Vector3 downPosA = sidePosA - new Vector3(0, upA, 0);
        Vector3 downPosB = sidePosB - new Vector3(0, upB, 0);

        // Move down
        t = 0f;

        while (t < 1f)
        {
            t += Time.deltaTime * moveSpeed;
            socketA.transform.position = Vector3.Lerp(sidePosA, downPosA, t);
            socketB.transform.position = Vector3.Lerp(sidePosB, downPosB, t);
            yield return new WaitForEndOfFrame();
        }

        // Finally, make sure sockets changed exact place
        socketA.transform.position = originPositionB;
        socketB.transform.position = originPositionA;
    }
    
    // Function to perform bubble sort and return a list of tuples representing swaps
    private List<Tuple<int, int>> BubbleSortWithSwaps(int[] arr)
    {
        List<Tuple<int, int>> swapList = new List<Tuple<int, int>>();
        int n = arr.Length;

        for (int i = 0; i < n - 1; i++)
        {
            for (int j = 0; j < n - i - 1; j++)
            {
                // If the element is greater than the next element, swap them
                if (arr[j] > arr[j + 1])
                {
                    swapList.Add(new Tuple<int, int>(j, j + 1));
                    (arr[j], arr[j + 1]) = (arr[j + 1], arr[j]);
                }
            }
        }
        return swapList;
    }
}