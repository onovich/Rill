using System;
using System.Net.Sockets;
using System.Threading;
using MortiseFrame.LitIO;

namespace MortiseFrame.Rill {

    internal static class ServerSendDomain {

        internal static void Send(ServerContext ctx, IMessage msg, ClientStateEntity client) {
            ctx.Message_Enqueue(msg, client.clientfd);
            client.sendPending.Set();
        }

        internal static void ThreadTick_Send(ServerContext ctx, ClientStateEntity client) {

            if (ctx.Listener == null) {
                return;
            }

            if (client == null) {
                return;
            }

            try {

                while (client.clientfd.Connected) {

                    client.sendPending.Reset();
                    DequeueAndSerializeAll(ctx, client);
                    client.sendPending.WaitOne();

                }

            } catch (ThreadAbortException) {
            } catch (ThreadInterruptedException) {
            } catch (Exception exception) {
                RLog.Log("SendLoop Exception: " + exception);
            } finally {
                client.clientfd.Close();
            }

        }

        static void DequeueAndSerializeAll(ServerContext ctx, ClientStateEntity client) {
            while (ctx.Message_TryDequeue(client.clientfd, out IMessage message)) {

                if (message == null) {
                    continue;
                }

                byte[] buff = ctx.writeBuff;
                int offset = 0;

                var src = message.ToBytes();
                if (src.Length >= 4096 - 5) {
                    RLog.Log("Message is too long");
                }

                int len = src.Length + 5;
                byte msgID = ctx.GetMessageID(message);

                ByteWriter.Write<int>(buff, len, ref offset);
                ByteWriter.Write<byte>(buff, msgID, ref offset);
                Buffer.BlockCopy(src, 0, buff, offset, src.Length);
                offset += src.Length;

                if (offset == 0) {
                    return;
                }

                client.clientfd.Send(buff, 0, offset, System.Net.Sockets.SocketFlags.None);
                ctx.Buffer_ClearWriteBuffer();
            }
        }

    }

}