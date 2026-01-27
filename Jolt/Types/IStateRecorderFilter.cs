namespace Jolt
{
    public interface IStateRecorderFilter
    {
        public bool ShouldSaveBody();

        public bool ShouldSaveConstraint();

        public bool ShouldSaveContact();

        bool ShouldRestoreContact();
    }
}
