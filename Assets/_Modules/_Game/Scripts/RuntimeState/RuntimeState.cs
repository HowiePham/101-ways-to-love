using Mimi.Rx.Variables;

namespace Mimi.Prototypes
{
    public class RuntimeState
    {
        private static readonly RuntimeState Instance = new RuntimeState();

        public RxVar<int> TopCompletedLevelOrder { get; } = new RxVar<int>();
        public RxVar<int> CurrentLevelOrder { get; } = new RxVar<int>();
        public RxVar<int> TopLevelOrder { get; } = new RxVar<int>();

        private RuntimeState()
        {
        }

        public static RuntimeState Get()
        {
            return Instance;
        }
    }
}