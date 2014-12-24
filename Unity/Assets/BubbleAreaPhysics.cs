using UnityEngine;
using System.Collections.Generic;


public class BubbleAreaPhysics : MonoBehaviour
{
    public float gravityMagnitude = 9.8f;
    public float breakDistance = 0f;

    private int maxAttachedBubbleCount = 10;

    private List<BubblePhysics> attachedBubbles = new List<BubblePhysics>();

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        var rad = GetRadius();

        // Detach bubbles out of area
        var stillAttached = new List<BubblePhysics>();

        foreach (var bubble in attachedBubbles)
        {
            var diff = transform.position - bubble.transform.position;

            var tRad = rad - bubble.GetRadius() + breakDistance;
            var tRadSq = tRad * tRad;

            if (diff.sqrMagnitude < tRadSq)
            {
                stillAttached.Add(bubble);
            }
        }

        attachedBubbles = stillAttached;

        // Attach bubbles in area (if room)
        foreach (var bubble in BubblePhysics.LiveBubbles)
        {
            if (attachedBubbles.Count >= maxAttachedBubbleCount)
            {
                break;
            }

            if (!attachedBubbles.Contains(bubble))
            {
                var diff = transform.position - bubble.transform.position;

                var tRad = rad - bubble.GetRadius();
                var tRadSq = tRad * tRad;

                if (diff.sqrMagnitude < tRadSq)
                {

                    attachedBubbles.Add(bubble);
                }
            }
        }

        // Affect bubbles with gravity
        foreach (var bubble in attachedBubbles)
        {
            var diff = transform.position - bubble.transform.position;

            var ratioFromCenter = diff.magnitude / rad;

            // Gravity = const* 1/distance^2
            var forceMagnitude = gravityMagnitude * 1 / (1 + (ratioFromCenter * ratioFromCenter));

            // Scale force based on number of inner bubbles
            forceMagnitude /= (1 + attachedBubbles.Count);

            var force = diff.normalized * forceMagnitude;

            // Apply to the bubble and the area
            GetComponent<Rigidbody2D>().AddForce(-force);
            bubble.GetComponent<Rigidbody2D>().AddForce(force);
        }

        // Update text to match count
        var bubbleText = GetComponent<BubbleText>();
        bubbleText.text = attachedBubbles.Count + "";

    }

    private float GetRadius()
    {
        var radius = gameObject.GetComponent<CircleCollider2D>().radius;

        // Just use the x scale as a shortcut
        return radius * transform.localScale.x;
    }
}
