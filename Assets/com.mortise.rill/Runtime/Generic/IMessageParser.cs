
namespace MortiseFrame.Rill.Generic {

    public interface IMessageParser<T> where T : IMessage<T> {

        T ParseBytes(ushort id, byte[] body, ref int offset);

    }
}