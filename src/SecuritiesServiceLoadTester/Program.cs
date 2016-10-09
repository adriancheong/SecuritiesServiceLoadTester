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
        private static readonly string SECURITY_SERVICE_ADDRESS = "http://128.199.219.151:16555/";
        private static readonly int DEFAULT_SPAM_COUNT = 1000;
        private static readonly int NUMBER_OF_THREADS = 4;

        public static void Main(string[] args)
        {
            int spamCount = DEFAULT_SPAM_COUNT;
            if (args != null && args.Length == 1)
            {
                int.TryParse(args[0], out spamCount);
            }

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            //beginLoadTestSync(spamCount);
            beginLoadTestAsync(spamCount);
            stopwatch.Stop();
            Console.WriteLine("Load test complete. Total Securities Queried: {0}", spamCount);
            Console.WriteLine("Load test complete. Total time taken: {0} seconds", stopwatch.Elapsed.Seconds);

            Console.WriteLine("Press enter to exit");
            Console.ReadLine();
        }

        private static void beginLoadTestSync(int spamCount)
        {
            for (int i = 0; i < spamCount; i++)
            {
                string param = "ISIN" + (i);
                string paramString = @"Security/" + param;
                string result = restGet(SECURITY_SERVICE_ADDRESS, paramString);
                //Console.WriteLine(result);
            }
        }

        private static void beginLoadTestAsync(int spamCount)
        {
            Task[] tasks = new Task[NUMBER_OF_THREADS];
            for (int i = 0; i < tasks.Length; i++)
            {
                tasks[i] = Task.Run(() => beginLoadTestSync(spamCount/tasks.Length));
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
