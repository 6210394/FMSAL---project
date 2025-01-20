using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EditModeScript : MonoBehaviour
{
    public List<buildingObjects> objects;
    public buildingObjects currentObject;
    Vector3 currentPos;

    public Transform currentPreviewPos;
    public Transform cam;
    public RaycastHit hit;
    public LayerMask layerMask;

    public float offset = 1f;
    public float currentGridSize = 1f;
    public float[] gridSizes = new float[] { 0.1f, 0.5f, 1f };

    public bool isBuilding = false;

    void Start()
    {
        foreach(buildingObjects obj in objects)
        {
            Debug.Log(obj.name);
        }
            
        currentObject = objects[0];
        ChangeCurrentBuilding();
    }


    void Update()
    {
        if(isBuilding)
        {
            startPreview();
            if(Input.GetKeyDown(KeyCode.V))
            {
                
            }
        }

        
    }

    public void ChangeCurrentBuilding()
    {
        GameObject curprev = Instantiate(currentObject.preview, currentPos, Quaternion.identity);
        currentPreviewPos = curprev.transform;
    }

    public void startPreview()
    {
        if(Physics.Raycast(cam.position, cam.forward, out hit, 100, layerMask))
        {
            if(hit.transform != transform)
            {
                Debug.Log(hit.point);
                showPreview(hit);
            }
        }
    }

    public void showPreview(RaycastHit hit)
    {
        currentPos = hit.point;
        currentPos -= Vector3.one * offset;
        currentPos /= currentGridSize;
        currentPos = new Vector3(Mathf.Round(currentPos.x), Mathf.Round(currentPos.y), Mathf.Round(currentPos.z));
        currentPos *= currentGridSize;
        currentPos += Vector3.one * offset;

        currentPreviewPos.position = currentPos;
    }
}

[System.Serializable]
public class buildingObjects
{
    public string name;
    public GameObject preview;
    public GameObject buildingObject;
}

