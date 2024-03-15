using System.Collections.Generic;
using System.Net.Sockets;
using System.Collections.Concurrent;
using System;

namespace MortiseFrame.Rill {

    internal class ClientContext {

        internal bool isTest;

        Socket client;
        internal Socket Client => client;

        // Message
        Queue<IMessage> messageQueue;

        // Event
        ClientEventCenter evt;
        internal ClientEventCenter Evt => evt;

        // Protocol
        BiDictionary<byte, Type> protocolDicts;

        // Buffer
        internal byte[] readBuff;
        internal byte[] writeBuff;

        // Service
        IDService idService;
        internal IDService IDService => idService;

        internal ClientContext() {
            messageQueue = new Queue<IMessage>();
            readBuff = new byte[4096];
            writeBuff = new byte[4096];
            evt = new ClientEventCenter();
            idService = new IDService();
        }

        internal void Client_Set(Socket socket) {
            this.client = socket;
        }

        // Message
        internal void Message_Enqueue(IMessage message) {
            messageQueue.Enqueue(message);
        }

        internal bool Message_TryDequeue(out IMessage message) {
            return messageQueue.TryDequeue(out message);
        }

        internal int Message_GetCount() {
            return messageQueue.Count;
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