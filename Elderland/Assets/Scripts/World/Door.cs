using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Door : MonoBehaviour
{
    public enum State { Open, Closed }

    [SerializeField]
    private State state;
    [SerializeField]
    private float liftHeight;
    [SerializeField]
    private float liftSpeed;
    [SerializeField]
    private Type type;
    [SerializeField]
    private UnityEvent onCloseEnd;

    private enum Type { OpenDownwards = -1, OpenUpwards = 1, }

    private Vector3 closedPosition;

    private void Awake()
    {
        if (state == State.Open)
        {
            closedPosition = 
                transform.position - transform.up * (liftHeight) * (int) type;
        }
        else
        {
            closedPosition = transform.position;
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
            Vector3 openPosition = 
                closedPosition + transform.up * (liftHeight) * (int) type;
            transform.position = openPosition;
            state = State.Open;
        }
    }

    public void CloseInstantely()
    {
        if (state == State.Open)
        {
            transform.position = closedPosition;
            state = State.Closed;
        }
    }

    private IEnumerator OpenCoroutine()
    {
        while (true)
        {
            Vector3 currentPosition = transform.position;
            Vector3 openPosition = 
                this.closedPosition + transform.up * (liftHeight) * (int) type;
            Vector3 incremented = Vector3.MoveTowards(currentPosition, openPosition, liftSpeed * Time.deltaTime);
            transform.position = incremented;
            if (Vector3.Distance(currentPosition, openPosition) < 0.05f)
            {
                transform.position = openPosition;
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
            Vector3 currentPosition = closedPosition;
            Vector3 incremented = Vector3.MoveTowards(openPosition, currentPosition, liftSpeed * Time.deltaTime);
            transform.position = incremented;
            if (Vector3.Distance(openPosition, currentPosition) < 0.05f)
            {
                transform.position = openPosition;
                if (onCloseEnd != null)
                    onCloseEnd.Invoke();
                break;
            }
            else
            {
                yield return new WaitForEndOfFrame();
            }
        }
    }
}
