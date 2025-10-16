using UnityEngine;

public class SpinImage : MonoBehaviour
{

    // Update is called once per frame
    void Update()
    {
        transform.Rotate(0, 0, 1f * Time.deltaTime);
    }
}
