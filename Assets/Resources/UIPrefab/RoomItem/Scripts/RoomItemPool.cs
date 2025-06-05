using TMPro;
using UnityEngine;
using UnityEngine.UI;


public class RoomItem : MonoBehaviour,IPoolable
{
    [Header("UI References")]
    [SerializeField] private Image iconImage;
    [SerializeField] private TextMeshProUGUI roomNameText;
    //[SerializeField] private GameObject liveBadge;

    public void Initialize(string roomName, bool isLive)
    {
        //iconImage.sprite = icon;
        roomNameText.text = roomName;
        //liveBadge.SetActive(isLive);
    }
    public void OnPoolGet()
    {
        // 重置对象状态
        gameObject.SetActive(true);
    }

    public void OnPoolReturn()
    {
        // 清理对象状态
        // 重置状态
        if (roomNameText != null) roomNameText.text = "";

        gameObject.SetActive(false);
    }
}
