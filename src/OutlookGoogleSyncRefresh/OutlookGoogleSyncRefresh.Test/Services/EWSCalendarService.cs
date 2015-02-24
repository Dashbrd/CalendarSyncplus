#region File Header
// /******************************************************************************
//  * 
//  *      Copyright (C) Ankesh Dave 2015 All Rights Reserved. Confidential
//  * 
//  ******************************************************************************
//  * 
//  *      Project:        OutlookGoogleSyncRefresh
//  *      SubProject:     Test
//  *      Author:         Dave, Ankesh
//  *      Created On:     12-02-2015 12:10 PM
//  *      Modified On:    12-02-2015 12:10 PM
//  *      FileName:       EWSCalendarService.cs
//  * 
//  *****************************************************************************/
#endregion

using System;
using System.Collections.Generic;

using Microsoft.Exchange.WebServices.Data;

namespace Test.Services
{
    public class EWSCalendarService
    {
        public EWSCalendarService()
        {
        }

        public ExchangeService GetAuthenticatedEWSInstance()
        {
            var service = new ExchangeService(ExchangeVersion.Exchange2010_SP2)
            {
                //Url = new Uri(@"https://cas.etn.com/ews/exchange.asmx"),
                UseDefaultCredentials = true
            };

            //https://cas.etn.com/ews/exchange.asmx
            service.AutodiscoverUrl("ankeshdave@outlook.com", ValidateRedirectionUrlCallback);

            return service;
        }

        private bool ValidateRedirectionUrlCallback(string redirectionUrl)
        {
            return true;
        }

        public void FindCalendarFolder()
        {
            var service = GetAuthenticatedEWSInstance();


            CalendarFolder calendarFolder = CalendarFolder.Bind(service, WellKnownFolderName.Calendar);

            var result = calendarFolder.FindAppointments(new CalendarView(DateTime.Now.AddDays(-1), DateTime.Now.AddDays(1)));


            foreach (Appointment appointment in result)
            {
                appointment.Load(new PropertySet(BasePropertySet.FirstClassProperties) { RequestedBodyType = BodyType.Text });

                var body = appointment.Body;
                var sub = appointment.Subject;
                var con = appointment.UniqueBody;
                var start = appointment.Start;
                var end = appointment.End;

            }
        }
    }
}