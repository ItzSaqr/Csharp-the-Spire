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
        public Action<Player, Enemy> Execute;
    }

    abstract class Enemy : Character
    {
        public string Name;
        public EnemyIntent Intent;

        public abstract void ChooseIntent();

        public void ExecuteIntent(Player player)
        {
            Intent.Execute(player, this);
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
                    Execute = (player, self) =>
                    {
                        int dmg = self.ModifyOutgoingDamage(7);
                        dmg = player.ModifyIncomingDamage(dmg);

                        player.TakeDamage(dmg);
                    }
                };
            }
            else
            {
                Intent = new EnemyIntent
                {
                    Text = "Deals 4 damage, applies debuff",
                    Execute = (player, self) =>
                    {
                        int dmg = self.ModifyOutgoingDamage(4);
                        dmg = player.ModifyIncomingDamage(dmg);

                        player.TakeDamage(dmg);
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
                    Execute = (player, self) =>
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
                    Execute = (player, self) =>
                    {
                        int dmg = self.ModifyOutgoingDamage(14);
                        dmg = player.ModifyIncomingDamage(dmg);

                        player.TakeDamage(dmg);
                    }
                };
            }

            if (turn % 2 == 1 && turn != 1)
            {
                Intent = new EnemyIntent
                {
                    Text = "Deals 8 damage, applies debuff",
                    Execute = (player, self) =>
                    {
                        int dmg = self.ModifyOutgoingDamage(8);
                        dmg = player.ModifyIncomingDamage(dmg);

                        player.TakeDamage(dmg);
                        player.ApplyVulnerable(2);
                    }
                };
            }
        }
    }
}
