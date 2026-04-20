using System;
using System.Collections.Generic;

namespace ProjectYH_Server.Models
{
    public class PlayerIdentity
    {
        public string Guid { get; set; } = string.Empty;
        public string Nickname { get; set; } = string.Empty;
        public string ConnectionId { get; set; } = string.Empty;
        public bool IsReady { get; set; }
        public bool IsHost { get; set; }
        public bool HasFinished { get; set; }
    }

    public class GameProgressData
    {
        public string Guid { get; set; } = string.Empty;
        public int Combo { get; set; }
        public int Score { get; set; }
        public float Rate { get; set; }
        public string LastJudge { get; set; } = string.Empty; 
    }

    public class GameResultData
    {
        public string Guid { get; set; } = string.Empty;
        public string Nickname { get; set; } = string.Empty;
        public int PerfectCount { get; set; }
        public int GreatCount { get; set; }
        public int GoodCount { get; set; }
        public int BadCount { get; set; }
        public int MissCount { get; set; }
        public int MaxCombo { get; set; }
        public float FinalRate { get; set; }
        public int FinalScore { get; set; }
        public string Grade { get; set; } = string.Empty;
    }

    public enum RoomState
    {
        Waiting,
        Playing,
        Finished
    }

    public class RoomData
    {
        public string RoomId { get; set; } = string.Empty;
        public string RoomName { get; set; } = string.Empty;
        public bool IsPrivate { get; set; }
        public int MaxPlayers { get; set; } = 8;
        public RoomState State { get; set; } = RoomState.Waiting;
        public List<PlayerIdentity> Players { get; set; } = new List<PlayerIdentity>();
        public List<GameResultData> Results { get; set; } = new List<GameResultData>();
    }
}
