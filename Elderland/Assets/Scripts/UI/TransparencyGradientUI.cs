using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class TransparencyGradientUI : MonoBehaviour
{
    public enum State { On, Off }

    [SerializeField]
    private State state;
    [SerializeField]
    private float gradientSpeed;
    [SerializeField]
    private UnityEvent onTurnOnComplete;
    [SerializeField]
    private UnityEvent onTurnOffComplete;

    private MaskableGraphic graphic;

    private void Awake()
    {
        graphic = GetComponent<MaskableGraphic>();
    }

    public void TurnOn()
    {
        if (state == State.Off)
        {
            StopCoroutine("TurnOffCoroutine");
            StartCoroutine("TurnOnCoroutine");
            state = State.On;
        }
    }

    public void TurnOff()
    {
        if (state == State.On)
        {
            StopCoroutine("TurnOnCoroutine");
            StartCoroutine("TurnOffCoroutine");
            state = State.Off;
        }
    }

    private IEnumerator TurnOnCoroutine()
    {
        while (true)
        {
            float currentValue = graphic.color.a;
            float onValue = 1;
            float incrementedValue = Mathf.MoveTowards(currentValue, onValue, gradientSpeed * Time.deltaTime);
            graphic.color = new Color(graphic.color.r, graphic.color.g, graphic.color.b, incrementedValue);
            if (Mathf.Abs(currentValue - onValue) < 0.05f)
            {
                if (onTurnOnComplete != null)
                    onTurnOnComplete.Invoke();
                graphic.color = new Color(graphic.color.r, graphic.color.g, graphic.color.b, onValue);
                break;
            }
            else
            {
                yield return new WaitForEndOfFrame();
            }
        }
    }

    private IEnumerator TurnOffCoroutine()
    {
        while (true)
        {
            float currentValue = graphic.color.a;
            float offValue = 0;
            float incrementedValue = Mathf.MoveTowards(currentValue, offValue, gradientSpeed * Time.deltaTime);
            graphic.color = new Color(graphic.color.r, graphic.color.g, graphic.color.b, incrementedValue);
            if (Mathf.Abs(currentValue - offValue) < 0.05f)
            {
                if (onTurnOffComplete != null)
                    onTurnOffComplete.Invoke();
                graphic.color = new Color(graphic.color.r, graphic.color.g, graphic.color.b, offValue);
                break;
            }
            else
            {
                yield return new WaitForEndOfFrame();
            }
        }
    }
}
