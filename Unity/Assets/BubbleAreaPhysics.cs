using UnityEngine;
using System.Collections.Generic;


public class BubbleAreaPhysics : MonoBehaviour
{
    public float gravityMagnitude = 9.8f;
    public float breakDistance = 0f;

    private int maxAttachedBubbleCount = 100;//10;
    private bool areBubblesLocked = false;

    internal List<BubblePhysics> attachedBubbles = new List<BubblePhysics>();

    internal void LockBubbles()
    {
        areBubblesLocked = true;
    }

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        var rad = GetRadius();

        if (!areBubblesLocked)
        {

            // Detach bubbles out of area
            var stillAttached = new List<BubblePhysics>();

            foreach (var bubble in attachedBubbles)
            {
                var diff = transform.position - bubble.transform.position;

                var tRad = rad + bubble.GetRadius() + breakDistance;
                var tRadSq = tRad * tRad;

                if (diff.sqrMagnitude < tRadSq)
                {
                    stillAttached.Add(bubble);
                }
                else
                {
                    bubble.IsAttachedToBubbleArea = false;
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

                if (bubble.IsAttachedToBubbleArea)
                {
                    continue;
                }

                if (!attachedBubbles.Contains(bubble))
                {
                    var diff = transform.position - bubble.transform.position;

                    var tRad = rad + bubble.GetRadius();
                    var tRadSq = tRad * tRad;

                    if (diff.sqrMagnitude < tRadSq)
                    {
                        attachedBubbles.Add(bubble);
                        bubble.IsAttachedToBubbleArea = true;
                    }
                }
            }
        }

        // Change size based on number
        var scale = 2 + (3 * attachedBubbles.Count * 0.1f);
        transform.localScale = new Vector3(scale, scale, 1);

        // Affect bubbles with gravity
        foreach (var bubble in attachedBubbles)
        {
            var diff = transform.position - bubble.transform.position;

            var ratioFromCenter = diff.magnitude / rad;

            // Gravity = const* 1/distance^2
            var forceMagnitude = gravityMagnitude * 1 / (1 + (ratioFromCenter * ratioFromCenter));

            // Scale force based on number of inner bubbles
            //forceMagnitude /= (1 + attachedBubbles.Count);

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
