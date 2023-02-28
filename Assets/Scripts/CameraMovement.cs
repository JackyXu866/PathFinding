using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    Camera cam;
    void Start()
    {
        cam = GetComponent<Camera>();
    }

    // Update is called once per frame
    void Update()
    {
        float x = Input.GetAxis("Horizontal");
        float y = Input.GetAxis("Vertical");
        float z = Input.GetAxis("Mouse ScrollWheel");

        cam.orthographicSize -= z * 10;
        transform.Translate(new Vector3(x, y, 0) * Time.deltaTime * 2 * cam.orthographicSize);
    }
}
