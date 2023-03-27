using System;
using UnityEngine;
using MortiseFrame.Rill.Generic;
using MortiseFrame.Rill.TCP.Client;
using MortiseFrame.Rill.TCP.Server;
using MortiseFrame.LitIO;

namespace MortiseFrame.Rill.Sample {

    public class MessageSample : IMessage<MessageSample> {

        string data;

        public MessageSample(string data) {
            this.data = data;
        }

        public ushort GetID() {
            return 1;
        }
        public ushort GetSize() {
            return (ushort)(sizeof(uint));
        }
        public byte[] GetBytes(ref int offset) {
            var bytes = new byte[GetSize()];
            ByteWrite.Write<uint>(bytes, data, ref offset);
            return bytes;
        }
        public void ParseBytes(byte[] bytes, ref int offset) {
            data = ByteReader.Read<string>(bytes, ref offset);
        }
        public override string ToString() {
            return $"EchoMessage: {data}";
        }

    }

}