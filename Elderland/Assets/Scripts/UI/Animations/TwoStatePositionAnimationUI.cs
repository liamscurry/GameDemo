using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TwoStatePositionAnimationUI : MonoBehaviour
{
    [SerializeField]
    private Vector3 stateTwoOffset;
    [SerializeField]
    private float speed;

    private enum State { One, Two };

    private State currentState;
    private Vector3 stateOnePosition;
    private Vector3 stateTwoPosition;

    private void Awake()
    {
        currentState = State.One;

        stateOnePosition =
            transform.position;
        stateTwoPosition =
            transform.position + Matrix4x4.TRS(Vector3.zero, transform.rotation, Vector3.one).MultiplyPoint(stateTwoOffset);
    }

    public void ToStateOne()
    {
        if (currentState == State.Two)
        {
            currentState = State.One;
            StartCoroutine(MoveToStateOne());
        }
    }

    public void ToStateTwo()
    {
        if (currentState == State.One)
        {
            currentState = State.Two;
            StartCoroutine(MoveToStateTwo());
        }
    }

    private IEnumerator MoveToStateOne()
    {
        while (true)
        {
            yield return new WaitForFixedUpdate();
            
            transform.position = Vector3.MoveTowards(transform.position, stateOnePosition, speed * Time.deltaTime);
            
            if ((transform.position - stateOnePosition).magnitude < 0.05f)
            {
                transform.position = stateOnePosition;
                break;
            }
            else
            {
                yield return new WaitForEndOfFrame();
            }
        }
    }

    private IEnumerator MoveToStateTwo()
    {
        while (true)
        {
            yield return new WaitForEndOfFrame();
            
            transform.position = Vector3.MoveTowards(transform.position, stateTwoPosition, speed * Time.deltaTime);

            if ((transform.position - stateTwoPosition).magnitude < 0.05f)
            {
                transform.position = stateTwoPosition;
                break;
            }
            else
            {
                yield return new WaitForEndOfFrame();
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Vector3 stateTwoPosition =
            transform.position + Matrix4x4.TRS(Vector3.zero, transform.rotation, Vector3.one).MultiplyPoint(stateTwoOffset);

        Gizmos.color = Color.black;
        Gizmos.DrawLine(transform.position, stateTwoPosition);
        Gizmos.DrawCube(transform.position, Vector3.one * 0.25f);
        Gizmos.DrawCube(stateTwoPosition, Vector3.one * 0.25f);
    }
}
