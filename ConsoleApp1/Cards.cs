using CardGame.Passives;
using CardGame.Rewards;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CardGame.Cards
{
    enum CardType
    {
        Attack,
        Skill,
        Power,
        Status,
        Curse
    }

    enum Rarity
    {
        Common,
        Uncommon,
        Rare
    }

    abstract class Card
    {
        public string Name;
        public int Cost;
        public string Description;
        public CardType Type;
        public Rarity Rarity;

        public bool Rewardable;
        public bool Exhaust;

        public abstract void Play(Player player, Enemy enemy, Combat combat);
    }

    class Strike : Card
    {
        public Strike()
        {
            Name = "Strike";
            Cost = 1;
            Description = "Deal 6 damage";
            Type = CardType.Attack;
            Rarity = Rarity.Common;

            Rewardable = false;
            Exhaust = false;
        }
        public override void Play(Player player, Enemy enemy, Combat combat)
        {
            int dmg = 6;
            dmg = player.ModifyOutgoingDamage(dmg);
            dmg = enemy.ModifyIncomingDamage(dmg);
            enemy.TakeDamage(dmg);
        }
    }

    class Defend : Card
    {
        public Defend()
        {
            Name = "Defend";
            Cost = 1;
            Description = "Gain 5 block";
            Type = CardType.Skill;
            Rarity = Rarity.Common;

            Rewardable = false;
            Exhaust = false;
        }

        public override void Play(Player player, Enemy enemy, Combat combat)
        {
            player.GainBlock(5);
        }
    }

    class Bash : Card
    {
        public Bash()
        {
            Name = "Bash";
            Cost = 2;
            Description = "Deal 8 damage. Apply 3 vulnerable";
            Type = CardType.Attack;
            Rarity = Rarity.Common;

            Rewardable = false;
            Exhaust = false;
        }

        public override void Play(Player player, Enemy enemy, Combat combat)
        {
            int dmg = 8;
            dmg = player.ModifyOutgoingDamage(dmg);
            dmg = enemy.ModifyIncomingDamage(dmg);
            enemy.TakeDamage(dmg);
            enemy.ApplyVulnerable(3);
        }
    }

    class DeadlyPoison : Card
    {
        public DeadlyPoison()
        {
            Name = "Deadly Poison";
            Cost = 1;
            Description = "Apply 5 poison";
            Type = CardType.Skill;
            Rarity = Rarity.Common;

            Rewardable = true;
            Exhaust = false;
        }

        public override void Play(Player player, Enemy enemy, Combat combat)
        {
            enemy.ApplyPoison(5);
        }

    }

    class Prepare : Card
    {
        public Prepare()
        {
            Name = "Prepare";
            Cost = 1;
            Description = "Draw 3 cards";
            Type = CardType.Skill;
            Rarity = Rarity.Common;

            Rewardable = true;
            Exhaust = false;
        }

        public override void Play(Player player, Enemy enemy, Combat combat)
        {
            player.DrawCards(3);
        }
    }

    class Concentrate : Card
    {
        public Concentrate()
        {
            Name = "Concentrate";
            Cost = 0;
            Description = "Discard 2 cards, gain 1 energy";
            Type = CardType.Skill;
            Rarity = Rarity.Common;

            Rewardable = true;
            Exhaust = false;
        }

        public override void Play(Player player, Enemy enemy, Combat combat)
        {
            combat.DiscardFromHand(2);
            player.Energy += 1;
        }
    }

    class Shiv : Card
    {
        public Shiv()
        {
            Name = "Shiv";
            Cost = 0;
            Description = "Deal 4 damage. Exhaust.";
            Type = CardType.Attack;
            Rarity = Rarity.Common;

            Rewardable = false;
            Exhaust = true;
        }

        public override void Play(Player player, Enemy enemy, Combat combat)
        {
            int dmg = 4;
            dmg = player.ModifyOutgoingDamage(dmg);
            dmg = enemy.ModifyIncomingDamage(dmg);
            enemy.TakeDamage(dmg);
        }
    }

    class InfiniteBlades : Card
    {
        public InfiniteBlades()
        {
            Name = "Infinite Blades";
            Cost = 1;
            Description = "Add a Shiv into your hand every turn";
            Type = CardType.Power;
            Rarity = Rarity.Uncommon;

            Rewardable = true;
            Exhaust = false;
        }

        public override void Play(Player player, Enemy enemy, Combat combat)
        {
            player.Passives.Add(new CreateShiv());
        }
    }
}
