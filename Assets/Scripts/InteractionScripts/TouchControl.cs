using System.Collections;
using UnityEngine;

public abstract class TouchControl : MonoBehaviour
{
    protected float heldTime = 0.0f;
    public const float threshold = 5f;
    public IEnumerator OnTouchDown()
    {
        heldTime = Time.time;
        float magnitude = 0f;
#if UNITY_EDITOR || UNITY_STANDALONE
        while (!Input.GetMouseButtonUp(0))
        {
            magnitude += FixedTouchField.instance.TouchDist.magnitude;
            yield return null;
        }
#elif UNITY_ANDROID
        while (Input.touchCount > 0 && Input.GetTouch(0).phase != TouchPhase.Ended){
            magnitude += FixedTouchField.instance.TouchDist.magnitude;
            yield return null;
        }
#endif
        if (magnitude < threshold)
        {
            OnTouchUp();
            heldTime = 0.0f;
        }
    }

    public abstract void OnTouchUp();
}