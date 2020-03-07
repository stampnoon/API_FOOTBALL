using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
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
        API API = new API();

        #region ================ Public Function ================
        [HttpGet("{leagueID}")]
        public ActionResult<IEnumerable<List_LeagueOddsFixture>> GetOdds_SoccerByLeague_ThisDay(string leagueID)
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

            if (ListFixture.Count != 0)
            {
                //Get Json object Odds
                string JsonStr_Odds = GetOddByLeagueID(leagueID);
                List<RootObjectOdds> APIJsonObjectOdds = JsonConvert.DeserializeObject<List<RootObjectOdds>>(JsonStr_Odds);
                if (APIJsonObjectOdds != null)
                {
                    ListOdds = APIJsonObjectOdds[0].api.odds; //Json ก้อนใหญ่มีก้อนเดียวเสมอ
                }

                //====== Match FixtureID between "Fixture" & "Odds" =======
                foreach (var eachfixture in ListFixture)
                {
                    //หาตัวที่มี FixtureID เดียวกัน
                    var OddsMatch = ListOdds.FirstOrDefault(it => it.fixture.fixture_id == eachfixture.fixture_id);
                    if (OddsMatch != null)
                    {
                        double perHome = 0, perDraw = 0, perAway = 0;
                        string OddsMaker = "";
                        Value oddHome = new Value();
                        Value oddDraw = new Value();
                        Value oddAway = new Value();

                        foreach (var eachOdds in OddsMatch.bookmakers)
                        {
                            oddHome = eachOdds.bets[0].values.FirstOrDefault(it2 => it2.value == "Home");
                            oddDraw = eachOdds.bets[0].values.FirstOrDefault(it2 => it2.value == "Draw");
                            oddAway = eachOdds.bets[0].values.FirstOrDefault(it2 => it2.value == "Away");
                            if (oddHome != null && oddDraw != null && oddAway != null)
                            {
                                OddsMaker = eachOdds.bookmaker_name;
                                perHome = (Convert.ToDouble(oddAway.odd) * 100) / (Convert.ToDouble(oddHome.odd) + Convert.ToDouble(oddDraw.odd) + Convert.ToDouble(oddAway.odd));
                                perDraw = (Convert.ToDouble(oddDraw.odd) * 100) / (Convert.ToDouble(oddHome.odd) + Convert.ToDouble(oddDraw.odd) + Convert.ToDouble(oddAway.odd));
                                perAway = (Convert.ToDouble(oddHome.odd) * 100) / (Convert.ToDouble(oddHome.odd) + Convert.ToDouble(oddDraw.odd) + Convert.ToDouble(oddAway.odd));
                                break;
                            }
                        }

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
                            OddsBookmaker = OddsMaker,
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
                    LeagueFlag = ListLeagueFixOdds[0].LeagueFlag,
                    LeagueCountry = ListLeagueFixOdds[0].LeagueCountry,
                    LeagueOddsFixture = ListLeagueFixOdds.OrderBy(c => c.EventDate).ToList()
                };
                Ret_LeagureOddsFixture.Add(item2);
            }
            return Ret_LeagureOddsFixture;
        }

        [HttpPost]
        public ActionResult<IEnumerable<List_LeagueOddsFixture>> GetOdds_SoccerManyLeague_ThisDay([FromBody] string[] AllLeague)
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

                if (ListFixture.Count != 0)
                {
                    //Get Json object Odds
                    string JsonStr_Odds = GetOddByLeagueID(leagueID);
                    List<RootObjectOdds> APIJsonObjectOdds = JsonConvert.DeserializeObject<List<RootObjectOdds>>(JsonStr_Odds);
                    if (APIJsonObjectOdds != null)
                    {
                        ListOdds = APIJsonObjectOdds[0].api.odds; //Json ก้อนใหญ่มีก้อนเดียวเสมอ
                    }

                    foreach (var eachfixture in ListFixture)
                    {
                        //หาตัวที่มี FixtureID เดียวกัน
                        var OddsMatch = ListOdds.FirstOrDefault(it => it.fixture.fixture_id == eachfixture.fixture_id);
                        if (OddsMatch != null)
                        {
                            double perHome = 0, perDraw = 0, perAway = 0;
                            string OddsMaker = "";
                            Value oddHome = new Value();
                            Value oddDraw = new Value();
                            Value oddAway = new Value();

                            foreach (var eachOdds in OddsMatch.bookmakers)
                            {
                                oddHome = eachOdds.bets[0].values.FirstOrDefault(it2 => it2.value == "Home");
                                oddDraw = eachOdds.bets[0].values.FirstOrDefault(it2 => it2.value == "Draw");
                                oddAway = eachOdds.bets[0].values.FirstOrDefault(it2 => it2.value == "Away");
                                if (oddHome != null && oddDraw != null && oddAway != null)
                                {
                                    OddsMaker = eachOdds.bookmaker_name;
                                    perHome = (Convert.ToDouble(oddAway.odd) * 100) / (Convert.ToDouble(oddHome.odd) + Convert.ToDouble(oddDraw.odd) + Convert.ToDouble(oddAway.odd));
                                    perDraw = (Convert.ToDouble(oddDraw.odd) * 100) / (Convert.ToDouble(oddHome.odd) + Convert.ToDouble(oddDraw.odd) + Convert.ToDouble(oddAway.odd));
                                    perAway = (Convert.ToDouble(oddHome.odd) * 100) / (Convert.ToDouble(oddHome.odd) + Convert.ToDouble(oddDraw.odd) + Convert.ToDouble(oddAway.odd));
                                    break;
                                }
                            }

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
                                OddsBookmaker = OddsMaker,
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
                        LeagueFlag = ListLeagueFixOdds[0].LeagueFlag,
                        LeagueCountry = ListLeagueFixOdds[0].LeagueCountry,
                        LeagueOddsFixture = ListLeagueFixOdds.OrderBy(c => c.EventDate).ToList()
                    };
                    Ret_LeagureOddsFixture.Add(item2);
                }
            }
            return Ret_LeagureOddsFixture.OrderBy(c => c.LeagueCountry).ToList();
        }

        [HttpGet]
        public ActionResult<IEnumerable<List_LeagueOddsFixture>> GetOdds_SoccerAllLeague_ThisDay()
        {
            string[] Hot_League = HotLeague.HotLeagueID;
            string date = DateTime.Now.ToString("yyyy-MM-dd", new CultureInfo("en-US"));
            List<List_LeagueOddsFixture> Ret_LeagureOddsFixture = new List<List_LeagueOddsFixture>();
            List<Football_API.Models.Models_Fixture.Fixture> ListFixture = new List<Football_API.Models.Models_Fixture.Fixture>();

            //Get Json object Fixture
            string JsonStr_Fixture = GetAllFixtureThisDay(date);
            List<RootObjectFixture> APIJsonObjectFixture = JsonConvert.DeserializeObject<List<RootObjectFixture>>(JsonStr_Fixture);
            if (APIJsonObjectFixture != null)
            {
                ListFixture = APIJsonObjectFixture[0].api.fixtures; //Json ก้อนใหญ่มีก้อนเดียวเสมอ
            }

            if (ListFixture.Count != 0)
            {
                //Group by leagueID in all Fixture
                var groupLeagueID = from lstfixture in ListFixture
                                    group lstfixture by lstfixture.league_id into newGroup
                                    orderby newGroup.Key
                                    select newGroup.Key.ToString();

                foreach (string ID in Hot_League)
                {
                    if (groupLeagueID.Contains(ID))
                    {
                        List<LeagueOddsFixture> ListLeagueFixOdds = new List<LeagueOddsFixture>();
                        List<Football_API.Models.Models_Odds.Odd> ListOdds = new List<Football_API.Models.Models_Odds.Odd>();

                        //Get Json object Odds
                        string JsonStr_Odds = GetOddByLeagueID(ID);
                        List<RootObjectOdds> APIJsonObjectOdds = JsonConvert.DeserializeObject<List<RootObjectOdds>>(JsonStr_Odds);
                        if (APIJsonObjectOdds != null)
                        {
                            ListOdds = APIJsonObjectOdds[0].api.odds; //Json ก้อนใหญ่มีก้อนเดียวเสมอ
                        }

                        foreach (var eachfixture in ListFixture)
                        {
                            //หาตัวที่มี FixtureID เดียวกัน
                            var OddsMatch = ListOdds.FirstOrDefault(it => it.fixture.fixture_id == eachfixture.fixture_id);
                            if (OddsMatch != null)
                            {
                                double perHome = 0, perDraw = 0, perAway = 0;
                                string OddsMaker = "";
                                Value oddHome = new Value();
                                Value oddDraw = new Value();
                                Value oddAway = new Value();

                                foreach (var eachOdds in OddsMatch.bookmakers)
                                {
                                    oddHome = eachOdds.bets[0].values.FirstOrDefault(it2 => it2.value == "Home");
                                    oddDraw = eachOdds.bets[0].values.FirstOrDefault(it2 => it2.value == "Draw");
                                    oddAway = eachOdds.bets[0].values.FirstOrDefault(it2 => it2.value == "Away");
                                    if (oddHome != null && oddDraw != null && oddAway != null)
                                    {
                                        OddsMaker = eachOdds.bookmaker_name;
                                        perHome = (Convert.ToDouble(oddAway.odd) * 100) / (Convert.ToDouble(oddHome.odd) + Convert.ToDouble(oddDraw.odd) + Convert.ToDouble(oddAway.odd));
                                        perDraw = (Convert.ToDouble(oddDraw.odd) * 100) / (Convert.ToDouble(oddHome.odd) + Convert.ToDouble(oddDraw.odd) + Convert.ToDouble(oddAway.odd));
                                        perAway = (Convert.ToDouble(oddHome.odd) * 100) / (Convert.ToDouble(oddHome.odd) + Convert.ToDouble(oddDraw.odd) + Convert.ToDouble(oddAway.odd));
                                        break;
                                    }
                                }

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
                                    OddsBookmaker = OddsMaker,
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
                            LeagueFlag = ListLeagueFixOdds[0].LeagueFlag,
                            LeagueCountry = ListLeagueFixOdds[0].LeagueCountry,
                            LeagueOddsFixture = ListLeagueFixOdds.OrderBy(c => c.EventDate).ToList()
                        };
                        Ret_LeagureOddsFixture.Add(item2);
                    }
                }
            }
            return Ret_LeagureOddsFixture.OrderBy(c => c.LeagueCountry).ToList();
        }
        #endregion


        #region ================ Private Function ================
        private string GetFixtureByLeagueID(string leagueID, string date)
        {
            string URL = "https://api-football-v1.p.rapidapi.com/v2/fixtures/league/" + leagueID + "/" + date;
            return API.callAPIService(URL);
        }

        private string GetOddByLeagueID(string leagueID)
        {
            string URL = "https://api-football-v1.p.rapidapi.com/v2/odds/league/" + leagueID + "/label/1";
            return API.callAPIService(URL);
        }

        private string GetAllFixtureThisDay(string date)
        {
            string URL = "https://api-football-v1.p.rapidapi.com/v2/fixtures/date/" + date;
            return API.callAPIService(URL);
        }
        #endregion
    }
}
