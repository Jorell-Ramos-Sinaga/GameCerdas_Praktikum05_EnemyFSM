using UnityEngine;

public class CameraTargetFollow : MonoBehaviour
{
    public Transform player;

    void LateUpdate()
    {
        transform.position =
            player.position +
            Vector3.up * 1.5f;
    }
}