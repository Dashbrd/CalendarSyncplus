#region File Header
// /******************************************************************************
//  * 
//  *      Copyright (C) Ankesh Dave 2015 All Rights Reserved. Confidential
//  * 
//  ******************************************************************************
//  * 
//  *      Project:        CalendarSyncPlus
//  *      SubProject:     CalendarSyncPlus.Presentation
//  *      Author:         Dave, Ankesh
//  *      Created On:     17-06-2015 2:16 PM
//  *      Modified On:    17-06-2015 2:16 PM
//  *      FileName:       FileDialogService.cs
//  * 
//  *****************************************************************************/
#endregion

using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Waf.Applications.Services;
using CalendarSyncPlus.Application.Views;
using IDefaultFileDialogService = System.Waf.Applications.Services.IFileDialogService;
using IFileDialogService = CalendarSyncPlus.Services.Interfaces.IFileDialogService;

namespace CalendarSyncPlus.Presentation.Services
{
    [Export(typeof(IFileDialogService))]
    public class FileDialogService : CalendarSyncPlus.Services.Interfaces.IFileDialogService
    {
        private readonly IShellView _shellView;
        private readonly IDefaultFileDialogService _defaultFileDialogService;

        [ImportingConstructor]
        public FileDialogService(IShellView shellView,IDefaultFileDialogService defaultFileDialogService)
        {
            _shellView = shellView;
            _defaultFileDialogService = defaultFileDialogService;
        }

        public FileDialogResult ShowOpenFileDialog(IEnumerable<FileType> fileTypes,FileType defaultFileType,string defaultFileName)
        {
            return _defaultFileDialogService.ShowOpenFileDialog(_shellView, fileTypes, defaultFileType, defaultFileName);
        }

        public FileDialogResult ShowSaveFileDialog(IEnumerable<FileType> fileTypes, FileType defaultFileType, string defaultFileName)
        {
            return _defaultFileDialogService.ShowSaveFileDialog(_shellView, fileTypes, defaultFileType, defaultFileName);
        }
    }
}