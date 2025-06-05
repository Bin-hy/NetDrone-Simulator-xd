using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class RoomView : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private LiveController LiveController;
    [SerializeField] private Transform content; // ScrollView的Content对象
    [SerializeField] private GameObject roomPrefab; // RoomItem预制体
    private const string roomPrefabUri = "UIPrefab/RoomItem/RoomItem";

    [Header("Pool Settings")]
    [SerializeField] private int initialPoolSize = 10;
    [SerializeField] private int maxPoolSize = 30;

    private List<GameObject> activeRoomItems = new List<GameObject>();
    private Dictionary<string, GameObject> roomItemMap = new Dictionary<string, GameObject>(); // 用stream.id做键

    private void Awake()
    {
        InitializeRoomPrefab();
        InitializePool();
    }
    private void Start()
    {
        OnUpdate();
    }
    private void InitializeRoomPrefab()
    {
        if (roomPrefab == null)
        {
            try
            {
                roomPrefab = Resources.Load<GameObject>(roomPrefabUri);
                if (roomPrefab == null)
                {
                    Debug.LogError($"Prefab '{roomPrefabUri}' not found in Resources");
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Failed to load prefab '{roomPrefabUri}'. Error: {e}");
            }
        }
    }

    private void InitializePool()
    {
        if (roomPrefab == null) return;

        var poolConfig = new PoolConfig
        {
            prefab = roomPrefab,
            initialSize = initialPoolSize,
            maxSize = maxPoolSize,
            autoExpand = true,
            expandAmount = 5
        };

        ObjectPool.Instance.InitializePool(poolConfig);
    }

    public void OnUpdate()
    {
        StartCoroutine(IEUpdate());
    }

    private IEnumerator IEUpdate()
    {
        yield return LiveController.LoadRoomStreams();
        var streams = LiveController.RoomSteams.ToList();
        Debug.Log($"update roomlists :{streams.Count}");
        UpdateRoomList(streams);
        
        yield return 0;
    }
    private void UpdateRoomList(List<StreamData> streams)
    {
        ClearAllRoomItems();
        // 3. 更新或创建新的房间项
        foreach (var stream in streams)
        {
            if (roomItemMap.TryGetValue(stream.id, out var roomItem))
            {
                // 更新现有项
                UpdateRoomItem(roomItem, stream);
            }
            else
            {
                // 创建新项
                CreateRoomItem(stream);
            }
        }
    }

    private void CreateRoomItem(StreamData stream)
    {
        if (roomPrefab == null) return;

        GameObject roomItem = ObjectPool.Instance.GetObject(
            roomPrefab,
            Vector3.zero,
            Quaternion.identity,
            false
        );

        roomItem.transform.SetParent(content);
        roomItem.transform.localScale = roomPrefab.transform.localScale;
        // 初始化或更新UI
        UpdateRoomItem(roomItem, stream);
        // 设置正确位置：放在content列表下面排列 TODO

        // 添加到管理列表
        activeRoomItems.Add(roomItem);
        roomItemMap[stream.id] = roomItem;
    }

    private void UpdateRoomItem(GameObject roomItem, StreamData stream)
    {
        var roomItemComponent = roomItem.GetComponent<RoomItem>();
        if (roomItemComponent != null)
        {
            roomItemComponent.Initialize(
                stream.id,
                true
            );
        }
        else
        {
            Debug.LogWarning("RoomItem prefab is missing RoomItemUI component");
        }
    }

    private void OnDestroy()
    {
        ClearAllRoomItems();
    }

    private void ClearAllRoomItems()
    {
        foreach (var roomItem in activeRoomItems)
        {
            ObjectPool.Instance.ReturnObject(roomItem);
        }
        activeRoomItems.Clear();
        roomItemMap.Clear();
    }
}