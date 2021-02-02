using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class DisplayObject : MonoBehaviour
{
    public GameObject ObjectDisplayOverlay;
    public GameObject InputText;
    public Transform DisplayPos;

    private GameObject displayedObject;
    private GameObject pickableObject;
    private float MovementSpeed = 2;

    private Vector3 DISPLAY_POS = new Vector3(-10, 0, -10);
    private Vector3 lastPos;

    private const int ROOM_SIZE = 10;

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space) && ObjectDisplayOverlay.activeInHierarchy == false)
        {
            PickUpObject();
        }
        if (Input.GetKeyDown(KeyCode.Escape) && ObjectDisplayOverlay.activeInHierarchy)
        {
            if(displayedObject)
                Destroy(displayedObject);
            ObjectDisplayOverlay.SetActive(false);
            transform.position = lastPos;
        }

        if(ObjectDisplayOverlay.activeInHierarchy == false)
            Move();
    }

    /*
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
    }*/

    private void PickUpObject()
    {
        if (ObjectDisplayOverlay.activeInHierarchy == false && pickableObject && pickableObject.CompareTag("pickable"))
        {
            ObjectDisplayOverlay.gameObject.SetActive(true);
            print(pickableObject.name + " has been picked!");
            displayedObject = Instantiate(pickableObject, DisplayPos);
            displayedObject.transform.localPosition = Vector3.zero;
            pickableObject = null;
            lastPos = transform.position;
            transform.position = DISPLAY_POS;
            InputText.SetActive(false);
        }
    }

    private void Move()
    {
        float moveX = Input.GetAxis("Horizontal") * MovementSpeed * Time.deltaTime;
        float moveZ = Input.GetAxis("Vertical") * MovementSpeed * Time.deltaTime;
        Vector3 newPos = transform.position + new Vector3(moveX, 0, moveZ);
        if(!(newPos.x > ROOM_SIZE || newPos.z > ROOM_SIZE || newPos.x < 0 || newPos.z < 0))
            transform.Translate(moveX, 0, moveZ);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("pickable") && !ObjectDisplayOverlay.activeInHierarchy)
        {
            //UI Text zum aufheben anzeigen
            InputText.SetActive(true);
            pickableObject = other.gameObject;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("pickable") && !ObjectDisplayOverlay.activeInHierarchy)
        {
            InputText.SetActive(false);
            pickableObject = null;
        }
    }
}
