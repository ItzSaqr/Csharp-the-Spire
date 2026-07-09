using CardGame.Char;
using CardGame.Cards;
using CardGame.Enemies;
using CardGame.CombatNamespace;
using CardGame;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace CardGame
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public Combat combat;
        public MainWindow()
        {
            InitializeComponent();

            StartGame();
        }

        private void StartGame()
        {
            var player = new Player { Hp = 80, MaxHp = 80, MaxEnergy = 3 };

            for (int i = 0; i < 5; i++) player.Deck.Add(new Strike());
            for (int i = 0; i < 5; i++) player.Deck.Add(new Defend());
            player.Deck.Add(new Bash());
            player.Deck.Add(new InfiniteBlades());

            var enemy = new Gremlin();

            combat = new Combat(player, enemy, new ConsoleCombatUI());

            UpdateUI();
        }

        private void UpdateUI()
        {
            HpText.Text = $"HP: {combat.Player.Hp}/{combat.Player.MaxHp}";
            BlockText.Text = $"Block: {combat.Player.Block}";
            EnergyText.Text = $"Energy: {combat.Player.Energy}/{combat.Player.MaxEnergy}";
            
            EnemyNameText.Text = $"Enemy: {combat.Enemy.Name}";
            EnemyHpText.Text = $"HP: {combat.Enemy.Hp}/{combat.Enemy.MaxHp}";
            EnemyBlockText.Text = $"Block: {combat.Enemy.Block}";
            EnemyIntentText.Text = $"Intent: {combat.Enemy.Intent.Text}";

            HandPanel.Children.Clear();
            foreach (var card in combat.Player.Hand)
            {
                var button = new Button
                {
                    Width = 120,
                    Height = 180,
                    Margin = new Thickness(5),
                    Content = $"[{card.Cost}] {card.Name}\n{card.Description}",
                    Tag = card,
                    BorderThickness = new Thickness(3),
                    BorderBrush = GetBorderColor(card.Type)
                };
                button.Click += Card_Click;

                HandPanel.Children.Add(button);
            }
        }

        private void Card_Click(object sender, RoutedEventArgs e)
        {
            var button = (Button)sender;
            var card = (Card)button.Tag;

            combat.PlayCard(card);

            UpdateUI();
        }

        private void EndTurn_Click(object sender, RoutedEventArgs e)
        {
            combat.EndPlayerTurn();
            UpdateUI();
        }

        private void ViewPile(List<Card> cards)
        {
            PilePanel.Children.Clear();
            foreach (var card in cards)
            {
                var button = new Button
                {
                    Width = 120,
                    Height = 180,
                    Margin = new Thickness(5),
                    Content = $"[{card.Cost}] {card.Name}\n{card.Description}",
                    BorderThickness = new Thickness(3),
                    BorderBrush = GetBorderColor(card.Type)
                };

                PilePanel.Children.Add(button);
            }

            PileOverlay.Visibility = Visibility.Visible;
        }

        private Brush GetBorderColor(CardType type)
        {
            return type switch
            {
                CardType.Attack => Brushes.Red,
                CardType.Skill => Brushes.Blue,
                CardType.Power => Brushes.Gold,
                _ => Brushes.Gray,
            };
        }

        private void ViewDrawPile_Click(object sender, RoutedEventArgs e)
        {
            ViewPile(combat.Player.DrawPile);
        }

        private void ViewDiscardPile_Click(object sender, RoutedEventArgs e)
        {
            ViewPile(combat.Player.DiscardPile);
        }

        private void ViewExhaustPile_Click(object sender, RoutedEventArgs e)
        {
            ViewPile(combat.Player.ExhaustPile);
        }

        private void ClosePileOverlay_Click(object sender, RoutedEventArgs e)
        {
            PileOverlay.Visibility = Visibility.Collapsed;
        }

        private void BorderPile_Click(object sender, RoutedEventArgs e)
        {
            e.Handled = true; 
        }

    }
}