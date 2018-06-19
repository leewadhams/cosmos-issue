using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Mongo2CosmosExporter
{
    public class App
    {
        private readonly ICreateTestData _testDateGenerator;
        private readonly ILogger _logger;

        public App(ICreateTestData testDateGenerator, ILogger<App> logger)
        {
            _logger = logger;
            _testDateGenerator = testDateGenerator;
        }

        public void Run()
        {
            try
            {
                _testDateGenerator.CreateTestData();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Something bad happened :-(");
            }

            Console.ReadLine();
        }
    }
}