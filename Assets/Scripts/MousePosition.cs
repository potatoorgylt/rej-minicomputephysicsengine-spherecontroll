using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MousePosition : MonoBehaviour
{
    private Camera cam;

    void Start()
    {
        cam = Camera.main;
    }

    Vector3 point = new Vector3();
    Vector3 startPoint = Vector3.zero;

    Vector2 mousePos = new Vector2();
    Event currentEvent;

    void OnGUI()
    {
        currentEvent = Event.current;

        if (Input.GetMouseButton(0))
        {
            point = new Vector3(startPoint.x - mousePos.x, startPoint.y - mousePos.y, 0f) / 10f;

            SetStartPoint(new Vector3(mousePos.x, mousePos.y, 0f));
        }
        else if(Input.GetMouseButton(1))
        {
            point = new Vector3(0f, 0f, startPoint.z - mousePos.y) / -10f;

            SetStartPoint(new Vector3(0f, 0f, mousePos.y));
        }
        else 
        {
            startPoint = Vector3.zero;
            point = Vector3.zero;
        }
    }

    private void SetStartPoint(Vector3 startPoint)
    {
        mousePos.x = currentEvent.mousePosition.x;
        mousePos.y = cam.pixelHeight - currentEvent.mousePosition.y;

        if (this.startPoint == Vector3.zero)
        {
            this.startPoint = startPoint;
        }
    }

    public Vector3 MousePosPoint()
    {
        return point;
    }
}
