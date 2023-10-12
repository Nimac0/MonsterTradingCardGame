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
        public List<Card> Carddeck { get; set; }

        public List<Card> buyCards()
        {
            List<Card> newCards;
            return newCards;
        }

        public void addNewCard(ref List<Card> playerdeck, List<Card> boughtCards)
        {
            int cardNumber = 0;

            foreach(Card card in boughtCards)
            {
                cardNumber++;
                Console.WriteLine(cardNumber + ". " + card.CardName);
            }

            Console.WriteLine("Input the number of the card you want to keep: ");
            int chosenCard = Convert.ToInt32(Console.ReadLine());

            playerdeck.Add(boughtCards.ElementAt(chosenCard));
        }

        public User(string userName, string password,int coins, int eloValue, List<Card> carddeck)
        {
            this.UserName = userName;
            this.Password = password;
            this.Coins = coins;
            this.EloValue = eloValue;
            this.Carddeck = carddeck;
        }

    }
}
