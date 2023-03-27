using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class SendManager : MonoBehaviour
{
    public static void SendToHost(List<string> msg)
    {
        Debug.Log("send_to_host_ui " + (PROTOCOL)Convert.ToInt32(msg[0]));

        //ȣ��Ʈ�� UI�����ʹ� ���ÿ��� ���� �����Ѵ�.
        List<string> clone = msg.ToList();
        ReceiveClient(clone);
    }

    public static void SendToGuest(List<string> msg)
    {
        Debug.Log("send_to_guest_ui " + (PROTOCOL)Convert.ToInt32(msg[0]));
        string s_msg = "";
        for (int i = 0; i < msg.Count; i++)
        {
            s_msg += msg[i];
            if (i < msg.Count - 1)
            {
                s_msg += "/";
            }
        }
        //string���� ��ȯ�� ����Ÿ�� ��뿡�� �����Ͽ� �����ش�.
        MultiManager.instance.ProtocolToGuest(s_msg);
    }

    public static void ReceiveClient(List<string> msg)
    {
        GameClientManager.instance.ReceivePacket(msg);
    }

    public static void SendToServer(byte player_index, List<string> msg)
    {
        Debug.Log("send_to_game_room " + (PROTOCOL)Convert.ToInt32(msg[0]));

        if (!MultiManager.instance.is_host)
        {
            string s_msg = "";
            for (int i = 0; i < msg.Count; i++)
            {
                s_msg += msg[i] + "/";
            }
            s_msg += player_index;
            //string���� ��ȯ�� ����Ÿ�� ��뿡�� �����Ͽ� �����ش�.
            MultiManager.instance.ProtocolToServer(s_msg);
        }
        else
        {        
            //ȣ��Ʈ�� �����ʹ� ���ÿ��� ���� �����Ѵ�.
            List<string> clone = msg.ToList();
            ReceiveServer(player_index, clone);
        }
    }

    public static void ReceiveServer(byte player, List<string> msg)
    {
        Debug.Log("receive_game_room " + (PROTOCOL)Convert.ToInt32(msg[0]));
        List<string> clone = msg.ToList();
        GameManager.instance.ReceivePecket(player, clone);
    }
}
