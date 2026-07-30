using CardGame.Cards;
using CardGame.Char;
using CardGame.Passives;
using System;
using System.Collections.Generic;
using System.Linq;
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

namespace CardGame
{
    /// <summary>
    /// Логика взаимодействия для RestView.xaml
    /// </summary>
    public partial class RestView : UserControl
    {
        private Player player;
        private List<RestOption> allOptions;
        private Action onRestComplete;
        public RestView()
        {
            InitializeComponent();

            allOptions = new List<RestOption>
            {
                new HealOption(),
                new UpgradeCardOption(this),
                new GiryaOption()
            };

        }
        public void SetOnRestComplete(Action callback)
        {
            onRestComplete = callback;
        }

        public void SetPlayer(Player player)
        {
            this.player = player;
        }

        public void DrawScene(Player player)
        {
            SetPlayer(player);
            UpdateInfo();
            DrawOptions();
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

        private void DrawOptions()
        {
            OptionsPanel.Children.Clear();

            var visibleOptions = allOptions.Where(option => option.IsShown(player)).ToList();

            foreach (var option in visibleOptions) 
            {
                bool canUse = option.CanUse(player);
                var button = CreateButton(option, canUse);
                OptionsPanel.Children.Add(button);
            }
        }

        private Button CreateButton(RestOption option, bool canUse)
        {
            var button = new Button
            {
                Width = 220,
                Height = 80,
                Margin = new Thickness(5),
                Content = $"{option.Name}\n{option.Description}",
                Tag = option,
                IsEnabled = canUse
            };

            if (canUse)
            {
                button.Background = new SolidColorBrush(Color.FromRgb(50, 50, 80));
                button.BorderBrush = new SolidColorBrush(Color.FromRgb(100, 100, 200));
                button.BorderThickness = new Thickness(2);
                button.Click += OptionButton_Click;
            }
            else
            {
                button.Background = new SolidColorBrush(Color.FromRgb(40, 40, 50));
                button.BorderBrush = new SolidColorBrush(Color.FromRgb(60, 60, 70));
                button.BorderThickness = new Thickness(2);
            }

            return button;
        }

        private void OptionButton_Click(object sender, RoutedEventArgs e)
        {
            var button = (Button)sender;
            var option = (RestOption)button.Tag;

            if (option.CanUse(player))
            {
                if (option is UpgradeCardOption)
                {
                    option.Use(player);

                    UpdateInfo();
                    DrawOptions();
                }
                else
                {
                    option.Use(player);

                    UpdateInfo();
                    DrawOptions();
                    onRestComplete?.Invoke();
                }
            }
        }

        public void ShowCardSelection(List<Card> cards)
        {
            CardSelectionOverlay.Visibility = Visibility.Visible;
            UpgradeCardsPanel.Children.Clear();

            foreach (var card in cards)
            {
                var button = CreateCardButton(card);
                UpgradeCardsPanel.Children.Add(button);
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
                Text = card.Upgraded ? "★ UPGRADED" : "Click to upgrade",
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

            // Если карта уже улучшена - отключаем кнопку
            if (card.Upgraded)
            {
                button.IsEnabled = false;
                button.Opacity = 0.5;
            }
            else
            {
                button.Click += (s, e) => ConfirmUpgrade(card);
            }

            // Добавляем тултип с информацией об улучшенной версии
            if (!card.Upgraded)
            {
                var upgradedCard = CreateUpgradedCard(card);
                if (upgradedCard != null)
                {
                    button.ToolTip = new ToolTip
                    {
                        Content = new TextBlock
                        {
                            Text = $"Upgraded version:\n{upgradedCard.Name}\nCost: [{upgradedCard.Cost}]\n{upgradedCard.Description}",
                            TextWrapping = TextWrapping.Wrap,
                            Width = 200,
                            Foreground = Brushes.Gold
                        }
                    };
                }
            }

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
        private Card CreateUpgradedCard(Card original)
        {
            if (original == null) return null;

            // Создаем копию через рефлексию
            var copy = (Card)Activator.CreateInstance(original.GetType());
            copy.Upgrade();
            return copy;
        }

        private void ConfirmUpgrade(Card card)
        {
            // Показываем подтверждение с описанием улучшенной версии
            var upgradedCard = CreateUpgradedCard(card);
            if (upgradedCard != null)
            {
                var result = MessageBox.Show(
                    $"Upgrade '{card.Name}'?\n\nCurrent: {card.Description}\n\nUpgraded: {upgradedCard.Description}\n\nCost: [{card.Cost}] -> [{upgradedCard.Cost}]",
                    "Confirm Upgrade",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question
                );

                if (result == MessageBoxResult.Yes)
                {
                    card.Upgrade();
                    CardSelectionOverlay.Visibility = Visibility.Collapsed;
                    UpdateInfo();
                    DrawOptions();

                    onRestComplete?.Invoke();
                }
            }
        }

        private void CloseCardSelection_Click(object sender, RoutedEventArgs e)
        {
            CardSelectionOverlay.Visibility = Visibility.Collapsed;
        }

        private void BorderCardSelection_Click(object sender, RoutedEventArgs e)
        {
            e.Handled = true;
        }
        private void CancelUpgrade_Click(object sender, RoutedEventArgs e)
        {
            CardSelectionOverlay.Visibility = Visibility.Collapsed;
        }
    }

    public abstract class RestOption
    {
        public string Name;
        public string Description;


        public virtual bool IsShown(Player player)
        {
            return true;
        }

        public virtual bool CanUse(Player player)
        {
            return true;
        }

        public abstract void Use(Player player);
    }

    public class HealOption : RestOption
    {
        public HealOption()
        {
            Name = "Heal";
            Description = "Heal 30% of max HP";
        }

        public override bool CanUse(Player player)
        {
            return true;
        }

        public override void Use(Player player)
        {
            int heal = (int)(player.MaxHp * 0.3);
            player.Heal(heal);
        }
    }

    public class UpgradeCardOption : RestOption
    {
        private RestView restView;
        public UpgradeCardOption(RestView view)
        {
            restView = view;
            Name = "Upgrade card";
            Description = "Upgrade a card in your deck";
        }

        public override bool CanUse(Player player)
        {
            return player.Deck.Any(card => !card.Upgraded);
        }

        public override void Use(Player player)
        {
            var upgradableCards = player.Deck
                .Where(card => !card.Upgraded)
                .ToList();

            if (upgradableCards.Count == 0) return;

            if (upgradableCards.Count == 1)
            {
                // Если только одна карта - сразу апгрейдим
                var result = MessageBox.Show(
                    $"Upgrade '{upgradableCards[0].Name}'?\n\nCurrent: {upgradableCards[0].Description}",
                    "Confirm Upgrade",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question
                );

                if (result == MessageBoxResult.Yes)
                {
                    upgradableCards[0].Upgrade();
                }
            }
            else
            {
                // Показываем оверлей для выбора
                restView.ShowCardSelection(upgradableCards);
            }
        }
    }

    public class GiryaOption : RestOption
    {
        public GiryaOption()
        {
            Name = "Lift";
            Description = "Get permanent +1 Strength";
        }

        public override bool IsShown(Player player)
        {
            if (player.Passives.Any(p => p.Name == "Girya")) return true;
            else return false;
        }

        public override bool CanUse(Player player)
        {
            foreach (var passive in player.Passives)
            {
                if (passive is Girya girya && girya.strength < 3)
                {
                    return true;
                }
            }
            return false;
        }

        public override void Use(Player player)
        {
            foreach (var passive in player.Passives)
            {
                if (passive is Girya girya && girya.strength < 3)
                {
                    girya.RestOptionUse();
                }
            }
        }
    }

}
