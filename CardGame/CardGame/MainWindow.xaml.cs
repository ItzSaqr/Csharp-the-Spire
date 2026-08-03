using CardGame.Char;
using CardGame.Cards;
using CardGame.Enemies;
using CardGame.CombatNamespace;
using CardGame.Passives;
using CardGame.Map;
using CardGame.Rewards;
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
    public class Game
    {
        public Player Player;
        public GameMap Map;
        public MapNode CurrentNode;
        public Combat? Combat;
        private MainWindow mainWindow;

        public void StartRun()
        {
            Player = new Player { Hp = 80, MaxHp = 80, MaxEnergy = 3 };

            for (int i = 0; i < 5; i++) Player.Deck.Add(new Strike());
            for (int i = 0; i < 5; i++) Player.Deck.Add(new Defend());
            Player.Deck.Add(new Bash());

            Map = new MapGenerator().Generate();

            // Устанавливаем начальный узел как доступный

            var startFloor = Map.Floors[0];
            foreach (var node in startFloor)
            {
                if (node.Active)
                {
                    node.Available = true;
                }
            }
        }

        public void setMainWindow(MainWindow window)
        {
            mainWindow = window;
        }

        public void EnterNode(MapNode node)
        {
            CurrentNode = node;
            node.Visited = true;
            node.Available = false;

            // открыть следующие узлы за завершенным
            foreach (var next in node.Next)
            {
                next.Available = true;
            }

            switch (node.Type)
            {
                case NodeType.BasicEnemy:
                    StartCombat(new Snake(), CombatType.Basic);
                    mainWindow?.SwitchToCombat();
                    break;
                case NodeType.EliteEnemy:
                    StartCombat(new Gremlin(), CombatType.Elite);
                    mainWindow?.SwitchToCombat();
                    break;
                case NodeType.Boss:
                    StartCombat(new Gremlin(), CombatType.Boss);
                    mainWindow?.SwitchToCombat();
                    break;
                case NodeType.RestSite:
                    mainWindow?.SwitchToCampfire();
                    break;
                case NodeType.Shop:
                    // TODO: Показать магазин
                    break;
                case NodeType.Treasure:
                    // TODO: Показать сундук
                    break;
            }

            foreach (var nod in Map.Floors[node.Floor])
            {
                nod.Available = false;
            }
        }

        public void StartCombat(Enemy enemy, CombatType type)
        {
            Combat = new Combat(Player, enemy, new ConsoleCombatUI());
        }

        public void ShowCampfire()
        {
            mainWindow?.SwitchToCampfire();
        }
    }
    public partial class MainWindow : Window
    {
        public Combat combat;
        private Game game;
        private MapView mapView;
        private RestView restView;

        public MainWindow()
        {
            InitializeComponent();

            StartGame();
        }

        private void StartGame()
        {
            game = new Game();
            game.setMainWindow(this);
            game.StartRun();

            restView = new RestView();
            restView.SetPlayer(game.Player);
            RestContent.Content = restView;

            ShowMap();
        }

        public void ShowMap()
        {
            MapUI.Visibility = Visibility.Visible;
            CombatUI.Visibility = Visibility.Collapsed;
            RestUI.Visibility = Visibility.Collapsed;

            MapContent.Content = new MapView(game.Map, game, this);
        }

        public void SwitchToCombat()
        {
            MapUI.Visibility = Visibility.Collapsed;
            RestUI.Visibility = Visibility.Collapsed;
            CombatUI.Visibility = Visibility.Visible;

            combat = game.Combat;
            UpdateUI();
        }

        public void SwitchToMap()
        {
            CombatUI.Visibility = Visibility.Collapsed;
            RestUI.Visibility = Visibility.Collapsed;
            MapUI.Visibility = Visibility.Visible;

            MapContent.Content = new MapView(game.Map, game, this);
        }

        public void SwitchToCampfire()
        {
            MapUI.Visibility = Visibility.Collapsed;
            CombatUI.Visibility = Visibility.Collapsed;
            RestUI.Visibility = Visibility.Visible;

            restView.SetPlayer(game.Player);
            restView.SetOnRestComplete(SwitchToMap);
            restView.DrawScene(game.Player);
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

            if (combat.State == CombatState.Victory)
            {
                ShowReward();
            }
            else if (combat.State == CombatState.Defeat)
            {
                MessageBox.Show("You died!");
            }
        }

        private void EndTurn_Click(object sender, RoutedEventArgs e)
        {
            combat.EndPlayerTurn();
            UpdateUI();

            if (combat.State == CombatState.Victory)
            {
                ShowReward();
            }
            else if (combat.State == CombatState.Defeat)
            {
                MessageBox.Show("You died!");
            }
        }

        private void ShowReward()
        {
            var generator = new RewardGenerator();
            var reward = generator.Generate(combat.Enemy switch
            {
                Snake => CombatType.Basic,
                Gremlin => CombatType.Elite,
                _ => CombatType.Basic
            });

            RewardOverlay.Visibility = Visibility.Visible;

            RewardPanel.Children.Clear();

            var goldButton = new Button
            {
                Width = 140,
                Height = 200,
                Margin = new Thickness(8),
                Content = $"Gold: {reward.Gold}",
                BorderThickness = new Thickness(3),
                BorderBrush = Brushes.Gold,
                Background = new SolidColorBrush(Color.FromRgb(30, 30, 50)),
                Foreground = Brushes.White,
                FontSize = 16,
                FontWeight = FontWeights.Bold
            };
            int goldAmount = reward.Gold;

            goldButton.Click += (s, e) =>
            {
                RewardPanel.Children.Remove(goldButton);
                TakeGold_Click(goldAmount);
            };
            RewardPanel.Children.Add(goldButton);

            var cardButton = new Button
            {
                Width = 140,
                Height = 200,
                Margin = new Thickness(8),
                Content = "Card Choice",
                BorderThickness = new Thickness(3),
                BorderBrush = Brushes.Purple,
                Background = new SolidColorBrush(Color.FromRgb(30, 30, 50)),
                Foreground = Brushes.White,
                FontSize = 16,
                FontWeight = FontWeights.Bold
            };

            cardButton.Click += (s, e) =>
            {
                ShowCardChoice(reward.CardChoices);
            };

            RewardPanel.Children.Add(cardButton);

            if (reward.Relics.Count != 0)
            {
                foreach (var relic in reward.Relics)
                {
                    var relicButton = new Button
                    {
                        Width = 140,
                        Height = 200,
                        Margin = new Thickness(8),
                        Content = $"Relic: {relic.Name}",
                        BorderThickness = new Thickness(3),
                        BorderBrush = Brushes.Purple,
                        Background = new SolidColorBrush(Color.FromRgb(30, 30, 50)),
                        Foreground = Brushes.White,
                        FontSize = 16,
                        FontWeight = FontWeights.Bold
                    };

                    relicButton.Click += (s, e) =>
                    {
                        RewardPanel.Children.Remove(relicButton);
                        TakeRelic_Click(relic);
                    };
                }
            }
        }
        
        private void SkipReward_Click(object sender, RoutedEventArgs e)
        {
            RewardOverlay.Visibility = Visibility.Collapsed;
            SwitchToMap();
        }

        private void TakeGold_Click(int gold)
        {
            game.Player.Gold += gold;
            CheckAllRewardsTaken();
        }

        private void TakeRelic_Click(PassiveEffect relic)
        {
            game.Player.Passives.Add(relic);
            CheckAllRewardsTaken();
        }

        private void ShowCardChoice(List<Card> cards)
        {
            CardChoiceOverlay.Visibility = Visibility.Visible;
            CardChoicePanel.Children.Clear();

            foreach (var card in cards)
            {
                var button = CreateCardButton(card);
                button.Click += (s, e) => SelectCard(card);
                CardChoicePanel.Children.Add(button);
            }
        }

        private void SelectCard(Card selectedCard)
        {
            game.Player.Deck.Add(selectedCard);

            CardChoiceOverlay.Visibility = Visibility.Collapsed;

            Button cardChoiceButton = null;
            foreach (var child in RewardPanel.Children)
            {
                if (child is Button btn && btn.Content.ToString() == "Card Choice")
                {
                    cardChoiceButton = btn;
                    break;
                }
            }

            if (cardChoiceButton != null)
            {
                RewardPanel.Children.Remove(cardChoiceButton);
            }

            CheckAllRewardsTaken();
        }

        private void CheckAllRewardsTaken()
        {
            if (RewardPanel.Children.Count == 0)
            {
                SkipRewardButton.Content = "Continue";
            }
        }

        private void ViewPile(List<Card> cards)
        {
            PilePanel.Children.Clear();
            foreach (var card in cards)
            {
                var button = CreateCardButton(card);
                PilePanel.Children.Add(button);
            }

            PileOverlay.Visibility = Visibility.Visible;
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

        private void CancelCardChoice_Click(object sender, RoutedEventArgs e)
        {
            CardChoiceOverlay.Visibility = Visibility.Collapsed;
        }

        private void BorderPile_Click(object sender, RoutedEventArgs e)
        {
            e.Handled = true; 
        }

        enum GameState
        {
            Map,
            Combat,
            Reward,
            Shop,
            Campfire,
            Gameover
        }

        private void DrawHand()
        {
            HandPanel.Children.Clear();
            foreach (var card in combat.Player.Hand)
            {
                var button = CreateCardButton(card);
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

        private Button CreateCardButton(Card card)
        {
            var button = new Button
            {
                Width = 140,
                Height = 200,
                Margin = new Thickness(8),
                Tag = card,
                BorderThickness = new Thickness(3),
                BorderBrush = GetCardBorderColor(card.Type),
                Background = new SolidColorBrush(Color.FromRgb(30, 30, 50)),
                Cursor = System.Windows.Input.Cursors.Hand
            };

            // Создаем контент карты
            var grid = new Grid
            {
                RowDefinitions =
                {
                    new RowDefinition { Height = new GridLength(30) },
                    new RowDefinition { Height = new GridLength(25) },
                    new RowDefinition { Height = new GridLength(1, GridUnitType.Star) },
                    new RowDefinition { Height = new GridLength(25) }
                },
                ColumnDefinitions =
                {
                    new ColumnDefinition { Width = GridLength.Auto },
                    new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) }
                }
            };

            // Стоимость
            var costText = new TextBlock
            {
                Text = $"[{card.Cost}]",
                FontSize = 18,
                FontWeight = FontWeights.Bold,
                Foreground = Brushes.White,
                Margin = new Thickness(5, 0, 0, 0)
            };
            Grid.SetRow(costText, 0);
            Grid.SetColumn(costText, 0);
            grid.Children.Add(costText);

            // Название
            var nameText = new TextBlock
            {
                Text = card.Name,
                FontSize = 16,
                FontWeight = FontWeights.SemiBold,
                Foreground = Brushes.White,
                TextAlignment = TextAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center
            };
            Grid.SetRow(nameText, 1);
            Grid.SetColumn(nameText, 0);
            Grid.SetColumnSpan(nameText, 2);
            grid.Children.Add(nameText);

            // Описание
            var descText = new TextBlock
            {
                Text = card.Description,
                FontSize = 12,
                Foreground = new SolidColorBrush(Color.FromRgb(180, 180, 200)),
                TextWrapping = TextWrapping.Wrap,
                TextAlignment = TextAlignment.Center,
                Margin = new Thickness(5),
                VerticalAlignment = VerticalAlignment.Center
            };
            Grid.SetRow(descText, 2);
            Grid.SetColumn(descText, 0);
            Grid.SetColumnSpan(descText, 2);
            grid.Children.Add(descText);

            // Индикатор улучшения
            var upgradeText = new TextBlock
            {
                Text = card.Upgraded ? "★ UPGRADED" : "",
                FontSize = 11,
                Foreground = card.Upgraded ? Brushes.Gold : new SolidColorBrush(Color.FromRgb(100, 200, 100)),
                TextAlignment = TextAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center,
                FontWeight = FontWeights.SemiBold
            };
            Grid.SetRow(upgradeText, 3);
            Grid.SetColumn(upgradeText, 0);
            Grid.SetColumnSpan(upgradeText, 2);
            grid.Children.Add(upgradeText);

            button.Content = grid;

            return button;
        }

        private Brush GetCardBorderColor(CardType type)
        {
            return type switch
            {
                CardType.Attack => new SolidColorBrush(Color.FromRgb(200, 60, 60)),
                CardType.Skill => new SolidColorBrush(Color.FromRgb(60, 100, 200)),
                CardType.Power => new SolidColorBrush(Color.FromRgb(200, 180, 60)),
                _ => new SolidColorBrush(Color.FromRgb(100, 100, 100))
            };
        }
    }
}