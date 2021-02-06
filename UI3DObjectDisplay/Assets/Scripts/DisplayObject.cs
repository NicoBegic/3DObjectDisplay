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

    //ObjectRotation
    private Vector3 prevPos = Vector3.zero;
    private Vector3 deltaPos = Vector3.zero;
    private bool rotateToTarget = false;
    private Quaternion targetRotation;

    private InfoPoint lastTickedInfP;

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
        {
            //ObjectRotation
            if (Input.GetMouseButton(0))
            {
                deltaPos = Input.mousePosition - prevPos;
                RotateDisplayObject();
            }
            prevPos = Input.mousePosition;
            //InfoPoint Logic
            if (Input.GetMouseButtonDown(1))
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;
                if (Physics.Raycast(ray, out hit) && hit.transform.CompareTag("infoPoint"))
                    ShowInfoPoint(hit.transform.gameObject);
            }
            //Exit out of DisplayView
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (displayedObject)
                    Destroy(displayedObject);
                ObjectDisplayOverlay.SetActive(false);
                transform.position = lastPos;
                prevPos = Vector3.zero;
                deltaPos = Vector3.zero;
                InfoBox.SetActive(false);
            }

            if (rotateToTarget)
            {
                displayedObject.transform.rotation = Quaternion.RotateTowards(displayedObject.transform.rotation, targetRotation, Time.deltaTime * ROT_SPEED);
                rotateToTarget = (displayedObject.transform.rotation != targetRotation);
            }
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
            pickableObject = null;
            lastPos = transform.position;
            transform.position = DISPLAY_POS;
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
        float moveX = Input.GetAxis("Horizontal") * MOVE_SPEED * Time.deltaTime;
        float moveZ = Input.GetAxis("Vertical") * MOVE_SPEED * Time.deltaTime;
        Vector3 newPos = transform.position + new Vector3(moveX, 0, moveZ);
        if(!(newPos.x > ROOM_SIZE || newPos.z > ROOM_SIZE || newPos.x < 0 || newPos.z < 0))
            transform.Translate(moveX, 0, moveZ);
    }

    private void RotateDisplayObject()
    {
        float lrDot = -Vector3.Dot(deltaPos, Camera.main.transform.right);
        float udDot = Vector3.Dot(deltaPos, Camera.main.transform.up);
        displayedObject.transform.Rotate(transform.up, lrDot, Space.World);
        displayedObject.transform.Rotate(Camera.main.transform.right, udDot, Space.World);
    }

    private void ShowInfoPoint(GameObject infoPoint)
    {
        InfoBox.SetActive(true);
        Text info = InfoBox.GetComponentInChildren<Text>();

        if (lastTickedInfP)
            lastTickedInfP.ChangeMat(false);
        lastTickedInfP = infoPoint.GetComponent<InfoPoint>();
        info.text = lastTickedInfP.InfoText;
        lastTickedInfP.ChangeMat(true);

        Transform parentTrans = displayedObject.transform;
        Transform childTrans = infoPoint.transform;
        Transform targetTrans = Camera.main.transform;

        Vector3 rightAnglePoint = Vector3.Project(parentTrans.position - childTrans.position, childTrans.forward * 5); //Get point to create 90 degree angle for right angle
        rightAnglePoint = childTrans.position + rightAnglePoint; //transform point to world space
        float sideC = Vector3.Distance(parentTrans.position, targetTrans.position); //Get hypotenuse
        float sideA = Vector3.Distance(parentTrans.position, rightAnglePoint); //Get sideA
        float sideB = Mathf.Sqrt((sideC * sideC) - (sideA * sideA)); //Get sideB. (C squared - A squared = B squared)

        Vector3 desiredRelTargetPos = rightAnglePoint + (childTrans.forward * sideB); //relative target point (if target were to rotate around parent to align with child's forward direction)
        Vector3 parentToTargetDir = targetTrans.position - parentTrans.position; //parent to target position direction
        Vector3 parentToRelTargetDir = desiredRelTargetPos - parentTrans.position; //parent to desired target position relative to setup

        Vector3 rotAxis = Vector3.Cross(parentToTargetDir, parentToRelTargetDir); //get rotation axis
        float rotAngle = Mathf.Sqrt(Vector3.Dot(parentToTargetDir, parentToTargetDir) * Vector3.Dot(parentToRelTargetDir, parentToRelTargetDir)) + Vector3.Dot(parentToTargetDir, parentToRelTargetDir); //Get rotation angle
        Quaternion inverseRot = new Quaternion(rotAxis.x, rotAxis.y, rotAxis.z, rotAngle).normalized; //Construct new Quaternion

        targetRotation = Quaternion.Inverse(inverseRot) * parentTrans.rotation;
        rotateToTarget = true;
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
