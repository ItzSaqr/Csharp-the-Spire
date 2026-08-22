using CardGame.Char;
using CardGame.Passives;
using CardGame.Rewards;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace CardGame.WPF
{

    public partial class TreasureView : UserControl
    {
        private Player player;
        private Action onTreasureComplete;
        private RewardGenerator rewardGenerator;
        private List<PassiveEffect> generatedRelics;

        public TreasureView()
        {
            InitializeComponent();

            rewardGenerator = new RewardGenerator();
            generatedRelics = new List<PassiveEffect>();
        }

        public void SetPlayer(Player player)
        {
            this.player = player;
        }

        public void SetOnTreasureComplete(Action callback)
        {
            onTreasureComplete = callback;
        }

        public void GenerateTreasure()
        {
            UpdateInfo();
            RelicsPanel.Children.Clear();
            generatedRelics.Clear();

            int relicCount = 1;
            generatedRelics = rewardGenerator.GenerateRandomRelicsFromPool(relicCount);

            int gold = Random.Shared.Next(100, 151);
            var btn = CreateGoldButton(gold);
            RelicsPanel.Children.Add(btn);

            foreach (var relic in generatedRelics)
            {
                var relicButton = CreateRelicButton(relic);
                RelicsPanel.Children.Add(relicButton);
            }

            ContinueButton.Visibility = Visibility.Visible;
        }

        private Button CreateRelicButton(PassiveEffect relic)
        {
            var button = new Button
            {
                Width = 160,
                Height = 220,
                Margin = new Thickness(10),
                BorderThickness = new Thickness(3),
                Background = new SolidColorBrush(Color.FromRgb(30, 30, 50)),
                Cursor = System.Windows.Input.Cursors.Hand
            };

            button.BorderBrush = GetRelicBorderColor(relic.Type);

            var grid = new Grid
            {
                RowDefinitions =
                {
                    new RowDefinition { Height = new GridLength(40) },
                    new RowDefinition { Height = new GridLength(1, GridUnitType.Star) },
                    new RowDefinition { Height = new GridLength(30) }
                }
            };

            // Название
            var nameText = new TextBlock
            {
                Text = relic.Name,
                FontSize = 18,
                FontWeight = FontWeights.Bold,
                Foreground = Brushes.White,
                TextAlignment = TextAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(5)
            };
            Grid.SetRow(nameText, 0);
            grid.Children.Add(nameText);

            // Описание
            var descText = new TextBlock
            {
                Text = relic.Description,
                FontSize = 13,
                Foreground = new SolidColorBrush(Color.FromRgb(180, 180, 200)),
                TextWrapping = TextWrapping.Wrap,
                TextAlignment = TextAlignment.Center,
                Margin = new Thickness(5),
                VerticalAlignment = VerticalAlignment.Center
            };
            Grid.SetRow(descText, 1);
            grid.Children.Add(descText);

            button.Content = grid;

            button.Click += (s, e) => TakeRelic(relic, button);

            return button;
        }

        private void TakeRelic(PassiveEffect relic, Button button)
        {
            if (player == null) return;
            player.Passives.Add(relic);
            button.IsEnabled = false;
            button.Opacity = 0.5;
        }

        private Brush GetRelicBorderColor(PassiveType type)
        {
            return type switch
            {
                PassiveType.CommonRelic => new SolidColorBrush(Color.FromRgb(100, 100, 100)),
                PassiveType.UncommonRelic => new SolidColorBrush(Color.FromRgb(50, 150, 50)),
                PassiveType.RareRelic => new SolidColorBrush(Color.FromRgb(200, 180, 50)),
                _ => new SolidColorBrush(Color.FromRgb(100, 100, 100))
            };
        }

        private void ContinueButton_Click(object sender, RoutedEventArgs e)
        {
            onTreasureComplete?.Invoke();
        }

        private Button CreateGoldButton(int goldAmount)
        {
            var button = new Button
            {
                Width = 160,
                Height = 220,
                Margin = new Thickness(10),
                BorderThickness = new Thickness(3),
                Background = new SolidColorBrush(Color.FromRgb(30, 30, 50)),
                Cursor = System.Windows.Input.Cursors.Hand
            };

            button.BorderBrush = new SolidColorBrush(Color.FromRgb(255, 215, 0));

            var grid = new Grid
            {
                RowDefinitions =
                {
                    new RowDefinition { Height = new GridLength(40) },
                    new RowDefinition { Height = new GridLength(1, GridUnitType.Star) },
                    new RowDefinition { Height = new GridLength(30) }
                }
            };

            // Название
            var nameText = new TextBlock
            {
                Text = "Gold",
                FontSize = 18,
                FontWeight = FontWeights.Bold,
                Foreground = Brushes.White,
                TextAlignment = TextAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(5)
            };
            Grid.SetRow(nameText, 0);
            grid.Children.Add(nameText);

            // Описание
            var descText = new TextBlock
            {
                Text = goldAmount.ToString(),
                FontSize = 20,
                Foreground = new SolidColorBrush(Color.FromRgb(180, 180, 200)),
                TextWrapping = TextWrapping.Wrap,
                TextAlignment = TextAlignment.Center,
                Margin = new Thickness(5),
                VerticalAlignment = VerticalAlignment.Center
            };
            Grid.SetRow(descText, 1);
            grid.Children.Add(descText);

            button.Content = grid;

            button.Click += (s, e) => TakeGold(goldAmount, button);

            return button;
        }

        private void TakeGold(int goldAmount, Button button)
        {
            if (player == null) return;
            player.Gold += goldAmount;
            button.IsEnabled = false;
            button.Opacity = 0.5;
            UpdateInfo();
        }

        private void UpdateInfo()
        {
            if (player == null)
            {
                HpText.Text = "HP: --/--";
                GoldText.Text = "💰 --";
                return;
            }

            HpText.Text = $"HP: {player.Hp}/{player.MaxHp}";
            GoldText.Text = $"💰 {player.Gold}";
        }
    }
}
