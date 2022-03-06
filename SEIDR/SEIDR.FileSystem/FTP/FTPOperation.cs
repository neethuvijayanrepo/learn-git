namespace SEIDR.FileSystem
{
    public enum FTPOperation : byte
    {
        SEND = 0,
        RECEIVE = 1,
        MAKE_DIR_LOCAL = 2,
        MAKE_DIR_REMOTE = 3,
        DELETE_LOCAL = 4,
        DELETE_REMOTE = 5,
        SYNC_LOCAL = 6,
        SYNC_REMOTE = 7,
        SYNC_BOTH = 8,
        SYNC_REGISTER,
        MOVE_REMOTE
    }
}