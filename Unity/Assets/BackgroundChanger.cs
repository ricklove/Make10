using UnityEngine;
using System.Collections.Generic;

public class BackgroundChanger : MonoBehaviour
{

    private List<GameObject> children = new List<GameObject>();
    private int lastChildIndex = 0;

    void Start()
    {
        foreach (Transform c in transform)
        {
            children.Add(c.gameObject);
        }
    }

    void Update()
    {

    }

    public void ChangeBackground()
    {
        // Enable one random child
        children.ForEach(c => c.SetActive(false));

        var i = Random.Range(0, children.Count - 1);

        if (i == lastChildIndex) { i = 0; }

        children[i].SetActive(true);
    }
}
