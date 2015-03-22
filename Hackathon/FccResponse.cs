
namespace Hackathon
{
    public class Block
    {
        public string FIPS { get; set; }
    }

    public class County
    {
        public string FIPS { get; set; }
        public string name { get; set; }
    }

    public class State
    {
        public string FIPS { get; set; }
        public string code { get; set; }
        public string name { get; set; }
    }

    public class FccResponse
    {
        public Block Block { get; set; }
        public County County { get; set; }
        public State State { get; set; }
        public string status { get; set; }
        public string executionTime { get; set; }
    }
}
