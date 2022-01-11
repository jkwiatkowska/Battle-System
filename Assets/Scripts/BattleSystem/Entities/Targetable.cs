using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Targetable : MonoBehaviour
{
    Entity ParentEntity;
    [SerializeField] List<GameObject> ShowWhenSelected;
    [SerializeField] List<GameObject> ShowWhenNotSelected;
    public Entity Entity
    {
        get
        {
            return ParentEntity;
        }
        private set
        {
            ParentEntity = value;
        }
    }

    private void Awake()
    {
        ParentEntity = GetComponentInParent<Entity>();
        ToggleSelect(false);
    }

    public void ToggleSelect(bool select)
    {
        foreach (var item in ShowWhenSelected)
        {
            item.SetActive(select);
        }

        foreach (var item in ShowWhenNotSelected)
        {
            item.SetActive(!select);
        }
    }
}
