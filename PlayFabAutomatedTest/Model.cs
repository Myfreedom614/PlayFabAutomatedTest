using System.Collections.Generic;

namespace PlayFabAutomatedTest
{
    public class PPlayerInfo
    {
        public string CustomID;
        public string PFID;
        public string SessionTicket;
        public string EntityToken;
        public string EntityId;
        public string EntityType;

        public PPlayerInfo()
        {

        }

        public PPlayerInfo(string customId, string pfId, string st, string et, string eId, string eType)
        {
            this.CustomID = customId;
            this.PFID = pfId;
            this.SessionTicket = st;
            this.EntityToken = et;
            this.EntityId = eId;
            this.EntityType = eType;
        }
        public override string ToString()
        {
            return $"{CustomID}\t{PFID}\t{EntityId}\t{SessionTicket}\t{EntityToken}";
        }
    }

    public class Latency
    {
        public string region { get; set; }
        public int latency { get; set; }
    }

    public class DO
    {
        public List<Latency> Latencies { get; set; }
        public string[] SelectedGameModes { get; set; }
        public string PlayerUniqueId { get; set; }
        public string MatchIP { get; set; }
        public string SelectedMap { get; set; }
    }

    public class CreateMatchTKRequest
    {
        public DO DataObject { get; set; }
        public string TitleAccountId { get; set; }
        public string QueueName { get; set; }
        public int GiveUpAfterSeconds { get; set; }
    }

    public class CreateDockerMgrMatchTKRequest
    {
        public string Gamekey { get; set; }
        public string QueueName { get; set; }
        public string MatchId { get; set; }
    }
}
