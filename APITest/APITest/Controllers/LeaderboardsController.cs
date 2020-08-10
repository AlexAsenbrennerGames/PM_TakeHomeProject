using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using APITest.Models;
using System.Collections;

namespace APITest.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LeaderboardsController : ControllerBase
    {
        private readonly LeaderboardContext _context;

        public LeaderboardsController(LeaderboardContext context)
        {
            _context = context;
        }

        // GET: api/Leaderboards
        [HttpGet("GetAllLeaderboards")]
        public async Task<ActionResult<IEnumerable<Leaderboard>>> GetAllLeaderboards()
        {
            return await _context.Leaderboards.ToListAsync();
        }

        // POST: api/Leaderboards/CreateLeaderboard
        [HttpPost("CreateLeaderboard")]
        public async Task<ActionResult> CreateLeaderboard(CreateLeaderboardRequest request)
        {
            if (request.LeaderboardName == null || request.LeaderboardName.Length <= 0 || request.LeaderboardName.Length > 64)
            {
                return BadRequest("Invalid LeaderboardName");
            }

            if (!Enum.IsDefined(typeof(LeaderboardConfigurationType), request.LeaderboardConfigurationType))
            {
                return BadRequest("Invalid LeaderboardConfigurationType: Needs to be Ascending or Descending");
            }

            Leaderboard leaderboard = new Leaderboard();
            leaderboard.Name = request.LeaderboardName;
            leaderboard.LeaderboardConfigurationType = request.LeaderboardConfigurationType;

            _context.Leaderboards.Add(leaderboard);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                if (LeaderboardExists(leaderboard.Name))
                {
                    return Conflict();
                }
                else
                {
                    throw;
                }
            }

            return Ok(new CreateLeaderboardResult());
        }

        // POST: api/Leaderboards/AddEntriesToLeaderboard
        [HttpPost("AddEntriesToLeaderboard")]
        public async Task<ActionResult> AddEntriesToLeaderboard(AddEntriesToLeaderboardRequest request)
        {
            if (request.LeaderboardName == null || request.LeaderboardName.Length <= 0 || request.LeaderboardName.Length > 64)
            {
                return BadRequest("Invalid LeaderboardName");
            }

            if (request.Entries == null || !(request.Entries is IList) || request.Entries.Count <= 0)
            {
                return BadRequest("Invalid Entries");
            }

            if (!LeaderboardExists(request.LeaderboardName))
            {
                return BadRequest("Given Leaderboard Name does not exist");
            }

            foreach (EntryData entryData in request.Entries)
            {
                if (entryData.UserId != null && entryData.UserId.Length > 0)
                {
                    LeaderboardEntry leaderboardEntry = new LeaderboardEntry();
                    leaderboardEntry.UserId = entryData.UserId;
                    leaderboardEntry.Score = entryData.Score;
                    leaderboardEntry.LeaderboardName = request.LeaderboardName;

                    if (LeaderboardEntryExists(leaderboardEntry.UserId, leaderboardEntry.LeaderboardName))
                    {
                        _context.LeaderboardEntries.Update(leaderboardEntry);
                    }
                    else
                    {
                        _context.LeaderboardEntries.Add(leaderboardEntry);
                    }
                }
            }
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                throw;
            }

            return Ok(new AddEntriesToLeaderboardResult());
        }

        // POST: api/Leaderboards/GetTopNLeaderboardEntries
        [HttpPost("GetTopNLeaderboardEntries")]
        public async Task<ActionResult> GetTopNLeaderboardEntries([FromBody] GetTopNLeaderboardEntriesRequest request)
        {
            if (request.LeaderboardName == null || request.LeaderboardName.Length <= 0 || request.LeaderboardName.Length > 64)
            {
                return BadRequest("Invalid LeaderboardName");
            }

            if (request.Amount <= 0)
            {
                return BadRequest("Invalid Amount: Amount must be greater than 0");
            }

            Leaderboard leaderboard = _context.Leaderboards.Find(request.LeaderboardName);
            LeaderboardEntry[] entries = new LeaderboardEntry[0];
            if (leaderboard.LeaderboardConfigurationType.Equals(LeaderboardConfigurationType.Ascending.ToString()))
            {
                entries = await _context.LeaderboardEntries.Where(entry => entry.LeaderboardName == request.LeaderboardName).OrderBy(entry => entry.Score).Take(request.Amount).ToArrayAsync();
            } 
            if (leaderboard.LeaderboardConfigurationType.Equals(LeaderboardConfigurationType.Descending.ToString()))
            {
                entries = await _context.LeaderboardEntries.Where(entry => entry.LeaderboardName == request.LeaderboardName).OrderByDescending(entry => entry.Score).Take(request.Amount).ToArrayAsync();
            }

            List<PosistionedLeaderboardEntry> posistionedLeaderboardEntries = new List<PosistionedLeaderboardEntry>();
            for (int i = 0; i < entries.Length; i++)
            {
                LeaderboardEntry entry = entries[i];

                PosistionedLeaderboardEntry posistionedLeaderboardEntry = new PosistionedLeaderboardEntry();
                posistionedLeaderboardEntry.Position = i;
                posistionedLeaderboardEntry.UserId = entry.UserId;
                posistionedLeaderboardEntry.Score = entry.Score;

                posistionedLeaderboardEntries.Add(posistionedLeaderboardEntry);
            }

            GetTopNLeaderboardEntriesResult getTopNLeaderboardEntriesResult = new GetTopNLeaderboardEntriesResult();
            getTopNLeaderboardEntriesResult.PosistionedLeaderboardEntries = posistionedLeaderboardEntries;

            return Ok(getTopNLeaderboardEntriesResult);
        }

        private bool LeaderboardExists(string name)
        {
            return _context.Leaderboards.Any(e => e.Name == name);
        }

        private bool LeaderboardEntryExists(string userId, string leaderboardName)
        {
            return _context.LeaderboardEntries.Any(e => e.UserId == userId && e.LeaderboardName == leaderboardName);
        }
    }
}
