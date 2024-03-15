using System.Collections.Generic;
using System.Net.Sockets;
using System.Collections.Concurrent;
using System;

namespace MortiseFrame.Rill {

    public class ClientContext {

        public bool isTest;

        Socket client;
        public Socket Client => client;

        // Message
        Queue<IMessage> messageQueue;

        // Event
        ClientEventCenter evt;
        public ClientEventCenter Evt => evt;

        // Protocol
        BiDictionary<byte, Type> protocolDicts;

        // Buffer
        public byte[] readBuff;
        public byte[] writeBuff;

        // Service
        IDService idService;
        public IDService IDService => idService;

        public ClientContext() {
            messageQueue = new Queue<IMessage>();
            readBuff = new byte[4096];
            writeBuff = new byte[4096];
            evt = new ClientEventCenter();
            idService = new IDService();
        }

        public void Client_Set(Socket socket) {
            this.client = socket;
        }

        // Message
        public void Message_Enqueue(IMessage message) {
            messageQueue.Enqueue(message);
        }

        public bool Message_TryDequeue(out IMessage message) {
            return messageQueue.TryDequeue(out message);
        }

        public int Message_GetCount() {
            return messageQueue.Count;
        }

        // Buffer
        public void Buffer_ClearReadBuffer() {
            Array.Clear(readBuff, 0, readBuff.Length);
        }

        public void Buffer_ClearWriteBuffer() {
            Array.Clear(writeBuff, 0, writeBuff.Length);
        }

        // Protocol
        public void RegisterMessage(IMessage msg) {
            if (protocolDicts.ContainsValue(msg.GetType())) {
                var msgId = IDService.PickMsgId();
                protocolDicts.Add(msgId, msg.GetType());
            }
        }

        public object GetMessage(byte id) {
            var has = protocolDicts.TryGetByKey(id, out Type type);
            if (!has) {
                throw new ArgumentException("No type found for the given ID.", id.ToString());
            }
            if (type == null) {
                throw new ArgumentException("No type found for the given ID.", id.ToString());
            }
            return Activator.CreateInstance(type);
        }

        public byte GetMessageID(IMessage msg) {
            var type = msg.GetType();
            var has = protocolDicts.TryGetByValue(type, out byte id);
            if (!has) {
                throw new ArgumentException("ID Not Found");
            }
            return id;
        }

        public byte GetMessageID<T>() {
            var has = protocolDicts.TryGetByValue(typeof(T), out byte id);
            if (!has) {
                throw new ArgumentException("ID Not Found");
            }
            return id;
        }

        public void Clear() {
            messageQueue.Clear();
            protocolDicts.Clear();
            evt.Clear();
            idService.Reset();
        }

    }

}