namespace MortiseFrame.Rill {

    internal static class ServerStopDomain {

        internal static void Stop(ServerContext ctx) {

            if (!ctx.Active) return;

            RLog.Log("[] Server: stopping...");

            ctx.Listener?.Close();

            ctx.ListenerThread?.Interrupt();
            ctx.ListenerThread_Clear();

            ctx.ClientState_ForEachOrderly((client) => {
                try { client.clientfd.Close(); } catch { }
            });

            ctx.Clear();

        }

    }

}