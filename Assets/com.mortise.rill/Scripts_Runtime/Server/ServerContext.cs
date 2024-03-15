using System.Collections.Generic;
using System.Net.Sockets;
using System.Collections.Concurrent;
using System;
using System.Threading;

namespace MortiseFrame.Rill {

    internal class ServerContext {

        // Listener
        Socket listener;
        internal Socket Listener => listener;

        // Thread
        Thread listenerThread;
        public Thread ListenerThread => listenerThread;
        public bool Active => listenerThread != null && listenerThread.IsAlive;

        // Clients
        Dictionary<Socket, ClientStateEntity> clients;
        SortedList<int, Socket> clientOrderList;

        // Message Queue
        Dictionary<Socket, Queue<IMessage>> messageQueue;

        // Event
        ServerEventCenter evt;
        internal ServerEventCenter Evt => evt;

        // Protocol Dict
        BiDictionary<byte, Type> protocolDict;

        // Buffer
        internal byte[] readBuff;
        internal byte[] writeBuff;

        // Service
        IDService idService;
        internal IDService IDService => idService;

        internal ServerContext() {
            clients = new Dictionary<Socket, ClientStateEntity>();
            clientOrderList = new SortedList<int, Socket>();
            messageQueue = new Dictionary<Socket, Queue<IMessage>>();
            readBuff = new byte[4096];
            writeBuff = new byte[4096];
            evt = new ServerEventCenter();
            idService = new IDService();
        }

        internal void Server_Set(Socket socket) {
            this.listener = socket;
        }

        // Client
        public void ClientState_Add(ClientStateEntity client) {
            clients.Add(client.clientfd, client);
            clientOrderList.Add(client.clientIndex, client.clientfd);
        }

        public void ClientState_Remove(Socket client) {
            clientOrderList.Remove(clients[client].clientIndex);
            clients.Remove(client);
        }

        public void ClientState_ForEachOrderly(Action<ClientStateEntity> action) {
            for (int i = 0; i < clientOrderList.Count; i++) {
                action(clients[clientOrderList.Values[i]]);
            }
        }

        // Message
        internal void Message_Enqueue(IMessage message, Socket client) {
            if (!messageQueue.ContainsKey(client)) {
                messageQueue.Add(client, new Queue<IMessage>());
            }
            messageQueue[client].Enqueue(message);
        }

        internal bool Message_TryDequeue(Socket client, out IMessage message) {
            if (messageQueue.ContainsKey(client) && messageQueue[client].Count > 0) {
                return messageQueue[client].TryDequeue(out message);
            }
            message = null;
            return false;
        }

        internal int Message_GetCount(Socket client) {
            if (messageQueue.ContainsKey(client)) {
                return messageQueue[client].Count;
            }
            return 0;
        }

        // Buffer
        internal void Buffer_ClearReadBuffer() {
            Array.Clear(readBuff, 0, readBuff.Length);
        }

        internal void Buffer_ClearWriteBuffer() {
            Array.Clear(writeBuff, 0, writeBuff.Length);
        }

        // Protocol
        internal void RegisterMessage(Type msgType) {
            if (!protocolDict.ContainsValue(msgType)) {
                var msgId = IDService.PickMsgId();
                protocolDict.Add(msgId, msgType);
            }
        }

        internal object GetMessage(byte id) {
            var has = protocolDict.TryGetByKey(id, out Type type);
            if (!has) {
                throw new ArgumentException("No type found for the given ID.", id.ToString());
            }
            if (type == null) {
                throw new ArgumentException("No type found for the given ID.", id.ToString());
            }
            return Activator.CreateInstance(type);
        }

        internal byte GetMessageID(IMessage msg) {
            var type = msg.GetType();
            var has = protocolDict.TryGetByValue(type, out byte id);
            if (!has) {
                throw new ArgumentException("ID Not Found");
            }
            return id;
        }

        internal byte GetMessageID<T>() {
            var has = protocolDict.TryGetByValue(typeof(T), out byte id);
            if (!has) {
                throw new ArgumentException("ID Not Found");
            }
            return id;
        }

        // Thread
        internal void ListenerThread_Clear() {
            listenerThread = null;
        }

        internal void ListnerThread_Set(Thread thread) {
            listenerThread = thread;
        }

        internal void Clear() {
            messageQueue.Clear();
            protocolDict.Clear();
            evt.Clear();
            idService.Reset();
        }

    }

}