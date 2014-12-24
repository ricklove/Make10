using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class BubbleText : MonoBehaviour
{
    private string text = "";

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

        if (textMesh.text.Length > 0 && textMesh.text[0] >= '0' && textMesh.text[0] <= '9')
        {
            if (textMesh.text.Length > 1)
            {
                textMesh.transform.localPosition = new Vector3(
                    -1.4f,
                    textMesh.transform.localPosition.y,
                    textMesh.transform.localPosition.z);
            }
            else
            {
                textMesh.transform.localPosition = new Vector3(
                    -.6f,
                    textMesh.transform.localPosition.y,
                    textMesh.transform.localPosition.z);
            }
        }

    }
}
