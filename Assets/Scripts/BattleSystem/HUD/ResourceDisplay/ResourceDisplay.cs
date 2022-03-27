using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ResourceDisplay : MonoBehaviour
{
    public string ResourceName;

    public abstract void SetValues(float current, float max);
    public abstract void UpdateValues(float current, float max);
}
