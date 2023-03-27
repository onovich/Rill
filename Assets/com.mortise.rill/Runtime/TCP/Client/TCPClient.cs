using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using MortiseFrame.Rill.Generic;
using MortiseFrame.LitIO;

namespace MortiseFrame.Rill.TCP.Client {

    public class TCPClient {

        Socket client;
        const ushort HEADERLENGTH = 4;
        const int BUFFERLENGTH = 1024;

        public TCPClient() {
            client = new Socket(AddressFamily.InterNetwork,
            SocketType.Stream,
            ProtocolType.Tcp);
        }

        public bool Connect(string host, int port) {
            if (!IPAddress.TryParse(host, out var address)) {
                return false;
            }
            try {
                IPEndPoint ep = new IPEndPoint(address, port);
                client.Connect(ep);
                return true;
            } catch {
                return false;
            }
        }

        public async Task<int> SendAsync<T>(T msg) where T : IMessage<T> {

            // 创建缓冲区
            var buffer = new byte[BUFFERLENGTH];

            // 获取消息头
            var msgID = msg.GetID();
            var msgSize = msg.GetSize();

            // 获取消息体
            var offset = 0;
            var body = msg.GetBytes(ref offset);

            // 写入消息头
            offset = 0;
            ByteWritter.Write<ushort>(buffer, msgID, ref offset);
            ByteWritter.Write<ushort>(buffer, msgSize, ref offset);

            // 写入消息体
            ByteWritter.WriteArray<byte>(buffer, body, ref offset);

            // 异步传输
            var bufferSegment = new ArraySegment<byte>(buffer);
            try {
                return await client.SendAsync(bufferSegment, SocketFlags.None);
            } catch {
                Close();
                throw;
            }

        }

        public async Task<(int, IMessage<T>)> ReceiveAsync<T>(IMessageParser<T> parser) where T : IMessage<T> {

            // 创建缓冲区
            var buffer = new byte[BUFFERLENGTH];

            // 获取消息头
            var header = new byte[HEADERLENGTH];
            var headerSize = await client.ReceiveAsync(header, SocketFlags.None);
            if (headerSize == 0) {
                return (0, null);
            }

            // 解析消息头
            var offset = 0;
            var msgID = ByteReader.Read<ushort>(header, ref offset);
            var msgSize = ByteReader.Read<ushort>(header, ref offset);

            // 获取消息体
            var body = new byte[msgSize];
            var bodySize = await client.ReceiveAsync(body, SocketFlags.None);
            if (bodySize == 0) {
                return (0, null);
            }

            // 解析消息体
            var msg = parser.ParseBytes(msgID, body, ref offset);
            return (msgSize, msg);

        }

        void Close() {
            if (client != null) {
                client.Close();
            }
        }

    }

}