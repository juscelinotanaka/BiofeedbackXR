using System;
using System.Collections;
using UnityEngine;

public class CameraFollower : MonoBehaviour
{
    public Transform objectToFollow;
    public Vector3 offset = -Vector3.forward;
    [SerializeField] private float _zoomTime = 1;

    [SerializeField] private AnimationCurve _animationCurve = AnimationCurve.Linear(0, 0, 1, 1);

    private void FixedUpdate()
    {
        // transform.position = Vector3.Lerp(transform.position,
        //     objectToFollow.position + objectToFollow.forward * offset.z + Vector3.up * offset.y,
        //     Time.deltaTime * 10);

        transform.position = objectToFollow.position + objectToFollow.forward * offset.z + Vector3.up * offset.y;
    }

    [ContextMenu("Zoom out")]
    public void ZoomOut()
    {
        ZoomOut(null);
    }

    public void ZoomOut(Action callback = null)
    {
        StartCoroutine(ZoomOutCoroutine(callback));
    }

    private IEnumerator ZoomOutCoroutine(Action callback)
    {
        Vector3 original = offset;
        Vector3 twice = offset * 2;
        float lerp = 0;
        do
        {
            lerp += Time.deltaTime / _zoomTime;
            offset = Vector3.Lerp(original, twice, _animationCurve.Evaluate(lerp));
            yield return null;
        } while (lerp <= 1);

        yield return new WaitForSeconds(_zoomTime);

        lerp = 0;
        do
        {
            lerp += Time.deltaTime / _zoomTime;
            offset = Vector3.Lerp(twice, original, _animationCurve.Evaluate(lerp));
            yield return null;
        } while (lerp <= 1);

        offset = original;

        callback?.Invoke();
    }
}