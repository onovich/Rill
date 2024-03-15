
namespace MortiseFrame.Rill.Generic {

    public interface IMessage<T> {

        byte[] GetBytes(ref int offset);
        ushort GetSize();
        ushort GetID();

    }
}