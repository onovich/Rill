using System;
using System.Collections.Generic;

namespace MortiseFrame.Rill {

    internal class ClientEventCenter {

        readonly Dictionary<int, List<Action<object>>> eventsDict;
        readonly List<Action<string>> errorEvent;

        internal ClientEventCenter() {
            eventsDict = new Dictionary<int, List<Action<object>>>();
            errorEvent = new List<Action<string>>();
        }

        internal void On(ClientContext ctx, IMessage msg, Action<object> listener) {
            var msgId = ctx.GetMessageID(msg);
            if (!eventsDict.ContainsKey(msgId)) {
                eventsDict[msgId] = new List<Action<object>>();
            }

            eventsDict[msgId].Add(listener);
        }

        internal void OnError(ClientContext ctx, Action<string> listener) {
            errorEvent.Add(listener);
        }

        internal void Off(ClientContext ctx, IMessage msg, Action<object> listener) {
            var msgId = ctx.GetMessageID(msg);
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