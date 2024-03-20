# Rill
Rill is a lightweight networking library based on Socket and TCP, named after a "small stream", signifying a continuous and gentle flow.<br/>
**Rill 是基于 Socket 和 TCP 的轻量级网络库，名称取自于“小溪”，涓流细而不止。**

It utilizes safe multithreading and employs ManualResetEvent to avoid wasteful idle cycles when the message queue is empty (inspired by the approach taken in Telepathy).<br/>
**线程安全，使用 ManualResetEvent 避免消息队列为空时的空转损耗（参考了 Telepathy 的写法）**

# Inspiration
[Telepathy](https://github.com/MirrorNetworking/Telepathy)<br/>
[Networkweaver](https://github.com/GameArki/GameArkiSetup/tree/main/Assets/com.gamearki.networkweaver.tcp)

# Readiness
The core functionalities have been developed, but they have not been tested in commercial projects. It is not recommended for use in official projects. Suitable for small projects or for reference and learning purposes.<br/>
**功能基本完成，未经商业项目实验。不建议用于正式项目。可以用于小型项目或者参考学习。**

# To Be Implemented
- Server max message size limit.
- Pool.

# Client Sample
```
//  Register
public void RegisterAllProtocol(ClientCore client) {
    client.Register(typeof(ConnectResMessage));
    client.Register(typeof(GameStartBroadMessage));
    client.Register(typeof(RoleMoveReqMessage));
    client.Register(typeof(RoleSyncBroadMessage));
    client.Register(typeof(GameResultBroadMessage));
}

//  Send
public void Send(ClientCore client, IMessage msg) {
    client.Send(msg);
}

//  Tick
public void Tick(ClientCore client, float dt) {
    client.Tick(dt);
}

//  Connect
public void Connect(ClientCore client) {
    var remoteIP = RequestConst.REMOTE_IP;
    var port = RequestConst.REMOTE_PORT;
    client.Connect(remoteIP, port);
}

//  On  
void On(ClientCore client) {
    client.On<ConnectResMessage>((msg) => OnNetResConnect((ConnectResMessage)msg));
    client.On<GameStartBroadMessage>((msg) => OnNetResGameStart((GameStartBroadMessage)msg));
    client.On<RoleSyncBroadMessage>((msg) => OnNetResEntitiesSync((RoleSyncBroadMessage)msg));
    client.On<GameResultBroadMessage>((msg) => OnNetResGameResult((GameResultBroadMessage)msg));

    client.OnConnected(() => OnNetResConnect());
    client.OnError((msg) => OnNetResConnectError(loginBusinessContext, msg));
    client.OnDisconnected(() => OnNetResDisconnect());
}

//  Off
void Off(ClientCore client) {
    client.Off<ConnectResMessage>((msg) => OnNetResConnect((ConnectResMessage)msg));
    client.Off<GameStartBroadMessage>((msg) => OnNetResGameStart((GameStartBroadMessage)msg));
    client.Off<RoleSyncBroadMessage>((msg) => OnNetResEntitiesSync((RoleSyncBroadMessage)msg));
    client.Off<GameResultBroadMessage>((msg) => OnNetResGameResult((GameResultBroadMessage)msg));

    client.OffConnected(() => OnNetResConnect());
    client.OffError((msg) => OnNetResConnectError(loginBusinessContext, msg));
    client.OffDisconnected(() => OnNetResDisconnect());
}

// Stop
public void Stop(ClientCore client) {
    client.Stop();
}

// Send
public void SendRoleMoveReq(ClientCore client, Vector2 axis) {
    var msg = new RoleMoveReqMessage();
    msg.moveAxis = axis.ToFVector2();
    client.Send(msg);
}

// Config
public void SetNoDelay(ClientCore client, bool noDelay) {
    client.SetNoDelay(noDelay);
}

public void SetSendTimeout(ClientCore client, int sendTimeout) {
    client.SetSendTimeout(sendTimeout);
}

public void SetReceiveTimeout(ClientCore client, int receiveTimeout) {
    client.SetReceiveTimeout(receiveTimeout);
}

public void SetBufferLength(ClientCore client, int bufferLength) {
    client.SetBufferLength(bufferLength);
}
```

# Server Sample
```
//  Register
public void RegisterAllProtocol(ServerCore server) {
    server.Register(typeof(ConnectResMessage));
    server.Register(typeof(GameStartBroadMessage));
    server.Register(typeof(RoleMoveReqMessage));
    server.Register(typeof(RoleSyncBroadMessage));
    server.Register(typeof(GameResultBroadMessage));
}

//  Send
public void Send(ServerCore server, IMessage msg, ConnectionEntity conn) {
    server.Send(msg, conn);
}

//  Tick
public void Tick(ServerCore server, float dt) {
    server.Tick(dt);
}

//  Connect
public void Start(ServerCore server) {
    var ip = RequestConst.LOCAL_IP;
    var port = RequestConst.LOCAL_PORT;
    server.Start(ip, port);
}

//  On  
void On(ServerCore server) {
    server.On<ConnectResMessage>((msg, conn) => OnNetResConnect((ConnectResMessage)msg, conn));
    server.On<GameStartBroadMessage>((msg, conn) => OnNetResGameStart((GameStartBroadMessage)msg, conn));
    server.On<RoleSyncBroadMessage>((msg, conn) => OnNetResEntitiesSync((RoleSyncBroadMessage)msg, conn));
    server.On<GameResultBroadMessage>((msg, conn) => OnNetResGameResult((GameResultBroadMessage)msg, conn));

    server.OnConnected((conn) => OnNetResConnect(conn));
    server.OnError((msg, conn) => OnNetResConnectError(loginBusinessContext, msg, conn));
    server.OnDisconnected((conn) => OnNetResDisconnect(conn));

    // "conn" refers to ConnectionEntity, which is a high-level encapsulation of a Socket client.
    // Each ConnectionEntity is assigned a unique auto-increment index upon creation.
}

//  Off
void Off(ServerCore server) {
    server.Off<ConnectResMessage>((msg, conn) => OnNetResConnect((ConnectResMessage)msg, conn));
    server.Off<GameStartBroadMessage>((msg, conn) => OnNetResGameStart((GameStartBroadMessage)msg, conn));
    server.Off<RoleSyncBroadMessage>((msg, conn) => OnNetResEntitiesSync((RoleSyncBroadMessage)msg, conn));
    server.Off<GameResultBroadMessage>((msg, conn) => OnNetResGameResult((GameResultBroadMessage)msg, conn));

    server.OffConnected((conn) => OnNetResConnect(conn));
    server.OffError((msg, conn) => OnNetResConnectError(loginBusinessContext, msg, conn));
    server.OffDisconnected((conn) => OnNetResDisconnect(conn));
}

// Stop
public void Stop(ServerCore server) {
    server.Stop();
}

// Send
public static void SendRoleSyncBroad(RequestInfraContext ctx, FVector2[] posArray) {
    ctx.ServerCore.ForEachOrderly((conn) => {
        var msg = new RoleSyncBroadMessage();
        msg.posArray = posArray;
        ctx.ServerCore.Send(msg, conn);
    });
}

// Config
public void SetNoDelay(ServerCore server, bool noDelay) {
    server.SetNoDelay(noDelay);
}

public void SetSendTimeout(ServerCore server, int sendTimeout) {
    server.SetSendTimeout(sendTimeout);
}

public void SetReceiveTimeout(ServerCore server, int receiveTimeout) {
    server.SetReceiveTimeout(receiveTimeout);
}

public void SetBufferLength(ServerCore server, int bufferLength) {
    server.SetBufferLength(bufferLength);
}
```

# Message Sample
```
using System;
using MortiseFrame.Abacus;
using MortiseFrame.LitIO;
using MortiseFrame.Rill;

public struct RoleMoveReqMessage : IMessage {

    public FVector2 moveAxis;

    public void WriteTo(byte[] dst, ref int offset) {
        ByteWriter.Write<FVector2>(dst, moveAxis, ref offset);
    }

    public void FromBytes(byte[] src, ref int offset) {
        moveAxis = ByteReader.Read<FVector2>(src, ref offset);
    }

    public int GetEvaluatedSize(out bool isCertain) {
        isCertain = true;
        int count = ByteCounter.Count<FVector2>();
        return count;
    }

    public byte[] ToBytes() {
        int count = GetEvaluatedSize(out bool isCertain);
        int offset = 0;
        byte[] src = new byte[count];
        WriteTo(src, ref offset);
        if (isCertain) {
            return src;
        } else {
            byte[] dst = new byte[offset];
            Buffer.BlockCopy(src, 0, dst, 0, offset);
            return dst;
        }
    }

}
```

# Default Config
```
bool NoDelay = true;
int SendTimeout = 5000;
int ReceiveTimeout = 0;
int BufferLength = 4096;
```

# 

# Dependency
# Dependency
Serialization / Deserialization Tool 
[LitIO](https://github.com/onovich/LitIO)

# UPM URL
ssh://git@github.com/onovich/Rill.git?path=/Assets/com.mortise.rill#main
