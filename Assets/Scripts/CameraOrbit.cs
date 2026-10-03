using UnityEngine;

public class CameraOrbit : MonoBehaviour
{
    public Transform target;

    public float mouseSensitivity = 3f;
    public float clampAngle = 70f;

    private float rotX;
    private float rotY;

    private void Start()
    {
        rotX = target.eulerAngles.x;
        rotY = target.eulerAngles.y;
    }

    private void LateUpdate()
    {
        if (Input.GetMouseButton(1))
        {
            float mouseX = Input.GetAxis("Mouse X");
            float mouseY = Input.GetAxis("Mouse Y");

            rotY += mouseX * mouseSensitivity;
            rotX -= mouseY * mouseSensitivity;

            rotX = Mathf.Clamp(rotX, -clampAngle, clampAngle);

            target.rotation = Quaternion.Euler(rotX, rotY, 0f);
        }
    }
}