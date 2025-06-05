using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
public class PoolConfig
{
    public GameObject prefab;          // 预制体
    public int initialSize = 1;      // 初始池大小
    public int maxSize = 0;          // 最大池大小(0表示无限制)
    public bool autoExpand = true;    // 是否自动扩展
    public int expandAmount = 5;      // 每次扩展数量
}

public class ObjectPool : MonoBehaviour
{
    // 单例模式，方便全局访问
    public static ObjectPool _instance;

    [SerializeField] private List<PoolConfig> poolConfigs = new List<PoolConfig>();
    // 存储不同预制体的对象池字典
    private Dictionary<GameObject, Queue<GameObject>> poolDictionary = new Dictionary<GameObject, Queue<GameObject>>();
    private Dictionary<GameObject, PoolConfig> configDictionary = new Dictionary<GameObject, PoolConfig>();
    private Dictionary<GameObject, GameObject> instanceToPrefabMap = new Dictionary<GameObject, GameObject>();
    // 池统计信息
    public class PoolStats
    {
        public int totalCreated;
        public int activeCount;
        public int inactiveCount;
    }

    // 2. 公共访问点
    public static ObjectPool Instance
    {
        get
        {
            if (_instance == null)
            {
                // 查找现有实例
                _instance = FindObjectOfType<ObjectPool>();

                // 如果没有则创建新实例
                if (_instance == null)
                {
                    GameObject obj = new GameObject("ObjectPool");
                    _instance = obj.AddComponent<ObjectPool>();
                    DontDestroyOnLoad(obj); // 跨场景不销毁
                }
            }
            return _instance;
        }
    }

    // 初始化对象池
    public void InitializeAllPool(GameObject prefab, int initialSize)
    {
        foreach(var config in poolConfigs)
        {
            InitializePool(config);
        }
    }

    public void InitializePool(PoolConfig config)
    {
        Debug.Log($"InitializePool : {config.prefab.name}");
        if (config.prefab == null)
        {
            Debug.LogError("Cannot initialize pool with null prefab");
            return;
        }
        
        if (poolDictionary.ContainsKey(config.prefab))
        {
            Debug.LogWarning($"Pool for {config.prefab.name} already initialized");
            return;
        }

        var objectQueue = new Queue<GameObject>();
        poolDictionary[config.prefab] = objectQueue;
        configDictionary[config.prefab] = config;

        for (int i = 0; i < config.initialSize; i++)
        {
            GameObject obj = CreateNewObject(config.prefab);
            objectQueue.Enqueue(obj);
        }
    }
    // 创建新对象
    private GameObject CreateNewObject(GameObject prefab)
    {
        GameObject obj = Instantiate(prefab);
        obj.SetActive(false);
        instanceToPrefabMap[obj] = prefab;
        return obj;
    }
    // 从池中获取对象
    public GameObject GetObject(GameObject prefab, Vector3 position, Quaternion rotation,bool isResetPR = false)
    {
        if (!poolDictionary.ContainsKey(prefab))
        {
            Debug.LogWarning($"Pool for {prefab.name} not found. Creating default pool.");
            InitializePool(new PoolConfig { prefab = prefab, initialSize = 1 });
        }

        var pool = poolDictionary[prefab];
        var config = configDictionary[prefab];

        // 如果池为空且允许扩展，就扩展池
        if (pool.Count == 0 && ShouldExpandPool(config))
        {
            ExpandPool(config);
        }

        GameObject obj;
        if (pool.Count > 0)
        {
            obj = pool.Dequeue();
        }
        else
        {
            // 即使不允许扩展，如果没有可用对象也创建一个
            obj = CreateNewObject(prefab);
        }

        if(!isResetPR){
        obj.transform.position = position;
            obj.transform.rotation = rotation;
        }
        obj.SetActive(true);

        // 调用对象的OnPoolGet方法(如果有)
        IPoolable poolable = obj.GetComponent<IPoolable>();
        poolable?.OnPoolGet();

        return obj;
    }
    // 判断是否应该扩展池
    private bool ShouldExpandPool(PoolConfig config)
    {
        if (!config.autoExpand) return false;
        if (config.maxSize <= 0) return true; // 无限制
        return poolDictionary[config.prefab].Count + config.expandAmount <= config.maxSize;
    }

    // 扩展池
    private void ExpandPool(PoolConfig config)
    {
        var pool = poolDictionary[config.prefab];
        int expandBy = config.maxSize > 0 ?
            Mathf.Min(config.expandAmount, config.maxSize - pool.Count) :
            config.expandAmount;

        for (int i = 0; i < expandBy; i++)
        {
            GameObject obj = CreateNewObject(config.prefab);
            pool.Enqueue(obj);
        }
    }

    // 返回对象到池中
    public void ReturnObject(GameObject obj)
    {
        if (!instanceToPrefabMap.TryGetValue(obj, out var prefab))
        {
            Debug.LogWarning($"Trying to return object {obj.name} that doesn't belong to any pool");
            Destroy(obj);
            return;
        }

        var pool = poolDictionary[prefab];
        var config = configDictionary[prefab];

        // 调用对象的OnPoolReturn方法(如果有)
        IPoolable poolable = obj.GetComponent<IPoolable>();
        poolable?.OnPoolReturn();

        obj.SetActive(false);

        // 如果池已满且设置了最大大小，就销毁对象
        if (config.maxSize > 0 && pool.Count >= config.maxSize)
        {
            instanceToPrefabMap.Remove(obj);
            Destroy(obj);
        }
        else
        {
            pool.Enqueue(obj);
        }
    }

    // 清空指定池
    public void ClearPool(GameObject prefab)
    {
        if (!poolDictionary.ContainsKey(prefab)) return;

        var pool = poolDictionary[prefab];
        while (pool.Count > 0)
        {
            GameObject obj = pool.Dequeue();
            instanceToPrefabMap.Remove(obj);
            Destroy(obj);
        }

        poolDictionary.Remove(prefab);
        configDictionary.Remove(prefab);
    }

    // 清空所有池
    public void ClearAllPools()
    {
        foreach (var prefab in new List<GameObject>(poolDictionary.Keys))
        {
            ClearPool(prefab);
        }
    }
}
// 可池化对象接口
public interface IPoolable
{
    void OnPoolGet();    // 当从池中取出时调用
    void OnPoolReturn(); // 当返回池中时调用
}
