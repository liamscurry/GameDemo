using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ObjectiveUIManager : MonoBehaviour
{
    [SerializeField]
    private GameObject objectiveContainer;
    [SerializeField]
    private GameObject objective1;
    [SerializeField]
    private GameObject objective2;

    private Vector3 outPosition;
    private RectTransform rectTransform;
    private GameObject mainObjective;
    private GameObject oldMainObjective;

    private bool transitioning;

    private void Awake()
    {
        rectTransform = (RectTransform) transform;
        outPosition = rectTransform.anchoredPosition;
        transitioning = false;
    }

    public void SetMainObjective(GameObject newMainObjective)
    {
        oldMainObjective = mainObjective;
        mainObjective = newMainObjective;
        StopAllCoroutines();
        StartCoroutine(TransitionMainObjectiveCoroutine(0.5f, 4, 0.5f, rectTransform, 0, outPosition.x, null));
    }
    
    public void PingObjectives()
    {
        if (!transitioning)
            StartCoroutine(PingObjectivesCoroutine(0.5f, 4, 0.5f, rectTransform, 0, outPosition.x, null));
    }

    public void AddSideObjective()
    {

    }

    private IEnumerator TransitionCoroutine(
        float duration,
        RectTransform targetTransform,
        float startX,
        float targetX)
    {
        float timer = 0;

        while (timer < duration)
        {
            yield return new WaitForEndOfFrame();
            timer += Time.deltaTime;
            UpdateTransitionPosition(targetTransform, timer / duration, startX, targetX);
        }

        UpdateTransitionPosition(targetTransform, 1f, startX, targetX);
    }

    private IEnumerator PingObjectivesCoroutine(
        float inDuration,
        float pauseDuration,
        float outDuration,
        RectTransform targetTransform,
        float inX,
        float outX,
        UnityEvent inEvent)
    {
        transitioning = true;
        float timer = 0;

        while (timer < inDuration)
        {
            yield return new WaitForEndOfFrame();
            timer += Time.deltaTime;
            UpdateTransitionPosition(targetTransform, timer / inDuration, outPosition.x, 0f);
        }

        UpdateTransitionPosition(targetTransform, 1f, outPosition.x, 0);

        yield return new WaitForSeconds(pauseDuration);

        timer = 0;
        while (timer < outDuration)
        {
            yield return new WaitForEndOfFrame();
            timer += Time.deltaTime;
            UpdateTransitionPosition(targetTransform, timer / outDuration, 0, outPosition.x);
        }

        UpdateTransitionPosition(targetTransform, 1, 0, outPosition.x);
        transitioning = false;
    }

    private IEnumerator TransitionMainObjectiveCoroutine(
        float inDuration,
        float pauseDuration,
        float outDuration,
        RectTransform targetTransform,
        float inX,
        float outX,
        UnityEvent inEvent)
    {
        transitioning = true;

        if (oldMainObjective == null)
            SetInitialMainMissionCoroutine();

        float timer = 0;

        while (timer < inDuration)
        {
            yield return new WaitForEndOfFrame();
            timer += Time.deltaTime;
            UpdateTransitionPosition(targetTransform, timer / inDuration, outPosition.x, 0f);
        }

        UpdateTransitionPosition(targetTransform, 1f, outPosition.x, 0);

        if (oldMainObjective != null)
            yield return StartCoroutine(SwapMainMissionCoroutine());

        yield return new WaitForSeconds(pauseDuration);

        timer = 0;
        while (timer < outDuration)
        {
            yield return new WaitForEndOfFrame();
            timer += Time.deltaTime;
            UpdateTransitionPosition(targetTransform, timer / outDuration, 0, outPosition.x);
        }

        UpdateTransitionPosition(targetTransform, 1, 0, outPosition.x);
        transitioning = false;
    }

    private void UpdateTransitionPosition(RectTransform targetTransform, float percentage, float startX, float targetX)
    {
        float lerpX = Mathf.Lerp(startX, targetX, percentage);
        targetTransform.anchoredPosition =
            new Vector2(lerpX, targetTransform.anchoredPosition.y);
    }

    private void SetInitialMainMissionCoroutine()
    {
        mainObjective.transform.SetParent(objectiveContainer.transform);
        RectTransform newMainRectTransform = (RectTransform) mainObjective.transform;
        newMainRectTransform.anchoredPosition = new Vector2(0, 0);
    }

    private IEnumerator SwapMainMissionCoroutine() // Works for both null old main mission and assigned old main mission
    {
        if (oldMainObjective != null)
        {
            yield return StartCoroutine(
                TransitionCoroutine(
                    0.25f,
                    (RectTransform) oldMainObjective.transform,
                    0f,
                    500));
        }

        mainObjective.transform.SetParent(objectiveContainer.transform);
        RectTransform newMainRectTransform = (RectTransform) mainObjective.transform;
        newMainRectTransform.anchoredPosition = new Vector2(500, 0);
        yield return StartCoroutine(TransitionCoroutine(0.25f, newMainRectTransform, 500f, 0f));
    }
}
