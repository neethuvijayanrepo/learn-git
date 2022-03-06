using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SEIDR.FileSystem
{
    public enum FileOperation
    {
        TAG = 1,
        CHECK = 2,
        EXIST = 3,
        COPY = 4,
        MOVE = 5,
        COPYDIR = 6,
        MOVEDIR = 7,
        GRAB = 8,
        GRAB_ALL = 9,
        CREATEDIR = 10,
        DELETE = 11,
        CREATE_DUMMY = 12,
        ZIP = 13,
        UNZIP = 14,
        COPY_METRIX = 15,
        COPY_ALL = 16,
        MOVE_ALL,
        TAG_DEST,
        MOVE_METRIX,
        CHECK_FILTER,
        SIZE_CHECK,
        MOVE_ANY,
        COPY_ANY,
        CREATE_DUMMY_TAG,
        CLEAN_COPY,
        CLEAN_COPY_METRIX
    }
}
