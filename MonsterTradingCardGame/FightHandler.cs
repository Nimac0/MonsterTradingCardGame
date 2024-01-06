using Microsoft.VisualBasic;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonsterTradingCardGame
{
    //https://csharpindepth.com/articles/singleton#lock
    internal sealed class FightHandler
    {
        private static FightHandler instance = null;
        private static readonly object padlock = new object();
        private Dictionary<int, List<string>?> lobby = new Dictionary<int, List<string>?>();
        private static Mutex lobbyMutex = new Mutex();

        FightHandler()
        {
        }

        public static FightHandler Instance
        {
            get
            {
                lock (padlock)
                {
                    if (instance == null)
                    {
                        instance = new FightHandler();
                    }
                    return instance;
                }
            }
        }

        public string StartLobby(string authToken) //TODO create check if deck != null and make query to add deck to user
        {
            int? player1Id = null;
            int? player2Id = null;
            List<string>? battleLog;

            lock (lobbyMutex)
            {
                int? userId = SessionHandler.GetIdByUsername(SessionHandler.GetUsernameByToken(authToken));
                if (userId == null)
                {
                    return Response.CreateResponse("401", "Unauthorised", "", "application/json");
                }

                int? lookingForBattle = null;
                foreach (KeyValuePair<int, List<string>?> entry in this.lobby)
                {
                    if (entry.Value == null)
                    {
                        lookingForBattle = entry.Key;
                        this.lobby[entry.Key] = new List<string>();
                    }
                }

                if (lookingForBattle != null)
                {
                    Monitor.Exit(lobbyMutex);

                    player1Id = lookingForBattle;
                    player2Id = userId;

                    Fight fight = new Fight(CreateUserFromId(player1Id), CreateUserFromId(player2Id));

                    battleLog = fight.StartFight();

                    Monitor.Enter(lobbyMutex);
                    this.lobby[(int)player1Id] = battleLog;
                    Monitor.PulseAll(lobbyMutex);
                }
                else
                {
                    lobby.Add((int)userId, null);

                    while ((battleLog = this.lobby[(int)userId]) == null)
                    {
                        Console.WriteLine("waiting for other player...");
                        Monitor.Wait(lobbyMutex);
                    };

                    this.lobby.Remove((int)userId);
                }
            }

            return Response.CreateResponse("200", "OK", JsonConvert.SerializeObject(battleLog), "application/json");
        }

        public static User CreateUserFromId(int? userId)
        {
            DbQuery getUserData = new DbQuery(@"SELECT name, coins, elo, wins, losses FROM users WHERE id = @userid;");
            getUserData.AddParameterWithValue("userid", DbType.Int32, userId);

            User newUser = new User();
            using (IDataReader reader = getUserData.ExecuteReader())
            {
                if (reader.Read())
                {
                    newUser.Id = userId;
                    newUser.Name = reader.GetString(0);
                    newUser.Coins = reader.GetInt32(1);
                    newUser.EloValue = reader.GetInt32(2);
                    newUser.Wins = reader.GetInt32(3);
                    newUser.Losses = reader.GetInt32(4);
                    newUser.CardStack = new List<Card>();
                }
            }

            List<Card> cardDeck = new List<Card>();
            DbQuery getDeck = new DbQuery(@"SELECT cards.id,element,cardtype,damage,indeck,intrade,userid, cardname FROM cards WHERE userid = @userid AND indeck = TRUE;");
            getDeck.AddParameterWithValue("userid", DbType.Int32, userId);

            using (IDataReader reader = getDeck.ExecuteReader())
            {
                while (reader.Read())
                {
                    Card newCard = new Card(reader.GetString(0), (Element)reader.GetInt32(1), (CardType)reader.GetInt32(2), reader.GetFloat(3), reader.GetBoolean(4), reader.GetBoolean(5));

                    cardDeck.Add(newCard);
                }
                newUser.PlayingDeck = cardDeck;
                if(newUser.PlayingDeck.Count() == 4) return newUser;
            }
            return null;
        }
    }
}
