using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class PuzzleManager : MonoBehaviour
{
    [SerializeField]
    protected PuzzleVertex startVertex;
    [SerializeField]
    protected PuzzleVertex endVertex;
    [SerializeField]
    protected RectTransform highlightObject;
    [SerializeField]
    protected Transform selectedObject;
    [SerializeField]
    protected Slider indicatorTimer;
    [SerializeField]
    protected RawImage indicator;
    [SerializeField]
    protected Transform edgesParent;
    [SerializeField]
    protected RawImage[] edges; // up = 0, right = 1, down = 2, left = 3
    [SerializeField]
    protected RawImage background;
    [SerializeField]
    protected Color upColor;
    [SerializeField]
    protected Color rightColor;
    [SerializeField]
    protected Color downColor;
    [SerializeField]
    protected Color leftColor;
    [SerializeField]
    protected Color solvedColor;
    [SerializeField]
    protected float timePerVertex;
    [SerializeField]
    protected UnityEvent onExit;
    [SerializeField]
    protected UnityEvent onSolve;

    protected PuzzleVertex currentVertex;

    protected Color[] standardColors;
    protected Color[] deactivatedColors;
    protected Color baseBackgroundColor;

    protected const KeyCode upKeycode =    KeyCode.Joystick1Button3;
    protected const KeyCode rightKeycode = KeyCode.Joystick1Button1;
    protected const KeyCode downKeycode =  KeyCode.Joystick1Button0;
    protected const KeyCode leftKeycode =  KeyCode.Joystick1Button2;
    protected const KeyCode exitKeycode =  KeyCode.Joystick1Button6;

    protected bool solved;

    protected virtual void Start()
    {
        standardColors = 
            new Color[4] { upColor, rightColor, downColor, leftColor };
        deactivatedColors = 
            new Color[4] { new Color(0 ,0, 0, 0), new Color(0 ,0, 0, 0),
                           new Color(0 ,0, 0, 0), new Color(0 ,0, 0, 0) };
        baseBackgroundColor = background.color;
        Reset();
        solved = false;
        enabled = false;
    }

    protected void Update()
    {
        if (!solved)
        {
            UpdateInput();

            if (currentVertex != null && currentVertex.SolutionVertex != null)
            {
                indicatorTimer.value -= Time.deltaTime / timePerVertex;
                if (indicatorTimer.value <= 0)
                {
                    Reset();
                }
            }
            
            CheckForExit();
        }
    }

    public virtual void Enable()
    {
        enabled = true;
        Reset();
    }

    public void Disable()
    {
        enabled = false;
    }

    public void ExitPuzzle()
    {
        //GameInfo.Manager.OverlayFreezeInput();
        //GameInfo.Manager.OverlayUnfreezeInput();
        //GameInfo.CameraController.StartGameplay();
        if (onExit != null)
            onExit.Invoke();
        Disable();
    }

    protected virtual void UpdateInput()
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
            if (pressedDirection == currentVertex.SolutionDirection)
            {
                AdvanceVertex();
            }
            else
            {
                Reset();
            }
        }
    }

    protected void CheckForExit()
    {
        if (Input.GetKeyDown(exitKeycode))
        {
            ExitPuzzle();
        }
    }

    public void Reset()
    {
        solved = false;
        highlightObject.gameObject.SetActive(true);
        currentVertex = startVertex;
        background.color = baseBackgroundColor;
        UpdateSelected(GetDirectionColors());
    }

    protected void AdvanceVertex()
    {
        currentVertex = currentVertex.SolutionVertex;
        if (currentVertex.SolutionVertex == null)
        {
            // completed puzzle.
            solved = true;
            background.color = solvedColor;
            highlightObject.gameObject.SetActive(false);
            
            if (onSolve != null)
                onSolve.Invoke();
            Disable();
            
            UpdateSelected(deactivatedColors);
            indicatorTimer.value = 0;

            Disable();
        }
        else
        {
            UpdateSelected(GetDirectionColors());
        }
    }

    protected virtual void UpdateSelected(Color[] directionColors)
    {
        selectedObject.position = currentVertex.transform.position;
        highlightObject.position = currentVertex.transform.position;
        indicatorTimer.value = 1;

        float[] alphas = currentVertex.GetAlphas();

        for (int i = 0; i < 4; i++)
        {
            edges[i].color = directionColors[i] * alphas[i];
        }

        switch (currentVertex.SolutionDirection)
        {
            case PuzzleDirection.Up:
                indicator.color = directionColors[0];
                break;
            case PuzzleDirection.Right:
                indicator.color = directionColors[1];
                break;
            case PuzzleDirection.Down:
                indicator.color = directionColors[2];
                break;
            case PuzzleDirection.Left:
                indicator.color = directionColors[3];
                break;
            default:
                throw new System.Exception("Not a puzzle direction");
        }
    }

    protected virtual Color[] GetDirectionColors()
    {
        return standardColors;
    }
}
