using System;
using System.Collections.Generic;

namespace MortiseFrame.Rill {

    internal class ClientEventCenter {

        readonly Dictionary<int, List<Action<object>>> eventsDict;
        readonly List<Action<string>> errorEvent;
        readonly List<Action> connectEvent;

        internal ClientEventCenter() {
            eventsDict = new Dictionary<int, List<Action<object>>>();
            errorEvent = new List<Action<string>>();
        }

        internal void On(ClientContext ctx, Type type, Action<object> listener) {
            var msgId = ctx.GetMessageID(type);
            if (!eventsDict.ContainsKey(msgId)) {
                eventsDict[msgId] = new List<Action<object>>();
            }

            eventsDict[msgId].Add(listener);
        }

        internal void OnError(ClientContext ctx, Action<string> listener) {
            errorEvent.Add(listener);
        }

        internal void OnConnect(ClientContext ctx, Action listener) {
            connectEvent.Add(listener);
        }

        internal void Off(ClientContext ctx, Type msgType, Action<object> listener) {
            var msgId = ctx.GetMessageID(msgType);
            if (eventsDict.ContainsKey(msgId)) {
                eventsDict[msgId].Remove(listener);
            }
        }

        internal void OffError(ClientContext ctx, Action<string> listener) {
            errorEvent.Remove(listener);
        }

        internal void OffConnect(ClientContext ctx, Action listener) {
            connectEvent.Remove(listener);
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

        internal void EmitConnect() {
            foreach (var listener in connectEvent) {
                listener?.Invoke();
            }
        }

        internal void Clear() {
            eventsDict.Clear();
            errorEvent.Clear();
            connectEvent.Clear();
        }

    }

}