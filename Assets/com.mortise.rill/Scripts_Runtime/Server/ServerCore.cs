using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using MortiseFrame.LitIO;

namespace MortiseFrame.Rill {

    public class ServerCore {

        ServerContext ctx;

        public ServerCore() {
            ctx = new ServerContext();
        }

        // Register
        public void Register(Type msgType) {
            ctx.RegisterMessage(msgType);
        }

        // Tick 
        public void Tick(float dt) {
            ServerReceiveDomain.Tick_DeserializeAll(ctx);
        }

        // Send
        public void Send(IMessage msg, ConnectionEntity client) {
            ServerSendDomain.Enqueue(ctx, msg, client);
        }

        // Connect
        public void Start(IPAddress ip, int port) {
            ServerStartDomain.Start(ctx, ip, port);
        }

        // On
        public void On(Type msgType, Action<object> listener) {
            ctx.Evt.On(ctx, msgType, listener);
        }

        public void OnError(Action<string> listener) {
            ctx.Evt.OnError(ctx, listener);
        }

        // Off
        public void Off(Type msgType, Action<object> listener) {
            ctx.Evt.Off(ctx, msgType, listener);
        }

        // Stop
        public void Stop() {
            ServerStopDomain.Stop(ctx);
        }

    }

}