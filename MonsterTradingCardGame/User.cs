using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonsterTradingCardGame
{
    public class User
    {
        public string UserName { get; set; }
        public string Password { get; set; }
        public int Coins { get; set; }
        public int EloValue { get; set; }
        public List<Card> PlayingDeck { get; set; }
        public List<Card> CardStack { get; set; }


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
                            (MonsterType) rand.Next(Enum.GetNames(typeof(MonsterType)).Length)));
                        break;
                    case 1:
                        this.CardStack.Add(new SpellCard(
                            (Element) rand.Next(Enum.GetNames(typeof(Element)).Length)));
                        break;
                    default: break;
                }
            }
        }

        public void choosePlayingCards()
        {

        }

        public User(string userName, string password,int coins, int eloValue)
        {
            this.UserName = userName;
            this.Password = password;
            this.Coins = coins;
            this.EloValue = eloValue;
            this.PlayingDeck = new List<Card>(4) { };
            this.CardStack = new List<Card> { };
        }

    }
}
