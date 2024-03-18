using System;
using System.Collections.Generic;

namespace MortiseFrame.Rill {

    internal class ServerEventCenter {

        readonly Dictionary<int, List<Action<IMessage>>> eventsDict;
        readonly List<Action<string>> errorEvent;

        internal ServerEventCenter() {
            eventsDict = new Dictionary<int, List<Action<IMessage>>>();
            errorEvent = new List<Action<string>>();
        }

        internal void On<T>(ServerContext ctx, Action<IMessage> listener) where T : IMessage {
            var msgId = ctx.GetMessageID<T>();
            if (!eventsDict.ContainsKey(msgId)) {
                eventsDict[msgId] = new List<Action<IMessage>>();
            }

            eventsDict[msgId].Add(listener);
        }

        internal void OnError(ServerContext ctx, Action<string> listener) {
            errorEvent.Add(listener);
        }

        internal void Off<T>(ServerContext ctx, Action<IMessage> listener) where T : IMessage {
            var msgId = ctx.GetMessageID<T>();
            if (eventsDict.ContainsKey(msgId)) {
                eventsDict[msgId].Remove(listener);
            }
        }

        internal void Emit(int msgId, IMessage msg) {
            if (eventsDict.ContainsKey(msgId)) {
                foreach (var listener in eventsDict[msgId]) {
                    listener?.Invoke(msg);
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