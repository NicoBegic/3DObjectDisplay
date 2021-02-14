using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DisplayObject : MonoBehaviour
{
    public GameObject ObjectDisplayOverlay;
    public GameObject InputText;
    public Transform DisplayPos;
    public GameObject InfoBox;
    public Image BackgroundImg;
    public RenderTexture RenderCapTex;

    private GameObject displayedObject;
    private GameObject pickableObject;

    private const float MOVE_SPEED = 2;
    private const float ROT_SPEED = 80;
    private const int ROOM_SIZE = 10;
    private Vector3 DISPLAY_POS = new Vector3(-10, 0, -10);
    private Vector3 lastPos;
    private Quaternion lastRotation;

    //ObjectRotation
    private Vector3 prevPos = Vector3.zero;
    private Vector3 deltaPos = Vector3.zero;
    private bool rotateToTarget = false;
    private Quaternion targetRotation;

    private InfoPoint lastTickedInfP;
    private int lastTickedIndex;

    // Update is called once per frame
    void Update()
    {
        //Normal PlayView
        if (!ObjectDisplayOverlay.activeInHierarchy)
        {
            Move();
            if (Input.GetKeyDown(KeyCode.Space))
                PickUpObject();
        }//DisplayView
        else
            DisplayViewUpdate();
    }

    private void DisplayViewUpdate()
    {
        //ObjectRotation
        if (Input.GetMouseButton(0))
        {
            deltaPos = Input.mousePosition - prevPos;
            RotateDisplayObject();
        }
        prevPos = Input.mousePosition;
        //InfoPoint Input Logic
        if ((Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.UpArrow)) && !rotateToTarget)
        {
            ObjectController dispCtr = displayedObject.GetComponent<ObjectController>();
            GameObject infoPoint = null;
            if (Input.GetKeyDown(KeyCode.DownArrow))
            {
                int newIndex = (lastTickedIndex > 0) ? lastTickedIndex - 1 : dispCtr.InfoPoints.Length - 1;
                infoPoint = dispCtr.InfoPoints[newIndex].gameObject;
                lastTickedIndex = newIndex;
            }
            if (Input.GetKeyDown(KeyCode.UpArrow))
            {
                int newIndex = (lastTickedIndex < dispCtr.InfoPoints.Length - 1) ? lastTickedIndex + 1 : 0;
                infoPoint = dispCtr.InfoPoints[newIndex].gameObject;
                lastTickedIndex = newIndex;
            }
            if (infoPoint)
                ShowInfoPoint(infoPoint, Input.GetKeyDown(KeyCode.UpArrow));
        }
        //Exit out of DisplayView
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (displayedObject)
                Destroy(displayedObject);
            ObjectDisplayOverlay.SetActive(false);
            transform.position = lastPos;
            transform.rotation = lastRotation;
            prevPos = Vector3.zero;
            deltaPos = Vector3.zero;
            InfoBox.SetActive(false);
        }
        //Rotate Target around to infoPoint
        if (rotateToTarget)
        { 
            displayedObject.transform.rotation = Quaternion.RotateTowards(displayedObject.transform.rotation, targetRotation, Time.deltaTime * ROT_SPEED);
            rotateToTarget = (displayedObject.transform.rotation != targetRotation);
        }
    }

    private void PickUpObject()
    {
        if (pickableObject && pickableObject.CompareTag("pickable"))
        {
            CamCapture();
            ObjectDisplayOverlay.gameObject.SetActive(true);
            print(pickableObject.name + " has been picked!");
            displayedObject = Instantiate(pickableObject, DisplayPos);
            displayedObject.transform.localPosition = Vector3.zero;
            //Skallieren
            ObjectController picCtr = pickableObject.GetComponent<ObjectController>();
            displayedObject.transform.localScale = new Vector3(picCtr.DisplaySizeMultiplier, picCtr.DisplaySizeMultiplier, picCtr.DisplaySizeMultiplier);

            pickableObject = null;
            lastPos = transform.position;
            lastRotation = transform.rotation;
            transform.position = DISPLAY_POS;
            transform.rotation = Quaternion.identity;
            InputText.SetActive(false);
            displayedObject.GetComponent<ObjectController>().UIContainer.SetActive(true);
        }
    }

    void CamCapture()
    {
        Camera Cam = Camera.main;
        Cam.targetTexture = RenderCapTex;

        RenderTexture currentRT = RenderTexture.active;
        RenderTexture.active = Cam.targetTexture;

        Cam.Render();
        Texture2D Image = new Texture2D(Cam.targetTexture.width, Cam.targetTexture.height);
        Image.ReadPixels(new Rect(0, 0, Cam.targetTexture.width, Cam.targetTexture.height), 0, 0);
        Image.Apply();
        RenderTexture.active = currentRT;
        BackgroundImg.sprite = Sprite.Create(Image, new Rect(0, 0, Image.width, Image.height), new Vector2(0.5f, 0.5f));

        Cam.targetTexture = null;
    }

    private void Move()
    {
        float rotate = Input.GetAxis("Horizontal") * MOVE_SPEED * Time.deltaTime;
        float moveZ = Input.GetAxis("Vertical") * MOVE_SPEED * Time.deltaTime;
        transform.Rotate(new Vector3(0, rotate * 50, 0));
        Vector3 newPos = transform.position + (transform.rotation * new Vector3(0, 0, moveZ));
        if(newPos.x > ROOM_SIZE || newPos.z > ROOM_SIZE || newPos.x < 0 || newPos.z < 0)
        {
            if(newPos.x > ROOM_SIZE || newPos.x < 0)
                newPos.x = (newPos.x > ROOM_SIZE) ? ROOM_SIZE : 0;
            else
                newPos.z = (newPos.z > ROOM_SIZE) ? ROOM_SIZE : 0;
            transform.position = newPos;
        }
        else
            transform.Translate(0, 0, moveZ);
    }

    private void RotateDisplayObject()
    {
        float lrDot = -Vector3.Dot(deltaPos, Camera.main.transform.right);
        float udDot = Vector3.Dot(deltaPos, Camera.main.transform.up);
        displayedObject.transform.Rotate(transform.up, lrDot, Space.World);
        displayedObject.transform.Rotate(Camera.main.transform.right, udDot, Space.World);
    }

    private void ShowInfoPoint(GameObject infoPoint, bool forward)
    {
        InfoBox.SetActive(true);
        Text info = InfoBox.GetComponentInChildren<Text>();
         
        //new
        Vector2 lastPos = new Vector2(-displayedObject.transform.forward.x, -displayedObject.transform.forward.z);
        if(lastTickedInfP)
            lastPos = new Vector2(lastTickedInfP.transform.localPosition.x, lastTickedInfP.transform.localPosition.z);
        lastPos.Normalize();
        Vector2 newPos = new Vector2(infoPoint.transform.localPosition.x, infoPoint.transform.localPosition.z);
        newPos.Normalize();
        float rotAngle = Vector2.Angle(lastPos, newPos);
        if ((forward && rotAngle < 0) || (!forward && rotAngle > 0))
            rotAngle *= -1;
        targetRotation = Quaternion.AngleAxis(rotAngle, new Vector3(0, 1, 0)) * displayedObject.transform.rotation;
        rotateToTarget = true;

        if (lastTickedInfP)
            lastTickedInfP.ChangeMat(false);
        lastTickedInfP = infoPoint.GetComponent<InfoPoint>();
        info.text = lastTickedInfP.InfoText;
        lastTickedInfP.ChangeMat(true);
    }


    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("pickable") && !ObjectDisplayOverlay.activeInHierarchy)
        {
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
