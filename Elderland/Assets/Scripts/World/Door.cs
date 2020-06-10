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
    [SerializeField]
    private UnityEvent onOpenEnd;

    private enum Type { OpenDownwards = -1, OpenUpwards = 1, }

    private Vector3 closedPosition;

    private void Awake()
    {
        if (state == State.Open)
        {
            closedPosition = 
                transform.localPosition - transform.up * (liftHeight) * (int) type;
        }
        else
        {
            closedPosition = transform.localPosition;
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
            transform.localPosition = openPosition;
            state = State.Open;
        }
    }

    public void CloseInstantely()
    {
        if (state == State.Open)
        {
            transform.localPosition = closedPosition;
            state = State.Closed;
        }
    }

    private IEnumerator OpenCoroutine()
    {
        while (true)
        {
            Vector3 currentPosition = transform.localPosition;
            Vector3 openPosition = 
                this.closedPosition + transform.up * (liftHeight) * (int) type;
            Vector3 incremented = Vector3.MoveTowards(currentPosition, openPosition, liftSpeed * Time.deltaTime);
            transform.localPosition = incremented;
            if (Vector3.Distance(currentPosition, openPosition) < 0.05f)
            {
                transform.localPosition = openPosition;
                if (onOpenEnd != null)
                    onOpenEnd.Invoke();
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
            Vector3 openPosition = transform.localPosition;
            Vector3 currentPosition = closedPosition;
            Vector3 incremented = Vector3.MoveTowards(openPosition, currentPosition, liftSpeed * Time.deltaTime);
            transform.localPosition = incremented;
            if (Vector3.Distance(openPosition, currentPosition) < 0.05f)
            {
                transform.localPosition = currentPosition;
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
