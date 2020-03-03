using System.Collections.Generic;

namespace Football_API.Models.Models_Odds
{
    public class RootObjectOdds
    {
        public Api api { get; set; }
    }

    public class Api
    {
        //public int results { get; set; }
        public List<Odd> odds { get; set; }
    }

    public class Odd
    {
        public Fixture fixture { get; set; }
        public List<Bookmaker> bookmakers { get; set; }
    }

    public class Fixture
    {
        public string league_id { get; set; }
        public string fixture_id { get; set; }
        public int updateAt { get; set; }
    }

    public class Bookmaker
    {
        public int bookmaker_id { get; set; }
        public string bookmaker_name { get; set; }
        public List<Bet> bets { get; set; }
    }

    public class Bet
    {
        public int label_id { get; set; }
        public string label_name { get; set; }
        public List<Value> values { get; set; }
    }

    public class Value
    {
        public string value { get; set; }
        public string odd { get; set; }
    }

}