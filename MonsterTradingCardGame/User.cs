﻿using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonsterTradingCardGame
{
    public class User
    {
        public int? Id { get; set; }
        public string? Username { get; set; }
        public string? Password { get; set; }
        public int? Coins { get; set; }
        public int? EloValue { get; set; }
        public List<Card>? PlayingDeck { get; set; }
        public List<Card>? CardStack { get; set; }


        public void BuyCards(ref List<Card> playerdeck)
        {
            Random rand = new Random((int)DateTime.UtcNow.Ticks);

            if(this.Coins <= 0)
            {
                Console.WriteLine("Nope");
            }

            this.Coins = this.Coins - 5;

            for(int i = 0; i < 5; i++)
            {
                switch (rand.Next(2))
                {
                    case 0:
                        this.CardStack.Add(new MonsterCard(
                            (Element) rand.Next(Enum.GetNames(typeof(Element)).Length),
                            (CardType) rand.Next(Enum.GetNames(typeof(CardType)).Length),
                            rand.Next(15) * 5));
                        break;
                    case 1:
                        this.CardStack.Add(new SpellCard(
                            (Element) rand.Next(Enum.GetNames(typeof(Element)).Length),
                            rand.Next(15) * 5));
                        break;
                    default: break;
                }
            }
        }

        public void choosePlayingCards()
        {

        }

        public User()
        {

        }

        public User(string userName, string password,int coins, int eloValue) //TODO add playing deck/stack to constructor
        {
            this.Username = userName;
            this.Password = password;
            this.Coins = coins;
            this.EloValue = eloValue;
            this.PlayingDeck = new List<Card>(4) { };
            this.CardStack = new List<Card> { };
        }

    }
}
