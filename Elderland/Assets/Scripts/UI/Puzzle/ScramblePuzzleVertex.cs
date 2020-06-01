using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScramblePuzzleVertex : PuzzleVertex
{
    [SerializeField]
    private bool scramble;

    public bool Scramble { get { return scramble;} }
}
