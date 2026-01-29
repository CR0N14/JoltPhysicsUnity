using static Jolt.Bindings;

namespace Jolt
{
    [GenerateBindings("JPH_StateRecorder")]
    public partial struct StateRecorder
    {
        internal NativeHandle<JPH_StateRecorder> Handle;

        public static StateRecorder Create(IStateRecorder delegates)
        {
            return new StateRecorder { Handle = JPH_StateRecorder_Create(delegates) };
        }
    }
}
