using System;
using MortiseFrame.LitIO;

namespace MortiseFrame.Rill {

    internal static class ServerSendDomain {

        internal static void Tick_Send(ServerContext ctx, float dt) {

            if (ctx.Server == null) {
                return;
            }

            ctx.ClientState_ForEachOrderly((clientState) => {

                if (clientState.clientfd == null) {
                    return;
                }
                if (!clientState.clientfd.Connected) {
                    return;
                }

                while (ctx.Message_TryDequeue(clientState.clientfd, out IMessage message)) {
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

                    var client = ctx.Server;
                    client.Send(buff, offset, System.Net.Sockets.SocketFlags.None);

                    ctx.Buffer_ClearWriteBuffer();

                }

            });

        }

    }

}