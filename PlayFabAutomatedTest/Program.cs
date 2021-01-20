using Newtonsoft.Json;
using PlayFab;
using PlayFab.ClientModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PlayFabAutomatedTest
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("Please type help to get instruction");
                //Arg sample 1: -titleId AB20
                //Default is AB20 title, set other title if you want
                //Arg sample 2: -opt cmu -mun 20
                //Create 20 mock users
                //Arg sample 3: -opt cmr
                //Load all mock users and create mock requests
                //Arg sample 4: -opt cmr -limit 20
                //Create mock requests and up to 20 times
                //Arg sample 5: -opt dkmgr -limit 8
                //Create request to Docker Mgr API
                return;
            }

            CmdArguments cmdarg = CmdArguments.From(args);

            var titleId = cmdarg.GetStringIfExists("titleId");
            titleId = string.IsNullOrEmpty(titleId) ? "AB20" : titleId;
            PlayFabSettings.staticSettings.TitleId = titleId;

            var opt = cmdarg.GetStringIfExists("opt");
            if (!string.IsNullOrEmpty(opt))
            {
                switch (opt)
                {
                    case "cmu":
                        var _mun = cmdarg.GetStringIfExists("mun");
                        int mun;
                        if (!int.TryParse(_mun, out mun))
                        {
                            Console.WriteLine("Please type a valid number for mun arg");
                            return;
                        }
                        CreateMockUsers(titleId, mun);

                        break;
                    case "cmr":
                        var _limit = cmdarg.GetStringIfExists("limit");
                        int limit = 0;

                        if (!string.IsNullOrEmpty(_limit) && !int.TryParse(_limit, out limit))
                        {
                            Console.WriteLine("Please type a valid number for limit arg");
                            return;
                        }
                        var playerInfos = LoadPlayers(titleId, limit);
                        CreateMockRequest(playerInfos);
                        break;

                    case "dkmgr":
                        var _num = cmdarg.GetStringIfExists("limit");
                        int num = 0;

                        if (!string.IsNullOrEmpty(_num) && !int.TryParse(_num, out num))
                        {
                            Console.WriteLine("Please type a valid number for limit arg");
                            return;
                        }

                        if (num > 0)
                            SendDockerMgrHttpRequest(num);
                        else
                            SendDockerMgrHttpRequest();
                        break;
                }
            }

            Console.WriteLine("---------------End----------------");
            Console.ReadKey();
        }

        static List<PPlayerInfo> LoadPlayers(string titleId, int limit = 0)
        {
            var playerInfos = new List<PPlayerInfo>();
            if (File.Exists($"players_{titleId}.txt"))
            {
                var lines = File.ReadLines($"players_{titleId}.txt");

                if (limit > 0)
                    lines = lines.Take(limit);

                foreach (var line in lines)
                {
                    var parts = line.Split('\t');
                    playerInfos.Add(new PPlayerInfo() { CustomID = parts[0], EntityId = parts[2] });
                }
            }

            return playerInfos;
        }

        static async void CreateMockRequest(List<PPlayerInfo> players)
        {
            var tasks = new List<Task>();
            players.ForEach(p =>
                tasks.Add(Task.Run(() =>
                {
                    SendHttpRequest(p.EntityId, p.CustomID);
                }))
            );

            await Task.WhenAll(tasks);
        }

        static async void SendHttpRequest(string eId, string customId)
        {
            var req = new CreateMatchTKRequest()
            {
                DataObject = new DO()
                {
                    Latencies = new List<Latency>() {
                        new Latency() { region = "ChinaEast2", latency = 60 }
                        , new Latency() { region = "ChinaNorth2", latency = 70 }
                    },
                    SelectedGameModes = new string[] { "TDM" },
                    PlayerUniqueId = customId,
                    MatchIP = "139.217.102.203:9000",
                    SelectedMap = ""
                },
                TitleAccountId = eId,
                QueueName = "SimpleQueue", //"QueueForFranklinTHTest2", // Update the queue name here
                GiveUpAfterSeconds = 300
            };
            var json = JsonConvert.SerializeObject(req);
            var data = new StringContent(json, Encoding.UTF8, "application/json");

            var url = "http://139.217.102.203:9000/CreateSinglePlayerTicket"; // Update the url here

            using (var client = new HttpClient())
            {
                var response = await client.PostAsync(url, data);

                string result = response.Content.ReadAsStringAsync().Result;
                Console.WriteLine(result);
            }

        }

        static void CreateMockUsers(string titleId, int number)
        {
            var players = new List<PPlayerInfo>();

            for (int i = 0; i < number; i++)
            {
                var customId = "bot-" + Guid.NewGuid();
                var p = LoginPlayer(customId);
                if (p != null)
                {
                    players.Add(p);
                }
                else
                {
                    Console.WriteLine("Skip user for custom id " + customId);
                }

                Thread.Sleep(3000); // 5 seconds
            }

            foreach (var player in players)
                File.AppendAllText($"players_{titleId}.txt", player.ToString() + Environment.NewLine);
        }

        static PPlayerInfo LoginPlayer(string id)
        {
            PPlayerInfo player = new PPlayerInfo();
            LoginWithCustomIDRequest req = new LoginWithCustomIDRequest();
            req.CreateAccount = true;
            req.CustomId = id;
            var loginTask = PlayFabClientAPI.LoginWithCustomIDAsync(req);
            loginTask.Wait();
            if (loginTask.Result.Error != null)
            {
                Console.WriteLine("Error while logging in with {0} : {1}", id, loginTask.Result.Error.ErrorMessage);
                return null;
            }
            else
            {
                var result = loginTask.Result.Result;
                player.PFID = result.PlayFabId;
                player.SessionTicket = result.SessionTicket;
                player.EntityToken = result.EntityToken.EntityToken;
                player.EntityId = result.EntityToken.Entity.Id;
                player.EntityType = result.EntityToken.Entity.Type;
                player.CustomID = id;
            }
            return player;
        }

        static async void SendDockerMgrHttpRequest()
        {
            var tasks = new List<Task>();
            var ids = new List<string>() {
                "d27b8366-e410-4ff5-a020-e760640a9aa3",
"f6fe8abc-0c9e-4750-b78b-efc7ed553556",
"b43bf3f1-8c5c-40dd-b896-c413d8c59b01",
"4e4a4d36-238f-46c2-81b9-1059e5904d62",
"4c460f8f-09f1-44eb-b030-bbe8fa4a7e97",
"d864ecbb-8f20-4f0f-8e19-2e5b9b3aa13e",
"aa5c47fa-03c9-48c3-ac92-4dc17342495f",
"87ee7635-f567-4a3b-ae89-80d5f0ffab91",
"723af8e8-c3e3-49bc-8fd7-41f1ed00d71d",
"20ae5534-ca0c-46e8-8ef0-ffd2118ed38d"
            };
            ids.ForEach(p =>
                tasks.Add(Task.Run(() =>
                {
                    SendDockerMgrHttpRequest(p);
                }))
            );
            await Task.WhenAll(tasks);
        }

        static async void SendDockerMgrHttpRequest(int num, string matchId = null)
        {
            var tasks = new List<Task>();
            for (int i = 0; i < num; i++)
            {
                tasks.Add(Task.Run(() =>
                {
                    SendDockerMgrHttpRequest(matchId);
                }));
            }
            await Task.WhenAll(tasks);
        }

        static async void SendDockerMgrHttpRequest(string matchId = null)
        {
            var req = new CreateDockerMgrMatchTKRequest()
            {
                Gamekey = "boundry",
                QueueName = "SimpleQueue",
                MatchId = matchId != null ? matchId : Guid.NewGuid().ToString()
            };
            var json = JsonConvert.SerializeObject(req);
            var data = new StringContent(json, Encoding.UTF8, "application/json");

            //China North 2 - 139.217.111.218:9666
            //China East 2 - 139.217.230.102:9666
            var url = "http://139.217.111.218:9666/command/start-match"; // Update the url here

            using (var client = new HttpClient())
            {
                var response = await client.PostAsync(url, data);

                string result = response.Content.ReadAsStringAsync().Result;
                Console.WriteLine(result);
            }

        }
    }
}
