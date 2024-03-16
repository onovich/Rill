using System;
using System.Collections.Generic;
using System.Net.Sockets;

namespace MortiseFrame.Rill {

    public class ConnectionEntity {
        internal Socket clientfd;
        internal int clientIndex;
        byte[] buffer;
        object locker;

        Queue<IMessage> messageQueue;
        Queue<byte[]> receiveDataQueue;

        internal ConnectionEntity(Socket client, int clientIndex) {
            this.clientfd = client;
            this.clientIndex = clientIndex;
            this.buffer = new byte[CommonConst.BufferLength];
            locker = new object();
            messageQueue = new Queue<IMessage>();
            receiveDataQueue = new Queue<byte[]>();
        }

        // Buffer
        internal byte[] Buffer_Get() {
            lock (locker) {
                return buffer;
            }
        }

        internal void Buffer_Clear() {
            lock (locker) {
                Array.Clear(buffer, 0, buffer.Length);
            }
        }

        // Message Queue
        internal void Message_Enqueue(IMessage message) {
            lock (locker) {
                messageQueue.Enqueue(message);
            }
        }

        internal bool Message_TryDequeue(out IMessage message) {
            lock (locker) {
                return messageQueue.TryDequeue(out message);
            }
        }

        // Receive Data Queue
        internal void ReceiveData_Enqueue(byte[] data) {
            lock (locker) {
                receiveDataQueue.Enqueue(data);
            }
        }

        internal bool ReceiveDate_TryDequeue(out byte[] data) {
            lock (locker) {
                return receiveDataQueue.TryDequeue(out data);
            }
        }

    }

}