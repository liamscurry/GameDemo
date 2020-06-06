using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public sealed class MemoryPuzzleManager : PuzzleManager
{
    [SerializeField]
    private GameObject indicatorsPrefab;
    [SerializeField]
    private GameObject activeVertexParent;
    [SerializeField]
    private Color blankIndicatorColor;

    private bool initializedIndicators;

    private List<RawImage> indicators;
    private Vector3 startScale;

    protected override void Start()
    {
        standardColors = 
            new Color[4] { upColor, rightColor, downColor, leftColor };
        deactivatedColors = 
            new Color[4] { new Color(0 ,0, 0, 0), new Color(0 ,0, 0, 0),
                           new Color(0 ,0, 0, 0), new Color(0 ,0, 0, 0) };
        baseBackgroundColor = background.color;
        //Reset();
        solved = false;
        enabled = false;

        initializedIndicators = false;
        indicators = new List<RawImage>();
        Disable();
    }

    private void GenerateIndicators()
    {
        startScale = indicatorsPrefab.transform.localScale;
        indicators.Add(indicatorsPrefab.GetComponent<RawImage>());

        // initialize...
        MemoryPuzzleVertex[] activeVertexes =
            activeVertexParent.GetComponentsInChildren<MemoryPuzzleVertex>();

        int maxIndicators = 1;

        foreach (MemoryPuzzleVertex vertex in activeVertexes) // getting called correct number of times.
        {
            if (vertex.NumberOfDisplayIndicators > maxIndicators)
            {
                maxIndicators = vertex.NumberOfDisplayIndicators;
            }
        }

        for (int i = 0; i < maxIndicators - 1; i++)
        {
            GameObject indicator = 
                Instantiate(indicatorsPrefab, Vector3.zero, Quaternion.identity) as GameObject;
            indicator.transform.SetParent(indicatorsPrefab.transform.parent);
            indicator.transform.localPosition = Vector3.zero;
            indicator.transform.localScale = indicatorsPrefab.transform.localScale;
            indicators.Add(indicator.GetComponent<RawImage>()); // correctly populated.
        }
    }

    public override void Enable()
    {
        enabled = true;
        
        if (!initializedIndicators)
        {
            GenerateIndicators();
            initializedIndicators = true;
        }

        Reset();
    }

    protected override void UpdateSelected(Color[] directionColors)
    {
        selectedObject.position = currentVertex.transform.position;
        highlightObject.position = currentVertex.transform.position;
        indicatorTimer.value = 1;

        float[] alphas = currentVertex.GetAlphas();

        for (int i = 0; i < 4; i++)
        {
            edges[i].color = directionColors[i] * alphas[i];
        }

        UpdateIndicators(directionColors);
    }

    private void UpdateIndicators(Color[] directionColors) // working on currently
    {
        MemoryPuzzleVertex memoryCurrentVertex = 
            (MemoryPuzzleVertex) currentVertex;

        int numberOfActivatedIndicators = 
            memoryCurrentVertex.NumberOfDisplayIndicators;

        if (numberOfActivatedIndicators > 0)
        {
            for (int i = 0; i < numberOfActivatedIndicators; i++)
            {
                float positionI = i;

                if (numberOfActivatedIndicators == 1)
                    positionI = .5f;

                indicators[i].color = GetSolutionColor(directionColors, memoryCurrentVertex);
                indicators[i].transform.localScale = Vector3.one;
                ((RectTransform) indicators[i].transform).localPosition = new Vector3(positionI * 1f / numberOfActivatedIndicators * startScale.x, 0, 0);
                indicators[i].transform.localRotation = Quaternion.identity;
                indicators[i].transform.localScale =
                    new Vector3(1f / numberOfActivatedIndicators * startScale.x * .90f, startScale.y, startScale.z);
                memoryCurrentVertex = 
                    (MemoryPuzzleVertex) memoryCurrentVertex.SolutionVertex;
            }

            int numberOfDeactivatedIndicators = 
                indicators.Count - numberOfActivatedIndicators;

            for (int i = 0; i < numberOfDeactivatedIndicators; i++)
            {
                indicators[i + numberOfActivatedIndicators].color = new Color(0, 0, 0, 0);
            }    
        }
        else
        {
            Color offColor = (currentVertex.SolutionVertex == null) ? new Color(0, 0, 0, 0) : blankIndicatorColor;
            for (int i = 0; i < indicators.Count; i++)
            {
                indicators[i].color = offColor;
            }
        }
    }

    private Color GetSolutionColor(Color[] directionColors, MemoryPuzzleVertex memoryCurrentVertex)
    {
        switch (memoryCurrentVertex.SolutionDirection)
        {
            case PuzzleDirection.Up:
                return directionColors[0];
            case PuzzleDirection.Right:
                return directionColors[1];
            case PuzzleDirection.Down:
                return directionColors[2];
            case PuzzleDirection.Left:
                return directionColors[3];
            default:
                throw new System.Exception("Not a puzzle direction");
        }
    }
}
