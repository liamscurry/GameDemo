using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ShapePuzzleVertexType { Center, Lower, Upper }

public sealed class ShapePuzzleVertex : PuzzleVertex
{
    [SerializeField]
    private ShapePuzzleVertexType shapeType;

    public ShapePuzzleVertexType ShapeType { get { return shapeType; } }
}
