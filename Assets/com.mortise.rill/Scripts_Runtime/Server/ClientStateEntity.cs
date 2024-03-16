using System;
using System.Net.Sockets;

namespace MortiseFrame.Rill {

    public class ClientStateEntity {
        internal Socket clientfd;
        internal int clientIndex;
        byte[] buffer;
        object locker;

        internal ClientStateEntity(Socket client, int clientIndex) {
            this.clientfd = client;
            this.clientIndex = clientIndex;
            this.buffer = new byte[CommonConst.BufferLength];
            locker = new object();
        }

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

    }

}