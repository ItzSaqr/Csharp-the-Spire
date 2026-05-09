using CardGame.Passives;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CardGame.Enemies
{
    class EnemyIntent
    {
        public string Text;
        public Action<Player, Enemy, Combat> Execute;
    }

    abstract class Enemy : Character
    {
        public string Name;
        public EnemyIntent Intent;

        public abstract void ChooseIntent();

        public void ExecuteIntent(Player player, Combat combat)
        {
            Intent.Execute(player, this, combat);
        }
    }

    class Snake : Enemy
    {
        private int turn = 0;
        public Snake()
        {
            Name = "Snake";
            Hp = 26;
            ChooseIntent();
        }

        public override void ChooseIntent()
        {
            turn++;

            if (turn % 3 != 0)
            {
                Intent = new EnemyIntent
                {
                    Text = "Deals 7 damage",
                    Execute = (player, self, combat) =>
                    {
                        int dmg = 7;

                        combat.DealDamage(self, player, dmg, null);
                    }
                };
            }
            else
            {
                Intent = new EnemyIntent
                {
                    Text = "Deals 4 damage, applies debuff",
                    Execute = (player, self, combat) =>
                    {
                        int dmg = 4;
                        combat.DealDamage(self, player, dmg, null);
                        player.ApplyWeak(2);
                    }
                };
            }
        }
    }

    class Gremlin : Enemy
    {
        private int turn = 0;
        public Gremlin()
        {
            Name = "Gremlin";
            Hp = 85;
            ChooseIntent();
        }

        public override void ChooseIntent()
        {
            turn++;

            if (turn == 1)
            {
                Intent = new EnemyIntent
                {
                    Text = "Is going to buff",
                    Execute = (player, self, combat) =>
                    {
                        self.Passives.Add(new Enrage(2));
                    }
                };
            }

            if (turn % 2 == 0)
            {
                Intent = new EnemyIntent
                {
                    Text = "Deals 14 damage",
                    Execute = (player, self, combat) =>
                    {
                        int dmg = 14;

                        combat.DealDamage(self, player, dmg, null);
                    }
                };
            }

            if (turn % 2 == 1 && turn != 1)
            {
                Intent = new EnemyIntent
                {
                    Text = "Deals 8 damage, applies debuff",
                    Execute = (player, self, combat) =>
                    {
                        int dmg = 8;

                        combat.DealDamage(self, player, dmg, null);
                        player.ApplyVulnerable(2);
                    }
                };
            }
        }
    }
}
