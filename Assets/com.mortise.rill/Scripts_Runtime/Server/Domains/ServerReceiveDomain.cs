using System;
using MortiseFrame.LitIO;

namespace MortiseFrame.Rill {

    internal static class ServerReceiveDomain {

        internal static void ThreadTick_Receive(ServerContext ctx, ClientStateEntity client) {

            try {
                byte[] buff = ctx.readBuff;
                int count = client.clientfd.Receive(buff);
                if (count <= 0) {
                    return;
                }

                var offset = 0;
                while (offset < count) {
                    var len = ByteReader.Read<int>(buff, ref offset);
                    if (len == 0) {
                        break;
                    }
                    ReadMessage(ctx, buff, ref offset);
                }

                ctx.Buffer_ClearReadBuffer();

            } catch (Exception exception) {
                RLog.Log(" ReceiveLoop: finished receive function for:" + exception);

            } finally {
                client.clientfd.Close();
            }

        }

        // Read
        static void ReadMessage(ServerContext ctx, byte[] data, ref int offset) {
            var msgID = ByteReader.Read<byte>(data, ref offset);
            var msg = ctx.GetMessage(msgID) as IMessage;

            msg.FromBytes(data, ref offset);
            var evt = ctx.Evt;
            evt.Emit(msgID, msg);
        }

    }

}