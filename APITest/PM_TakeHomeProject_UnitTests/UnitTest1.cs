using APITest.Controllers;
using APITest.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PM_TakeHomeProject_UnitTests
{
    [TestClass]
    public class UnitTest1
    {
        private DbContextOptions<LeaderboardContext> options = new DbContextOptionsBuilder<LeaderboardContext>().UseInMemoryDatabase(databaseName: "Leaderboards").Options;
        private LeaderboardContext context;

        private const string VALID_LEADERBOARD_NAME = "Leaderboard";
        private const string TOO_SHORT_LEADERBOARD_NAME = "";
        private string TOO_LONG_LEADERBOARD_NAME = new string('0', 65);
        private const string INVALID_LEADERBOARD_CONFIGURATION_TYPE = "Invalid";

        private const string TOPN_VALID_LEADERBOARD_NAME_ASCENDING = "TopNLeaderboardAscending";
        private const string TOPN_VALID_LEADERBOARD_NAME_DESCENDING = "TopNLeaderboardDescending";
        private const string TOPN_USER1 = "User1";
        private const string TOPN_USER2 = "User2";
        private const string TOPN_USER3 = "User3";

        [TestMethod]
        public async Task TEST1_IF_GetAllLeaderboards_WHEN_CreateLeaderboard_THEN_GetLeaderboard()
        {
            context = new LeaderboardContext(options);
            var controller = new LeaderboardsController(context);

            ActionResult<IEnumerable<Leaderboard>> result = await controller.GetAllLeaderboards();
            IEnumerable<Leaderboard> data = result.Value;
            
            Assert.AreEqual(0, data.Count());

            await controller.CreateLeaderboard(new CreateLeaderboardRequest { LeaderboardName = VALID_LEADERBOARD_NAME, LeaderboardConfigurationType = LeaderboardConfigurationType.Ascending.ToString() });

            result = await controller.GetAllLeaderboards();
            data = result.Value;

            Assert.AreEqual(1, data.Count());
        }

        [TestMethod]
        public async Task TEST2_IF_CreateLeaderboard_WHEN_InvalidName_THEN_BadRequest()
        {
            context = new LeaderboardContext(options);
            var controller = new LeaderboardsController(context);

            ActionResult result = await controller.CreateLeaderboard(new CreateLeaderboardRequest { LeaderboardName = TOO_SHORT_LEADERBOARD_NAME, LeaderboardConfigurationType = LeaderboardConfigurationType.Ascending.ToString() });

            Assert.IsInstanceOfType(result, typeof(BadRequestObjectResult));

            result = await controller.CreateLeaderboard(new CreateLeaderboardRequest { LeaderboardName = null, LeaderboardConfigurationType = LeaderboardConfigurationType.Ascending.ToString() });

            Assert.IsInstanceOfType(result, typeof(BadRequestObjectResult));

            result = await controller.CreateLeaderboard(new CreateLeaderboardRequest { LeaderboardName = TOO_LONG_LEADERBOARD_NAME, LeaderboardConfigurationType = LeaderboardConfigurationType.Ascending.ToString() });

            Assert.IsInstanceOfType(result, typeof(BadRequestObjectResult));
        }

        [TestMethod]
        public async Task TEST3_IF_CreateLeaderboard_WHEN_InvalidLeaderboardConfigurationType_THEN_BadRequest()
        {
            context = new LeaderboardContext(options);
            var controller = new LeaderboardsController(context);

            ActionResult result = await controller.CreateLeaderboard(new CreateLeaderboardRequest { LeaderboardName = VALID_LEADERBOARD_NAME, LeaderboardConfigurationType = INVALID_LEADERBOARD_CONFIGURATION_TYPE });

            Assert.IsInstanceOfType(result, typeof(BadRequestObjectResult));
        }

        [TestMethod]
        public async Task TEST4_IF_AddEntriesToLeaderboard_WHEN_ValidEntries_THEN_EntriesAdded()
        {
            context = new LeaderboardContext(options);
            var controller = new LeaderboardsController(context);

            AddEntriesToLeaderboardRequest addEntriesToLeaderboardRequest = new AddEntriesToLeaderboardRequest();
            addEntriesToLeaderboardRequest.LeaderboardName = VALID_LEADERBOARD_NAME;
            addEntriesToLeaderboardRequest.Entries = new List<EntryData>();
            addEntriesToLeaderboardRequest.Entries.Add(new EntryData { UserId = RandomString(8), Score = random.Next(0, 100) });
            addEntriesToLeaderboardRequest.Entries.Add(new EntryData { UserId = RandomString(8), Score = random.Next(0, 100) });

            await controller.AddEntriesToLeaderboard(addEntriesToLeaderboardRequest);
            
            Assert.AreEqual(2, context.LeaderboardEntries.Count());
        }

        [TestMethod]
        public async Task TEST5_IF_AddEntriesToLeaderboard_WHEN_InvalidLeaderboardName_THEN_BadRequest()
        {
            context = new LeaderboardContext(options);
            var controller = new LeaderboardsController(context);

            AddEntriesToLeaderboardRequest addEntriesToLeaderboardRequest = new AddEntriesToLeaderboardRequest();
            addEntriesToLeaderboardRequest.LeaderboardName = TOO_LONG_LEADERBOARD_NAME;
            addEntriesToLeaderboardRequest.Entries = new List<EntryData>();
            addEntriesToLeaderboardRequest.Entries.Add(new EntryData { UserId = RandomString(8), Score = random.Next(0, 100) });
            addEntriesToLeaderboardRequest.Entries.Add(new EntryData { UserId = RandomString(8), Score = random.Next(0, 100) });

            ActionResult result = await controller.AddEntriesToLeaderboard(addEntriesToLeaderboardRequest);

            Assert.IsInstanceOfType(result, typeof(BadRequestObjectResult));

            addEntriesToLeaderboardRequest.LeaderboardName = TOO_SHORT_LEADERBOARD_NAME;
            result = await controller.AddEntriesToLeaderboard(addEntriesToLeaderboardRequest);

            Assert.IsInstanceOfType(result, typeof(BadRequestObjectResult));

            addEntriesToLeaderboardRequest.LeaderboardName = null;
            result = await controller.AddEntriesToLeaderboard(addEntriesToLeaderboardRequest);

            Assert.IsInstanceOfType(result, typeof(BadRequestObjectResult));
        }

        [TestMethod]
        public async Task TEST6_IF_AddEntriesToLeaderboard_WHEN_InvalidEntries_THEN_BadRequest()
        {
            context = new LeaderboardContext(options);
            var controller = new LeaderboardsController(context);

            AddEntriesToLeaderboardRequest addEntriesToLeaderboardRequest = new AddEntriesToLeaderboardRequest();
            addEntriesToLeaderboardRequest.LeaderboardName = VALID_LEADERBOARD_NAME;
            addEntriesToLeaderboardRequest.Entries = new List<EntryData>();

            ActionResult result = await controller.AddEntriesToLeaderboard(addEntriesToLeaderboardRequest);

            Assert.IsInstanceOfType(result, typeof(BadRequestObjectResult));

            addEntriesToLeaderboardRequest.Entries = null;
            result = await controller.AddEntriesToLeaderboard(addEntriesToLeaderboardRequest);

            Assert.IsInstanceOfType(result, typeof(BadRequestObjectResult));
        }

        [TestMethod]
        public async Task TEST7_IF_GetTopNLeaderboardEntries_WHEN_AscendingLeaderboard_THEN_EntriesSortedLowestToHighest()
        {
            context = new LeaderboardContext(options);
            var controller = new LeaderboardsController(context);

            await controller.CreateLeaderboard(new CreateLeaderboardRequest { LeaderboardName = TOPN_VALID_LEADERBOARD_NAME_ASCENDING, LeaderboardConfigurationType = LeaderboardConfigurationType.Ascending.ToString() });

            AddEntriesToLeaderboardRequest addEntriesToLeaderboardRequest = new AddEntriesToLeaderboardRequest();
            addEntriesToLeaderboardRequest.LeaderboardName = TOPN_VALID_LEADERBOARD_NAME_ASCENDING;
            addEntriesToLeaderboardRequest.Entries = new List<EntryData>();
            addEntriesToLeaderboardRequest.Entries.Add(new EntryData { UserId = TOPN_USER1, Score = 10 });
            addEntriesToLeaderboardRequest.Entries.Add(new EntryData { UserId = TOPN_USER2, Score = 5 });
            addEntriesToLeaderboardRequest.Entries.Add(new EntryData { UserId = TOPN_USER3, Score = 15 });

            await controller.AddEntriesToLeaderboard(addEntriesToLeaderboardRequest);

            GetTopNLeaderboardEntriesRequest getTopNLeaderboardEntriesRequest = new GetTopNLeaderboardEntriesRequest();
            getTopNLeaderboardEntriesRequest.LeaderboardName = TOPN_VALID_LEADERBOARD_NAME_ASCENDING;
            getTopNLeaderboardEntriesRequest.Amount = 3;

            ActionResult result = await controller.GetTopNLeaderboardEntries(getTopNLeaderboardEntriesRequest);

            OkObjectResult okObjectResult = result as OkObjectResult;

            GetTopNLeaderboardEntriesResult topNResult = (GetTopNLeaderboardEntriesResult)okObjectResult.Value;
            Assert.AreEqual(3, topNResult.PosistionedLeaderboardEntries.Count());

            PosistionedLeaderboardEntry[] data = topNResult.PosistionedLeaderboardEntries.ToArray();

            Assert.AreEqual(0, data[0].Position);
            Assert.AreEqual(1, data[1].Position);
            Assert.AreEqual(2, data[2].Position);

            Assert.AreEqual(5, data[0].Score);
            Assert.AreEqual(10, data[1].Score);
            Assert.AreEqual(15, data[2].Score);

            Assert.AreEqual(TOPN_USER2, data[0].UserId);
            Assert.AreEqual(TOPN_USER1, data[1].UserId);
            Assert.AreEqual(TOPN_USER3, data[2].UserId);
        }

        [TestMethod]
        public async Task TEST8_IF_GetTopNLeaderboardEntries_WHEN_DescendingLeaderboard_THEN_EntriesSortedHighestToLowest()
        {
            context = new LeaderboardContext(options);
            var controller = new LeaderboardsController(context);

            await controller.CreateLeaderboard(new CreateLeaderboardRequest { LeaderboardName = TOPN_VALID_LEADERBOARD_NAME_DESCENDING, LeaderboardConfigurationType = LeaderboardConfigurationType.Descending.ToString() });

            AddEntriesToLeaderboardRequest addEntriesToLeaderboardRequest = new AddEntriesToLeaderboardRequest();
            addEntriesToLeaderboardRequest.LeaderboardName = TOPN_VALID_LEADERBOARD_NAME_DESCENDING;
            addEntriesToLeaderboardRequest.Entries = new List<EntryData>();
            addEntriesToLeaderboardRequest.Entries.Add(new EntryData { UserId = TOPN_USER1, Score = 10 });
            addEntriesToLeaderboardRequest.Entries.Add(new EntryData { UserId = TOPN_USER2, Score = 5 });
            addEntriesToLeaderboardRequest.Entries.Add(new EntryData { UserId = TOPN_USER3, Score = 15 });

            await controller.AddEntriesToLeaderboard(addEntriesToLeaderboardRequest);

            GetTopNLeaderboardEntriesRequest getTopNLeaderboardEntriesRequest = new GetTopNLeaderboardEntriesRequest();
            getTopNLeaderboardEntriesRequest.LeaderboardName = TOPN_VALID_LEADERBOARD_NAME_DESCENDING;
            getTopNLeaderboardEntriesRequest.Amount = 3;

            ActionResult result = await controller.GetTopNLeaderboardEntries(getTopNLeaderboardEntriesRequest);

            OkObjectResult okObjectResult = result as OkObjectResult;

            GetTopNLeaderboardEntriesResult topNResult = (GetTopNLeaderboardEntriesResult)okObjectResult.Value;
            Assert.AreEqual(3, topNResult.PosistionedLeaderboardEntries.Count());

            PosistionedLeaderboardEntry[] data = topNResult.PosistionedLeaderboardEntries.ToArray();

            Assert.AreEqual(0, data[0].Position);
            Assert.AreEqual(1, data[1].Position);
            Assert.AreEqual(2, data[2].Position);

            Assert.AreEqual(15, data[0].Score);
            Assert.AreEqual(10, data[1].Score);
            Assert.AreEqual(5, data[2].Score);

            Assert.AreEqual(TOPN_USER3, data[0].UserId);
            Assert.AreEqual(TOPN_USER1, data[1].UserId);
            Assert.AreEqual(TOPN_USER2, data[2].UserId);
        }

        private static Random random = new Random();
        public static string RandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }
    }
}
