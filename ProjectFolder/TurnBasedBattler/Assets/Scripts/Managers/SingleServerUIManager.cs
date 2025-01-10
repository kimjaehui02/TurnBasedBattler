using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class SingleServerUIManager : MonoBehaviour
{
    [SerializeField]
    private Button button;

    [SerializeField]
    private Text text1;

    [SerializeField]
    private Text text2;

    public void UIUpdate(int numb, ServerInfo serverInfo)//(int numb, string Ip, int tcp, int udp)
    {
        button.onClick.AddListener(() => GameManager.Instance.ConnectSubServer(serverInfo.ipAddress, serverInfo.tcpPort, serverInfo.udpPort));

        text1.text = $"¼­¹ö : {numb}";
        text2.text = $"TCP : {serverInfo.tcpPort} \n" +
                     $"UDP : {serverInfo.udpPort}";
        
    }

}
