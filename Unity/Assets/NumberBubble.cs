using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

[ExecuteInEditMode]
public class NumberBubble : MonoBehaviour
{

    public string text;
    public float inputAttachedBreakDistance = 3.0f;
    public float inputAttachedForce = 10f;


    private Vector3 mousePosDelta;
    private Vector3 mousePosLast;

    private string attachedInputID;

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        var textMesh = transform.FindChild("Text").GetComponent<TextMesh>();

        if (textMesh.text != text)
        {
            textMesh.text = text;
        }

        // Interact with touch or mouse
        // Convert all input to world positions on the plane
        var inputs = new List<BubbleInput>();

        foreach (Touch touch in Input.touches)
        {
            if (touch.phase != TouchPhase.Ended && touch.phase != TouchPhase.Canceled)
            {
                // TODO: Handle touch
                //touch.
                throw new System.NotImplementedException();
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
                delta = new Vector2(mousePosDelta.x, mousePosDelta.y),
            });
        }


        // React if touch is near
        var pos = new Vector2(transform.position.x, transform.position.y);
        var radius = gameObject.GetComponent<CircleCollider2D>().radius;
        var radiusSq = radius * radius;
        var rigid = gameObject.GetComponent<Rigidbody2D>();

        BubbleInput attachedInput = null;

        // Check for still attached
        var lastAttachedInput = inputs.FirstOrDefault(a => a.id == attachedInputID);

        if (lastAttachedInput != null)
        {

            var aDiff = lastAttachedInput.position - pos;
            if (aDiff.magnitude < radius + inputAttachedBreakDistance)
            {
                attachedInput = lastAttachedInput;
            }
            else
            {
                Debug.Log("Broken attachment to: " + lastAttachedInput.id);
                attachedInputID = null;
            }
        }

        // if not attached to anything, search for a new attachment
        if (attachedInput == null)
        {
            foreach (var input in inputs)
            {
                var diff = input.position - pos;

                if (diff.sqrMagnitude < radiusSq)
                {
                    attachedInputID = input.id;
                }
            }
        }

        // Pull towards attached input
        if (attachedInput != null)
        {
            Debug.Log("Pull to: " + attachedInput.id);

            var diff = attachedInput.position - pos;
            var distanceFromEdge = 1 + (diff.magnitude - radius);

            // Make the force like a spring
            var forceMagnitude = distanceFromEdge * distanceFromEdge;
            forceMagnitude *= inputAttachedForce;

            var force = diff.normalized * forceMagnitude;

            // Add force
            rigid.AddForce(force);
        }



    }
}

public class BubbleInput
{
    public string id;
    public Vector2 position;
    public Vector2 delta;
}
