using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace APITest.Models
{
    public class Leaderboard
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public string Name { get; set; }
        public string LeaderboardConfigurationType { get; set; }
    }

    public class LeaderboardEntry
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public string UserId { get; set; }
        public string LeaderboardName { get; set; }
        public int Score { get; set; }
    }

    public class PosistionedLeaderboardEntry
    {
        public string UserId { get; set; }
        public int Score { get; set; }
        public long Position { get; set; }
    }

    public class EntryData
    {
        public string UserId { get; set; }
        public int Score { get; set; }
    }

    public class CreateLeaderboardRequest
    {
        public string LeaderboardName { get; set; }
        public string LeaderboardConfigurationType { get; set; }
    }

    public class AddEntriesToLeaderboardRequest
    {
        public string LeaderboardName { get; set; }
        public List<EntryData> Entries { get; set; }
    }

    public class GetTopNLeaderboardEntriesRequest
    {
        public string LeaderboardName { get; set; }
        public int Amount { get; set; }
    }

    public class CreateLeaderboardResult
    {

    }

    public class AddEntriesToLeaderboardResult
    {

    }

    public class GetTopNLeaderboardEntriesResult
    {
        public List<PosistionedLeaderboardEntry> PosistionedLeaderboardEntries { get; set; }
    }

    public enum LeaderboardConfigurationType
    {
        Ascending,
        Descending
    }
}
