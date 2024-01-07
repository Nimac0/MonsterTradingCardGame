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

            string expectedReponse = Response.CreateResponse("400", "Bad Request", "", "application/json");
            MethodRouter methodRouter = new MethodRouter();

            string response = methodRouter.HandleMethod(mockmethod, wrongRoute, mockBody, mockAuthToken);

            Assert.That(response.Equals(expectedReponse));
        }

        [Test]
        public void TestWrongAmountOfCards()
        {//doesnt work bc db access needed
            string mockAuthToken = "test";
            string mockRequest = "[\r\n  \"3fa85f64-5717-4562-b3fc-2c963f66afa7\",\r\n  \"3fa85f64-5717-4562-b3fc-2c963f66afa8\",\r\n  \"3fa85f64-5717-4562-b3fc-2c963f66afa6\"\r\n]";
            
            CardHandler cardHandler = new CardHandler();

            var expectedResponse = Response.CreateResponse("400", "Bad Request", "", "application/json");
            
            var response = cardHandler.ChooseDeck(mockRequest, mockAuthToken);
            Console.WriteLine(response);

            Assert.That(response.Equals(expectedResponse));
        }

        [Test]
        public void TestUpdateUser()
        {
            var MockDbQuery = new Mock<DbQuery>();
            MockDbQuery.Setup(c => c.NewCommand(It.IsAny<string>()));

            UserHandler userHandler = new UserHandler();
            userHandler.dbQuery = MockDbQuery.Object;
        }

        [Test]
        public void TestGetUsernameByToken()
        {
            string mockAuthToken = "test";

            string expectedResponse = "testname";

            string response = SessionHandler.Instance.GetUsernameByToken(mockAuthToken);

            Assert.That(response.Equals(expectedResponse));
        }

        
    }
}
