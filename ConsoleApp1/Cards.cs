using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CardGame.Cards
{
    abstract class Card
    {
        public string Name;
        public int Cost;
        public string Description;

        public abstract void Play(Player player, Enemy enemy);
    }

    class Strike : Card
    {
        public Strike()
        {
            Name = "Strike";
            Cost = 1;
            Description = "Deals 6 damage";
        }
        public override void Play(Player player, Enemy enemy)
        {
            int dmg = 6;
            dmg = player.ModifyOutgoingDamage(dmg);
            dmg = enemy.ModifyUpcomingDamage(dmg);
            enemy.TakeDamage(dmg);
        }
    }

    class Defend : Card
    {
        public Defend()
        {
            Name = "Defend";
            Cost = 1;
            Description = "Gives 5 block";
        }

        public override void Play(Player player, Enemy enemy)
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
            Description = "Deals 8 damage. Applies 3 vulnerable";
        }

        public override void Play(Player player, Enemy enemy)
        {
            int dmg = 8;
            dmg = player.ModifyOutgoingDamage(dmg);
            dmg = enemy.ModifyUpcomingDamage(dmg);
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
            Description = "Applies 5 poison";
        }

        public override void Play(Player player, Enemy enemy)
        {
            enemy.ApplyPoison(5);
        }

    }
}
