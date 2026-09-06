using UnityEngine;

public class CarOrbitCamera : MonoBehaviour
{
    public Transform target;       // Drag your car here in the Inspector
    public float distance = 6f;    // Distance from the car
    public float xSpeed = 200f;    // Horizontal rotation speed
    public float ySpeed = 200f;    // Vertical rotation speed
    public float yMinLimit = -10f; // Lowest angle look-down
    public float yMaxLimit = 60f;  // Highest angle look-up

    private float x = 0f;
    private float y = 0f;

    void Start()
    {
        Vector3 angles = transform.eulerAngles;
        x = angles.y;
        y = angles.x;
    }

    void LateUpdate()
    {
        if (!target) return;

        // Orbit only when holding the right mouse button (change to Input.GetAxis("Mouse X") if you want it always rotating)
        //if (Input.GetMouseButton(1))
        {
            x += Input.GetAxis("Mouse X") * xSpeed * Time.deltaTime;
            y -= Input.GetAxis("Mouse Y") * ySpeed * Time.deltaTime;
            y = Mathf.Clamp(y, yMinLimit, yMaxLimit);
        }

        // Calculate rotation and position around the car
        Quaternion rotation = Quaternion.Euler(y, x, 0);
        Vector3 position = rotation * new Vector3(0.0f, 0.0f, -distance) + target.position + Vector3.up * 1.5f;

        transform.rotation = rotation;
        transform.position = position;
    }
}