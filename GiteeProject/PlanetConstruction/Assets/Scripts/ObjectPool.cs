using System.Collections.Generic;
using UnityEngine;

public class ObjectPool
{
    private GameObject prefab;
    private Transform parent;
    private Queue<GameObject> pool = new Queue<GameObject>();
    private HashSet<GameObject> activeObjects = new HashSet<GameObject>();
    private HashSet<GameObject> pooledObjects = new HashSet<GameObject>();
    private int initialSize;
    private bool allowExpand;

    public ObjectPool(GameObject prefab, int initialSize = 10, bool allowExpand = true, Transform parent = null)
    {
        this.prefab = prefab;
        this.initialSize = initialSize;
        this.allowExpand = allowExpand;
        this.parent = parent;

        for (int i = 0; i < initialSize; i++)
        {
            CreateNewObject();
        }
    }

    private GameObject CreateNewObject()
    {
        GameObject obj = Object.Instantiate(prefab, parent);
        obj.SetActive(false);
        pool.Enqueue(obj);
        pooledObjects.Add(obj);
        return obj;
    }

    public GameObject Spawn(Vector3 position, Quaternion rotation)
    {
        GameObject obj = null;

        if (pool.Count > 0)
        {
            obj = pool.Dequeue();
        }
        else if (allowExpand)
        {
            obj = CreateNewObject();
            pool.Dequeue();
        }
        else
        {
            Debug.LogWarning($"对象池 {prefab.name} 已耗尽且不允许扩展！");
            return null;
        }

        pooledObjects.Remove(obj);
        activeObjects.Add(obj);

        obj.transform.position = position;
        obj.transform.rotation = rotation;
        obj.SetActive(true);

        IPoolable poolable = obj.GetComponent<IPoolable>();
        if (poolable != null)
        {
            poolable.OnSpawnFromPool();
        }

        return obj;
    }

    public void Recycle(GameObject obj)
    {
        if (obj == null)
        {
            Debug.LogWarning("尝试回收空对象！");
            return;
        }

        if (pooledObjects.Contains(obj))
        {
            Debug.LogWarning($"对象 {obj.name} 已经在池中，跳过重复回收！");
            return;
        }

        if (!activeObjects.Contains(obj))
        {
            Debug.LogWarning($"对象 {obj.name} 不属于此对象池，无法回收！");
            return;
        }

        activeObjects.Remove(obj);

        IPoolable poolable = obj.GetComponent<IPoolable>();
        if (poolable != null)
        {
            poolable.OnReturnToPool();
        }

        obj.SetActive(false);
        obj.transform.SetParent(parent);
        pooledObjects.Add(obj);
        pool.Enqueue(obj);
    }

    public void Clear()
    {
        activeObjects.Clear();
        pooledObjects.Clear();
        
        while (pool.Count > 0)
        {
            GameObject obj = pool.Dequeue();
            if (obj != null)
            {
                Object.Destroy(obj);
            }
        }
    }

    public int AvailableCount => pool.Count;
    public int ActiveCount => activeObjects.Count;
    public int TotalCount => pooledObjects.Count + activeObjects.Count;
}
