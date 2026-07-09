using CardGame.Enemies;
using CardGame.Passives;
using CardGame.Char;
using CardGame.Rewards;
using CardGame.CombatNamespace;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace CardGame.Cards
{
    public enum CardType
    {
        Attack,
        Skill,
        Power,
        Status,
        Curse
    }

    public enum Rarity
    {
        Common,
        Uncommon,
        Rare
    }

    public abstract class Card
    {
        public string Name;
        public int Cost;
        public string Description;
        public CardType Type;
        public Rarity Rarity;

        public bool Rewardable;
        public bool Exhaust;
        public bool Upgraded;
        public bool Innate;

        public abstract void Play(Player player, Enemy enemy, Combat combat);

        public virtual void Upgrade()
        {
            Name += "+";
            Upgraded = true;
        }
    }

    public class Strike : Card
    {
        private int damage = 6;
        public Strike()
        {
            Name = "Strike";
            Cost = 1;
            Description = "Deal 6 damage";
            Type = CardType.Attack;
            Rarity = Rarity.Common;

            Rewardable = false;
            Exhaust = false;
            Upgraded = false;
            Innate = false;
        }
        public override void Play(Player player, Enemy enemy, Combat combat)
        {
            combat.DealDamage(player, enemy, damage, this);
        }

        public override void Upgrade()
        {
            if (Upgraded) return;
            Name += "+";
            Upgraded = true;
            damage = 9;
            Description = "Deal 9 damage";
        }
    }

    public class Defend : Card
    {
        private int block = 5;
        public Defend()
        {
            Name = "Defend";
            Cost = 1;
            Description = "Gain 5 block";
            Type = CardType.Skill;
            Rarity = Rarity.Common;

            Rewardable = false;
            Exhaust = false;
            Upgraded = false;
            Innate = false;
        }

        public override void Play(Player player, Enemy enemy, Combat combat)
        {
            player.GainBlock(block);
        }

        public override void Upgrade()
        {
            if (Upgraded) return;
            Name += "+";
            Upgraded = true;
            block = 8;
            Description = "Gain 8 block";
        }
    }

    public class Bash : Card
    {
        private int damage = 8;
        private int vulnerable = 2;
        public Bash()
        {
            Name = "Bash";
            Cost = 2;
            Description = "Deal 8 damage. Apply 2 vulnerable";
            Type = CardType.Attack;
            Rarity = Rarity.Common;

            Rewardable = false;
            Exhaust = false;
            Upgraded = false;
            Innate = false;
        }

        public override void Play(Player player, Enemy enemy, Combat combat)
        {
            combat.DealDamage(player, enemy, damage, this);
            enemy.ApplyVulnerable(vulnerable);
        }

        public override void Upgrade()
        {
            if (Upgraded) return;
            Name += "+";
            Upgraded = true;
            damage = 11;
            vulnerable = 3;
            Description = "Deal 11 damage. Apply 3 vulnerable";
        }
    }

    public class DeadlyPoison : Card
    {
        private int poison = 5;
        public DeadlyPoison()
        {
            Name = "Deadly Poison";
            Cost = 1;
            Description = "Apply 5 poison";
            Type = CardType.Skill;
            Rarity = Rarity.Common;

            Rewardable = true;
            Exhaust = false;
            Upgraded = false;
            Innate = false;
        }

        public override void Play(Player player, Enemy enemy, Combat combat)
        {
            enemy.ApplyPoison(poison);
        }

        public override void Upgrade()
        {
            if (Upgraded) return;
            Name += "+";
            Upgraded = true;
            poison = 8;
            Description = "Apply 8 poison";
        }
    }

    public class Prepare : Card
    {
        private int draw = 3;
        public Prepare()
        {
            Name = "Prepare";
            Cost = 1;
            Description = "Draw 3 cards";
            Type = CardType.Skill;
            Rarity = Rarity.Common;

            Rewardable = true;
            Exhaust = false;
            Upgraded = false;
            Innate = false;
        }

        public override void Play(Player player, Enemy enemy, Combat combat)
        {
            player.DrawCards(draw);
        }

        public override void Upgrade()
        {
            if (Upgraded) return;
            Name += "+";
            Upgraded = true;
            draw = 4;
            Description = "Draw 4 cards";
        }
    }

    public class Concentrate : Card
    {
        private int discard = 3;
        private int energy = 2;
        public Concentrate()
        {
            Name = "Concentrate";
            Cost = 0;
            Description = "Discard 3 cards, gain 2 energy";
            Type = CardType.Skill;
            Rarity = Rarity.Common;

            Rewardable = true;
            Exhaust = false;
            Upgraded = false;
            Innate = false;
        }

        public override void Play(Player player, Enemy enemy, Combat combat)
        {
            combat.DiscardFromHand(discard);
            player.Energy += energy;
        }
        public override void Upgrade()
        {
            if (Upgraded) return;
            Name += "+";
            Upgraded = true;
            discard = 2;
            Description = "Discard 2 cards, gain 2 energy";
        }
    }

    public class Shiv : Card
    {
        private int damage = 4;
        public Shiv()
        {
            Name = "Shiv";
            Cost = 0;
            Description = "Deal 4 damage. Exhaust.";
            Type = CardType.Attack;
            Rarity = Rarity.Common;

            Rewardable = false;
            Exhaust = true;
            Upgraded = false;
            Innate = false;
        }

        public override void Play(Player player, Enemy enemy, Combat combat)
        {
            combat.DealDamage(player, enemy, damage, this);
        }

        public override void Upgrade()
        {
            if (Upgraded) return;
            Name += "+";
            Upgraded = true;
            damage = 6;
            Description = "Deal 6 damage. Exhaust.";
        }
    }

    public class InfiniteBlades : Card
    {
        public InfiniteBlades()
        {
            Name = "Infinite Blades";
            Cost = 1;
            Description = "Add a Shiv into your hand every turn.";
            Type = CardType.Power;
            Rarity = Rarity.Uncommon;

            Rewardable = true;
            Exhaust = false;
            Upgraded = false;
            Innate = false;
        }

        public override void Play(Player player, Enemy enemy, Combat combat)
        {
            player.Passives.Add(new CreateShiv());
        }

        public override void Upgrade()
        {
            if (Upgraded) return;
            Name += "+";
            Upgraded = true;
            Innate = true;
            Description = "Innate. Add a Shiv into your hand every turn.";
        }
    }
}
