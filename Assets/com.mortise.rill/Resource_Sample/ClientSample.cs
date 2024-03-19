using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;
using System.Text;
using UnityEngine.UI;
using MortiseFrame.LitIO;
using System;
using MortiseFrame.Rill;

public class ClientSample : MonoBehaviour {

    [SerializeField] Text userName;
    [SerializeField] Text recvText;
    [SerializeField] Button loginButton;
    [SerializeField] Button connectButton;
    [SerializeField] Button closeButton;

    ClientCore clientCore;

    void Start() {

        loginButton.onClick.AddListener(OnClickLogin);
        connectButton.onClick.AddListener(OnClickConnect);
        closeButton.onClick.AddListener(OnClickClose);

        clientCore = new ClientCore();
        Register();
        BindingEvent();

        RLog.Log = (msg) => Debug.Log("On Log: " + msg);
        RLog.Warning = (msg) => Debug.Log("On Info: " + msg);
        RLog.Error = (msg) => Debug.Log("On Error: " + msg);

    }

    void BindingEvent() {
        clientCore.On<ConnectResMessage>((msg) => OnConnectRes((ConnectResMessage)msg));
        clientCore.On<LoginResMessage>((msg) => LoginRes((LoginResMessage)msg));
        clientCore.OnError(OnError);
    }

    void UnbindingEvent() {
        clientCore.Off<ConnectResMessage>((msg) => OnConnectRes((ConnectResMessage)msg));
        clientCore.Off<LoginResMessage>((msg) => LoginRes((LoginResMessage)msg));
        clientCore.OffError(OnError);
    }

    void LoginRes(LoginResMessage msg) {
        Debug.Log("On LoginRes; status: " + msg.status + ", userToken: " + msg.userToken);
        recvText.text = "LoginRes; status: " + msg.status + ", userToken: " + msg.userToken;
    }

    void OnConnectRes(ConnectResMessage msg) {
        Debug.Log("On ConnectRes");
        recvText.text = "ConnectRes";
    }

    void OnConnect() {
        Debug.Log("On Connected");
    }

    void OnError(string error) {
        Debug.Log("On Error: " + error);
        recvText.text = "Error: " + error;
    }

    void Register() {
        clientCore.Register(typeof(ConnectResMessage));
        clientCore.Register(typeof(LoginResMessage));
        clientCore.Register(typeof(LoginReqMessage));
        clientCore.Register(typeof(CloseReqMessage));
    }

    void OnClickConnect() {
        clientCore.Connect("127.0.0.1", 8080);
    }

    void OnClickLogin() {
        LoginReqMessage msg = new LoginReqMessage();
        msg.userToken = userName.text;
        clientCore.Send(msg);
    }

    void OnClickClose() {
        CloseReqMessage msg = new CloseReqMessage();
        clientCore.Send(msg);
    }

    void Update() {
        if (clientCore == null) {
            return;
        }
        clientCore.Tick(0);
    }

    void OnDestroy() {
        if (clientCore == null) {
            return;
        }
        clientCore.Stop();
        UnbindingEvent();
        RLog.Log = null;
        RLog.Warning = null;
        RLog.Error = null;
    }

}