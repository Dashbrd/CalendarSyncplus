#region File Header

// /******************************************************************************
//  * 
//  *      Copyright (C) Ankesh Dave 2015 All Rights Reserved. Confidential
//  * 
//  ******************************************************************************
//  * 
//  *      Project:        CalendarSyncPlus
//  *      SubProject:     CalendarSyncPlus.Application
//  *      Author:         Dave, Ankesh
//  *      Created On:     05-02-2015 1:31 PM
//  *      Modified On:    05-02-2015 1:31 PM
//  *      FileName:       IController.cs
//  * 
//  *****************************************************************************/

#endregion

namespace CalendarSyncPlus.Application.Controllers
{
    public interface IController
    {
        void Initialize();

        void Run(bool startMinimized);

        void Shutdown();
    }
}