using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace MortiseFrame.Rill {

    internal static class ServerConnectDomain {

        internal static void Bind(ServerContext ctx, IPAddress ip, int port) {
            try {

                IPEndPoint localEndPoint = new IPEndPoint(ip, port);
                var listenfd = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                listenfd.NoDelay = true;
                listenfd.Bind(localEndPoint);

                listenfd.Listen(0);
                ctx.Server_Set(listenfd);

                RLog.Log($"Server Has Started On {ip}:{port}.\nWaiting For A Connection...");

            } catch (Exception e) {
                RLog.Log(e.ToString());
            }
        }


    }

}