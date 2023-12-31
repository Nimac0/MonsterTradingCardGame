﻿using Microsoft.VisualBasic;
using MonsterTradingCardGame.Db;
using MonsterTradingCardGame.http;
using MonsterTradingCardGame.Schemas;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonsterTradingCardGame.RequestHandler
{
    //https://csharpindepth.com/articles/singleton#lock
    public sealed class FightHandler
    {
        public IDatabase dbQuery = new DbQuery();
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

        public string StartLobby(string authToken)
        {
            int? player1Id = null;
            int? player2Id = null;
            List<string>? battleLog;

            lock (lobbyMutex)
            {
                int? userId = SessionHandler.Instance.GetIdByUsername(SessionHandler.Instance.GetUsernameByToken(authToken));
                if (userId == null)
                {
                    return Response.CreateResponse("401", "Unauthorised", "", "application/json");
                }
                CardHandler cardHandler = new CardHandler();
                if(cardHandler.GetCardsOrDeck(authToken, true).Count() != 4) return Response.CreateResponse("403", "Forbidden", "", "application/json");

                int? lookingForBattle = null;
                foreach (KeyValuePair<int, List<string>?> entry in lobby)
                {
                    if (entry.Value == null)
                    {
                        lookingForBattle = entry.Key;
                        lobby[entry.Key] = new List<string>();
                    }
                }

                if (lookingForBattle != null)
                {
                    Monitor.Exit(lobbyMutex);

                    player1Id = lookingForBattle;
                    player2Id = userId;

                    Fight fight = new Fight(CreateUserFromId(player1Id), CreateUserFromId(player2Id));

                    battleLog = fight.StartFight();
                
                    if (battleLog == null) return Response.CreateResponse("500", "Internal Server Error", JsonConvert.SerializeObject(battleLog), "application/json");
                    Monitor.Enter(lobbyMutex);

                    lobby[(int)player1Id] = battleLog;
                    Monitor.PulseAll(lobbyMutex);
                }
                else
                {
                    lobby.Add((int)userId, null);

                    while ((battleLog = lobby[(int)userId]) == null)
                    {
                        Console.WriteLine("waiting for other player...");
                        Monitor.Wait(lobbyMutex);
                    };

                    lobby.Remove((int)userId);
                }
            }

            return Response.CreateResponse("200", "OK", JsonConvert.SerializeObject(battleLog), "application/json");
        }

        public User CreateUserFromId(int? userId)
        {
            IDatabase getUserData = this.dbQuery.NewCommand(@"SELECT name, coins, elo, wins, losses FROM users WHERE id = @userid;");
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
            IDatabase getDeck = this.dbQuery.NewCommand(@"SELECT cards.id,element,cardtype,damage,indeck,intrade,userid, cardname FROM cards WHERE userid = @userid AND indeck = TRUE;");
            getDeck.AddParameterWithValue("userid", DbType.Int32, userId);

            using (IDataReader reader = getDeck.ExecuteReader())
            {
                while (reader.Read())
                {
                    Card newCard = new Card(reader.GetString(0), (Element)reader.GetInt32(1), (CardType)reader.GetInt32(2), reader.GetFloat(3), reader.GetBoolean(4), reader.GetBoolean(5), reader.GetString(7));

                    cardDeck.Add(newCard);
                }
                newUser.PlayingDeck = cardDeck;
                if (newUser.PlayingDeck.Count() == 4) return newUser;
            }
            return null;
        }
    }
}
