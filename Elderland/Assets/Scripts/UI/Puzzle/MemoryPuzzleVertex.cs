using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MemoryPuzzleVertex : PuzzleVertex
{
    [SerializeField]
    private int numberOfDisplayIndicators = 0;

    public int NumberOfDisplayIndicators { get { return numberOfDisplayIndicators; } }
}
