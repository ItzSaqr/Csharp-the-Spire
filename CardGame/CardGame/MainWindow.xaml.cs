using CardGame.Char;
using CardGame.Cards;
using CardGame.Enemies;
using CardGame.CombatNamespace;
using CardGame.Passives;
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
            for (int i = 0; i < 9; i++) player.Deck.Add(new InfiniteBlades());
            player.Passives.Add(new PenNib());

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

            DrawHand();
            DrawPlayerPassives();
            DrawEnemyPassives();
            DrawPlayerEffects();
            DrawEnemyEffects();
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

        private void DrawHand()
        {
            HandPanel.Children.Clear();
            foreach (var card in combat.Player.Hand)
            {
                var costText = new TextBlock
                {
                    Text = $"[{card.Cost}]",
                    FontSize = 20,
                    FontWeight = FontWeights.Bold,
                    TextAlignment = TextAlignment.Left,
                    HorizontalAlignment = HorizontalAlignment.Left,
                    VerticalAlignment = VerticalAlignment.Top
                };
                Grid.SetColumn(costText, 0);
                Grid.SetRow(costText, 0);

                var nameText = new TextBlock
                {
                    Text = card.Name,
                    FontWeight = FontWeights.SemiBold,
                    TextAlignment = TextAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Top,
                    HorizontalAlignment = HorizontalAlignment.Center
                };

                Grid.SetColumn(nameText, 0);
                Grid.SetRow(nameText, 1);
                Grid.SetColumnSpan(nameText, 2);

                var descText = new TextBlock
                {
                    Text = card.Description,
                    TextWrapping = TextWrapping.Wrap,
                    TextAlignment = TextAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center
                };
                Grid.SetRow(descText, 2);
                Grid.SetColumn(descText, 0);
                Grid.SetColumnSpan(descText, 2);

                var button = new Button
                {
                    Width = 120,
                    Height = 180,
                    Margin = new Thickness(5),
                    Padding = new Thickness(0),
                    HorizontalContentAlignment = HorizontalAlignment.Stretch,
                    VerticalContentAlignment = VerticalAlignment.Stretch,
                    Content = new Grid
                    {
                        RowDefinitions =
                        {
                            new RowDefinition {Height = new GridLength(30)},
                            new RowDefinition {Height = new GridLength(20)},
                            new RowDefinition {Height = new GridLength(1, GridUnitType.Star)}
                        },
                        ColumnDefinitions =
                        {
                            new ColumnDefinition{Width = GridLength.Auto},
                            new ColumnDefinition{Width = new GridLength(1, GridUnitType.Star)}
                        },
                        Children =
                        {
                            costText,
                            nameText,
                            descText
                        }
                    },
                    Tag = card,
                    BorderThickness = new Thickness(3),
                    BorderBrush = GetBorderColor(card.Type),
                };
                button.Click += Card_Click;

                HandPanel.Children.Add(button);
            }
        }
        private void DrawPlayerPassives()
        {
            PlayerPassivesPanel.Children.Clear();
            foreach (var passive in combat.Player.Passives)
            {
                var button = new Button
                {
                    Width = 40,
                    Height = 55,
                    Margin = new Thickness(3),
                    Content = $"{passive.Name}",
                    BorderThickness = new Thickness(3),
                    BorderBrush = Brushes.Purple,
                };

                button.ToolTip = new ToolTip
                {
                    Content = new TextBlock
                    {
                        Text = $"{passive.Name}\n{passive.Description}" +
                        (passive.GetDescription() is string desc ? $"\n{desc}" : ""),
                        TextWrapping = TextWrapping.Wrap,
                        Width = 200
                    }
                };
                ToolTipService.SetInitialShowDelay(button, 100);
                PlayerPassivesPanel.Children.Add(button);
            }
        }

        private void DrawEnemyPassives()
        {
            EnemyPassivesPanel.Children.Clear();
            foreach (var passive in combat.Enemy.Passives)
            {
                var button = new Button
                {
                    Width = 40,
                    Height = 55,
                    Margin = new Thickness(3),
                    Content = $"{passive.Name}",
                    BorderThickness = new Thickness(3),
                    BorderBrush = Brushes.Purple,
                };

                button.ToolTip = new ToolTip
                {
                    Content = new TextBlock
                    {
                        Text = $"{passive.Name}\n{passive.Description}" +
                        (passive.GetDescription() is string desc ? $"\n{desc}" : ""),
                        TextWrapping = TextWrapping.Wrap,
                        Width = 200
                    }
                };
                ToolTipService.SetInitialShowDelay(button, 100);
                EnemyPassivesPanel.Children.Add(button);
            }
        }

        private void DrawPlayerEffects()
        {
            PlayerEffectsPanel.Children.Clear();
            if (combat.Player.Vulnerable > 0)
            {
                var button = new Button
                {
                    Width = 40,
                    Height = 55,
                    Margin = new Thickness(3),
                    Content = $"Vuln {combat.Player.Vulnerable}",
                    BorderThickness = new Thickness(3),
                    BorderBrush = Brushes.OrangeRed,
                };
                button.ToolTip = new ToolTip
                {
                    Content = new TextBlock
                    {
                        Text = $"Vulnerable\nTake 50% more damage from attacks for {combat.Player.Vulnerable} turns.",
                        TextWrapping = TextWrapping.Wrap,
                        Width = 200
                    }
                };
                ToolTipService.SetInitialShowDelay(button, 100);
                PlayerEffectsPanel.Children.Add(button);
            }

            if (combat.Player.Weak > 0)
            {
                var button = new Button
                {
                    Width = 40,
                    Height = 55,
                    Margin = new Thickness(3),
                    Content = $"Weak {combat.Player.Weak}",
                    BorderThickness = new Thickness(3),
                    BorderBrush = Brushes.LightBlue,
                };
                button.ToolTip = new ToolTip
                {
                    Content = new TextBlock
                    {
                        Text = $"Weak\nDeal 25% less damage with attacks for {combat.Player.Weak} turns.",
                        TextWrapping = TextWrapping.Wrap,
                        Width = 200
                    }
                };
                ToolTipService.SetInitialShowDelay(button, 100);
                PlayerEffectsPanel.Children.Add(button);
            }

            if(combat.Player.Dexterity != 0)
            {
                var button = new Button
                {
                    Width = 40,
                    Height = 55,
                    Margin = new Thickness(3),
                    Content = $"Dex {combat.Player.Dexterity}",
                    BorderThickness = new Thickness(3),
                    BorderBrush = Brushes.Green,
                };
                button.ToolTip = new ToolTip
                {
                    Content = new TextBlock
                    {
                        Text = $"Dexterity\nIncreases block gained from skills by {combat.Player.Dexterity}.",
                        TextWrapping = TextWrapping.Wrap,
                        Width = 200
                    }
                };
                ToolTipService.SetInitialShowDelay(button, 100);
                PlayerEffectsPanel.Children.Add(button);
            }

            if(combat.Player.Strength != 0)
            {
                var button = new Button
                {
                    Width = 40,
                    Height = 55,
                    Margin = new Thickness(3),
                    Content = $"Str {combat.Player.Strength}",
                    BorderThickness = new Thickness(3),
                    BorderBrush = Brushes.Red,
                };
                button.ToolTip = new ToolTip
                {
                    Content = new TextBlock
                    {
                        Text = $"Strength\nIncreases damage dealed by attacks by {combat.Player.Strength}.",
                        TextWrapping = TextWrapping.Wrap,
                        Width = 200
                    }
                };
                ToolTipService.SetInitialShowDelay(button, 100);
                PlayerEffectsPanel.Children.Add(button);
            }

            if(combat.Player.Poison > 0)
            {
                var button = new Button
                {
                    Width = 40,
                    Height = 55,
                    Margin = new Thickness(3),
                    Content = $"Str {combat.Player.Poison}",
                    BorderThickness = new Thickness(3),
                    BorderBrush = Brushes.DarkGreen,
                };
                button.ToolTip = new ToolTip
                {
                    Content = new TextBlock
                    {
                        Text = $"Poison\nDeals {combat.Player.Poison} damage at the end of the turn.",
                        TextWrapping = TextWrapping.Wrap,
                        Width = 200
                    }
                };
                ToolTipService.SetInitialShowDelay(button, 100);
                PlayerEffectsPanel.Children.Add(button);
            }
        }

        private void DrawEnemyEffects()
        {
            EnemyEffectsPanel.Children.Clear();
            if (combat.Enemy.Vulnerable > 0)
            {
                var button = new Button
                {
                    Width = 40,
                    Height = 55,
                    Margin = new Thickness(3),
                    Content = $"Vuln {combat.Enemy.Vulnerable}",
                    BorderThickness = new Thickness(3),
                    BorderBrush = Brushes.OrangeRed,
                };
                button.ToolTip = new ToolTip
                {
                    Content = new TextBlock
                    {
                        Text = $"Vulnerable\nTake 50% more damage from attacks for {combat.Enemy.Vulnerable} turns.",
                        TextWrapping = TextWrapping.Wrap,
                        Width = 200
                    }
                };
                ToolTipService.SetInitialShowDelay(button, 100);
                EnemyEffectsPanel.Children.Add(button);
            }

            if (combat.Enemy.Weak > 0)
            {
                var button = new Button
                {
                    Width = 40,
                    Height = 55,
                    Margin = new Thickness(3),
                    Content = $"Weak {combat.Enemy.Weak}",
                    BorderThickness = new Thickness(3),
                    BorderBrush = Brushes.LightBlue,
                };
                button.ToolTip = new ToolTip
                {
                    Content = new TextBlock
                    {
                        Text = $"Weak\nDeal 25% less damage with attacks for {combat.Enemy.Weak} turns.",
                        TextWrapping = TextWrapping.Wrap,
                        Width = 200
                    }
                };
                ToolTipService.SetInitialShowDelay(button, 100);
                EnemyEffectsPanel.Children.Add(button);
            }

            if (combat.Enemy.Dexterity != 0)
            {
                var button = new Button
                {
                    Width = 40,
                    Height = 55,
                    Margin = new Thickness(3),
                    Content = $"Dex {combat.Enemy.Dexterity}",
                    BorderThickness = new Thickness(3),
                    BorderBrush = Brushes.Green,
                };
                button.ToolTip = new ToolTip
                {
                    Content = new TextBlock
                    {
                        Text = $"Dexterity\nIncreases block gained from skills by {combat.Enemy.Dexterity}.",
                        TextWrapping = TextWrapping.Wrap,
                        Width = 200
                    }
                };
                ToolTipService.SetInitialShowDelay(button, 100);
                EnemyEffectsPanel.Children.Add(button);
            }

            if (combat.Enemy.Strength != 0)
            {
                var button = new Button
                {
                    Width = 40,
                    Height = 55,
                    Margin = new Thickness(3),
                    Content = $"Str {combat.Enemy.Strength}",
                    BorderThickness = new Thickness(3),
                    BorderBrush = Brushes.Red,
                };
                button.ToolTip = new ToolTip
                {
                    Content = new TextBlock
                    {
                        Text = $"Strength\nIncreases damage dealed by attacks by {combat.Enemy.Strength}.",
                        TextWrapping = TextWrapping.Wrap,
                        Width = 200
                    }
                };
                ToolTipService.SetInitialShowDelay(button, 100);
                EnemyEffectsPanel.Children.Add(button);
            }

            if (combat.Enemy.Poison > 0)
            {
                var button = new Button
                {
                    Width = 40,
                    Height = 55,
                    Margin = new Thickness(3),
                    Content = $"Str {combat.Enemy.Poison}",
                    BorderThickness = new Thickness(3),
                    BorderBrush = Brushes.DarkGreen,
                };
                button.ToolTip = new ToolTip
                {
                    Content = new TextBlock
                    {
                        Text = $"Poison\nDeals {combat.Enemy.Poison} damage at the end of the turn.",
                        TextWrapping = TextWrapping.Wrap,
                        Width = 200
                    }
                };
                ToolTipService.SetInitialShowDelay(button, 100);
                EnemyEffectsPanel.Children.Add(button);
            }
        }
    }
}