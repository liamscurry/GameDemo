using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthbarShadow : MonoBehaviour
{
    [SerializeField]
    private Transform mimicTransform;
    [SerializeField]
    private MeshRenderer meshRenderer;
    [SerializeField]
    private float delayDuration;
    [SerializeField]
    private float shadowSpeed;

    private bool mimicing;

    private void Update()
    {
        if (mimicTransform.localScale.x > transform.localScale.x + 0.05f)
        {
            if (!meshRenderer.gameObject.activeSelf)
                meshRenderer.gameObject.SetActive(true);

            transform.localScale =
                new Vector3(mimicTransform.localScale.x,
                            transform.localScale.y,
                            transform.localScale.z);
            StopAllCoroutines();
        }
        else if (mimicTransform.localScale.x < transform.localScale.x - 0.05f &&
                 !mimicing)
        {
            StartCoroutine(DelayedMimicCoroutine(delayDuration, shadowSpeed));
            mimicing = true;
        }
    }

    public void Zero()
    {
        transform.localScale =
                new Vector3(0,
                            transform.localScale.y,
                            transform.localScale.z);
        StopAllCoroutines();
        meshRenderer.gameObject.SetActive(false);
    }

    private IEnumerator DelayedMimicCoroutine(float delay, float speed)
    {
        yield return new WaitForSeconds(delay);

        while (true)
        {
            yield return new WaitForEndOfFrame();
            float currentXScale = transform.localScale.x;
            float targetXScale = mimicTransform.localScale.x;
            float incrementedScale = Mathf.MoveTowards(currentXScale, targetXScale, speed * Time.deltaTime);
            transform.localScale =
                new Vector3(incrementedScale,
                            transform.localScale.y,
                            transform.localScale.z);

            if (Matho.IsInRange(incrementedScale, targetXScale, 0.025f))
                break;
        }

        if (mimicTransform.localScale.x > 0.0025f)
        {
            transform.localScale =
                    new Vector3(mimicTransform.localScale.x,
                                transform.localScale.y,
                                transform.localScale.z);
        }
        else
        {
            transform.localScale =
                new Vector3(0,
                            transform.localScale.y,
                            transform.localScale.z);
            meshRenderer.gameObject.SetActive(false);
        }

        mimicing = false;
    }
}
