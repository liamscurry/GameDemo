using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public sealed class ScrambledPuzzleManager : PuzzleManager
{

    protected override Color[] GetDirectionColors()
    {
        ScramblePuzzleVertex scrambedCurrentVertex =
            (ScramblePuzzleVertex) currentVertex;
        if (scrambedCurrentVertex.Scramble)
        {
            int randomStart = Random.Range(0, 4);
            Color[] scrambledColors = new Color[4];
            for (int i = 0; i < 4; i++)
            {
                scrambledColors[i] = standardColors[(randomStart + i) % 4];
            }
            return scrambledColors;
            //standardColors = 
            //new Color[4] { upColor, rightColor, downColor, leftColor };
        }
        else
        {
            return standardColors;
        }
    }
}
