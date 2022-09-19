using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class ScatterTool : MonoBehaviour
{
    [HideInInspector]
    public List<GameObject> scatterObjects = new List<GameObject>();
    [HideInInspector]
    public GameObject scatteredObjects;
    [HideInInspector]
    public GameObject selectedScatterObj;
    public bool sweep = true;
    public bool randomSpread = true;
    [Range(0,1)]
    public float spread = 0.3f;
    public bool randomizeRotationX = false;
    public bool randomizeRotationY = true;
    public bool randomizeRotationZ = false;
    public bool randomizeSize = true;
    [Range(0, 10)]
    public float minSize = 1.0f;
    [Range(0, 10)]
    public float maxSize = 3.0f;
    [Range(10, 100)]
    public float density = 30.0f;
}
