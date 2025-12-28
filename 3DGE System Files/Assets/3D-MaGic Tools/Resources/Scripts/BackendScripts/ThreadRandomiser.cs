using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Unity.VisualScripting;
using UnityEngine;

[ExecuteInEditMode]
public class ThreadRandomiser
{
    private static ThreadRandomiser _instance;

    // Private constructor to prevent instantiation
    private ThreadRandomiser() { }

    public static ThreadRandomiser Instance
    {
        get
        {
            // If the instance is null, create a new instance
            if (_instance == null)
            {
                _instance = new ThreadRandomiser();
            }
            return _instance;
        }
    }

    List<int> RandomNumbers = new List<int>();
    int RandomNumberIndex = 0;

    public void GenerateRandomNumbers(int _totalRandomNumbers = 100)
    {
        RandomNumberIndex = 0;
        RandomNumbers.Clear();
        for(int i = 0; i < _totalRandomNumbers * 2; i++)
        {
            RandomNumbers.Add(UnityEngine.Random.Range(0, int.MaxValue));
        }
    }

    public int GetRandomNumber(int _threadType)
    {
        int randomReturn = RandomNumbers[(RandomNumberIndex + ((RandomNumbers.Count/4) * _threadType)) % RandomNumbers.Count];
        RandomNumberIndex++;
        return randomReturn;
    }

    // Add your singleton-specific methods and properties here
}


