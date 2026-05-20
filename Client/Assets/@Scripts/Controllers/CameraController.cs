using UnityEngine;

public class CameraController : MonoBehaviour
{
    public BaseObject Target { get; set; }

    void LateUpdate()
    {
        if (Target == null)
            return;

        transform.position = new Vector3(
            Target.transform.position.x,
            Target.transform.position.y,
            -10f);
    }
}
