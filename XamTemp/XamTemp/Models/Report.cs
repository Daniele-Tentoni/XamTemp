namespace XamTemp.Models
{
    using MongoDB.Bson;
    using Realms;
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;

    class Report: RealmObject
    {
        [PrimaryKey]
        public ObjectId Id { get; set; } = ObjectId.GenerateNewId();
        public double Temperature { get; set; } = 36;
        public int Saturation { get; set; } = 100;
        public bool Sent { get; set; }
        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.Now;
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
