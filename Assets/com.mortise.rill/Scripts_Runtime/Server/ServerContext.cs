using System.Collections.Generic;
using System.Net.Sockets;
using System.Collections.Concurrent;
using System;

namespace MortiseFrame.Rill {

    internal class ServerContext {

        internal bool isTest;

        Socket server;
        internal Socket Server => server;

        Dictionary<Socket, ClientStateEntity> clients;
        SortedList<int, Socket> clientOrderList;

        // Message
        Dictionary<Socket, Queue<IMessage>> messageQueue;

        // Event
        ServerEventCenter evt;
        internal ServerEventCenter Evt => evt;

        // Protocol
        BiDictionary<byte, Type> protocolDicts;

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
            this.server = socket;
        }

        // Clientfd
        public void ClientState_Add(ClientStateEntity clientState) {
            clients.Add(clientState.clientfd, clientState);
            clientOrderList.Add(clientState.clientIndex, clientState.clientfd);
        }

        public void ClientState_Remove(Socket clientfd) {
            clientOrderList.Remove(clients[clientfd].clientIndex);
            clients.Remove(clientfd);
        }

        public void ClientState_ForEachOrderly(Action<ClientStateEntity> action) {
            for (int i = 0; i < clientOrderList.Count; i++) {
                action(clients[clientOrderList.Values[i]]);
            }
        }

        // Message
        internal void Message_Enqueue(IMessage message, Socket clientfd) {
            if (!messageQueue.ContainsKey(clientfd)) {
                messageQueue.Add(clientfd, new Queue<IMessage>());
            }
            messageQueue[clientfd].Enqueue(message);
        }

        internal bool Message_TryDequeue(Socket clientfd, out IMessage message) {
            if (messageQueue.ContainsKey(clientfd) && messageQueue[clientfd].Count > 0) {
                return messageQueue[clientfd].TryDequeue(out message);
            }
            message = null;
            return false;
        }

        internal int Message_GetCount(Socket clientfd) {
            if (messageQueue.ContainsKey(clientfd)) {
                return messageQueue[clientfd].Count;
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
            if (!protocolDicts.ContainsValue(msgType)) {
                var msgId = IDService.PickMsgId();
                protocolDicts.Add(msgId, msgType);
            }
        }

        internal object GetMessage(byte id) {
            var has = protocolDicts.TryGetByKey(id, out Type type);
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
            var has = protocolDicts.TryGetByValue(type, out byte id);
            if (!has) {
                throw new ArgumentException("ID Not Found");
            }
            return id;
        }

        internal byte GetMessageID<T>() {
            var has = protocolDicts.TryGetByValue(typeof(T), out byte id);
            if (!has) {
                throw new ArgumentException("ID Not Found");
            }
            return id;
        }

        internal void Clear() {
            messageQueue.Clear();
            protocolDicts.Clear();
            evt.Clear();
            idService.Reset();
        }

    }

}