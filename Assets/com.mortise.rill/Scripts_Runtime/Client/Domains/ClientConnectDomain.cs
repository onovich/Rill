using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace MortiseFrame.Rill {

    internal static class ClientConnectDomain {

        internal static void Connect(ClientContext ctx, string remoteIP, int port) {
            if (ctx.Connecting || ctx.Connected) {
                return;
            }

            ctx.Connecting_Set(true);

            var receiveThread = new Thread(() => {
                Listen(ctx, remoteIP, port);
            });

            receiveThread.IsBackground = true;
            ctx.ReceiveThread_Set(receiveThread);
            receiveThread.Start();
        }

        static void Listen(ClientContext ctx, string remoteIP, int port) {

            Thread sendThread = null;

            try {

                var client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                ctx.Client_Set(client);

                IPAddress ipAddress = IPAddress.Parse(remoteIP);
                IPEndPoint ep = new IPEndPoint(ipAddress, port);

                client.NoDelay = ctx.NoDelay;
                client.SendTimeout = ctx.SendTimeout;
                client.ReceiveTimeout = ctx.ReceiveTimeout;

                client.Connect(ep);
                ctx.Connecting_Set(false);
                ctx.Evt.EmitConnect();

                sendThread = new Thread(() => {
                    ClientSendDomain.ThreadTick_Send(ctx);
                });
                sendThread.IsBackground = true;
                sendThread.Start();

                ClientReceiveDomain.ThreadTick_Receive(ctx);

            } catch (SocketException exception) {
                RLog.Log("Client Receive: Failed To Connect To IP =" + remoteIP + " PORT = " + port + " Reason = " + exception);
                ctx.Evt.EmitDisconnect();
            } catch (ThreadInterruptedException) {
            } catch (ThreadAbortException) {
            } catch (ObjectDisposedException) {
            } catch (Exception exception) {
                RLog.Error("Client Receive Exception: " + exception);
            }

            sendThread?.Interrupt();
            ctx.Connecting_Set(false);
            ctx.Client?.Close();
        }

    }

}