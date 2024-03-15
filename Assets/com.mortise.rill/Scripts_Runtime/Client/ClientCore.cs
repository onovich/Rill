using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using MortiseFrame.LitIO;

namespace MortiseFrame.Rill {

    public class ClientCore {

        ClientContext ctx;

        public ClientCore() {
            ctx = new ClientContext();
        }

        // Tick 
        public void Tick(float dt) {
            ClientReceiveDomain.Tick_Receive(ctx, dt);
            ClientSendDomain.Tick_Send(ctx, dt);
        }

        // Send
        public void Send(IMessage msg) {
            ctx.Message_Enqueue(msg);
        }

        // Connect
        public async Task Connect(string remoteIP, int port) {
            await ClientConnectDomain.Connect(ctx, remoteIP, port);
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

    }

}