namespace CalendarSyncPlus.Services.Utilities
{
    public class SyncEventArgs
    {
        internal SyncEventArgs(string message, UserActionEnum action)
        {
            Message = message;
            UserAction = action;
        }

        internal SyncEventArgs(StatsFieldEnum action, int value)
        {
            UserAction = UserActionEnum.StatsUpdate;
            StatsField = action;
            StatsValue = value;
        }

        public string Message { get; private set; }
        public UserActionEnum UserAction { get; private set; }
        public StatsFieldEnum StatsField { get; private set; }
        public int StatsValue { get; private set; }
    }

    public enum StatsFieldEnum
    {
        SourceCount,
        SourceAddCount,
        SourceDeleteCount,
        SourceUpdateCount,
        DestCount,
        DestAddCount,
        DestDeleteCount,
        DestUpdateCount
    }

    public enum UserActionEnum
    {
        ConfirmDelete,
        StatsUpdate
    }
}