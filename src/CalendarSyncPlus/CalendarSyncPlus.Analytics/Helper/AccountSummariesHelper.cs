using System.Collections.Generic;
using Google.Apis.Analytics.v3;
using Google.Apis.Analytics.v3.Data;

namespace CalendarSyncPlus.Analytics.Helper
{
    public class AccountSummariesHelper
    {
        /// <summary>
        ///     Lists account summaries (lightweight tree comprised of
        ///     accounts/properties/profiles) to which the user has access.
        ///     Documentation:
        ///     https://developers.google.com/analytics/devguides/config/mgmt/v3/mgmtReference/management/accountSummaries/list
        /// </summary>
        /// <param name="service">Valid authenticated Analytics Service</param>
        /// <returns>
        ///     List of <see cref="Account" /> Summaries resource -
        ///     https://developers.google.com/analytics/devguides/config/mgmt/v3/mgmtReference/management/accountSummaries
        /// </returns>
        public static AccountSummaries AccountSummaryList(AnalyticsService service)
        {
            //List all of the activities in the specified collection for the current user.  
            // Documentation: https://developers.google.com/+/api/latest/activities/list

            var list = service.Management.AccountSummaries.List();
            list.MaxResults = 1000; // Maximum number of Account Summaries to return per request. 

            var feed = list.Execute();
            var allRows = new List<AccountSummary>();

            //// Loop through until we arrive at an empty page
            while (feed.Items != null)
            {
                allRows.AddRange(feed.Items);

                // We will know we are on the last page when the next page token is
                // null.
                // If this is the case, break.
                if (feed.NextLink == null)
                {
                    break;
                }

                // Prepare the next page of results             
                list.StartIndex = feed.StartIndex + list.MaxResults;
                // Execute and process the next page request
                feed = list.Execute();
            }

            feed.Items = allRows;

            return feed;
        }

        public static string GetAccountId(AnalyticsService service)
        {
            //Get account summary and display them.
            foreach (var account in AccountSummaryList(service).Items)
            {
                // Account
                //Console.WriteLine("Account: " + account.Name + "(" + account.Id + ")");

                return account.Id;
                //foreach (WebPropertySummary wp in account.WebProperties)
                //{

                //    // Web Properties within that account
                //    Console.WriteLine("\tWeb Property: " + wp.Name + "(" + wp.Id + ")");


                //    //Don't forget to check its not null. Believe it or not it could be.  
                //    if (wp.Profiles != null)
                //    {

                //        foreach (ProfileSummary profile in wp.Profiles)
                //        {
                //            // Profiles with in that web property.
                //            Console.WriteLine("\t\tProfile: " + profile.Name + "(" + profile.Id + ")");
                //        }
                //    }
                //}
            }
            return null;
        }
    }
}