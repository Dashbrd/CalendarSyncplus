using System.Collections.Generic;
using System.Waf.Applications.Services;

namespace CalendarSyncPlus.Services.Interfaces
{
    public interface IFileDialogService
    {
        FileDialogResult ShowOpenFileDialog(IEnumerable<FileType> fileTypes, FileType defaultFileType,
            string defaultFileName);

        FileDialogResult ShowSaveFileDialog(IEnumerable<FileType> fileTypes, FileType defaultFileType,
            string defaultFileName);
    }
}