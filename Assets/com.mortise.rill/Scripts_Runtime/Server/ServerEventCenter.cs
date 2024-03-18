using System;
using System.Collections.Generic;

namespace MortiseFrame.Rill {

    internal class ServerEventCenter {

        readonly Dictionary<int, List<Action<IMessage, ConnectionEntity>>> eventsDict;
        readonly List<Action<string>> errorEvent;

        internal ServerEventCenter() {
            eventsDict = new Dictionary<int, List<Action<IMessage, ConnectionEntity>>>();
            errorEvent = new List<Action<string>>();
        }

        internal void On<T>(ServerContext ctx, Action<IMessage, ConnectionEntity> listener) where T : IMessage {
            var msgId = ctx.GetMessageID<T>();
            if (!eventsDict.ContainsKey(msgId)) {
                eventsDict[msgId] = new List<Action<IMessage, ConnectionEntity>>();
            }

            eventsDict[msgId].Add(listener);
        }

        internal void OnError(ServerContext ctx, Action<string> listener) {
            errorEvent.Add(listener);
        }

        internal void Off<T>(ServerContext ctx, Action<IMessage, ConnectionEntity> listener) where T : IMessage {
            var msgId = ctx.GetMessageID<T>();
            if (eventsDict.ContainsKey(msgId)) {
                eventsDict[msgId].Remove(listener);
            }
        }

        internal void Emit(int msgId, IMessage msg, ConnectionEntity conn) {
            if (eventsDict.ContainsKey(msgId)) {
                foreach (var listener in eventsDict[msgId]) {
                    listener?.Invoke(msg, conn);
                }
            }
        }

        internal void EmitError(string error) {
            foreach (var listener in errorEvent) {
                listener?.Invoke(error);
            }
        }

        internal void Clear() {
            eventsDict.Clear();
            errorEvent.Clear();
        }

    }

}