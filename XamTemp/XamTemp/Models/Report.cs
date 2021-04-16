namespace XamTemp.Models
{
    using MongoDB.Bson;
    using Realms;
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using Xamarin.CommunityToolkit.Helpers;
    using XamTemp.Resources.Strings;

    class Report: RealmObject
    {
        [PrimaryKey]
        public ObjectId Id { get; set; } = ObjectId.GenerateNewId();
        public double Temperature { get; set; } = 36;
        public int Saturation { get; set; } = 100;
        public bool Sent { get; set; }
        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.Now;

        public LocalizedString ReportToSend = new LocalizedString(() => AppResources.ReportToSend);
    }

    class ReportGroup: ObservableCollection<Report>
    {
        public DateTimeOffset Date { get; set; }
        public ReportGroup(DateTimeOffset date, List<Report> reports): base(reports)
        {
            Date = date;
        }
    }
}
