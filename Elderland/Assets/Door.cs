using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Door : MonoBehaviour
{
    public enum State { Open, Closed }

    [SerializeField]
    private State state;
    [SerializeField]
    private float liftHeight;
    [SerializeField]
    private float liftSpeed;

    private Vector3 downPosition;

    private void Awake()
    {
        if (state == State.Open)
        {
            downPosition = transform.position - transform.up * (liftHeight);
        }
        else
        {
            downPosition = transform.position;
        }
    }   

    public void Open()
    {
        if (state == State.Closed)
        {
            StopCoroutine("CloseCoroutine");
            StartCoroutine("OpenCoroutine");
            state = State.Open;
        }
    }

    public void Close()
    {
        if (state == State.Open)
        {
            StopCoroutine("OpenCoroutine");
            StartCoroutine("CloseCoroutine");
            state = State.Closed;
        }
    }

    public void OpenInstantely()
    {
        if (state == State.Closed)
        {
            Vector3 openPosition = downPosition + transform.up * (liftHeight);
            transform.position = openPosition;
            state = State.Open;
        }
    }

    public void CloseInstantely()
    {
        if (state == State.Open)
        {
            Vector3 closedPosition = downPosition;
            transform.position = closedPosition;
            state = State.Closed;
        }
    }

    private IEnumerator OpenCoroutine()
    {
        while (true)
        {
            Vector3 closedPosition = transform.position;
            Vector3 openPosition = downPosition + transform.up * (liftHeight);
            Vector3 incremented = Vector3.MoveTowards(closedPosition, openPosition, liftSpeed * Time.deltaTime);
            transform.position = incremented;
            if (Vector3.Distance(closedPosition, openPosition) < 0.05f)
            {
                break;
            }
            else
            {
                yield return new WaitForEndOfFrame();
            }
        }
    }

    private IEnumerator CloseCoroutine()
    {
        while (true)
        {
            Vector3 openPosition = transform.position;
            Vector3 closedPosition = downPosition;
            Vector3 incremented = Vector3.MoveTowards(openPosition, closedPosition, liftSpeed * Time.deltaTime);
            transform.position = incremented;
            if (Vector3.Distance(openPosition, closedPosition) < 0.05f)
            {
                break;
            }
            else
            {
                yield return new WaitForEndOfFrame();
            }
        }
    }
}
