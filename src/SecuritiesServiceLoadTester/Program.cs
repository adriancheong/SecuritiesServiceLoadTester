using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace SecuritiesServiceLoadTester
{
    public class Program
    {
        private static string SECURITY_SERVICE_ADDRESS = "http://Classic-959273653.ap-southeast-1.elb.amazonaws.com:16555/";
        private static string MODE = "default";
        private static readonly int DEFAULT_SPAM_COUNT = 200;
        private static readonly int DEFAULT_NUMBER_OF_THREADS = 1;
        private static readonly int DEFAULT_NUMBER_OF_RUNS = 1;

        private static int threads = DEFAULT_NUMBER_OF_THREADS;
        private static int spamCount = DEFAULT_SPAM_COUNT;
        private static int loadLosses;
        private static int computeLosses;

        //Usage:
        //dotnet /root/SecuritiesServiceLoadTester.dll
        //Params:
        //HOST: -h <host ip>
        //REAL TIME MODE: -rt
        public static void Main(string[] args)
        {
            parse(args);
            Console.WriteLine("SERVER HOST:\t" + SECURITY_SERVICE_ADDRESS);
            Console.WriteLine("Load Test Mode:\t" + MODE);

            switch (MODE)
            {
                case "realtime":
                    performRealTimeScaleAnalysis();
                    break;
                default:
                    defaultMode(args);
                    break;
            }
            

            Console.WriteLine("Press enter to exit");
            Console.ReadLine();
        }

        private static void defaultMode(string[] args)
        {
            double syncLoadTestTotal = 0;
            double asyncLoadTestTotal = 0;
            double syncComputeTestTotal = 0;
            double asyncComputeTestTotal = 0;

            Console.WriteLine("---=== Starting Load Tests ===---");
            Console.WriteLine("Number of calls:\t" + spamCount);
            Console.WriteLine("Number of threads:\t" + threads);
            Console.WriteLine("Number of runs:\t\t" + DEFAULT_NUMBER_OF_RUNS);
            Console.WriteLine();

            for (int i = 0; i < DEFAULT_NUMBER_OF_RUNS; i++)
            {
                Console.WriteLine("---=== Commencing run {0} ===---", i + 1);
                loadLosses = 0;
                computeLosses = 0;
                syncLoadTestTotal += performSyncLoadTest(spamCount);
                //System.Threading.Thread.Sleep(2000);
                //asyncLoadTestTotal += performAsyncLoadTest(spamCount);
                System.Threading.Thread.Sleep(2000);
                syncComputeTestTotal += performSyncComputeTest(spamCount);
                //System.Threading.Thread.Sleep(2000);
                //asyncComputeTestTotal += performAsyncComputeTest(spamCount);
                Console.WriteLine("Run Completed. Load Losses: " + loadLosses);
                Console.WriteLine("Run Completed. Compute Losses: " + computeLosses);
                Console.WriteLine("---=== Run {0} Completed ===---", i + 1);
                Console.WriteLine();
                System.Threading.Thread.Sleep(5000);
            }

            Console.WriteLine();
            Console.WriteLine("---=== Results ===---");
            Console.WriteLine("Sync Load Test Average:\t\t{0} seconds", syncLoadTestTotal / DEFAULT_NUMBER_OF_RUNS);
            Console.WriteLine("Async Load Test Average:\t{0} seconds", asyncLoadTestTotal / DEFAULT_NUMBER_OF_RUNS);
            Console.WriteLine("Sync Compute Test Average:\t{0} seconds", syncComputeTestTotal / DEFAULT_NUMBER_OF_RUNS);
            Console.WriteLine("Async Compute Test Average:\t{0} seconds", asyncComputeTestTotal / DEFAULT_NUMBER_OF_RUNS);
            Console.WriteLine();
        }

        private static void parse(string[] args)
        {
            if (args == null)
                return;

            if (args.Contains("-h"))
            {
                string host = args[Array.IndexOf(args, "-h") + 1];
                SECURITY_SERVICE_ADDRESS = "http://" + host + ":16555/";
            }

            if (args.Contains("-t"))
            {
                int.TryParse(args[Array.IndexOf(args, "-t") + 1], out threads);
            }

            if (args.Contains("-c"))
            {
                int.TryParse(args[Array.IndexOf(args, "-c") + 1], out spamCount);
            }

            if (args.Contains("-rt"))
            {
                MODE = "realtime";
            }
        }

        private static void performRealTimeScaleAnalysis()
        {
            Console.WriteLine("Performing Real Time Scale Analysis");
            Stopwatch stopwatch = new Stopwatch();
            for (int i = 0; i < 10000; i++)
            {
                stopwatch.Restart();
                computeTest(2);
                Console.WriteLine("Web server response time (seconds): {0} ", stopwatch.Elapsed.TotalSeconds);
            }
        }

        private static double performAsyncComputeTest(int spamCount)
        {
            Stopwatch stopwatch = new Stopwatch();
            Console.WriteLine("Begin Async Compute Test");
            stopwatch.Start();
            runMethodAsynchronously(() => computeTest(spamCount / threads));
            stopwatch.Stop();
            Console.WriteLine("Compute test complete. Total time taken: {0} seconds", stopwatch.Elapsed.TotalSeconds);

            return stopwatch.Elapsed.TotalSeconds;
        }

        private static double performSyncComputeTest(int spamCount)
        {
            Stopwatch stopwatch = new Stopwatch();
            Console.WriteLine("Begin Sync Compute Test");
            stopwatch.Start();
            computeTest(spamCount);
            stopwatch.Stop();
            Console.WriteLine("Compute test complete. Total time taken: {0} seconds", stopwatch.Elapsed.TotalSeconds);

            return stopwatch.Elapsed.TotalSeconds;
        }

        private static void computeTest(int spamCount)
        {
            for (int i = 0; i < spamCount; i++)
            {
                string param = "ISIN" + (i);
                string paramString = @"Security/Compute/" + param;
                try
                {
                    string result = restGet(SECURITY_SERVICE_ADDRESS, paramString);
                }
                catch (Exception e)
                {
                    Console.WriteLine("Exception Occured: " + e.Message);
                    computeLosses++;
                }
            }
        }

        private static double performAsyncLoadTest(int spamCount)
        {
            Stopwatch stopwatch = new Stopwatch();
            Console.WriteLine("Begin Async Load Test");
            stopwatch.Start();
            runMethodAsynchronously(() => loadTest(spamCount / threads));
            stopwatch.Stop();
            Console.WriteLine("Load test complete. Total Securities Queried: {0}", spamCount);
            Console.WriteLine("Load test complete. Total time taken: {0} seconds", stopwatch.Elapsed.TotalSeconds);

            return stopwatch.Elapsed.TotalSeconds;
        }

        private static double performSyncLoadTest(int spamCount)
        {
            Stopwatch stopwatch = new Stopwatch();
            Console.WriteLine("Begin Sync Load Test");
            stopwatch.Start();
            loadTest(spamCount);
            stopwatch.Stop();
            Console.WriteLine("Load test complete. Total Securities Queried: {0}", spamCount);
            Console.WriteLine("Load test complete. Total time taken: {0} seconds", stopwatch.Elapsed.TotalSeconds);

            return stopwatch.Elapsed.TotalSeconds;
        }

        private static void loadTest(int spamCount)
        {
            for (int i = 0; i < spamCount; i++)
            {
                string param = "ISIN" + (i);
                string paramString = @"Security?securityId="+param+"&property=Valuation";
                try
                {
                    string result = restGet(SECURITY_SERVICE_ADDRESS, paramString);
                    if (String.IsNullOrEmpty(result))
                    {
                        Console.WriteLine("Empty or null value returned");
                        loadLosses++;
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine("Exception Occured: " + e.Message);
                    loadLosses++;
                }
                
            }
        }

        private static void runMethodAsynchronously(Action action)
        {
            Task[] tasks = new Task[threads];
            for (int i = 0; i < tasks.Length; i++)
            {
                tasks[i] = Task.Run(action);
            }
            Task.WaitAll(tasks);
        }

        private static string restGet(string url, string parameters)
        {
            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri(url);

            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            HttpResponseMessage response = client.GetAsync(parameters).Result;  // Blocking call!
            if (response.IsSuccessStatusCode)
            {
                var result = response.Content.ReadAsStringAsync().Result;
                return result;
            }
            else
            {
                return string.Format("Error: {0} - ({1})", (int)response.StatusCode, response.ReasonPhrase);
            }
        }
    }
}
