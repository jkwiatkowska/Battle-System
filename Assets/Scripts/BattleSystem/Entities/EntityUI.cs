using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntityUI : MonoBehaviour
{
    Camera Camera;
    private void Awake()
    {
        Camera = Camera.main;
    }

    void LateUpdate()
    {
        transform.LookAt(transform.position + Camera.transform.rotation * Vector3.forward, Camera.transform.rotation * Vector3.up);
    }
}
