using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

[ExecuteInEditMode]
public class BubblePhysics : MonoBehaviour
{

    

    public float attachedInputAttachDistance = 0f;
    public float attachedInputBreakDistance = 1000.0f;
    public float attachedInputBaseForce = 15f;
    public float attachedInputMaxForce = 75f;
    public float attachedInputStartSpringDistance = 1.0f;

    public float attachedBubbleForceAdjustmentWhenHasInput = 0.01f;

    public float attachedBubbleAttachDistance = 0.7f;
    public float attachedBubbleBreakDistance = 0.7f;
    public float attachedBubbleBaseForce = 1f;
    public float attachedBubbleMaxForce = 2f;
    public float attachedBubbleStartSpringDistance = 0f;

    public bool IsAttachedToBubbleArea { get; set; }


    private Vector3 mousePosDelta;
    private Vector3 mousePosLast;

    private string attachedInputID;
    private List<BubblePhysics> attachedBubbles = new List<BubblePhysics>();

    private static List<BubblePhysics> allBubbles = new List<BubblePhysics>();

    public static List<BubblePhysics> LiveBubbles
    {
        get
        {
            // Trim disposed bubbles
            allBubbles = allBubbles.Where(b => b && b.gameObject).ToList();

            // Return active bubbles
            return allBubbles.Where(b => b.gameObject.activeSelf).ToList();
        }
    }

    // Use this for initialization
    void Start()
    {
        allBubbles.Add(this);
    }

    void FixedUpdate()
    {

    }

    // Update is called once per frame
    void Update()
    {
        UpdateBubbleAttachmentPhysics();
        UpdateInputPhysics();
    }

    private void UpdateBubbleAttachmentPhysics()
    {
        var radius = GetRadius();

        // Get other bubbles
        var otherBubbles = LiveBubbles.Where(b => b != this).ToList();

        // Detach from far bubbles
        var stillAttached = new List<BubblePhysics>();

        foreach (var oBubble in attachedBubbles)
        {
            var diff = transform.position - oBubble.transform.position;
            var oRadius = oBubble.GetRadius();

            var tRad = radius + oRadius + attachedBubbleBreakDistance;
            var tRadSq = tRad * tRad;

            if (diff.sqrMagnitude < tRadSq)
            {
                stillAttached.Add(oBubble);
            }
        }

        attachedBubbles = stillAttached;

        // Attach to near bubbles
        foreach (var oBubble in otherBubbles)
        {
            if (attachedBubbles.Contains(oBubble))
            {
                continue;
            }

            var diff = transform.position - oBubble.transform.position;
            var oRadius = oBubble.GetRadius();

            var tRad = radius + oRadius + attachedBubbleAttachDistance;
            var tRadSq = tRad * tRad;

            if (diff.sqrMagnitude < tRadSq)
            {
                attachedBubbles.Add(oBubble);
            }
        }

        // Pull to attached bubbles
        var rigid = gameObject.GetComponent<Rigidbody2D>();

        foreach (var oBubble in attachedBubbles)
        {
            var diff = oBubble.transform.position - transform.position;

            var forceAdjustment = attachedInputID != null ?
                attachedBubbleForceAdjustmentWhenHasInput : 1.0f;

            PullToEdge(radius + oBubble.GetRadius(), rigid, diff,
                attachedBubbleStartSpringDistance,
                attachedBubbleBaseForce * forceAdjustment,
                attachedBubbleMaxForce * forceAdjustment);
        }

    }

    private void UpdateInputPhysics()
    {

        // Interact with touch or mouse
        // Convert all input to world positions on the plane
        var inputs = new List<BubbleInput>();

        foreach (Touch touch in Input.touches)
        {
            if (touch.phase != TouchPhase.Ended && touch.phase != TouchPhase.Canceled)
            {
                Vector3 worldPos = Camera.main.ScreenToWorldPoint(touch.position);

                //mousePosDelta = worldPos - mousePosLast;

                inputs.Add(new BubbleInput()
                {
                    id = "touch" + touch.fingerId,
                    position = new Vector2(worldPos.x, worldPos.y),
                    //delta = new Vector2(worldPosDelta.x, worldPosDelta.y),
                });
            }
        }

        // Mouse position
        // http://answers.unity3d.com/questions/332013/calculating-change-in-mouse-position.html
        if (Input.GetMouseButtonDown(0))
        {
            Vector3 worldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        }
        else if (Input.GetMouseButton(0))
        {
            Vector3 worldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

            mousePosDelta = worldPos - mousePosLast;
            mousePosLast = worldPos;

            inputs.Add(new BubbleInput()
            {
                id = "mouse",
                position = new Vector2(mousePosLast.x, mousePosLast.y),
                //delta = new Vector2(mousePosDelta.x, mousePosDelta.y),
            });
        }

        // React if touch is near
        var pos = new Vector2(transform.position.x, transform.position.y);
        var radius = GetRadius();
        var radiusSq = radius * radius;
        var rigid = gameObject.GetComponent<Rigidbody2D>();

        BubbleInput attachedInput = null;

        // Check for still attached
        var lastAttachedInput = inputs.FirstOrDefault(a => a.id == attachedInputID);

        if (lastAttachedInput != null)
        {

            var aDiff = lastAttachedInput.position - pos;
            if (aDiff.magnitude < radius + attachedInputBreakDistance)
            {
                attachedInput = lastAttachedInput;
            }
            else
            {
                //Debug.Log("Broken attachment to: " + lastAttachedInput.id);
                attachedInputID = null;
            }
        }
        

        // if not attached to anything, search for a new attachment
        if (attachedInput == null)
        {
            // Reset attachment id
            attachedInputID = null;

            // look for new input
            var aRadius = radius + attachedInputAttachDistance;
            var aRadiusSq = aRadius * aRadius;

            foreach (var input in inputs)
            {
                var diff = input.position - pos;

                if (diff.sqrMagnitude < aRadiusSq)
                {
                    attachedInputID = input.id;
                }
            }
        }

        // Pull towards attached input
        if (attachedInput != null)
        {
            //Debug.Log("Pull to: " + attachedInput.id);

            var diff = attachedInput.position - pos;
            var startSpringDistance = attachedInputStartSpringDistance;
            var baseForceMagnitude = attachedInputBaseForce;
            var maxForceMagnitude = attachedInputMaxForce;

            PullToEdge(radius, rigid, diff, startSpringDistance, baseForceMagnitude, maxForceMagnitude);
        }
    }

    private static void PullToEdge(float radius, Rigidbody2D rigid, Vector3 diff, float startSpringDistance, float baseForceMagnitude, float maxForceMagnitude)
    {
        var distanceFromEdge = diff.magnitude - radius;

        if (distanceFromEdge > 0)
        {
            var springDistance = startSpringDistance + distanceFromEdge;

            // Make the force like a spring
            var forceMagnitude = springDistance * springDistance;
            forceMagnitude *= baseForceMagnitude;

            forceMagnitude = Mathf.Min(forceMagnitude, maxForceMagnitude);

            var force = diff.normalized * forceMagnitude;

            // Add force
            rigid.AddForce(force);
        }
    }

    public float GetRadius()
    {
        // TODO: Consider circle collider radius could be transformed
        var radius = gameObject.GetComponent<CircleCollider2D>().radius;
        return radius;
    }

}

public class BubbleInput
{
    public string id;
    public Vector2 position;
    //public Vector2 delta;
}
