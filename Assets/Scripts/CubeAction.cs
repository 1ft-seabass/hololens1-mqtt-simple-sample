using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MiniJSON;

#if UNITY_UWP
using System;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
 
// M2Mqtt
using uPLibrary.Networking.M2Mqtt;
using uPLibrary.Networking.M2Mqtt.Messages;
#endif

public class CubeAction : MonoBehaviour
{

#if UNITY_UWP
    MqttClient client;
    string clientId;
    string topicPublishPath;
    string topicSubscribePath;
    string mqttBrokerAddress;
    string mqttUserName;
    string mqttPassword;
#endif

    // Use this for initialization
    void Start()
    {

#if UNITY_UWP

        Debug.Log(string.Format("[UNITY_UWP] Start"));

        Debug.Log(string.Format("[UNITY_UWP] MQTT Process"));

        // 接続先のアドレス
        mqttBrokerAddress = "<BrokerAddress>";

        // MQTTブローカー接続時のUserName
        mqttUserName = "mqttUserName";
        // MQTTブローカー接続時のmqttPassword
        mqttPassword = "mqttPassword";

        // publish先のtopic
        topicPublishPath = "pub/HoloLens";
        // subscribe先のtopic
        topicSubscribePath = "sub/HoloLens";



        clientId = Guid.NewGuid().ToString();
 
        client = new MqttClient(mqttBrokerAddress);
        client.ProtocolVersion = MqttProtocolVersion.Version_3_1;
         
        client.MqttMsgPublishReceived += client_MqttMsgPublishReceived;
 
        
 
        try
        {
            // ID/PASS MQTT connect
            client.Connect(clientId, mqttUserName, mqttPassword);

            // No Passward MQTT connect
            // client.Connect(clientId);
        }
        catch (Exception e)
        {
            Debug.Log(string.Format("Exception has occurred in connecting to MQTT {0} ", e ));
            throw new Exception("Exception has occurred in connecting to MQTT", e.InnerException);
        }
 
        Debug.Log("Connect OK");
 
        // Subscribe
        client.Subscribe(new string[] { topicSubscribePath }, new byte[] { 2 });
 
        // Publish
        try
        {
            // JSONデータを作る
            var data = new Dictionary<string, object>();
            data["status"] = "connected";
            data["message"] = "HoloLens connected!!!";
            string json = Json.Serialize(data);
 
            // publish a message with QoS 0
            client.Publish(topicPublishPath, Encoding.UTF8.GetBytes(json), MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE, true);
        }
        catch (Exception e)
        {
            Debug.Log("Exception has occurred in publishEvent");
            throw new Exception("Exception has occurred in publishEvent ", e);
        }
#else

        Debug.Log(string.Format("[UNITY_EDITOR] Start"));

#endif



    }

    // Update is called once per frame
    void Update()
    {

    }

    void OnSelect()
    {
        Debug.Log("OnSelect");

#if UNITY_UWP

        // Publish
        try
        {
            // JSONデータを作る
            var data = new Dictionary<string, object>();
            data["status"] = "selected";
            data["message"] = "Cube selected!!!";
            string json = Json.Serialize(data);

            // publish a message with QoS 0
            client.Publish(topicPublishPath, Encoding.UTF8.GetBytes(json), MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE, true);
        }
        catch (Exception e)
        {
            Debug.Log("Exception has occurred in publishEvent");
            throw new Exception("Exception has occurred in publishEvent ", e);
        }

#endif
    }



#if UNITY_UWP

    // this code runs when a message was received
    void client_MqttMsgPublishReceived(object sender, MqttMsgPublishEventArgs e)
    {
        string ReceivedMessage = Encoding.UTF8.GetString(e.Message);
         
        Debug.Log("client_MqttMsgPublishReceived");
        Debug.Log(ReceivedMessage);
 
        Task.Run(async () =>
        {
            // Unityの描画に処理を戻す
            UnityEngine.WSA.Application.InvokeOnAppThread(() => {
 
                var jsonDataTree = Json.Deserialize(ReceivedMessage) as Dictionary<string, object>;
 
                // JSONデータのmessage値を取得する
                Debug.Log(jsonDataTree["message"]);

                // ここでGameObjectなど描画に関する処理を書く
                GameObject.Find("TextMessage").GetComponent<TextMesh>().text = (string)jsonDataTree["message"];
 
            }, true);
 
            await Task.Delay(100);
 
        });
 
         
    }
#endif

    void OnClosed()
    {
#if UNITY_UWP
        client.Disconnect();
#endif
    }
}