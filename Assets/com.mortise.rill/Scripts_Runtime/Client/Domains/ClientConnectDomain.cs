using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace MortiseFrame.Rill {

    public static class ClientConnectDomain {

        public static async Task Connect(ClientContext ctx, string remoteIP, int port) {
            try {
                var client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                client.NoDelay = true;
                IPAddress ipAddress = IPAddress.Parse(remoteIP);

                await Task.Factory.FromAsync(
                    client.BeginConnect,
                    client.EndConnect,
                    new IPEndPoint(ipAddress, port),
                    null);

                ctx.Client_Set(client);

            } catch (SocketException e) {
                var errorMsg = RequestErrorCollection.ErrorMessages[(int)e.SocketErrorCode];
                RLog.Log($"连接失败: {errorMsg}");
                ctx.Evt.EmitError(errorMsg);
            } catch (Exception e) {
                RLog.Error($"异常: {e.ToString()}");
            }
        }

    }

}