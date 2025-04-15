using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EditModeScript : MonoBehaviour
{
    //0.1 GRID CAUSES PREVIEW TO BE SLIGHTLY ELEVATED

    public List<buildingObjects> objects;
    public buildingObjects currentObject;
    Vector3 currentPos;
    Quaternion currentRot = Quaternion.identity;

    public Transform currentPreviewPos;
    public Transform cam;
    public RaycastHit hit;
    public LayerMask layerMask;

    public float offset = 1f;
    public float currentGridSize = 1f;
    public float[] gridSizes = new float[] { 0.1f, 0.5f, 1f };

    public bool isBuilding = false;
    private int currentGridSizeIndex = 0;


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
        if(Input.GetKeyDown(KeyCode.B)) //Toggle Building Mode
        {
            isBuilding = !isBuilding;
        }

        if(isBuilding)
        {
            startPreview();
            if(Input.GetKeyDown(KeyCode.V)) //Cycle Through Grid Sizes
            {
                CycleGridSize();
            }
            if(Input.GetAxis("Mouse ScrollWheel") > 0) //Rotate Object Right
            {
                currentRot *= Quaternion.Euler(0, GetRotationBasedOnGridSize(), 0);
            }
            if(Input.GetAxis("Mouse ScrollWheel") < 0) //Rotate Object Left
            {
                currentRot *= Quaternion.Euler(0, -GetRotationBasedOnGridSize(), 0);
            }
            if(Input.GetMouseButtonDown(0)) //Place Object
            {
                Instantiate(currentObject.buildingObject, currentPos, currentRot);
            }
        }
    }

    private void CycleGridSize()
    {
        currentGridSizeIndex = (currentGridSizeIndex + 1) % gridSizes.Length;
        currentGridSize = gridSizes[currentGridSizeIndex];
        Debug.Log("Current Grid Size: " + currentGridSize);
    }
    private float GetRotationBasedOnGridSize()
    {
        float rotationAngle = 0f;
        switch (currentGridSize)
        {
            case 0.1f:
                rotationAngle = 5f;
                break;
            case 0.5f:
                rotationAngle = 15f;
                break;
            case 1f:
                rotationAngle = 45f;
                break;
        }
        return rotationAngle;
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
        currentPos -= currentObject.pivotPoint;
        currentPos /= currentGridSize;
        currentPos = new Vector3(Mathf.Round(currentPos.x), Mathf.Round(currentPos.y), Mathf.Round(currentPos.z));
        currentPos *= currentGridSize;

        currentPreviewPos.position = currentPos;
        currentPreviewPos.rotation = currentRot;
    }
}

[System.Serializable]
public class buildingObjects
{
    public string name;
    public Vector3 pivotPoint; //should be at the bottom middle of the model, used to spawn the model at the correct height and center
    public GameObject preview;
    public GameObject buildingObject;
}

