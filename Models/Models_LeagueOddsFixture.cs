using System;
using System.Collections.Generic;

namespace Football_API.Models
{
    public class List_LeagueOddsFixture
    {
        public string LeagueID { get; set; }
        public string LeagueName { get; set; }
        public string LeagueFlag { get; set; }
        public string LeagueCountry { get; set; }
        public List<LeagueOddsFixture> LeagueOddsFixture { get; set; }
    }

    public class LeagueOddsFixture
    {
        public string LeagueID { get; set; }
        public string LeagueName { get; set; }
        public string LeagueCountry { get; set; }
        public string LeagueLogo { get; set; }
        public string LeagueFlag { get; set; }
        public DateTime EventDate { get; set; }
        public string MatchStatus { get; set; }
        public string HometeamName { get; set; }
        public string HometeamLogo { get; set; }
        public string HometeamScore { get; set; }
        public string AwayteamName { get; set; }
        public string AwayteamLogo { get; set; }
        public string AwayteamScore { get; set; }
        public string OddsBookmaker { get; set; }
        public string OddsLabal { get; set; }
        public string OddsHome { get; set; }
        public string OddsDraw { get; set; }
        public string OddsAway { get; set; }
        public string PerHome { get; set; }
        public string PerDraw { get; set; }
        public string PerAway { get; set; }
    }

}

