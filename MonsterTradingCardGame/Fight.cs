using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace MonsterTradingCardGame
{
    internal class Fight
    {
        public enum fightResult
        {
            WIN = 0,
            LOSE,
            DRAW
        }

        private User _player;
        private User _enemy;
        private int _round_counter = 0;
        private static Random rng = new Random();

        public Fight(User player, User enemy)
        {
            this._player = player;
            this._enemy = enemy;
        }

        public fightResult StartFight()
        {

            do
            {
                if (!this._enemy.PlayingDeck.Any()) return fightResult.WIN;
                if (!this._player.PlayingDeck.Any()) return fightResult.LOSE;

                this._round_counter++;

                Card currPlayerCard = this.pickCard(this._player.PlayingDeck);
                Card currEnemyCard = this.pickCard(this._enemy.PlayingDeck);
                Card? WinnerCard = this.Attack(currPlayerCard, currEnemyCard);

                if (WinnerCard == null) continue;

                if(WinnerCard == currPlayerCard)
                {
                    this._player.CardStack.Add(this._enemy.PlayingDeck.First()); //take losers card to cardstack
                    this._enemy.PlayingDeck.RemoveAt(0);
                    continue;
                }
                this._enemy.CardStack.Add(this._player.PlayingDeck.First());
                this._player.PlayingDeck.RemoveAt(0);
                continue;

            } while (this._round_counter <= 100);

            return fightResult.DRAW;
        }

        public Card pickCard(List<Card> deck)
        {
            //https://stackoverflow.com/questions/273313/randomize-a-listt
            int n = deck.Count;
            while (n > 1)
            {
                n--;
                int k = rng.Next(n + 1);
                Card value = deck[k];
                deck[k] = deck[n];
                deck[n] = value;
            }
            return deck.First();
        }

        public Card? Attack(Card playerCard, Card enemyCard)
        {
            float dmgMultiplier = 1;

            if (playerCard.Type == CardType.SPELL || enemyCard.Type == CardType.SPELL)
            {
                dmgMultiplier = getMultiplier(playerCard.CardElement, enemyCard.CardElement);
            }
            if (getImmunity(playerCard, enemyCard)) return playerCard;
            if (getImmunity(enemyCard, playerCard)) return enemyCard;

            return (playerCard.Damage * dmgMultiplier) > (enemyCard.Damage / dmgMultiplier) ?
                    playerCard : (playerCard.Damage * dmgMultiplier) < (enemyCard.Damage / dmgMultiplier) ?
                        enemyCard : null;
        }

        public float getMultiplier(Element playerElement, Element enemyElement)
        {
            if (playerElement == enemyElement) return 1;
            switch(playerElement)
            {
                case Element.WATER when enemyElement == Element.FIRE:
                case Element.FIRE when enemyElement == Element.NORMAL:
                case Element.NORMAL when enemyElement == Element.WATER:
                    return 2;
            }
            return 0.5f;
        }
        
        public bool getImmunity(Card playerCard, Card enemyCard)
        {
            switch(playerCard.Type)
            {
                case CardType.DRAGON when enemyCard.Type == CardType.GOBLIN:
                case CardType.WIZZARD when enemyCard.Type == CardType.ORKS:
                case CardType.SPELL when playerCard.CardElement == Element.WATER && enemyCard.Type == CardType.KNIGHT:
                case CardType.KRAKEN when enemyCard.Type == CardType.SPELL:
                case CardType.ELF when playerCard.CardElement == Element.FIRE && enemyCard.Type == CardType.DRAGON:
                    return true;
            }
            return false;
        }
    }
}
