using UnityEngine;

public class camera_movement : MonoBehaviour
{
    private Vector3 CameraPosition;
    
    [Header("Camera Settings")]
    public float CameraSpeed;
    public float RotateSpeed;

    void Start()
    {
        CameraPosition = this.transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        float rotateDir = 0f;

        if (Input.GetKey(KeyCode.W)) {
            CameraPosition.z -= CameraSpeed / 10;
        }
        if (Input.GetKey(KeyCode.S)) {
            CameraPosition.z += CameraSpeed / 10;
        }
        if (Input.GetKey(KeyCode.A)) {
            CameraPosition.x -= CameraSpeed / 10;
        }
        if (Input.GetKey(KeyCode.D)) {
            CameraPosition.x += CameraSpeed / 10;
        }
        if (Input.GetKey(KeyCode.Space)) {
            CameraPosition.y += CameraSpeed / 10;
        }
        if (Input.GetKey(KeyCode.LeftControl)) {
            CameraPosition.y -= CameraSpeed / 10;
        }

        if (Input.GetKey(KeyCode.Q)) {
            rotateDir = +1f;
        }
        if (Input.GetKey(KeyCode.E)) {
            rotateDir = -1f;
        }

        this.transform.eulerAngles += new Vector3(0, rotateDir * RotateSpeed, 0);
        this.transform.position = CameraPosition;
    }
}
