using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MonsterTradingCardGame;
using MonsterTradingCardGame.Schemas;
using Moq;

namespace MonsterTradingCardGameTest
{
    public class FightTest
    {
        [Test]
        public void TestHigherDamageWinsMonstersOnly()
        {
            Card winnerCard = new Card("test", Element.FIRE, CardType.DRAGON, 50, true, false, "FireDragon");
            Card loserCard = new Card("test1", Element.FIRE, CardType.DRAGON, 20, true, false, "FireDragon");

            User mockuser1 = new User();
            User mockuser2 = new User();

            Card actual = winnerCard;

            Fight newFight = new Fight(mockuser1,mockuser2);
            Card? result = newFight.Attack(winnerCard, loserCard);

            Assert.That(result.Equals(actual));
        }

        [Test]
        public void TestIgnoreElementAdvantageMonstersOnly()
        {
            Card winnerCard = new Card("test", Element.FIRE, CardType.DRAGON, 50, true, false, "FireDragon");
            Card loserCard = new Card("test1", Element.WATER, CardType.DRAGON, 20, true, false, "WaterDragon");

            User mockuser1 = new User();
            User mockuser2 = new User();

            Card actual = winnerCard;

            Fight newFight = new Fight(mockuser1, mockuser2);
            Card? result = newFight.Attack(winnerCard, loserCard);

            Assert.That(result.Equals(actual));
        }

        [Test]
        public void TestFireBeatsNormal()
        {
            Card winnerCard = new Card("test", Element.FIRE, CardType.SPELL, 50, true, false, "FireSpell");
            Card loserCard = new Card("test1", Element.NORMAL, CardType.DRAGON, 50, true, false, "RegularDragon");

            User mockuser1 = new User();
            User mockuser2 = new User();

            Card actual = winnerCard;

            Fight newFight = new Fight(mockuser1, mockuser2);
            Card? result = newFight.Attack(winnerCard, loserCard);

            Assert.That(result.Equals(actual));
        }

        [Test]
        public void TestWaterSpellBeatsKnight()
        {
            Card winnerCard = new Card("test", Element.WATER, CardType.SPELL, 50, true, false, "WaterSpell");
            Card loserCard = new Card("test1", Element.NORMAL, CardType.KNIGHT, 100, true, false, "RegularKnight");

            User mockuser1 = new User();
            User mockuser2 = new User();

            Card actual = winnerCard;

            Fight newFight = new Fight(mockuser1, mockuser2);
            Card? result = newFight.Attack(winnerCard, loserCard);

            Assert.That(result.Equals(actual));
        }

        [Test]
        public void TestDrawPossible()
        {
            Card winnerCard = new Card("test", Element.NORMAL, CardType.KNIGHT, 50, true, false, "Knight");
            Card loserCard = new Card("test1", Element.NORMAL, CardType.KNIGHT, 50, true, false, "Knight");

            User mockuser1 = new User();
            User mockuser2 = new User();

            Card actual = null;

            Fight newFight = new Fight(mockuser1, mockuser2);
            Card? result = newFight.Attack(winnerCard, loserCard);

            Assert.That(result.Equals(actual));
        }
    }
}
