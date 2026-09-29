using UnityEngine;

[RequireComponent(typeof(Altar))]
public class AltarRotator : MonoBehaviour
{
    [SerializeField] private KeyCode rotateLeftKey = KeyCode.Q;
    [SerializeField] private KeyCode rotateRightKey = KeyCode.R;
    [SerializeField] private float stepAngle = 90f;
    [SerializeField] private float rotateSpeed = 180f; // degrees per second

    private Altar altar;
    private Quaternion targetRotation;

    private void Awake()
    {
        altar = GetComponent<Altar>();
        targetRotation = transform.rotation;
    }

    private void Update()
    {
        if (altar.PlayerInRange)
        {
            if (Input.GetKeyDown(rotateLeftKey)) { Debug.Log("Rotate left pressed"); Rotate(-stepAngle); }
            else if (Input.GetKeyDown(rotateRightKey)) { Debug.Log("Rotate right pressed"); Rotate(stepAngle); }
        }

        // Turn smoothly toward the target instead of snapping.
        if (Quaternion.Angle(transform.rotation, targetRotation) > 0.01f)
        {
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotateSpeed * Time.deltaTime);
        }
    }

    // Left-multiplying applies the turn around the world's up axis.
    private void Rotate(float angle)
    {
        targetRotation = Quaternion.AngleAxis(angle, Vector3.up) * targetRotation;
    }

    private void OnGUI()
    {
        GUI.Box(new Rect(10, 10, 250, 60), "Altar Debug");
        GUI.Label(new Rect(20, 30, 230, 20), $"Player In Range: {altar.PlayerInRange}");
        GUI.Label(new Rect(20, 45, 230, 20), $"Target Y Rotation: {targetRotation.eulerAngles.y:F1}");
    }
}
