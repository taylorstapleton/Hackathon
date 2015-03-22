using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hackathon
{
    public class Tweet
    {
        public int Id { get; set; }
        public long TweetId { get; set; }
        public string Text { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
    }

    public class TweetWithBlock
    {
        public int Id { get; set; }
        public long TweetId { get; set; }
        public string Text { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string BlockGroup { get; set; }
    }
}
