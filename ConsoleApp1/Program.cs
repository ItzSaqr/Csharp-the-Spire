using CardGame.Cards;
using CardGame.Passives;
using System.Collections.Generic;

var player = new Player { Hp = 50 };

for (int i = 0; i < 5; i++)
    player.DrawPile.Add(new Strike());

for (int i = 0; i < 5; i++)
    player.DrawPile.Add(new Defend());

player.DrawPile.Add(new Bash());
player.DrawPile.Add(new Prepare());
player.DrawPile.Add(new Concentrate());

player.Reshuffle();

var enemy = new Snake();

var ui = new ConsoleCombatUI();
var combat = new Combat(player, enemy, ui);

while (combat.State != CombatState.Victory &&
       combat.State != CombatState.Defeat)
{
    if (combat.State == CombatState.PlayerTurn)
    {
        Console.Clear();
        Console.WriteLine("================================");
        Console.WriteLine($"PLAYER HP: {player.Hp} | Energy: {player.Energy}");
        Console.WriteLine($"PLAYER: {player.GetStatusText()}");

        Console.WriteLine();
        Console.WriteLine($"Draw: {player.DrawPile.Count} | Discard: {player.DiscardPile.Count} | Exhaust: {player.ExhaustPile.Count}");
        Console.WriteLine($"View piles: 'd' - Draw Pile | 's' - Discard Pile | 'x' - Exhaust Pile");

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

        if (input == "d")
        {
            ShowPile("Draw Pile:", player.DrawPile);
            continue;
        }

        if (input == "s")
        {
            ShowPile("Discard Pile:", player.DiscardPile);
            continue;
        }

        if (input == "x")
        {
            ShowPile("Exhaust Pile:", player.ExhaustPile);
            continue;
        }

        if (int.TryParse(input, out int index) &&
            index >= 0 && index < player.Hand.Count)
        {
            combat.PlayCard(player.Hand[index]);
        }
    }
}
static void ShowPile(string title, List<Card> pile)
{
    Console.Clear();
    Console.WriteLine("================================");
    Console.WriteLine(title);

    if (pile.Count == 0)
        Console.WriteLine("Pile is empty");

    for (int i = 0; i < pile.Count; i++)
    {
        var c = pile[i];
        Console.WriteLine($"{i}. {c.Name} [{c.Cost}] - {c.Description}");
    }

    Console.WriteLine("================================");
    Console.WriteLine();
    Console.WriteLine("Press Enter to return");
    Console.ReadLine();
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
    public List<PassiveEffect> Passives = new();

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

    public int ModifyIncomingDamage(int damage)
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
    public List<Card> ExhaustPile = new();

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

    public void DiscardRandom(int amount)
    {
        Random random = new Random();
        for (int i = 0; i < amount && Hand.Count > 0; i++)
        {
            int randInt = random.Next(Hand.Count);

            Card card = Hand[randInt];
            
            Hand.RemoveAt(randInt);
            DiscardPile.Add(card);
        }
    }

    public void DiscardSelected(List<Card> toDiscard)
    {
        foreach (Card card in toDiscard)
        {
            if (Hand.Remove(card))
                DiscardPile.Add(card);
        }
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
                    dmg = player.ModifyIncomingDamage(dmg);

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
                    dmg = player.ModifyIncomingDamage(dmg);

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

interface ICombatUI
{
    List<Card> ChooseCards(Player player, List<Card> source, int amount);
    void ShowMessage(string message);
}

class ConsoleCombatUI : ICombatUI
{
    public List<Card> ChooseCards(Player player, List<Card> source, int amount)
    {
        var selected = new List<Card>();
        if (amount >= source.Count) {
            selected.AddRange(source);
            return selected; 
        }
        while (selected.Count() < amount)
        {
            Console.Clear();
            Console.WriteLine($"Choose {amount} cards. {selected.Count}/{amount} chosen.");

            for (int i = 0; i < source.Count(); i++)
            {
                var c = source[i];
                var mark = selected.Contains(c) ? " [selected]" : "";
                Console.WriteLine($"{i}. {c.Name} [{c.Cost}] - {c.Description}{mark}");
            }

            var input = Console.ReadLine();

            if (!int.TryParse(input, out int index))
                continue;

            if (index < 0 || index >= source.Count)
                continue;

            var card = source[index];

            if (!selected.Contains(card))
            {
                selected.Add(card);
            }
            else if (selected.Contains(card))
            {
                selected.Remove(card);
            }
        }
        return selected;
    }

    public void ShowMessage(string message)
    {
        Console.WriteLine(message);
    }
}

class Combat
{
    private readonly ICombatUI ui;

    public Player Player { get; }
    public Enemy Enemy { get; }

    public CombatState State { get; private set; }

    public Combat(Player player, Enemy enemy, ICombatUI ui)
    {
        Player = player;
        Enemy = enemy;
        this.ui = ui;
        
        State = CombatState.PlayerTurn;

        StartPlayerTurn();
    }

    public void PlayCard(Card card)
    {
        if (State != CombatState.PlayerTurn) return;
        if (!Player.Hand.Contains(card)) return;
        if (Player.Energy < card.Cost) return;

        Player.Energy -= card.Cost;

        Player.Hand.Remove(card);

        card.Play(Player, Enemy, this);

        foreach (var passive in Player.Passives) passive.OnCardPlayed(Player, this, card);

        if (card.Exhaust) Player.ExhaustPile.Add(card);
        else Player.DiscardPile.Add(card);

        CheckEndCombat();
    }

    public void DiscardFromHand(int amount)
    {
        var cards = ui.ChooseCards(Player, Player.Hand, amount);
        Player.DiscardSelected(cards);
    }

    public void StartPlayerTurn()
    {
        State = CombatState.PlayerTurn;

        Player.Block = 0;
        Player.Energy = 3;
        Player.DrawCards(5);

        foreach (var passive in Player.Passives) passive.OnTurnStart(Player, this);
    }

    public void EndPlayerTurn()
    {
        if (State != CombatState.PlayerTurn) return;

        Player.DiscardHand();
        foreach (var passive in Player.Passives) passive.OnTurnEnd(Player, this);
        Player.OnTurnEnd();

        CheckEndCombat();
        if (State != CombatState.PlayerTurn) return;

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
