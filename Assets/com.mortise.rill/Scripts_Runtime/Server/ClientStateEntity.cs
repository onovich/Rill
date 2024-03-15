using System.Net.Sockets;
using System.Threading;

namespace MortiseFrame.Rill {

    public class ClientStateEntity {
        public Socket clientfd;
        public int clientIndex;
        public ManualResetEvent sendPending;

        public ClientStateEntity(Socket client, int clientIndex) {
            this.clientfd = client;
            this.clientIndex = clientIndex;
            sendPending = new ManualResetEvent(false);
        }
    }

}