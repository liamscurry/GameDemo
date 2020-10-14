using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class ObjectiveUIManager : MonoBehaviour
{
    [SerializeField]
    private GameObject mainObjectiveContainer;
    [SerializeField]
    private GameObject sideObjectiveContainer;
    [SerializeField]
    private float sideObjectiveSpacing;
    [SerializeField]
    private WaypointUI MainObjectiveWaypoint;

    private Vector3 outPosition;
    private RectTransform rectTransform;
    private GameObject mainObjective;
    private GameObject oldMainObjective;

    private bool transitioning;

    private List<RectTransform> sideObjectives;

    private void Awake()
    {
        rectTransform = (RectTransform) transform;
        outPosition = rectTransform.anchoredPosition;
        transitioning = false;
        sideObjectives = new List<RectTransform>();
    }

    public void SetMainObjectiveNoTransition(GameObject newMainObjective)
    {
        StopAllCoroutines();
        transitioning = false;
        GameObject.Destroy(mainObjective);
        oldMainObjective = null;
        mainObjective = newMainObjective;
        mainObjective.transform.SetParent(mainObjectiveContainer.transform);
        ((RectTransform) mainObjective.transform).anchoredPosition = new Vector2(0, 0);
        newMainObjective.SetActive(true);

        WaypointUIInfo waypointInfo =
            newMainObjective.GetComponent<WaypointUIInfo>();
        if (waypointInfo != null)
        {
            MainObjectiveWaypoint.gameObject.SetActive(true);
            MainObjectiveWaypoint.WorldPosition =
                newMainObjective.GetComponent<WaypointUIInfo>().WorldPosition;
        }
        else
        {
            MainObjectiveWaypoint.gameObject.SetActive(false);
        }
    }

    public void AddSideObjectives(GameObject newSideObjectivesParent)
    {
        newSideObjectivesParent.SetActive(true);
        StopAllCoroutines();
        transitioning = false;
        Image[] newSideObjectives =
            newSideObjectivesParent.GetComponentsInChildren<Image>();

        foreach (Image image in newSideObjectives)
        {
            RectTransform sideObjective = image.GetComponent<RectTransform>();

            if (sideObjectives.Contains(sideObjective))
            {
                throw new System.Exception("added side mission more than once");
            }
            else
            {
                sideObjective.gameObject.SetActive(true);
                sideObjective.transform.SetParent(sideObjectiveContainer.transform);
                sideObjective.anchoredPosition = new Vector2(0, 0);
                sideObjectives.Add(sideObjective);

                WaypointUIInfo waypointInfo =
                sideObjective.GetComponent<WaypointUIInfo>();
                if (waypointInfo != null)
                {
                    waypointInfo.Waypoint.gameObject.SetActive(true);
                }
            }
        }

        ArrangeSideObjectives();
        GameObject.Destroy(newSideObjectivesParent);
    }

    public void UpdateMainMissionWaypoint(WaypointUIInfo info)
    {
        MainObjectiveWaypoint.WorldPosition = info.WorldPosition; 
    }

    public void SetMainObjective(GameObject newMainObjective)
    {
        oldMainObjective = mainObjective;
        mainObjective = newMainObjective;
        newMainObjective.SetActive(true);
        StopAllCoroutines();
        StartCoroutine(TransitionMainObjectiveCoroutine(0.5f, 7, 0.5f, rectTransform, 0, outPosition.x, null));
        
        WaypointUIInfo waypointInfo =
            newMainObjective.GetComponent<WaypointUIInfo>();
        if (waypointInfo != null)
        {
            MainObjectiveWaypoint.gameObject.SetActive(true);
            MainObjectiveWaypoint.WorldPosition =
                newMainObjective.GetComponent<WaypointUIInfo>().WorldPosition;
        }
        else
        {
            MainObjectiveWaypoint.gameObject.SetActive(false);
        }
    }

    public void ClearMainObjective(bool transitionUI = true)
    {
        oldMainObjective = mainObjective;
        mainObjective = null;
        if (transitionUI)
        {
            StopAllCoroutines();
            StartCoroutine(TransitionMainObjectiveCoroutine(0.5f, 4, 0.5f, rectTransform, 0, outPosition.x, null));
        }
        MainObjectiveWaypoint.gameObject.SetActive(false);
    }
    
    public void PingObjectives()
    {
        if (!transitioning)
            StartCoroutine(PingObjectivesCoroutine(0.5f, 7f, 0.5f, rectTransform, 0, outPosition.x, null));
    }

    public void PingObjectives(float duration)
    {
        if (!transitioning)
            StartCoroutine(PingObjectivesCoroutine(0.5f, duration, 0.5f, rectTransform, 0, outPosition.x, null));
    }

    public void AddSideObjective(RectTransform sideObjective)
    {
        if (sideObjectives.Contains(sideObjective))
        {
            throw new System.Exception("added side mission more than once");
        }
        else
        {
            sideObjective.gameObject.SetActive(true);
            sideObjective.transform.SetParent(sideObjectiveContainer.transform);
            sideObjective.anchoredPosition = new Vector2(500, 0);
            sideObjectives.Add(sideObjective);
            StartCoroutine(AddSideObjectiveCoroutine(0.5f, 4, 0.5f, rectTransform, 0, outPosition.x, null));
        }
    }

    public void RemoveSideObjective(RectTransform sideObjective)
    {
        if (!sideObjectives.Contains(sideObjective))
        {
            throw new System.Exception("side mission not active");
        }
        else
        {
            StartCoroutine(RemoveSideObjectiveCoroutine(0.5f, 4, 0.5f, rectTransform, sideObjective, 0, outPosition.x, null));
            WaypointUIInfo waypointInfo = 
                sideObjective.GetComponent<WaypointUIInfo>();
            if (waypointInfo != null)
                GameObject.Destroy(waypointInfo.Waypoint.transform.parent.gameObject);
        }
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
        /*
        if (oldMainObjective == null)
        {
            if (sideObjectives.Count != 0)
                yield return StartCoroutine(ArrangeCoroutine(1f));
            //SetInitialMainMissionCoroutine();
        }*/

        float timer = 0;

        while (timer < inDuration)
        {
            yield return new WaitForEndOfFrame();
            timer += Time.deltaTime;
            UpdateTransitionPosition(targetTransform, timer / inDuration, outPosition.x, 0f);
        }

        UpdateTransitionPosition(targetTransform, 1f, outPosition.x, 0);

        if (mainObjective != null)
        { 
            if (sideObjectives.Count != 0)
                yield return StartCoroutine(ArrangeCoroutine(1f));
            yield return StartCoroutine(SwapMainMissionCoroutine());
        }
        else
        {
            yield return StartCoroutine(SwapMainMissionCoroutine());
            if (sideObjectives.Count != 0)
                yield return StartCoroutine(ArrangeCoroutine(1f));
        }

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
        GameObject.Destroy(oldMainObjective);
    }

    private IEnumerator AddSideObjectiveCoroutine(
        float inDuration,
        float pauseDuration,
        float outDuration,
        RectTransform targetTransform,
        float inX,
        float outX,
        UnityEvent inEvent)
    {
        transitioning = true;

        ArrangeSideObjectives();

        float timer = 0;

        while (timer < inDuration)
        {
            yield return new WaitForEndOfFrame();
            timer += Time.deltaTime;
            UpdateTransitionPosition(targetTransform, timer / inDuration, outPosition.x, 0f);
        }

        UpdateTransitionPosition(targetTransform, 1f, outPosition.x, 0);

        RectTransform newSideObjectiveTransform =
            (RectTransform) sideObjectives[sideObjectives.Count - 1].transform;
        yield return StartCoroutine(TransitionCoroutine(0.25f, newSideObjectiveTransform, 500f, 0f));

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

    private IEnumerator RemoveSideObjectiveCoroutine(
        float inDuration,
        float pauseDuration,
        float outDuration,
        RectTransform targetTransform,
        RectTransform completedTransform,
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

        yield return StartCoroutine(TransitionCoroutine(0.25f, completedTransform, 0, 500f));

        // only rearrange side objectives if non last element removed
        if (sideObjectives.IndexOf(completedTransform) != sideObjectives.Count - 1)
        {
            sideObjectives.Remove(completedTransform);
            yield return StartCoroutine(ArrangeCoroutine(1f));
        }
        else
        {
            sideObjectives.Remove(completedTransform);
        }

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
        GameObject.Destroy(completedTransform.gameObject);
    }

    private void ArrangeSideObjectives()
    {
        float currentYPosition = -sideObjectiveSpacing * 2;
        if (mainObjective != null)
        {
            currentYPosition -= ((RectTransform) mainObjective.transform).rect.height;
        }
        for (int i = 0; i < sideObjectives.Count; i++)
        {
            sideObjectives[i].anchoredPosition =
                new Vector2(sideObjectives[i].anchoredPosition.x, currentYPosition);
            currentYPosition -= sideObjectives[i].rect.height + sideObjectiveSpacing;
        }
    }

    private IEnumerator ArrangeCoroutine(float duration)
    {
        List<Vector2> startPositions = new List<Vector2>();
        List<Vector2> targetPositions = new List<Vector2>();
        float currentYPosition = -sideObjectiveSpacing * 2;

        if (mainObjective != null)
        {
            currentYPosition -= ((RectTransform) mainObjective.transform).rect.height;
        }

        for (int i = 0; i < sideObjectives.Count; i++)
        {
            startPositions.Add(sideObjectives[i].anchoredPosition);
            targetPositions.Add(
                new Vector2(sideObjectives[i].anchoredPosition.x, currentYPosition));
            currentYPosition -= sideObjectives[i].rect.height + sideObjectiveSpacing;
        }

        float timer = 0;
        while (timer < duration)
        {
            yield return new WaitForEndOfFrame();
            timer += Time.deltaTime;
            for (int i = 0; i < sideObjectives.Count; i++)
            {
                float lerpY = Mathf.Lerp(startPositions[i].y, targetPositions[i].y, timer / duration);
                sideObjectives[i].anchoredPosition =
                    new Vector2(sideObjectives[i].anchoredPosition.x, lerpY);
            }
        }

        for (int i = 0; i < sideObjectives.Count; i++)
        {
            float lerpY = Mathf.Lerp(startPositions[i].y, targetPositions[i].y, 1f);
            sideObjectives[i].anchoredPosition =
                new Vector2(sideObjectives[i].anchoredPosition.x, lerpY);
        }
    }

    private void UpdateTransitionPosition(RectTransform targetTransform, float percentage, float startX, float targetX)
    {
        float lerpX = Mathf.Lerp(startX, targetX, percentage);
        targetTransform.anchoredPosition =
            new Vector2(lerpX, targetTransform.anchoredPosition.y);
    }

    private void SetInitialMainMissionCoroutine()
    {
        mainObjective.transform.SetParent(mainObjectiveContainer.transform);
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

        if (mainObjective != null)
        {
            mainObjective.transform.SetParent(mainObjectiveContainer.transform);
            RectTransform newMainRectTransform = (RectTransform) mainObjective.transform;
            newMainRectTransform.anchoredPosition = new Vector2(500, 0);
            yield return StartCoroutine(TransitionCoroutine(0.25f, newMainRectTransform, 500f, 0f));
        }
    }
}
