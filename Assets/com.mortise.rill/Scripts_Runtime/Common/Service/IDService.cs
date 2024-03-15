namespace MortiseFrame.Rill {

    public class IDService {

        public byte msgIdRecord;

        public IDService() {
            msgIdRecord = 0;
        }

        public byte PickMsgId() {
            return ++msgIdRecord;
        }

        public void Reset() {
            msgIdRecord = 0;
        }

    }

}