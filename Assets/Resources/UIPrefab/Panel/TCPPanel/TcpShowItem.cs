using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TcpShowItem : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _timeStampText;
    [SerializeField] private TextMeshProUGUI _detail;
    // Start is called before the first frame update
    
    public string timeStampText
    {
        set { 
            _timeStampText.text = value; 
        }
    }
    public string detail
    {
        set
        {
            _detail.text = value;
        }
    }

    // 初始化提示细节 例如：收到ACK
    public void Init(string timeStampText,string detail)
    {
        _timeStampText.text = timeStampText;
        _detail.text = detail;
    }
}
