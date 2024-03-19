using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace MortiseFrame.Rill {

    internal static class ClientConnectDomain {

        internal static void Connect(ClientContext ctx, string remoteIP, int port) {
            if (ctx.Connecting || ctx.Connected) {
                RLog.Warning("Client can not create connection because an existing connection is connecting or connected");
                return;
            }

            var client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            ctx.Client_Set(client);

            ctx.Connecting_Set(true);

            RLog.Log("Client connecting to ip=" + remoteIP + " port=" + port);

            var receiveThread = new Thread(() => {
                Accept(ctx, remoteIP, port);
            });
            ctx.ReceiveThread_Set(receiveThread);
            receiveThread.Start();

        }

        static void Accept(ClientContext ctx, string remoteIP, int port) {

            Thread sendThread = null;

            try {

                var client = ctx.Client;
                IPAddress ipAddress = IPAddress.Parse(remoteIP);
                IPEndPoint ep = new IPEndPoint(ipAddress, port);
                client.Connect(ep);
                ctx.Connecting_Set(false);

                RLog.Log("Client connected to ip=" + remoteIP + " port=" + port);

                client.NoDelay = CommonConst.NoDelay;
                client.SendTimeout = CommonConst.SendTimeout;
                client.ReceiveTimeout = CommonConst.ReceiveTimeout;

                sendThread = new Thread(() => {
                    // ThreadFunctions.SendLoop(0, state.client, state.sendPipe, state.sendPending);
                });
                sendThread.IsBackground = true;
                sendThread.Start();

                ctx.Evt.EmitConnect();
                ClientReceiveDomain.ThreadTick_Receive(ctx);

            } catch (SocketException exception) {
                RLog.Log("Client Recv: failed to connect to ip=" + remoteIP + " port=" + port + " reason=" + exception);
            } catch (ThreadInterruptedException) {
            } catch (ThreadAbortException) {
            } catch (ObjectDisposedException) {
            } catch (Exception exception) {
                RLog.Error("Client Recv Exception: " + exception);
            }
            // state.receivePipe.Enqueue(0, EventType.Disconnected, default);

            sendThread?.Interrupt();
            ctx.Connecting_Set(false);
            ctx.Client?.Close();
        }

    }

}