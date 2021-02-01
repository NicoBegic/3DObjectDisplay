using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DisplayObject : MonoBehaviour
{
    public GameObject ObjectDisplayOverlay;
    public Transform DisplayPos;

    private GameObject displayedObject;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0) && ObjectDisplayOverlay.activeInHierarchy == false)
        {
            CheckIfRayCastHit();
        }
        else if (Input.GetKeyDown(KeyCode.Escape) && ObjectDisplayOverlay.activeInHierarchy)
        {
            if(displayedObject)
                Destroy(displayedObject);
            ObjectDisplayOverlay.SetActive(false);
        }
    }

    void CheckIfRayCastHit()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit))
        {
            if (hit.transform.CompareTag("clickable"))
            {
                ObjectDisplayOverlay.gameObject.SetActive(true);
                print(hit.collider.gameObject.name + " has been clicked!");
                displayedObject = Instantiate(hit.transform.gameObject,  DisplayPos);
                displayedObject.transform.localPosition = Vector3.zero;
            }
        }
    }
}
