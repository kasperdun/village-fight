using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using Photon.Pun;

public class CameraHandler : MonoBehaviour
{

    public static float PanSpeed = 30f;
    public static float ZoomSpeedTouch = 0.1f;
    public static float ZoomSpeedMouse = 10f;
    public static bool DontMove = false;

    private float[] BoundsX = new float[] { 0f, 60f };
    private float[] BoundsZ = new float[] { -10f, 75f };
    private float[] ZoomBounds = new float[] { 10f, 55f };

    private Camera cam;

    private Vector3 lastPanPosition;
    private int panFingerId; // Touch mode only

    private bool wasZoomingLastFrame; // Touch mode only
    private Vector2[] lastZoomPositions; // Touch mode only

    private Vector3 defaultBottomPos = new Vector3(30,20,0);
    private Vector3 defaultTopPos = new Vector3(30,20,100);
    void Awake()
    {
        cam = GetComponent<Camera>();
        if (PhotonNetwork.LocalPlayer.CustomProperties["Team"].ToString() == "Top")
        {
            cam.transform.position = defaultTopPos;
            cam.transform.rotation = Quaternion.Euler(45, 180, 0);
            BoundsZ = new float[] { 25f, 100f };
        }
        else
        {
            cam.transform.position = defaultBottomPos;
            cam.transform.rotation = Quaternion.Euler(45, 0, 0);
            BoundsZ = new float[] { -10f, 75f };
        }
    }

    void Update()
    {
        if (Input.touchSupported && Application.platform != RuntimePlatform.WebGLPlayer)
        {
            HandleTouch();
        }
        else
        {
            HandleMouse();
        }
    }

    void HandleTouch()
    {
        switch (Input.touchCount)
        {

            case 1: // Panning
                wasZoomingLastFrame = false;

                // If the touch began, capture its position and its finger ID.
                // Otherwise, if the finger ID of the touch doesn't match, skip it.
                Touch touch = Input.GetTouch(0);
                if (touch.phase == TouchPhase.Began)
                {
                    panFingerId = touch.fingerId;

                    if (EventSystem.current.IsPointerOverGameObject())
                    {
                        DontMove = true;
                        return;
                    }
                    lastPanPosition = touch.position;
                }
                else if (touch.fingerId == panFingerId && touch.phase == TouchPhase.Moved)
                {
                    if(!DontMove)
                        PanCamera(touch.position);
                }
                else if(touch.fingerId == panFingerId && touch.phase == TouchPhase.Ended)
                {
                    DontMove = false;
                }
                break;

            case 2: // Zooming
                Vector2[] newPositions = new Vector2[] { Input.GetTouch(0).position, Input.GetTouch(1).position };
                if (!wasZoomingLastFrame)
                {
                    lastZoomPositions = newPositions;
                    wasZoomingLastFrame = true;
                }
                else
                {
                    // Zoom based on the distance between the new positions compared to the 
                    // distance between the previous positions.
                    float newDistance = Vector2.Distance(newPositions[0], newPositions[1]);
                    float oldDistance = Vector2.Distance(lastZoomPositions[0], lastZoomPositions[1]);
                    float offset = newDistance - oldDistance;

                    ZoomCamera(offset, ZoomSpeedTouch);

                    lastZoomPositions = newPositions;
                }
                break;

            default:
                wasZoomingLastFrame = false;
                break;
        }
    }

    void HandleMouse()
    {
        // On mouse down, capture it's position.
        // Otherwise, if the mouse is still down, pan the camera.
        if (Input.GetMouseButtonDown(0))
        {
            if (EventSystem.current.IsPointerOverGameObject())
            {
                DontMove = true;
                return;
            }
            lastPanPosition = Input.mousePosition;
        }
        else if (Input.GetMouseButton(0))
        {
            if(!DontMove)
                PanCamera(Input.mousePosition);
        }
        else if (Input.GetMouseButtonUp(0))
        {
            DontMove = false;
        }

        // Check for scrolling to zoom the camera
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        ZoomCamera(scroll, ZoomSpeedMouse);
    }

    void PanCamera(Vector3 newPanPosition)
    {
        // Determine how much to move the camera
        Vector3 offset = cam.ScreenToViewportPoint(lastPanPosition - newPanPosition);
        // Vector3 move = new Vector3(offset.x * PanSpeed, 0, offset.y * PanSpeed);

        // Perform the movement
        var forward = cam.transform.forward;
        forward.y = 0;
        forward.Normalize();

        transform.Translate(forward * offset.y * PanSpeed, Space.World);
        transform.Translate(cam.transform.right * offset.x * PanSpeed, Space.World);

        // Ensure the camera remains within bounds.
        Vector3 pos = transform.position;
        pos.x = Mathf.Clamp(transform.position.x, BoundsX[0], BoundsX[1]);
        pos.z = Mathf.Clamp(transform.position.z, BoundsZ[0], BoundsZ[1]);
        transform.position = pos;

        // Cache the position
        lastPanPosition = newPanPosition;
    }

    void ZoomCamera(float offset, float speed)
    {
        if (offset == 0)
        {
            return;
        }

        cam.fieldOfView = Mathf.Clamp(cam.fieldOfView - (offset * speed), ZoomBounds[0], ZoomBounds[1]);
    }
}