using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Diagnostics;

namespace main
{
    public static class Program
    {
        //https://web.archive.org/web/20190629194439/https://developer.riotgames.com/spectating-games.html
        static readonly Dictionary<string, string> SpectatorDomains = new Dictionary<string, string>
        {
            {"NA1", "spectator.na.lol.riotgames.com:80" },
            {"EUW1", "spectator.euw1.lol.riotgames.com:80" },
            {"EUN1", "spectator.eu.lol.riotgames.com:8088" },
            {"JP1", "spectator.jp1.lol.riotgames.com:80" },
            {"KR", "spectator.kr.lol.riotgames.com:80" },
            {"OC1", "spectator.oc1.lol.riotgames.com:80" },
            {"BR1", "spectator.br.lol.riotgames.com:80" },
            {"LA1", "spectator.la1.lol.riotgames.com:80" },
            {"LA2", "spectator.la2.lol.riotgames.com:80" },
            {"RU", "spectator.ru.lol.riotgames.com:80" },
            {"TR1", "spectator.tr.lol.riotgames.com:80" },
            {"PBE1", "spectator.pbe1.lol.riotgames.com:80" }
        };

        static HttpClient Client = new HttpClient();
        static string APIKey;
        static string Region;
        static string summonerInfoLink => $"https://{Region}.api.riotgames.com/lol/summoner/v4/summoners/by-name/";
        static string spectatorInfoLink => $"https://{Region}.api.riotgames.com/lol/spectator/v4/active-games/by-summoner/";

        public static void Main()
        {
            Console.Write("API Key: ");
            APIKey = Console.ReadLine();

            do
            {
                Console.Write("Player Region: ");
                Region = Console.ReadLine().ToUpper();
            }
            while (!SpectatorDomains.ContainsKey(Region));

            Console.Write("Player Nick: ");
            string userId = GetSummonerId(Console.ReadLine());

            Console.WriteLine();

            Tuple<string, string> spectatorInfo = GetSpectatorInfo(userId);

            string path = @"C:\Riot Games\League of Legends\Game";
            while (!File.Exists(path))
            {
                Console.WriteLine("League of Legends executable folder path not found!\nPlease insert the path for your League executable manually!");
                string input = Console.ReadLine();
                if (input.EndsWith("League of Lengends.exe") && File.Exists(input))
                {
                    path = Path.GetFullPath(input);
                }
                else if (Directory.Exists(input) && Directory.GetFiles(input).Where(x => x.EndsWith("League of Lengends.exe")).Count() > 0)
                {
                    path = input;
                }
            }

            Console.WriteLine("Launching Spectator mode...");
            Process.Start("CMD.exe", $"/C cd /d {path}&start \"\" \"League of Legends.exe\" \"spectator {SpectatorDomains[Region]} {spectatorInfo.Item1} {spectatorInfo.Item2} {Region}\"");
        }

        static string GetSummonerId(string username)
        {
            string summonerInfo = Client.GetAsync($"{summonerInfoLink}{username}?api_key={APIKey}").Result.Content.ReadAsStringAsync().Result;
            JObject serializedSummoner = JsonConvert.DeserializeObject<JObject>(summonerInfo);

            if (serializedSummoner.TryGetValue("status", out JToken error))
            {
                LogError(error);
            }

            return serializedSummoner.Value<string>("id");
        }

        static Tuple<string, string> GetSpectatorInfo(string userId)
        {
            string spectatorInfo = Client.GetAsync($"{spectatorInfoLink}{userId}?api_key={APIKey}").Result.Content.ReadAsStringAsync().Result;
            JObject serializedSpectator = JsonConvert.DeserializeObject<JObject>(spectatorInfo);

            if (serializedSpectator.TryGetValue("status", out JToken error))
            {
                LogError(error);
            }

            string matchKey = serializedSpectator.Value<JObject>("observers").Value<string>("encryptionKey");
            string gameId = serializedSpectator.Value<string>("gameId");

            return new Tuple<string, string>(matchKey, gameId);
        }

        static void LogError(JToken error)
        {
            //Too lazy to write proper error messages
            Console.WriteLine("There was an Error retrieving info from the API!");
            Console.WriteLine($"Code: {error.Value<string>("status_code")}");
            Console.WriteLine($"Message: {error.Value<string>("message")}");
            throw new Exception();
        }
    }
}