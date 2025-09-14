using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class InterpolatedMove : MonoBehaviour
{
    [SerializeField] private float moveRequestTimeBuffer = 1; // Time after which queued move requests are discarded

    public event Action dashFinishEvent;
    public event Action dashStartEvent;

    private Coroutine moveObjectCoroutine = null;
    private Rigidbody rb;



    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        //rb.interpolation = RigidbodyInterpolation.Interpolate; // Smooth rendering
        //rb.isKinematic = true; // Important if you want full manual movement
    }

    public void MoveObject(Vector3 finalPos, AnimationCurve curve, float duration)
    {
        if (moveObjectCoroutine == null)
        {
            moveObjectCoroutine = StartCoroutine(InterpolatePosition(transform.position, finalPos, curve, duration));
        }
       
    }

    public void StopMovement()
    {
        if (moveObjectCoroutine != null)
        {
            StopCoroutine(moveObjectCoroutine);
            moveObjectCoroutine = null;
        }
    }

    private IEnumerator InterpolatePosition(Vector3 startPos, Vector3 finalPos, AnimationCurve curve, float duration)
    {
        dashStartEvent?.Invoke();

        float elapsed = 0f;
        while (elapsed < duration)
        {
            float t = elapsed / duration;
            float curveValue = curve.Evaluate(t);
            Vector3 targetPos = Vector3.Lerp(startPos, finalPos, curveValue);

            rb.MovePosition(targetPos); // ? Physics-aware movement

            elapsed += Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }
        rb.MovePosition(finalPos);

        moveObjectCoroutine = null;
        dashFinishEvent?.Invoke();
    }
}
