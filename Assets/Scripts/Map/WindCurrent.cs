using UnityEngine;
using UnityEngine.UIElements;

[RequireComponent(typeof(WindZone))]
[RequireComponent(typeof(Collider))]
public class WindCurrent : Mechanism
{
    [Header("Cycle Settings")]
    [SerializeField] private float onDuration = 3f;
    [SerializeField] private float offDuration = 2f;
    [Tooltip("Whether the wind is blowing when the scene starts.")]
    [SerializeField] private bool startActive = true;

    private Collider windCollider;
    private float timer;

    private void Awake()
    {
        windCollider = GetComponent<Collider>();
    }

    private void Start()
    {
        if (startActive)
        {
            Activate();
            timer = onDuration;
        }
        else
        {
            Deactivate();
            timer = offDuration;
        }
    }

    private void Update()
    {
        timer -= Time.deltaTime;

        if (timer > 0f) return;

        // Time's up: flip state and start the other phase's countdown.
        if (ActivationStatus)
        {
            Deactivate();
            timer = offDuration;
        }
        else
        {
            Activate();
            timer = onDuration;
        }
    }

    public override void Activate()
    {
        base.Activate();
        Effect();
    }

    public override void Deactivate()
    {
        base.Deactivate();
        Effect();
    }

    // Effect = apply the current state to the wind. Physics callbacks
    // (OnTriggerStay) still fire on a disabled script, so we toggle the
    // collider instead: no collider, no trigger events, no push.
    public override void Effect()
    {
        windCollider.enabled = ActivationStatus;
    }

    // Scene View debug aid: green while blowing, red while off.
    // Uses the transform matrix instead of collider.bounds, because a
    // disabled collider reports empty bounds and the gizmo would vanish.
    private void OnDrawGizmos()
    {
        BoxCollider box = GetComponent<BoxCollider>();
        if (box == null) return;

        Gizmos.color = ActivationStatus ? Color.green : Color.red;
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.DrawWireCube(box.center, box.size);
    }
}
