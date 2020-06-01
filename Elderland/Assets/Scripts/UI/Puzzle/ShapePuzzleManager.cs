using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public sealed class ShapePuzzleManager : PuzzleManager
{
    protected override void UpdateInput()
    {
        PuzzleDirection pressedDirection = PuzzleDirection.Up;
        bool pressed = false;
        if (Input.GetKeyDown(upKeycode))
        {
            pressedDirection = PuzzleDirection.Up;
            pressed = true;
        }
        else if (Input.GetKeyDown(rightKeycode))
        {
            pressedDirection = PuzzleDirection.Right;
            pressed = true;
        }
        else if (Input.GetKeyDown(downKeycode))
        {
            pressedDirection = PuzzleDirection.Down;
            pressed = true;
        }
        else if (Input.GetKeyDown(leftKeycode))
        {
            pressedDirection = PuzzleDirection.Left;
            pressed = true;
        }

        if (pressed)
        {
            ShapePuzzleVertex shapeCurrentVertex = 
                (ShapePuzzleVertex) currentVertex;
            if (pressedDirection == currentVertex.SolutionDirection &&
                ((Input.GetAxis("Left Trigger") < -0.2f &&
                shapeCurrentVertex.ShapeType == ShapePuzzleVertexType.Lower) ||
                (Input.GetAxis("Right Trigger") < -0.2f &&
                shapeCurrentVertex.ShapeType == ShapePuzzleVertexType.Upper) ||
                (Input.GetAxis("Left Trigger")  >= -0.2f &&
                 Input.GetAxis("Right Trigger") >= -0.2f &&
                shapeCurrentVertex.ShapeType == ShapePuzzleVertexType.Center)))
            {
                AdvanceVertex();
            }
            else
            {
                Reset();
            }
        }
    }
}
