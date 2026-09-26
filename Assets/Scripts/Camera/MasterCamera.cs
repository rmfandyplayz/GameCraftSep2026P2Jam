using UnityEngine;

public class MasterCamera : MonoBehaviour
{
    [Header("Follow Settings")]
    public Transform target;
    public float followSpeed = 5f;

    [Header("Dead Pan Zone")]
    public Vector2 deadZoneSize = new Vector2(3f, 2f);

    [Header("Camera Boundaries")]
    public float minX = -10f;
    public float maxX = 10f;
    public float minY = -10f;
    public float maxY = 10f;

    private Camera cameraComponent;

    private void Start()
    {
        cameraComponent = GetComponent<Camera>();
    }

    private void FixedUpdate()
    {
        if (target == null)
            return;

        Vector3 newPosition = transform.position;
        Vector2 distance = target.position - transform.position;
        Vector2 halfDeadZone = deadZoneSize / 2f;

        if (distance.x > halfDeadZone.x)
            newPosition.x = target.position.x - halfDeadZone.x;
        else if (distance.x < -halfDeadZone.x)
            newPosition.x = target.position.x + halfDeadZone.x;

        if (distance.y > halfDeadZone.y)
            newPosition.y = target.position.y - halfDeadZone.y;
        else if (distance.y < -halfDeadZone.y)
            newPosition.y = target.position.y + halfDeadZone.y;

        float halfCameraHeight = cameraComponent.orthographicSize;
        float halfCameraWidth = halfCameraHeight * cameraComponent.aspect;

        float cameraMinX = minX + halfCameraWidth;
        float cameraMaxX = maxX - halfCameraWidth;
        float cameraMinY = minY + halfCameraHeight;
        float cameraMaxY = maxY - halfCameraHeight;

        if (cameraMinX > cameraMaxX)
            cameraMinX = cameraMaxX = (minX + maxX) / 2f;

        if (cameraMinY > cameraMaxY)
            cameraMinY = cameraMaxY = (minY + maxY) / 2f;

        newPosition.x = Mathf.Clamp(newPosition.x, cameraMinX, cameraMaxX);
        newPosition.y = Mathf.Clamp(newPosition.y, cameraMinY, cameraMaxY);

        Vector3 finalPosition = Vector3.Lerp(
            transform.position,
            newPosition,
            followSpeed * Time.deltaTime
        );

        finalPosition.x = Mathf.Clamp(finalPosition.x, cameraMinX, cameraMaxX);
        finalPosition.y = Mathf.Clamp(finalPosition.y, cameraMinY, cameraMaxY);
        transform.position = finalPosition;
    }

    private void OnDrawGizmos()
    {
        Vector3 deadZoneCenter = new Vector3(transform.position.x, transform.position.y, 0f);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(deadZoneCenter, new Vector3(deadZoneSize.x, deadZoneSize.y, 0f));

        Vector3 bottomLeft = new Vector3(minX, minY, 0f);
        Vector3 topLeft = new Vector3(minX, maxY, 0f);
        Vector3 topRight = new Vector3(maxX, maxY, 0f);
        Vector3 bottomRight = new Vector3(maxX, minY, 0f);

        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(bottomLeft, topLeft);
        Gizmos.DrawLine(topLeft, topRight);
        Gizmos.DrawLine(topRight, bottomRight);
        Gizmos.DrawLine(bottomRight, bottomLeft);
    }
}
