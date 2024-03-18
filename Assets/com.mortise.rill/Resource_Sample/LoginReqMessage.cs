using System;
using MortiseFrame.LitIO;
using MortiseFrame.Rill;

public struct LoginReqMessage : IMessage {

    public string userToken;

    public void WriteTo(byte[] dst, ref int offset) {
        ByteWriter.WriteUTF8String(dst, userToken, ref offset);
    }

    public void FromBytes(byte[] src, ref int offset) {
        userToken = ByteReader.ReadUTF8String(src, ref offset);
    }

    public int GetEvaluatedSize(out bool isCertain) {
        int count = ByteCounter.CountUTF8String(userToken);
        isCertain = false;
        return count;
    }

    public byte[] ToBytes() {
        int count = GetEvaluatedSize(out bool isCertain);
        int offset = 0;
        byte[] src = new byte[count];
        WriteTo(src, ref offset);
        if (isCertain) {
            return src;
        } else {
            byte[] dst = new byte[offset];
            Buffer.BlockCopy(src, 0, dst, 0, offset);
            return dst;
        }
    }

}