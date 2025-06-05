using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

[Serializable]
public class RouterCoreStateData
{
    public string routeId;
    public int directSubNetworkCount;
}

public class RouterCoreState : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI routerID;
    [SerializeField] private TextMeshProUGUI directSubNetworkCount;

    public string RouterID {
        set {
            routerID.text = value;
        }
    }

    public int DirectSubNetworkCount {
        set { 
            routerID.text = value.ToString();
        }
    }
    
    public void Initialize(RouterCoreStateData data)
    {
        RouterID = data.routeId;
        DirectSubNetworkCount = data.directSubNetworkCount;
    }
}
