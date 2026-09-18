using UnityEngine;

public class CameraFollow : MonoBehaviour
{

    [Header("Target")]
    [SerializeField] private Transform target;

    [Header("Isometric Offset")]
    [Tooltip("Position offset from the target, in world space. Y = height, Z = distance, x = side offset")]
    [SerializeField] private Vector3 offset = new Vector3(0f, 12f, -8f);

    [Tooltip("Angle the camera looks down at the target. 45-55 is a common isometric angle.")]
    [SerializeField] private float lookDownAngle = 50f;

    [Header("Follow Feel")]
    [SerializeField] private float followSmoothTime = 0.15f;

    private Vector3 velocity = Vector3.zero;

    private void Awake()
    {
        // Ensure the camera starts at the correct position and rotation
        transform.rotation = Quaternion.Euler(lookDownAngle, transform.rotation.eulerAngles.y, 0f);
    }

    private void LateUpdate()
    {
        if (target == null) return;

        Vector3 desiredPosition = target.position + offset;
        transform.position = Vector3.SmoothDamp(transform.position, desiredPosition, ref velocity, followSmoothTime);

    }

    public void SetTarget(Transform newTarget) // [Observer / Target Tracking Pattern]
    {
        target = newTarget;
    }
}
