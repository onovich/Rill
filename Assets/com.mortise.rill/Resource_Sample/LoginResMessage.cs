using System;
using MortiseFrame.LitIO;
using MortiseFrame.Rill;

public struct LoginResMessage : IMessage {

    public sbyte status; // 1 为成功, -1 为失败
    public string userToken;

    public void WriteTo(byte[] dst, ref int offset) {
        ByteWriter.Write<sbyte>(dst, status, ref offset);
        ByteWriter.WriteUTF8String(dst, userToken, ref offset);
    }

    public void FromBytes(byte[] src, ref int offset) {
        status = ByteReader.Read<sbyte>(src, ref offset);
        userToken = ByteReader.ReadUTF8String(src, ref offset);
    }

}