using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Targetable : MonoBehaviour
{
    Entity ParentEntity;
    [SerializeField] List<GameObject> ShowWhenSelected;
    [SerializeField] List<GameObject> ShowWhenNotSelected;
    public bool Selected { get; private set; }
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

    public void Setup(Entity parentEntity)
    {
        ParentEntity = parentEntity;
        ToggleSelect(false);
    }

    public void ToggleSelect(bool select)
    {
        Selected = select;

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
