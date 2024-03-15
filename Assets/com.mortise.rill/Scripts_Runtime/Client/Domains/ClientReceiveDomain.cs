using MortiseFrame.LitIO;

namespace MortiseFrame.Rill {

    public static class ClientReceiveDomain {

        public static void Tick_Receive(ClientContext ctx, float dt) {
            var client = ctx.Client;
            if (client == null) {
                return;
            }
            if (!client.Poll(0, System.Net.Sockets.SelectMode.SelectRead)) {
                return;
            }
            byte[] buff = ctx.readBuff;
            int count = client.Receive(buff);
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
        }

        // Read
        static void ReadMessage(ClientContext ctx, byte[] data, ref int offset) {
            var msgID = ByteReader.Read<byte>(data, ref offset);
            var msg = ctx.GetMessage(msgID) as IMessage;

            msg.FromBytes(data, ref offset);
            var evt = ctx.Evt;
            evt.Emit(msgID, msg);
        }

    }

}