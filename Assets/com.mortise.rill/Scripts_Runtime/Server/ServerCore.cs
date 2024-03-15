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
        }

        // Send
        public void Send(IMessage msg, ClientStateEntity client) {
            ServerSendDomain.Send(ctx, msg, client);
        }

        // Connect
        public void Start(IPAddress ip, int port) {
            ServerStartDomain.Start(ctx, ip, port);
        }

        // On
        public void On(IMessage msg, Action<object> listener) {
            ctx.Evt.On(ctx, msg, listener);
        }

        public void OnError(Action<string> listener) {
            ctx.Evt.OnError(ctx, listener);
        }

        // Off
        public void Off(IMessage msg, Action<object> listener) {
            ctx.Evt.Off(ctx, msg, listener);
        }

        // Stop
        public void Stop() {
            ServerStopDomain.Stop(ctx);
        }

    }

}