using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using MortiseFrame.LitIO;
using MortiseFrame.Rill.Generic;

namespace MortiseFrame.Rill.TCP.Server {

    public class TCPServer {

        Socket server;
        ConcurrentDictionary<Socket, byte[]> clientBuffers;
        Func<Socket, Task> onConnected;
        CancellationTokenSource cancellation;
        const ushort HEADERLENGTH = 4;
        const int BUFFERLENGTH = 1024;

        public TCPServer() {
            server = new Socket(
                AddressFamily.InterNetwork,
                SocketType.Stream,
                ProtocolType.Tcp
                );
            clientBuffers = new ConcurrentDictionary<Socket, byte[]>();
            cancellation = new CancellationTokenSource();
        }

        public async Task Start(string ip, int port, Func<Socket, Task> onConnected = null) {
            var ep = new IPEndPoint(IPAddress.Parse(ip), port);
            this.onConnected = onConnected;

            try {
                server.Bind(ep);
                server.Listen(10);

                while (!cancellation.Token.IsCancellationRequested) {
                    var client = await server.AcceptAsync().ConfigureAwait(false);
                    if (onConnected != null) {
                        _ = Task.Run(async () => await onConnected(client), CancellationToken.None);
                    }
                }
            } catch (OperationCanceledException) {
                server.Close();
                server.Dispose();
                throw;
            } catch (Exception ex) {
                server.Close();
                server.Dispose();
                throw new InvalidOperationException("Failed to start the server.", ex);
            }
        }

        public async Task<(int, IMessage<T>)> ReceiveAsync<T>(IMessageParser<T> parser) where T : IMessage<T> {

            // 创建缓冲区
            var client = await server.AcceptAsync();
            var buffer = new byte[BUFFERLENGTH];

            try {

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

            } catch (Exception ex) {
                throw new Exception("Receive message error: ", ex);
            } finally {
                if (buffer != null) {
                    Array.Clear(buffer, 0, buffer.Length);
                }
                clientBuffers.TryRemove(client, out byte[] removedBuffer);
                client.Shutdown(SocketShutdown.Both);
                client.Dispose();
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
                return await server.SendAsync(bufferSegment, SocketFlags.None);
            } catch {
                throw;
            }

        }

        void Disconnect() {
            foreach (var item in clientBuffers.Keys) {
                item.Shutdown(SocketShutdown.Both);
                item.Dispose();
            }
        }

    }

}


