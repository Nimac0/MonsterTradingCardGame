using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MonsterTradingCardGame;
using MonsterTradingCardGame.RequestHandler;
using MonsterTradingCardGame.Schemas;
using Moq;

namespace MonsterTradingCardGameTest
{
    public class CardTest
    {
        [Test]
        public void TestStringCorrectlySplit()
        {
            string mockCardName = "FireElf";

            Card card = new Card("test", Element.NORMAL, CardType.GOBLIN, 50, false, false, mockCardName);

            string[] response = card.SplitCardName(mockCardName);

            string expected0 = "Fire";
            string expected1 = "Elf";

            Assert.That(response[0].Equals(expected0));
            Assert.That(response[1].Equals(expected1));

        }

        [Test]
        public void TestElementAndTypeCorrectlyAssigned()
        {
            string mockCardName = "FireElf";

            Card card = new Card("test1", Element.NORMAL, CardType.GOBLIN, 50, false, false, mockCardName);

            Card expectedCard = new Card("test1", Element.FIRE, CardType.ELF, 50, false, false, mockCardName);
            
            PackageHandler packageHandler = new PackageHandler();
            packageHandler.SetCardAttributes(card);

            Assert.That(card.CardElement.Equals(expectedCard.CardElement));
            Assert.That(card.Type.Equals(expectedCard.Type));
        }
    }
}
