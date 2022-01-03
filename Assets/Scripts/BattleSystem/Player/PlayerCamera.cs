using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCamera : MonoBehaviour
{
    [SerializeField] Vector3 Rotation;

    [SerializeField] float OffsetY = 0.65f;

    [SerializeField] float CameraDistance = 3f;
    [SerializeField] float MouseSensitivity = 4f;
    [SerializeField] float ScrollSensitvity = 2f;
    [SerializeField] float OrbitDampening = 10f;
    [SerializeField] float ScrollDampening = 6f;
    [SerializeField] Vector2 RotationYClamp = new Vector2(0.0f, 89.0f);

    [SerializeField] GameObject Target;

    [SerializeField] LayerMask CollisionMask;

    public Vector3 GetPlayerZVector()
    {
        return new Vector3(transform.position.x - transform.parent.position.x, 0f, transform.position.z - transform.parent.position.z).normalized;
    }

    public Vector3 GetPlayerXVector()
    {
        return transform.right.normalized;
    }

    public void Rotate (Vector2 input)
    {
        Rotation.x += input.x * MouseSensitivity;
        Rotation.y -= input.y * MouseSensitivity;

        Rotation.y = Mathf.Clamp(Rotation.y, RotationYClamp.x, RotationYClamp.y);

        Quaternion qt = Quaternion.Euler(Rotation.y, Rotation.x, 0);
        transform.parent.rotation = Quaternion.Lerp(transform.parent.rotation, qt, Time.deltaTime * OrbitDampening);

        if (transform.localPosition.z != CameraDistance * -1f)
        {
            transform.localPosition = new Vector3(0f, 0f, Mathf.Lerp(transform.localPosition.z, CameraDistance * -1f, Time.deltaTime * ScrollDampening));
        }

        transform.LookAt(transform.parent);
    }

    public void Zoom (float zoom)
    {
        var scrollAmount = zoom * ScrollSensitvity;

        scrollAmount *= (CameraDistance * 0.3f);

        CameraDistance += scrollAmount * -1f;

        CameraDistance = Mathf.Clamp(CameraDistance, 1.5f, 10f);
    }


    void LateUpdate()
    {
        transform.parent.position = Target.transform.position;
        transform.parent.position += new Vector3(0f, (Target.transform.up * OffsetY).y, 0f);

        Vector3 targetToCamera = transform.position - transform.parent.position;
        targetToCamera.Normalize();
        RaycastHit hit;

        if (Physics.Raycast(transform.parent.position, targetToCamera, out hit, CameraDistance, CollisionMask))
        {
            transform.position = hit.point;
        }
        else
        {
            transform.position = transform.parent.position + targetToCamera * CameraDistance;
        }
    }
}
