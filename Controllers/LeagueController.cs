using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;
using Football_API.Models;
using Newtonsoft.Json;
using RestSharp;

namespace FootballAPI.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]

    public class LeagueController : ControllerBase
    {
        API API = new API(); 

        #region ================ Public Function ================
        [HttpGet]
        public ActionResult<IEnumerable<List_League>> GetLeagueAll_Avilable()
        {
            string JsonStr_League = GetAllLeagueCurrent();
            List<RootObjectLeague> APIJsonObjectLeague = JsonConvert.DeserializeObject<List<RootObjectLeague>>(JsonStr_League);
            List<Football_API.Models.League> AllLeague = new List<Football_API.Models.League>();
            List<List_League> ListLeague = new List<List_League>();

            if (APIJsonObjectLeague != null)
            {
                AllLeague = APIJsonObjectLeague[0].api.leagues; //Api มีก้อนเดียวเสมอ
            }

            foreach (var League in AllLeague)
            {
                var ThisYear = Int32.Parse(DateTime.Now.ToString("yyyy", new CultureInfo("en-US")));
                if (Int32.Parse(League.season) == ThisYear || Int32.Parse(League.season) == ThisYear - 1)
                {
                    var item = new List_League
                    {
                        ID = League.league_id.ToString(),
                        Name = League.name,
                        Logo = League.logo,
                        Type = League.type,
                        Country = League.country,
                        Season = League.season,
                        Season_Start = League.season_start,
                        Season_End = League.season_end
                    };
                    ListLeague.Add(item);
                }

            }
            return ListLeague.OrderBy(c => c.Season).ThenBy(c => c.Name).ToList();
        }

        #endregion

        #region ================ Private Function ================
        private string GetAllLeagueCurrent()
        {
            string URL = "https://api-football-v1.p.rapidapi.com/v2/leagues/current/";
            return API.callAPIService(URL);
        }
        private string GetAllLeague()
        {

            string URL = "https://api-football-v1.p.rapidapi.com/v2/leagues";
            return API.callAPIService(URL);
        }
        #endregion
    }
}
