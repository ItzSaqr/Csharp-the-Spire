var player = new Player { Hp = 50 };

for (int i = 0; i < 5; i++)
    player.DrawPile.Add(new Strike());

for (int i = 0; i < 5; i++)
    player.DrawPile.Add(new Defend());

player.Reshuffle();

var enemy = new Snake();

var combat = new Combat(player, enemy);

while (combat.State != CombatState.Victory &&
       combat.State != CombatState.Defeat)
{
    if (combat.State == CombatState.PlayerTurn)
    {
        Console.WriteLine($"PLAYER HP: {player.Hp}, Energy: {player.Energy}");

        Console.WriteLine($"ENEMY HP: {enemy.Hp}, Block: {enemy.Block}");

        for (int i = 0; i < player.Hand.Count; i++)
        {
            var c = player.Hand[i];
            Console.WriteLine($"{i}: {c.Name} (cost {c.Cost})");
            Console.WriteLine($"{c.Description}");
        }

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

    public void TakeDamage(int amount)
    {
        int blocked = Math.Min(Block, amount);
        Block -= blocked;
        Hp -= amount - blocked;
    }

    public void GainBlock(int amount)
    {
        Block += amount;
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

abstract class Enemy : Character
{
    public string Name;

    public abstract void Act(Player player);
}

class Snake : Enemy
{
    private int turn = 0;
    public Snake()
    {
        Name = "Snake";
        Hp = 26;
    }

    public override void Act(Player player)
    {
        turn++;

        if (turn % 3 == 0)
            GainBlock(6);
        else player.TakeDamage(7);
    }
}

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
        enemy.TakeDamage(6);
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
        StartEnemyTurn();
    }

    public void StartEnemyTurn()
    {
        State = CombatState.EnemyTurn;

        CheckEndCombat();

        Enemy.Act(Player);

        CheckEndCombat();

        if (State == CombatState.EnemyTurn)
            StartPlayerTurn();
    }

    private void CheckEndCombat()
    {
        if (Enemy.Hp <= 0) State = CombatState.Victory;
        else if (Player.Hp <= 0) State = CombatState.Defeat;
    }
}
