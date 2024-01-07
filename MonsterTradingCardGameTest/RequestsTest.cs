using MonsterTradingCardGame;
using MonsterTradingCardGame.Db;
using MonsterTradingCardGame.Schemas;
using MonsterTradingCardGame.RequestHandler;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MonsterTradingCardGame.http;
using Newtonsoft.Json.Linq;
using System.Diagnostics.CodeAnalysis;
using System.Data;
using System.Text.RegularExpressions;
using static System.Net.Mime.MediaTypeNames;

namespace MonsterTradingCardGameTest
{
    public class RequestsTest
    {
        [SetUp]
        public void Setup()
        {
            string mockAuthToken = "test";
            int mockUserId = 1;
            User mockUser = new User();
            mockUser.Username = "testname";

            SessionHandler.tokenMap.Clear();
            SessionHandler.userMap.Clear();

            SessionHandler.tokenMap.Add(mockAuthToken, mockUserId);
            SessionHandler.userMap.Add(mockUserId, mockUser);
        }

        [Test]
        public void TestInvalidRequestResponse()
        {
            string mockmethod = "POST";
            string wrongRoute = "/user";
            string mockBody = "";
            string mockAuthToken = "";

            string expectedResponse = Response.CreateResponse("400", "Bad Request", "", "application/json");
            MethodRouter methodRouter = new MethodRouter();

            string response = methodRouter.HandleMethod(mockmethod, wrongRoute, mockBody, mockAuthToken);

            Assert.That(response.Substring(0, response.IndexOf(Environment.NewLine)).Equals(expectedResponse.Substring(0, expectedResponse.IndexOf(Environment.NewLine))));
        }

        [Test]
        public void TestUpdateUser()
        {
            string mockUsername = "testname";
            string mockAuthToken = "test";
            string mockRequestBody = "{\r\n  \"Name\": \"Hoax\",\r\n  \"Bio\": \"me playin...\",\r\n  \"Image\": \":-)\"\r\n}";

            var MockDbQuery = new Mock<IDatabase>();
            MockDbQuery.Setup(c => c.NewCommand(It.IsAny<string>())).Returns(MockDbQuery.Object);
            MockDbQuery.Setup(c => c.ExecuteNonQuery()).Returns(1);

            var MockUserHandler = new Mock<UserHandler>();
            MockUserHandler.Setup(c => c.GetUserData(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>())).Returns(Response.CreateResponse("200", "OK", "", "application/json"));

            MockUserHandler.Object.dbQuery = MockDbQuery.Object;

            var expectedResponse = Response.CreateResponse("200", "OK", "", "application/json");

            var response = MockUserHandler.Object.UpdateUser(mockUsername, mockRequestBody, mockAuthToken);

            Assert.That(response.Substring(0, response.IndexOf(Environment.NewLine)).Equals(expectedResponse.Substring(0, expectedResponse.IndexOf(Environment.NewLine))));
        }

        [Test]
        public void TestGetUsernameByToken()
        {
            string mockAuthToken = "test";

            string expectedResponse = "testname";

            string response = SessionHandler.Instance.GetUsernameByToken(mockAuthToken);

            Assert.That(response.Equals(expectedResponse));
        }

        [Test]
        public void TestRegisterUser()
        {
            string mockUsername = "testname";
            string mockAuthToken = "test";
            string mockRequestBody = "{\r\n  \"Username\": \"test2\",\r\n  \"Password\": \"test2\"\r\n}";

            var MockDbQuery = new Mock<IDatabase>();
            MockDbQuery.Setup(c => c.NewCommand(It.IsAny<string>())).Returns(MockDbQuery.Object);
            MockDbQuery.Setup(c => c.ExecuteScalar()).Returns(1);

            var MockUserHandler = new Mock<UserHandler>();
            MockUserHandler.Setup(c => c.GetUserData(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>())).Returns((string) null);

            MockUserHandler.Object.dbQuery = MockDbQuery.Object;

            var expectedResponse = Response.CreateResponse("201", "Created", "", "application/json");

            var response = MockUserHandler.Object.CreateUser(mockRequestBody, mockAuthToken);

            Assert.That(response.Substring(0, response.IndexOf(Environment.NewLine)).Equals(expectedResponse.Substring(0, expectedResponse.IndexOf(Environment.NewLine))));
        }

        [Test]
        public void TestUnauthorizedRequestToGetTrades()
        {
            string mockAuthToken = "wrongToken";
            TradeHandler tradeHandler = new TradeHandler();
            var response = tradeHandler.GetTrades(mockAuthToken);

            var expectedResponse = Response.CreateResponse("401", "Unauthorised", "", "application/json");

            Assert.That(response.Substring(0, response.IndexOf(Environment.NewLine)).Equals(expectedResponse.Substring(0, expectedResponse.IndexOf(Environment.NewLine))));
        }

        [Test]
        public void TestIsNotAdmin()
        {
            PackageHandler packageHandler = new PackageHandler();
            var response = packageHandler.CreateCardPackage("", "test");

            var expectedResponse = Response.CreateResponse("403", "Forbidden", "", "application/json");

            Assert.That(response.Substring(0, response.IndexOf(Environment.NewLine)).Equals(expectedResponse.Substring(0, expectedResponse.IndexOf(Environment.NewLine))));
        }

        [Test]
        public void TestRequestUserData()
        {
            var MockDbQuery = new Mock<IDatabase>();
            MockDbQuery.Setup(c => c.NewCommand(It.IsAny<string>())).Returns(MockDbQuery.Object);
            MockDbQuery.Setup(c => c.AddParameterWithValue(It.IsAny<string>(), It.IsAny<DbType>(), It.IsAny<string>()));

            var MockDataReader = new Mock<IDataReader>();
            MockDataReader.Setup(c => c.Read()).Returns(true); // will not work for loops
            MockDataReader.Setup(c => c.GetString(7)).Returns("test");
            MockDataReader.Setup(c => c.GetString(8)).Returns("biotest");
            MockDataReader.Setup(c => c.GetString(9)).Returns("imagetest");

            MockDbQuery.Setup(c => c.ExecuteReader()).Returns(MockDataReader.Object);

            UserHandler userHandler = new UserHandler();
            userHandler.dbQuery = MockDbQuery.Object;

            string response = userHandler.GetUserData("Bond", "", false);

            StringAssert.StartsWith("HTTP/1.1 200 OK", response);
            StringAssert.EndsWith(
                "{\"Name\":\"test\",\"Bio\":\"biotest\",\"Image\":\"imagetest\"}",
                Regex.Replace(response, @"\s", ""));
        }

        [Test]
        public void TestMeetsTradeRequirements()
        {
            var MockDbQuery = new Mock<IDatabase>();
            MockDbQuery.Setup(c => c.NewCommand(It.IsAny<string>())).Returns(MockDbQuery.Object);
            MockDbQuery.Setup(c => c.AddParameterWithValue(It.IsAny<string>(), It.IsAny<DbType>(), It.IsAny<string>()));

            var MockDataReader = new Mock<IDataReader>();
            MockDataReader.Setup(c => c.Read()).Returns(true); // will not work for loops
            MockDataReader.Setup(c => c.GetInt32(0)).Returns(3);
            MockDataReader.Setup(c => c.GetFloat(1)).Returns(5);
            MockDataReader.Setup(c => c.GetBoolean(2)).Returns(false);

            MockDbQuery.Setup(c => c.ExecuteReader()).Returns(MockDataReader.Object);

            TradeHandler tradeHandler = new TradeHandler();
            tradeHandler.dbQuery = MockDbQuery.Object;

            Trade trade1 = new Trade();
            trade1.Id = "tradeId";
            trade1.Type = "monster";
            trade1.MinimumDamage = 3;
            trade1.CardToTrade = "cardId";

            Assert.True(tradeHandler.CheckRequirements(trade1, "3.1415926"));

            Trade trade2 = new Trade();
            trade2.Id = "tradeId";
            trade2.Type = "spell";
            trade2.MinimumDamage = 3;
            trade2.CardToTrade = "cardId";

            Assert.False(tradeHandler.CheckRequirements(trade2, "3.1415926"));

            MockDataReader.Setup(c => c.GetBoolean(2)).Returns(true);

            Trade trade3 = new Trade();
            trade3.Id = "tradeId";
            trade3.Type = "monster";
            trade3.MinimumDamage = 3;
            trade3.CardToTrade = "cardId";

            Assert.False(tradeHandler.CheckRequirements(trade3, "3.1415926"));
        }
    }
}
