﻿using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonsterTradingCardGame.Schemas
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
        public int? Wins { get; set; }
        public int? Losses { get; set; }
        public string? Name { get; set; }
        public string? Bio { get; set; }
        public string? Image { get; set; }

        public User()
        {

        }

    }
}
