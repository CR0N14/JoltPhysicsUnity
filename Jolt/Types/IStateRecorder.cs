namespace Jolt
{
    public interface IStateRecorder
    {
        public unsafe void ReadBytes(void* outData, nuint inNumBytes);

        public bool IsEOF();

        public bool IsFailed();

        public unsafe void WriteBytes(void* inData, nuint inNumBytes);
    }
}
