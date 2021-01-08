using System.Collections;
using System.Collections.Generic;
using UnityEngine;
// reference:https://blog.csdn.net/qgy1677/article/details/78871617
public class MouseLookTest : MonoBehaviour
{
    public float distance = 0;

    public Transform player;
    private Vector3 offset;
    private float scrollSpeed = 5;
    private bool isRotating = false;
    private GameObject lookOBJ;
    // Use this for initialization
    void Start()
    {
        lookOBJ = new GameObject();
        lookOBJ.transform.position = player.position;
        transform.LookAt(lookOBJ.transform);
        offset = transform.position - player.position;
    }

    // Update is called once per frame
    void Update()
    {
        transform.LookAt(lookOBJ.transform);
        transform.position = player.position + offset;
        RotateView();
        ScrollView();
        TransformView();
    }

    void TransformView()
    {
        if (Input.GetKey(KeyCode.W))
        {
            lookOBJ.transform.position = lookOBJ.transform.position + new Vector3(0,scrollSpeed*0.005f,0);
            this.transform.position = this.transform.position + new Vector3(0, scrollSpeed * 0.005f, 0);
        }
        if (Input.GetKey(KeyCode.S))
        {
            lookOBJ.transform.position = lookOBJ.transform.position - new Vector3(0, scrollSpeed * 0.005f, 0);
            this.transform.position = this.transform.position - new Vector3(0, scrollSpeed * 0.005f, 0);
        }
        if (Input.GetKey(KeyCode.A))
        {
            lookOBJ.transform.position = lookOBJ.transform.position + new Vector3(scrollSpeed * 0.005f, 0, 0);
            this.transform.position = this.transform.position + new Vector3(scrollSpeed * 0.005f, 0, 0);
        }
        if (Input.GetKey(KeyCode.D))
        {
            lookOBJ.transform.position = lookOBJ.transform.position - new Vector3(scrollSpeed * 0.005f, 0, 0);
            this.transform.position = this.transform.position - new Vector3(scrollSpeed * 0.005f, 0, 0);
        }
    }

    void ScrollView()
    {
        distance = offset.magnitude;
        distance -= Input.GetAxis("Mouse ScrollWheel") * scrollSpeed;
        distance = Mathf.Clamp(distance, -5, 18);
        offset = offset.normalized * distance;
    }

    void RotateView()
    {
        if (Input.GetMouseButtonDown(1))
        {
            isRotating = true;
        }
        
        if (Input.GetMouseButtonUp(1))
        {
            isRotating = false;
        }

        if (isRotating)
        {
            Vector3 originalPos = transform.position;
            Quaternion originalRotation = transform.rotation;

            transform.RotateAround(player.position, player.up, Input.GetAxis("Mouse X"));
            transform.RotateAround(player.position, transform.right, -Input.GetAxis("Mouse Y"));
            // 影响了position和rotation

            //限制视角上下移动的范围
            float x = transform.eulerAngles.x;
            if (x < 10 || x > 80)
            {
                transform.position = originalPos;
                transform.rotation = originalRotation;
            }
        }

        offset = transform.position - player.position;
    }
}
