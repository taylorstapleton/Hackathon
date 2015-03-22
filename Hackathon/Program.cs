using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Hackathon
{
    using System.Linq.Expressions;
    using System.Net.Http;
    using System.Runtime.Serialization;

    using Newtonsoft.Json;

    using Tweetinvi;
    using Tweetinvi.Core.Events.EventArguments;

    class Program
    {
        //public static ConcurrentQueue<TweetWithBlock> ResultList = new ConcurrentQueue<TweetWithBlock>();

        public static ConcurrentDictionary<Tuple<double,double>,string> cache = new ConcurrentDictionary<Tuple<double, double>, string>(); 

        public static List<string> FinalResults = new List<string>();

        public static int roundTo = 2;

        static void Main()
        {
            try
            {
                if (File.Exists("C:\\Users\\tstapleton\\Desktop\\locationResults\\cache"))
                {
                    var cacheLines = File.ReadAllLines("C:\\Users\\tstapleton\\Desktop\\locationResults\\cache");

                    foreach (var cacheLine in cacheLines)
                    {
                        var splits = cacheLine.Split(' ');
                        double item1 = double.Parse(splits[0]);
                        double item2 = double.Parse(splits[1]);
                        cache.TryAdd(new Tuple<double, double>(item1, item2), splits[2]);
                    }
                }

                for (int i = 100; i < 470; i++)
                {
                    processFile(i);
                }
                

            }
            finally
            {
                List<string> lines = new List<string>();

                var keys = cache.Keys.ToList();

                foreach (var tuple in keys)
                {
                    if (string.IsNullOrWhiteSpace(cache[tuple]))
                    {
                        continue;
                    }
                    lines.Add(tuple.Item1 + " " + tuple.Item2 + " " + cache[tuple]);
                }

                File.WriteAllLines("C:\\Users\\tstapleton\\Desktop\\locationResults\\cache", lines);
                Console.WriteLine("finished batch!");
                Console.Read();
            }
        }


        public static void processFile(int fileNumber)
        {
            var rawLines = File.ReadAllLines(string.Format("C:\\Users\\tstapleton\\Desktop\\Sunday22\\TweetFile{0}.tweet", fileNumber)).ToList();

            List<TweetWithBlock> tweetsWithLocation = new List<TweetWithBlock>();

            List<Task> tasks = new List<Task>();
            int count = 0;
            for (int i = 0; i < 10000; i++)
            {
                if (count++ % 10 == 0)
                {
                    //Console.WriteLine(count);
                }

                var current = JsonConvert.DeserializeObject<Tweet>(rawLines[i]);

                if (current == null)
                {
                    continue;
                }

                current.Latitude = Math.Round(current.Latitude, roundTo);
                current.Longitude = Math.Round(current.Longitude, roundTo);

                tasks.Add(GetBlockInfo(current, count));
            }

            Task.WaitAll(tasks.ToArray());

            File.AppendAllLines(string.Format("C:\\Users\\tstapleton\\Desktop\\locationResults\\TweetFile{0}.tweet", fileNumber), FinalResults);
            FinalResults.Clear();

            //tweetsWithLocation.Add(await GetBlockInfo(current));
        }


        /*
        static void Main(string[] args)
        {
            while (true)
            {

                List<string> tweetstrings = File.ReadAllLines(string.Format("C:\\Users\\tstapleton\\Desktop\\Sunday22\\TweetFile{0}.tweet", fileNumber)).ToList();
                List<string> resultStrings = new List<string>();

                try
                {

                    List<Tweet> tweets = new List<Tweet>();

                    foreach (var tweet in tweetstrings)
                    {
                        tweets.Add(JsonConvert.DeserializeObject<Tweet>(tweet));
                    }

                    List<Task> taskList = new List<Task>();

                    int count = 0;
                    foreach (var tweet in tweets)
                    {
                        count++;
                        taskList.Add(GetBlockInfo(tweet));
                        if (count % 1000 == 0)
                        {
                            Task.WhenAll(taskList).Wait();
                            
                            Console.WriteLine("1000");

                            while (!ResultList.IsEmpty)
                            {
                                TweetWithBlock current;
                                ResultList.TryDequeue(out current);
                                resultStrings.Add(JsonConvert.SerializeObject(current));
                            }

                            File.AppendAllLines(
                                String.Format(
                                    "C:\\Users\\tstapleton\\Desktop\\locationresults\\TweetFile{0}.tweet",
                                    fileNumber),
                                resultStrings);

                            taskList.Clear();
                            count = 0;
                            resultStrings.Clear();

                        }
                    }

                    Task.WhenAll(taskList).Wait();

                }
                catch (Exception)
                {

                }
                finally
                {
                    File.AppendAllLines(
                        "C:\\Users\\tstapleton\\Desktop\\locationresults\\TweetFile0.tweet",
                        resultStrings);
                    resultStrings.Clear();
                }
                fileNumber++;
            }
        }
         * 
         */

        public static async Task GetBlockInfoAsync(Tweet tweet, int count)
        {

            HttpClient client = new HttpClient();
            string url = "http://data.fcc.gov/api/block/2010/find?latitude={0}&longitude={1}&format=json";
            url = string.Format(url, tweet.Latitude, tweet.Longitude);

            var result = await client.GetAsync(url);

            FccResponse location =  JsonConvert.DeserializeObject<FccResponse>(await result.Content.ReadAsStringAsync());

            /*
            ResultList.Enqueue(
                new TweetWithBlock()
                {
                    Id = tweet.Id,
                    Latitude = tweet.Latitude,
                    LocationData = location,
                    Longitude = tweet.Longitude,
                    Text = tweet.Text,
                    TweetId = tweet.TweetId
                });
             */ 

            Console.WriteLine(count);
        }

        public static async Task GetBlockInfo(Tweet tweet, int count)
        {
            string cacheResult;
            cache.TryGetValue(new Tuple<double, double>(tweet.Latitude, tweet.Longitude), out cacheResult);
            if (!string.IsNullOrWhiteSpace(cacheResult))
            {
                Console.WriteLine("cache hit!!!");
                FinalResults.Add(JsonConvert.SerializeObject(
                new TweetWithBlock()
                {
                    Id = tweet.Id,
                    Latitude = tweet.Latitude,
                    BlockGroup = cacheResult,
                    Longitude = tweet.Longitude,
                    Text = tweet.Text,
                    TweetId = tweet.TweetId
                }));
                return;
            }

            HttpClient client = new HttpClient();
            string url = "http://data.fcc.gov/api/block/2010/find?latitude={0}&longitude={1}&format=json&showall=false";
            url = string.Format(url, tweet.Latitude, tweet.Longitude);

            HttpResponseMessage result;
            try
            {
                 result = await client.GetAsync(url);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return;
            }

            Console.WriteLine(count);

            FccResponse location = JsonConvert.DeserializeObject<FccResponse>(result.Content.ReadAsStringAsync().Result);

            cache.TryAdd(new Tuple<double, double>(tweet.Latitude, tweet.Longitude), location.Block.FIPS);

            FinalResults.Add(JsonConvert.SerializeObject(
                new TweetWithBlock()
                {
                    Id = tweet.Id,
                    Latitude = tweet.Latitude,
                    BlockGroup = location.Block.FIPS,
                    Longitude = tweet.Longitude,
                    Text = tweet.Text,
                    TweetId = tweet.TweetId
                }));
        }

        public static void onTweetReceived(Object sebder, MatchedTweetReceivedEventArgs args)
        {
            //if (args.Tweet.Coordinates != null)
            {
                Console.WriteLine("tweet with coords!");
                Console.WriteLine(args.Tweet.Text);
                return;
            }

            Console.WriteLine("no coords");
        }
    }

    #region commented out

    /*
            TwitterCredentials.SetCredentials("16993590-Cvztsa1QmfKnMWzoFb3rI5gnSi9Mfb84lWFbuo3U7", "flmwsxw0mKqF8wPbwuEIG19JmCiUu6TtuWyP6QH4FAptu", "5TOLpTO4Qljofz1kziiLSNBLI", "fb2O8oqLzsG3KZ6FnX98FtMWXjYuLwyZKe2lpn4eJdwW2Vi000");

            var filteredStream = Stream.CreateFilteredStream();
            filteredStream.AddTrack("ancestry");
            filteredStream.AddTrack("ancestry.com");
            filteredStream.AddTrack("family history");
            filteredStream.AddTrack("findagrave");
            filteredStream.AddTrack("findagrave.com");
            filteredStream.AddTrack("rootsweb");
            filteredStream.AddTrack("rootsweb.com");
            filteredStream.MatchingTweetReceived += onTweetReceived;
            filteredStream.StartStreamMatchingAllConditions();
             * */

    #endregion
}
