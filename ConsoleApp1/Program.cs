using CardGame.Cards;

var player = new Player { Hp = 50 };

for (int i = 0; i < 5; i++)
    player.DrawPile.Add(new Strike());

for (int i = 0; i < 5; i++)
    player.DrawPile.Add(new Defend());

player.DrawPile.Add(new Bash());

player.Reshuffle();

var enemy = new Snake();

var combat = new Combat(player, enemy);

while (combat.State != CombatState.Victory &&
       combat.State != CombatState.Defeat)
{
    if (combat.State == CombatState.PlayerTurn)
    {
        Console.WriteLine("================================");
        Console.WriteLine($"PLAYER HP: {player.Hp} | Energy: {player.Energy}");
        Console.WriteLine($"PLAYER: {player.GetStatusText()}");

        Console.WriteLine();
        Console.WriteLine($"Draw: {player.DrawPile.Count} | Discard: {player.DiscardPile.Count}");

        Console.WriteLine();

        Console.WriteLine($"{enemy.Name} HP: {enemy.Hp}");
        Console.WriteLine($"ENEMY: {enemy.GetStatusText()}");
        Console.WriteLine($"INTENT: {enemy.Intent.Text}");
        Console.WriteLine("================================");

        Console.WriteLine("HAND:");

        for (int i = 0; i < player.Hand.Count; i++)
        {
            var c = player.Hand[i];
            Console.WriteLine($"{i}. {c.Name} [{c.Cost}] - {c.Description}");
        }

        Console.WriteLine();
        Console.WriteLine("Choose card index or 'e' to end turn:");

        var input = Console.ReadLine();

        Console.Clear();

        if (input == "e")
        {
            combat.EndPlayerTurn();
            continue;
        }

        if (int.TryParse(input, out int index) &&
            index >= 0 && index < player.Hand.Count)
        {
            combat.PlayCard(player.Hand[index]);
        }
    }
}


Console.WriteLine(combat.State);

class Character
{
    public int Hp;
    public int Block;
    public int Vulnerable;
    public int Weak;
    public int Strength;
    public int Dexterity;
    public int Poison;

    public void TakeDamage(int amount)
    {
        int blocked = Math.Min(Block, amount);
        Block -= blocked;
        Hp -= amount - blocked;
    }

    public void TakeDirectDamage(int amount)
    {
        Hp -= amount;
    }

    public void GainBlock(int amount)
    {
        Block += (amount+Dexterity);
    }

    public int ModifyOutgoingDamage(int damage)
    {
        damage += Strength;
        if (Weak > 0) damage = (int)(damage * 0.75);
        return damage;
    }

    public int ModifyUpcomingDamage(int damage)
    {
        if (Vulnerable > 0) damage = (int)(damage * 1.5);
        return damage;
    }

    public void ApplyWeak(int amount)
    {
        Weak += amount;
    }

    public void ApplyVulnerable(int amount)
    {
        Vulnerable += amount;
    }

    public void ApplyPoison(int amount)
    {
        Poison += amount;
    }

    public void OnTurnEnd()
    {
        if (Weak > 0) Weak--;
        if (Vulnerable > 0) Vulnerable--;
        if (Poison > 0)
        {
            TakeDirectDamage(Poison);
            Poison--;
        }
    }

    public string GetStatusText()
    {
        var parts = new List<string>();

        if (Block > 0) parts.Add($"Block {Block}");
        if (Weak > 0) parts.Add($"Weak {Weak}");
        if (Vulnerable > 0) parts.Add($"Vulnerable {Vulnerable}");
        if (Strength != 0) parts.Add($"Strength {Strength}");
        if (Dexterity != 0) parts.Add($"Dexterity {Dexterity}");
        if (Poison != 0) parts.Add($"Poison {Poison}");

        return parts.Count == 0 ? "No statuses" : string.Join(", ", parts);
    }
}

class Player : Character
{
    public int Energy;

    public List<Card> DrawPile = new();
    public List<Card> Hand = new();
    public List<Card> DiscardPile = new();

    public void Reshuffle()
    {
        DrawPile.AddRange(DiscardPile);
        DiscardPile.Clear();
        DrawPile = DrawPile.OrderBy(x => Random.Shared.Next()).ToList();
    }

    public void DrawCards(int amount)
    {
        for (int i = 0; i < amount; i++)
        {
            if (DrawPile.Count == 0)
                Reshuffle();

            if (DrawPile.Count == 0)
                return;

            Card card = DrawPile[0];
            DrawPile.RemoveAt(0);
            Hand.Add(card);
        }
    }

    public void DiscardCards(int amount)
    {
        int toDiscard = Math.Min(Hand.Count(), amount);
        var discarded = Hand.Take(toDiscard).ToList();
        DiscardPile.AddRange(discarded);
        Hand.RemoveRange(0, toDiscard);
    }

    public void DiscardHand()
    {
        DiscardPile.AddRange(Hand);
        Hand.Clear();
    }
}

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
                    dmg = player.ModifyUpcomingDamage(dmg);

                    player.TakeDamage(dmg);
                }
            };
        }
        else
        {
            Intent = new EnemyIntent
            {
                Text = "Deals 4 damage, applies 2 Weak",
                Execute = (player, self) =>
                {
                    int dmg = self.ModifyOutgoingDamage(4);
                    dmg = player.ModifyUpcomingDamage(dmg);

                    player.TakeDamage(dmg);
                    player.ApplyWeak(2);
                }
            };
        }
    }
}

enum CombatState
{
    PlayerTurn,
    EnemyTurn,
    Victory,
    Defeat
}

class Combat
{
    public Player Player { get; }
    public Enemy Enemy { get; }

    public CombatState State { get; private set; }

    public Combat(Player player, Enemy enemy)
    {
        Player = player;
        Enemy = enemy;
        
        State = CombatState.PlayerTurn;

        StartPlayerTurn();
    }

    public void PlayCard(Card card)
    {
        if (State != CombatState.PlayerTurn) return;
        if (!Player.Hand.Contains(card)) return;
        if (Player.Energy < card.Cost) return;

        Player.Energy -= card.Cost;

        card.Play(Player, Enemy);

        Player.Hand.Remove(card);
        Player.DiscardPile.Add(card);

        CheckEndCombat();
    }

    public void StartPlayerTurn()
    {
        State = CombatState.PlayerTurn;

        Player.Block = 0;
        Player.Energy = 3;
        Player.DrawCards(5);
    }

    public void EndPlayerTurn()
    {
        if (State != CombatState.PlayerTurn) return;
        Player.DiscardHand();
        Player.OnTurnEnd();
        StartEnemyTurn();
    }

    public void StartEnemyTurn()
    {
        State = CombatState.EnemyTurn;

        CheckEndCombat();

        Enemy.ExecuteIntent(Player);

        CheckEndCombat();

        if (State == CombatState.EnemyTurn)
        {
            Enemy.OnTurnEnd();
            Enemy.ChooseIntent();
            StartPlayerTurn();
        }
    }

    private void CheckEndCombat()
    {
        if (Enemy.Hp <= 0) State = CombatState.Victory;
        else if (Player.Hp <= 0) State = CombatState.Defeat;
    }
}
