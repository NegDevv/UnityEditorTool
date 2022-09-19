using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Linq;

[CustomEditor(typeof(ScatterTool))]
public class EditorScatterTool : Editor
{
    ScatterTool scatterTool;
    Ray scatterRay;
    RaycastHit hit;
    Vector2 mouseDelta;
    Editor gameObjectEditor;
    int currentPickerWindow;

    private void OnSceneGUI()
    {
        if(scatterTool.selectedScatterObj == null)
        {
            
        }

        Event guiEvent = Event.current; //Event.current.rawType
        scatterRay = HandleUtility.GUIPointToWorldRay(guiEvent.mousePosition);
        mouseDelta += guiEvent.delta;
        //Debug.Log(mouseDelta);

        Physics.Raycast(scatterRay, out hit);

        if (scatterTool.sweep && guiEvent.type == EventType.MouseDrag && guiEvent.button == 0 && mouseDelta.magnitude > (1000.0f / scatterTool.density))
        {
            ScatterObjects();
            mouseDelta = Vector2.zero;
        }
        else if (guiEvent.type == EventType.MouseDown && guiEvent.button == 0)
        {
            ScatterObjects();
        }

        if(guiEvent.type == EventType.MouseMove)
        {
            //Debug.Log("Mouse move");
        }

        if(guiEvent.type == EventType.MouseDrag)
        {
            //Debug.Log("Mouse drag");
        }

        DrawGizmos();

        if(guiEvent.type == EventType.Layout)
        {
            HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));
        }
    }

    private void ClearObjects()
    {
        foreach(Transform go in scatterTool.scatteredObjects.transform)
        {
            DestroyImmediate(go.gameObject);
        }
    }

    private void ScatterObjects()
    {
        if(scatterTool.scatteredObjects == null)
        {
            GameObject parent = new GameObject("Scattered object containers");
            parent.transform.parent = scatterTool.transform;
            scatterTool.scatteredObjects = parent;
        }

        if (scatterTool.randomSpread)
        {
            float dirX = Random.Range(-scatterTool.spread, scatterTool.spread) / 10.0f;
            float dirY = Random.Range(-scatterTool.spread, scatterTool.spread) / 10.0f;
            float dirZ = Random.Range(-scatterTool.spread, scatterTool.spread) / 10.0f;
            scatterRay.direction += new Vector3(dirX, dirY, dirZ);
        }

        if (Physics.Raycast(scatterRay, out hit))
        {
            //Debug.Log("Scatterray hit: " + hit.collider.gameObject);
            if (hit.collider != null)
            {
                if (scatterTool.scatterObjects.Count > 0)
                {
                    if (scatterTool.selectedScatterObj != null)
                    {
                        GameObject scatteredObj = Instantiate(scatterTool.selectedScatterObj, hit.point, Quaternion.identity, scatterTool.scatteredObjects.transform);

                        // Randomize size
                        if (scatterTool.randomizeSize)
                        {
                            if(scatterTool.minSize < 0.01f)
                            {
                                Debug.LogWarning("Min size is set lower than 0.01f!");
                            }

                            if (scatterTool.maxSize < 0.01f)
                            {
                                Debug.LogWarning("Max size is set lower than 0.01f!");
                            }

                            float sizeScale = Random.Range(scatterTool.minSize, scatterTool.maxSize);
                            scatteredObj.transform.localScale = new Vector3(sizeScale, sizeScale, sizeScale);
                        }

                        
                        scatteredObj.transform.up = hit.normal;

                        // Randomize selected rotations

                        if (scatterTool.randomizeRotationX)
                        {
                            scatteredObj.transform.Rotate(Vector3.right, Random.Range(0, 360));
                        }

                        if (scatterTool.randomizeRotationY)
                        {
                            scatteredObj.transform.Rotate(Vector3.up, Random.Range(0, 360));
                        }

                        if (scatterTool.randomizeRotationZ)
                        {
                            scatteredObj.transform.Rotate(Vector3.forward, Random.Range(0, 360));
                        }

                        string categoryName = scatteredObj.name.Substring(0, scatteredObj.name.Length - 7);

                        GameObject objCategory = null;

                        // If no existing category object is found create new one and set as parent
                        if (scatterTool.scatteredObjects.transform.Find(categoryName) == null)
                        {
                            objCategory = new GameObject(categoryName);
                            objCategory.transform.parent = scatterTool.scatteredObjects.transform;
                        }
                        else
                        {
                            objCategory = scatterTool.scatteredObjects.transform.Find(categoryName).gameObject;
                        }

                        scatteredObj.transform.parent = objCategory.transform;
                        
                        Debug.Log("Instantiated " + scatteredObj.name);
                    }
                    else
                    {
                        Debug.Log("Selected scatter object is null!");
                    }
                }
                else
                {
                    Debug.Log("Scatter object list is empty! Load at least 1 object.");
                }
            }
        }
    }

    private void DrawGizmos()
    {
        if (hit.collider != null)
        {
            Handles.color = Color.green;
            Handles.DrawWireDisc(hit.point, hit.normal, 0.5f);
            Handles.DrawLine(hit.point, hit.point + hit.normal * 2);
        }
    }

    public override void OnInspectorGUI()
    {
        // Draw default inspector as base
        DrawDefaultInspector();

        // Button to open object picker window
        if (GUILayout.Button("Load asset", GUILayout.Height(30)))
        {
            currentPickerWindow = EditorGUIUtility.GetControlID(FocusType.Passive) + 100;
            EditorGUIUtility.ShowObjectPicker<GameObject>(null, false, "", currentPickerWindow);
        }

        // Button to select folder to load assets from
        if(GUILayout.Button("Load asset folder", GUILayout.Height(30)))
        {
            string appPath = Application.dataPath;
            string path = EditorUtility.OpenFolderPanel("Load object folder", "", "");
            path = path.Replace(appPath, "Assets");
            LoadObjectsFromFolder(path);
        }

        // Called when object is picked from object picker window
        if (Event.current.commandName == "ObjectSelectorUpdated" && EditorGUIUtility.GetObjectPickerControlID() == currentPickerWindow)
        {
            GameObject addedObj = EditorGUIUtility.GetObjectPickerObject() as GameObject;
            //Debug.Log(AssetDatabase.GetAssetPath(addedObj));

            // If addedObj is not already in the list add it
            if (!scatterTool.scatterObjects.Contains(addedObj))
            {
                scatterTool.scatterObjects.Add(addedObj);
                scatterTool.selectedScatterObj = scatterTool.scatterObjects.Last(); // select the just added object
                Debug.Log("Loaded asset: " + addedObj.name);
            }
            else
            {
                Debug.Log(addedObj.name + " is already in the scatter object list");
            }
            
            currentPickerWindow = -1;
        }

        if (GUILayout.Button("Clear objects", GUILayout.Height(30)))
        {
            ClearObjects();
        }

        if (GUILayout.Button("Unload selected asset", GUILayout.Height(30)))
        {
            if(scatterTool.selectedScatterObj != null)
            {
                GameObject unloadedObj = scatterTool.selectedScatterObj;
                scatterTool.scatterObjects.Remove(unloadedObj);

                Debug.Log("Unloaded: " + unloadedObj.name);

                // Update selectedScatterObj if list has objects
                if (scatterTool.scatterObjects.Count() > 0)
                {
                    scatterTool.selectedScatterObj = scatterTool.scatterObjects.First();
                    Debug.Log("Selected: " + scatterTool.selectedScatterObj.name);
                }
                else
                {
                    scatterTool.selectedScatterObj = null;
                }
            }
        }

        if (GUILayout.Button("Unload all assets", GUILayout.Height(30)))
        {
            scatterTool.scatterObjects.Clear();
            scatterTool.selectedScatterObj = null;
            Debug.Log("Unloaded all assets");
        }

        // Begin a horizontal row of UI items
        GUILayout.BeginHorizontal("box");
        int num = 0;
        foreach (GameObject scatterObj in scatterTool.scatterObjects)
        {
            if(scatterObj != null)
            {
                // Seperate rows of 3 UI elements
                if (num % 3 == 0)
                {
                    // End horizontal row of UI items and begin a new one
                    GUILayout.EndHorizontal();
                    GUILayout.BeginHorizontal("box");
                }

                // Get asset preview texture
                Texture2D buttonTex = AssetPreview.GetAssetPreview(scatterObj);
                GUIContent guiContent = new GUIContent();
                guiContent.image = buttonTex;
                //guiContent.text = scatterObj.name;
                guiContent.tooltip = scatterObj.name;

                // Create Button with asset preview texture as image 
                if (GUILayout.Button(guiContent, GUILayout.Width(100), GUILayout.Height(100)))
                {
                    // Left click on button
                    if(Event.current.button == 0)
                    {
                        Debug.Log("Selected: " + guiContent.tooltip);
                        scatterTool.selectedScatterObj = scatterTool.scatterObjects.Find(x => x.name == guiContent.tooltip);
                    }
                    // Right click on button
                    else if (Event.current.button == 1)
                    {

                    }
                }
                num++;
            }
        }

        GUILayout.EndHorizontal();
    }

    // Enable preview GUI
    public override bool HasPreviewGUI() 
    { 
        return true; 
    }

    public override void OnPreviewGUI(Rect r, GUIStyle background)
    {
        // Update inspector preview window

        //Debug.Log("OnPreviewGUI called");
        GameObject previewObj = scatterTool.selectedScatterObj;

        if (previewObj != null)
        {
            if (gameObjectEditor == null)
            {
                gameObjectEditor = CreateEditor(previewObj);
            }
            else
            {
                DestroyImmediate(gameObjectEditor);
                gameObjectEditor = CreateEditor(previewObj);
            }

            gameObjectEditor.OnPreviewGUI(r, background);
        }
    }

    // Set preview window title
    public override GUIContent GetPreviewTitle()
    {
        GUIContent guiContent = new GUIContent();

        if (scatterTool.selectedScatterObj != null)
        {
            guiContent.text = "Selected: " + scatterTool.selectedScatterObj.name;
        }
        else
        {
            //Debug.LogWarning("No scatter object selected for preview");
        }

        guiContent.tooltip = "Currently selected scatter object";
        return guiContent;
    }

    // Loads all .obj and .fbx files from assetPath to the tool
    void LoadObjectsFromFolder(string assetPath = "Assets/Objects")
    {
        var files = Directory.EnumerateFiles(assetPath, "*.*", SearchOption.AllDirectories)
            .Where(s => s.EndsWith(".obj") || s.EndsWith(".fbx"));

        List<string> filePaths = new List<string>();

        foreach (var file in files)
        {
            string fixedPath = file.Replace(@"\", "/");
            filePaths.Add(fixedPath);
            //Debug.Log(fixedPath);
        }

        if(filePaths.Count() > 0)
        {
            scatterTool.scatterObjects.Clear();

            foreach (string filePath in filePaths)
            {
                //Debug.Log(filePath);
                GameObject loadedAsset = AssetDatabase.LoadAssetAtPath(filePath, typeof(GameObject)) as GameObject;
                scatterTool.scatterObjects.Add(loadedAsset);
            }

            if (scatterTool.selectedScatterObj == null)
            {
                if (scatterTool.scatterObjects.Count() > 0)
                {
                    scatterTool.selectedScatterObj = scatterTool.scatterObjects.First();
                }
            }
            Debug.Log("Loaded " + filePaths.Count() + " files from folder: " + assetPath);
        }
        else
        {
            Debug.Log("No suitable files found at: " + assetPath);
        }
    }
    
    private void OnEnable()
    {
        //Debug.Log("OnEnable called");
        scatterTool = target as ScatterTool;

        //string assetPath = "Assets/Objects";
        //LoadObjectsFromFolder(assetPath);

        // Select item in slot 0 as default
        if (scatterTool.scatterObjects.Count > 0)
        {
            scatterTool.selectedScatterObj = scatterTool.scatterObjects[0];
        }
        else
        {
            Debug.Log("Add at least 1 asset to the scatter objects list.");
        }
    }
}
