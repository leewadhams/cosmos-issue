using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Authentication;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Mongo2CosmosExporter
{
    public class Transferer : ICreateTestData, IDisposable
    {
        private bool _disposed;
        private readonly DbConnectionSettings _destination;
        private readonly int _batchSize;
        private readonly ILogger _logger;
        private readonly IMongoCollection<BsonDocument> _destinationCollection;
        private readonly Random _rnd = new Random();
        private readonly int _maxDegreeOfParallelism;

        public Transferer(IOptions<AppSettings> settings, ILogger<Transferer> logger)
        {
            _logger = logger;
            _destination = settings.Value.DestinationDb;
            _batchSize = settings.Value.BatchSize;
            _maxDegreeOfParallelism = settings.Value.MaxDegreeOfParallelism;

            _destinationCollection = GetCollection(_destination);
        }

        public void CreateTestData()
        {
            int totalInsertCount = 0;
            SemaphoreSlim semaphore = new SemaphoreSlim(300);
            var dir = Directory.GetCurrentDirectory();
            var uri = Path.Combine(dir, "exampleApplication.json");
            var docJson = File.ReadAllText(uri);

            Parallel.For(0, 1000000, new ParallelOptions { MaxDegreeOfParallelism = _maxDegreeOfParallelism }, (index) =>
            {
                semaphore.Wait();
                var doc = GenerateDummyDocument(docJson);

                try
                {
                    _destinationCollection.InsertOne(doc);
                    Interlocked.Increment(ref totalInsertCount);
                    Console.WriteLine($"Inserted document: {totalInsertCount}");
                }
                catch
                {
                    Console.WriteLine($"Partition Key was: {doc["CandidateId"].AsInt32}");
                    throw;
                }
                finally
                {
                    semaphore.Release();
                }
            });
        }

        private BsonDocument GenerateDummyDocument(string docJson)
        {
            var key = _rnd.Next(1, 10);
            var doc = BsonDocument.Parse(docJson);
            doc["CandidateId"] = key;

            return doc;
        }

        private IMongoCollection<BsonDocument> GetCollection(DbConnectionSettings connectionSettings)
        {
            var settings = new MongoClientSettings
            {
                Server = new MongoServerAddress(connectionSettings.Host, connectionSettings.Port)
            };

            if (connectionSettings.UseSsl)
            {
                settings.UseSsl = true;
                settings.SslSettings = new SslSettings { EnabledSslProtocols = SslProtocols.Tls12 };
            }
            settings.ConnectionMode = ConnectionMode.Direct;

            settings.Credentials = new[]
            {
                MongoCredential.CreateCredential(connectionSettings.AuthDbName, connectionSettings.UserName, connectionSettings.Password)
            };

            var client = new MongoClient(settings);
            var database = client.GetDatabase(connectionSettings.DbName);
            var todoTaskCollection = database.GetCollection<BsonDocument>(connectionSettings.CollectionName);

            return todoTaskCollection;
        }

        private void OutputConfiguration()
        {
            _logger.LogInformation($"BatchSize: {_batchSize}\n");
            _logger.LogInformation(_destination.ToString());
        }

        # region IDisposable

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                }
            }

            _disposed = true;
        }

        #endregion
    }
}