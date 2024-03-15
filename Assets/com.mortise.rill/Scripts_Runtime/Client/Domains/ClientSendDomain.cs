using System;
using MortiseFrame.LitIO;

namespace MortiseFrame.Rill {

    internal static class ClientSendDomain {

        internal static void Tick_Send(ClientContext ctx, float dt) {

            if (ctx.Client == null) {
                return;
            }

            while (ctx.Message_TryDequeue(out IMessage message)) {
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

                var client = ctx.Client;
                client.Send(buff, offset, System.Net.Sockets.SocketFlags.None);

                ctx.Buffer_ClearWriteBuffer();

            }

        }

    }

}