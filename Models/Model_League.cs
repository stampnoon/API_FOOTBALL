using System.Collections.Generic;

namespace Football_API.Models
{
    public class List_League
    {
        public string ID { get; set; }
        public string Name { get; set; }
        public string Logo { get; set; }
        public string Type { get; set; }
        public string Country { get; set; }
        public string Season { get; set; }
        public string Season_Start { get; set; }
        public string Season_End { get; set; }
    }

    public class RootObjectLeague
    {
        public Api api { get; set; }
    }

    public class Api
    {
        public int results { get; set; }
        public List<League> leagues { get; set; }
    }

    public class League
    {
        public int league_id { get; set; }
        public string name { get; set; }
        public string type { get; set; }
        public string country { get; set; }
        public string country_code { get; set; }
        public string season { get; set; }
        public string season_start { get; set; }
        public string season_end { get; set; }
        public string logo { get; set; }
        public string flag { get; set; }
        public int standings { get; set; }
        public int is_current { get; set; }
        public Coverage coverage { get; set; }
    }

    public class Coverage
    {
        public bool standings { get; set; }
        public Fixtures fixtures { get; set; }
        public bool players { get; set; }
        public bool topScorers { get; set; }
        public bool predictions { get; set; }
        public bool odds { get; set; }
    }

    public class Fixtures
    {
        public bool events { get; set; }
        public bool lineups { get; set; }
        public bool statistics { get; set; }
        public bool players_statistics { get; set; }
    }

}

