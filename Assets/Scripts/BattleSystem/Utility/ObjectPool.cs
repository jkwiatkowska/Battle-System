using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPool : MonoBehaviour
{
    [SerializeField] GameObject ObjectToPool;
    [SerializeField] int NumberToPool;
    [SerializeField] Transform ParentTransform;
    Queue<GameObject> PooledObjects;

    void Awake()
    {
        PooledObjects = new Queue<GameObject>();
        for (int i = 0; i < NumberToPool; i++)
        {
            AddNewToPool();
        }
    }

    void AddNewToPool()
    {
        var newObject = Instantiate(ObjectToPool, ParentTransform);
        PooledObjects.Enqueue(newObject);
        newObject.SetActive(false);
    }

    public GameObject GetFromPool()
    {
        if (PooledObjects.Count > 0)
        {
            return PooledObjects.Dequeue();
        }
        else
        {
            return null;
        }
    }

    public void ReturnToPool(GameObject returnedObject)
    {
        PooledObjects.Enqueue(returnedObject);
    }
}
