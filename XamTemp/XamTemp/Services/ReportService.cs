namespace XamTemp.Services
{
    using MongoDB.Bson;
    using Realms;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using XamTemp.Models;

    class ReportService
    {
        /// <summary>
        /// Get a list of all reports stored.
        /// </summary>
        /// <returns>List of reports.</returns>
        public async Task<IEnumerable<Report>> GetReportsAsync()
        {
            var realm = await Realm.GetInstanceAsync(config).ConfigureAwait(false);
            var reports = realm.All<Report>();
            return reports.OrderByDescending(o => o.CreatedAt);
        }

        /// <summary>
        /// Get a report by his id.
        /// </summary>
        /// <param name="id">Object id.</param>
        /// <returns>Report.</returns>
        public async Task<Report> GetReportByIdAsync(ObjectId id)
        {
            var realm = await Realm.GetInstanceAsync(config).ConfigureAwait(false);
            var report = realm.Find<Report>(id);
            return report;
        }

        /// <summary>
        /// Add a new report complete of all data.
        /// </summary>
        /// <param name="report">Report to add.</param>
        /// <returns>Report added. Use it to retreive the current object id.</returns>
        public async Task<Report> AddReportAsync(Report report)
        {
            var realm = await Realm.GetInstanceAsync(config).ConfigureAwait(false);
            Report added = null;
            realm.Write(() => added = realm.Add(report));
            return added;
        }

        public async Task<Report> SwitchSentReportAsync(Report report)
        {
            var realm = await Realm.GetInstanceAsync(config).ConfigureAwait(false);
            // realm.Write(() => report.Sent = !report.Sent);
            realm.Write(() => report.Sent = !report.Sent);
            return report;
        }

        /// <summary>
        /// Modify a report by his id. Obv, if you can change the id, it's a new object.
        /// </summary>
        /// <param name="report">Report modified.</param>
        /// <returns>Task executed.</returns>
        public async Task<Report> ModifyReportAsync(Report report)
        {
            var realm = await Realm.GetInstanceAsync(config).ConfigureAwait(false);
            Report added = null;
            realm.Write(() => added = realm.Add(report, true));
            return added;
        }

        /// <summary>
        /// Delete a report by his id.
        /// </summary>
        /// <param name="id">Report id.</param>
        /// <returns>Task executed.</returns>
        public async Task DeleteReportAsync(ObjectId id)
        {
            var realm = await Realm.GetInstanceAsync(config).ConfigureAwait(false);
            var elem = realm.Find<Report>(id);
            realm.Write(() => realm.Remove(elem));
        }

        /// <summary>
        /// Reset all report in the database.
        /// </summary>
        /// <returns>Task executed.</returns>
        public async Task ResetData()
        {
            var realm = await Realm.GetInstanceAsync(config).ConfigureAwait(false);
            realm.Write(() => realm.RemoveAll<Report>());
        }

        private readonly RealmConfiguration config = new RealmConfiguration
        {
            SchemaVersion = 1,
            MigrationCallback = (migration, oldSchemaVersion) =>
            {
                var newReports = migration.NewRealm.All<Report>();
                if (oldSchemaVersion < 1)
                {
                    foreach (var report in newReports)
                    {
                        report.Sent = false;
                    }
                }
            }
        };
    }
}
