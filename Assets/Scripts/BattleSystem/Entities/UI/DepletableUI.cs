using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class DepletableUI : MonoBehaviour
{
    public string DepletableName;

    public abstract void UpdateValues(float current, float max);
}
