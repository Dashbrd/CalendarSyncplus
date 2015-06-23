namespace CalendarSyncPlus.Services.Utilities
{
    public class SyncEventArgs
    {
        internal SyncEventArgs(string message, UserActionEnum actionEnum)
        {
            Message = message;
            UserActionEnum = actionEnum;
        }

        public string Message { get; private set; }
        public UserActionEnum UserActionEnum { get; private set; }
    }

    public enum UserActionEnum
    {
        ConfirmDelete
    }
}