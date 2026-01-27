namespace Jolt
{
    public enum EStateRecorderState : uint // or sbyte since in c++ it's uint8?
    {
        None = 0,
        Global = 1,
        Bodies = 2,
        Contacts = 4,
        Constraints = 8,
        All = Global | Bodies | Contacts | Constraints,
    }
}
