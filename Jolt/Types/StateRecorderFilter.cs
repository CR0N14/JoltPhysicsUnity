using static Jolt.Bindings;

namespace Jolt
{
    [GenerateBindings("JPH_StateRecorderFilter")]
    public partial struct StateRecorderFilter
    {
        internal NativeHandle<JPH_StateRecorderFilter> Handle;

        public static StateRecorderFilter Create(IStateRecorderFilter delegates)
        {
            return new StateRecorderFilter { Handle = JPH_StateRecorderFilter_Create(delegates) };
        }
    }
}
