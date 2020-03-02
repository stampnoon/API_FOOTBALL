using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Globalization;
using Football_API.Models.Models_Fixture;
using Football_API.Models.Models_Odds;
using Football_API.Models;
using Newtonsoft.Json;
using RestSharp;

namespace FootballAPI.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]

    public class OddsController : ControllerBase
    {
        private string KeyAPI = "90b616048dmsh3fb273edc577494p17d18fjsn910fdd366c9f";

        [HttpGet("{leagueID}")]
        public ActionResult<IEnumerable<List_LeagueOddsFixture>> GetOdds_SoccerByLeague(string leagueID)
        {
            string date = DateTime.Now.ToString("yyyy-MM-dd", new CultureInfo("en-US"));
            // Initial List 
            List<Football_API.Models.Models_Fixture.Fixture> ListFixture = new List<Football_API.Models.Models_Fixture.Fixture>();
            List<Football_API.Models.Models_Odds.Odd> ListOdds = new List<Football_API.Models.Models_Odds.Odd>();
            List<LeagueOddsFixture> ListLeagueFixOdds = new List<LeagueOddsFixture>();
            List<List_LeagueOddsFixture> Ret_LeagureOddsFixture = new List<List_LeagueOddsFixture>();

            //Get Json object Fixture
            string JsonStr_Fixture = GetFixtureByLeagueID(leagueID, date);
            List<RootObjectFixture> APIJsonObjectFixture = JsonConvert.DeserializeObject<List<RootObjectFixture>>(JsonStr_Fixture);
            if (APIJsonObjectFixture != null)
            {
                ListFixture = APIJsonObjectFixture[0].api.fixtures; //Json ก้อนใหญ่มีก้อนเดียวเสมอ
            }

            //Get Json object Odds
            string JsonStr_Odds = GetOddByLeagueID(leagueID);
            List<RootObjectOdds> APIJsonObjectOdds = JsonConvert.DeserializeObject<List<RootObjectOdds>>(JsonStr_Odds);
            if (APIJsonObjectOdds != null)
            {
                ListOdds = APIJsonObjectOdds[0].api.odds; //Json ก้อนใหญ่มีก้อนเดียวเสมอ
            }
            if (ListFixture.Count != 0)
            {
                //====== Match FixtureID between "Fixture" & "Odds" =======
                foreach (var eachfixture in ListFixture)
                {
                    //หาตัวที่มี FixtureID เดียวกัน
                    var OddsMatch = ListOdds.FirstOrDefault(it => it.fixture.fixture_id == eachfixture.fixture_id);
                    if (OddsMatch != null)
                    {
                        var oddHome = OddsMatch.bookmakers[0].bets[0].values.FirstOrDefault(it2 => it2.value == "Home");
                        var oddDraw = OddsMatch.bookmakers[0].bets[0].values.FirstOrDefault(it2 => it2.value == "Draw");
                        var oddAway = OddsMatch.bookmakers[0].bets[0].values.FirstOrDefault(it2 => it2.value == "Away");
                        double perHome = (Convert.ToDouble(oddAway.odd) * 100) / (Convert.ToDouble(oddHome.odd) + Convert.ToDouble(oddDraw.odd) + Convert.ToDouble(oddAway.odd));
                        double perDraw = (Convert.ToDouble(oddDraw.odd) * 100) / (Convert.ToDouble(oddHome.odd) + Convert.ToDouble(oddDraw.odd) + Convert.ToDouble(oddAway.odd));
                        double perAway = (Convert.ToDouble(oddHome.odd) * 100) / (Convert.ToDouble(oddHome.odd) + Convert.ToDouble(oddDraw.odd) + Convert.ToDouble(oddAway.odd));

                        var item = new LeagueOddsFixture
                        {
                            LeagueID = eachfixture.league_id.ToString(),
                            LeagueName = eachfixture.league.name,
                            LeagueCountry = eachfixture.league.country,
                            LeagueLogo = eachfixture.league.logo,
                            LeagueFlag = eachfixture.league.flag,
                            EventDate = eachfixture.event_date,
                            MatchStatus = eachfixture.status,
                            HometeamName = eachfixture.homeTeam.team_name,
                            HometeamLogo = eachfixture.homeTeam.logo,
                            HometeamScore = eachfixture.goalsHomeTeam,
                            AwayteamName = eachfixture.awayTeam.team_name,
                            AwayteamLogo = eachfixture.awayTeam.logo,
                            AwayteamScore = eachfixture.goalsAwayTeam,
                            OddsBookmaker = OddsMatch.bookmakers[0].bookmaker_name,
                            OddsLabal = OddsMatch.bookmakers[0].bets[0].label_name,
                            OddsHome = oddHome.odd,
                            OddsDraw = oddDraw.odd,
                            OddsAway = oddAway.odd,
                            PerHome = perHome.ToString(),
                            PerDraw = perDraw.ToString(),
                            PerAway = perAway.ToString()
                        };
                        ListLeagueFixOdds.Add(item);
                    }
                }
                //Add into Return List
                var item2 = new List_LeagueOddsFixture
                {
                    LeagueID = ListLeagueFixOdds[0].LeagueID,
                    LeagueName = ListLeagueFixOdds[0].LeagueName,
                    LeagueOddsFixture = ListLeagueFixOdds.OrderBy(c => c.EventDate).ToList()
                };
                Ret_LeagureOddsFixture.Add(item2);
            }
            return Ret_LeagureOddsFixture;
        }

        [HttpPost]
        public ActionResult<IEnumerable<List_LeagueOddsFixture>> GetOdds_SoccerManyLeague([FromBody] string[] AllLeague)
        {
            string date = DateTime.Now.ToString("yyyy-MM-dd", new CultureInfo("en-US"));
            List<List_LeagueOddsFixture> Ret_LeagureOddsFixture = new List<List_LeagueOddsFixture>();

            foreach (string leagueID in AllLeague)
            {
                List<LeagueOddsFixture> ListLeagueFixOdds = new List<LeagueOddsFixture>();
                List<Football_API.Models.Models_Fixture.Fixture> ListFixture = new List<Football_API.Models.Models_Fixture.Fixture>();
                List<Football_API.Models.Models_Odds.Odd> ListOdds = new List<Football_API.Models.Models_Odds.Odd>();
                //Get Json object Fixture
                string JsonStr_Fixture = GetFixtureByLeagueID(leagueID, date);
                List<RootObjectFixture> APIJsonObjectFixture = JsonConvert.DeserializeObject<List<RootObjectFixture>>(JsonStr_Fixture);
                if (APIJsonObjectFixture != null)
                {
                    ListFixture = APIJsonObjectFixture[0].api.fixtures; //Json ก้อนใหญ่มีก้อนเดียวเสมอ
                }

                //Get Json object Odds
                string JsonStr_Odds = GetOddByLeagueID(leagueID);
                List<RootObjectOdds> APIJsonObjectOdds = JsonConvert.DeserializeObject<List<RootObjectOdds>>(JsonStr_Odds);
                if (APIJsonObjectOdds != null)
                {
                    ListOdds = APIJsonObjectOdds[0].api.odds; //Json ก้อนใหญ่มีก้อนเดียวเสมอ
                }

                if (ListFixture.Count != 0)
                {
                    foreach (var eachfixture in ListFixture)
                    {
                        //หาตัวที่มี FixtureID เดียวกัน
                        var OddsMatch = ListOdds.FirstOrDefault(it => it.fixture.fixture_id == eachfixture.fixture_id);
                        if (OddsMatch != null)
                        {
                            var oddHome = OddsMatch.bookmakers[0].bets[0].values.FirstOrDefault(it2 => it2.value == "Home");
                            var oddDraw = OddsMatch.bookmakers[0].bets[0].values.FirstOrDefault(it2 => it2.value == "Draw");
                            var oddAway = OddsMatch.bookmakers[0].bets[0].values.FirstOrDefault(it2 => it2.value == "Away");
                            double perHome = (Convert.ToDouble(oddAway.odd) * 100) / (Convert.ToDouble(oddHome.odd) + Convert.ToDouble(oddDraw.odd) + Convert.ToDouble(oddAway.odd));
                            double perDraw = (Convert.ToDouble(oddDraw.odd) * 100) / (Convert.ToDouble(oddHome.odd) + Convert.ToDouble(oddDraw.odd) + Convert.ToDouble(oddAway.odd));
                            double perAway = (Convert.ToDouble(oddHome.odd) * 100) / (Convert.ToDouble(oddHome.odd) + Convert.ToDouble(oddDraw.odd) + Convert.ToDouble(oddAway.odd));

                            var item = new LeagueOddsFixture
                            {
                                LeagueID = eachfixture.league_id.ToString(),
                                LeagueName = eachfixture.league.name,
                                LeagueCountry = eachfixture.league.country,
                                LeagueLogo = eachfixture.league.logo,
                                LeagueFlag = eachfixture.league.flag,
                                EventDate = eachfixture.event_date,
                                MatchStatus = eachfixture.status,
                                HometeamName = eachfixture.homeTeam.team_name,
                                HometeamLogo = eachfixture.homeTeam.logo,
                                HometeamScore = eachfixture.goalsHomeTeam,
                                AwayteamName = eachfixture.awayTeam.team_name,
                                AwayteamLogo = eachfixture.awayTeam.logo,
                                AwayteamScore = eachfixture.goalsAwayTeam,
                                OddsBookmaker = OddsMatch.bookmakers[0].bookmaker_name,
                                OddsLabal = OddsMatch.bookmakers[0].bets[0].label_name,
                                OddsHome = oddHome.odd,
                                OddsDraw = oddDraw.odd,
                                OddsAway = oddAway.odd,
                                PerHome = perHome.ToString(),
                                PerDraw = perDraw.ToString(),
                                PerAway = perAway.ToString()
                            };
                            ListLeagueFixOdds.Add(item);
                        }
                    }
                    //Add into Return List
                    var item2 = new List_LeagueOddsFixture
                    {
                        LeagueID = ListLeagueFixOdds[0].LeagueID,
                        LeagueName = ListLeagueFixOdds[0].LeagueName,
                        LeagueOddsFixture = ListLeagueFixOdds.OrderBy(c => c.EventDate).ToList()
                    };
                    Ret_LeagureOddsFixture.Add(item2);
                }
            }
            return Ret_LeagureOddsFixture.OrderBy(c => c.LeagueName).ToList();
        }

        #region Private Function
        [HttpGet]
        private string GetFixtureByLeagueID(string leagueID, string date)
        {
            string URL = "https://api-football-v1.p.rapidapi.com/v2/fixtures/league/" + leagueID + "/" + date;
            var client = new RestClient(URL);
            var request = new RestRequest(Method.GET);
            request.AddHeader("x-rapidapi-host", "api-football-v1.p.rapidapi.com");
            request.AddHeader("x-rapidapi-key", KeyAPI);
            IRestResponse response = client.Execute(request);
            string JsonStr = response.Content.ToString();
            //ใส่ [] ครอบหน้าหลัง หากยังไม่มี 
            if (!JsonStr.StartsWith("["))
            {
                JsonStr = "[" + JsonStr;
            }
            if (!JsonStr.EndsWith("]"))
            {
                JsonStr = JsonStr + "]";
            }
            return JsonStr;
        }

        [HttpGet]
        private string GetOddByLeagueID(string leagueID)
        {
            string URL = "https://api-football-v1.p.rapidapi.com/v2/odds/league/" + leagueID + "/label/1";
            var client = new RestClient(URL);
            var request = new RestRequest(Method.GET);
            request.AddHeader("x-rapidapi-host", "api-football-v1.p.rapidapi.com");
            request.AddHeader("x-rapidapi-key", KeyAPI);
            IRestResponse response = client.Execute(request);
            string JsonStr = response.Content.ToString();
            if (!JsonStr.StartsWith("["))
            {
                JsonStr = "[" + JsonStr;
            }
            if (!JsonStr.EndsWith("]"))
            {
                JsonStr = JsonStr + "]";
            }
            return JsonStr;
        }
        #endregion
    }
}