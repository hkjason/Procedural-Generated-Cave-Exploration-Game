using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IgnoreCulling : MonoBehaviour
{
    private Renderer objectRenderer;

    // Start is called before the first frame update
    void Start()
    {
        objectRenderer = GetComponent<Renderer>();
    }

    // Update is called once per frame
    void LateUpdate()
    {
        objectRenderer.bounds.Expand(100f); // Expand bounds to a large value
    }
}
