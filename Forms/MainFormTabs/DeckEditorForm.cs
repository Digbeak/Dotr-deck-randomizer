using DOTR_MODDING_TOOL.Classes;
using DOTR_MODDING_TOOL.Properties;
using Equin.ApplicationFramework;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DOTR_Modding_Tool
{

    public partial class MainForm : Form
    {
        private List<Deck> deckList;
        private BindingSource deckListBinding = new BindingSource();
        private BindingListView<CardConstant> trunkCardConstantBinding;
        private SortableBindingList<DeckCard> deckCardListBinding;

        private List<int> bannedCardIds = new List<int>();/*{ 108, 146, 326, 402, 403, 418, 427, 428, 429, 528, 639, 659, 668, 671, 798 }*/
        private int[] bannedRitualIds = new int[] { 832, 833, 834, 837, 840, 842, 846, 849, 850, 853 };
        private float deckStrengthMultiplier = 1.0f;
        private List<int> powerupCardIds = new List<int>();

        Random rand = new Random();

        private List<List<CardConstant>> themes = new List<List<CardConstant>>();
        private List<CardConstant> dragons = new List<CardConstant>();
        private List<CardConstant> spellcasters = new List<CardConstant>();
        private List<CardConstant> zombies = new List<CardConstant>();
        private List<CardConstant> warriors = new List<CardConstant>();
        private List<CardConstant> beastwarriors = new List<CardConstant>();
        private List<CardConstant> beasts = new List<CardConstant>();
        private List<CardConstant> wingedbeasts = new List<CardConstant>();
        private List<CardConstant> fiends = new List<CardConstant>();
        private List<CardConstant> fairies = new List<CardConstant>();
        private List<CardConstant> insects = new List<CardConstant>();
        private List<CardConstant> dinosaurs = new List<CardConstant>();
        private List<CardConstant> reptiles = new List<CardConstant>();
        private List<CardConstant> fishes = new List<CardConstant>();
        private List<CardConstant> seaserpents = new List<CardConstant>();
        private List<CardConstant> machines = new List<CardConstant>();
        private List<CardConstant> thunders = new List<CardConstant>();
        private List<CardConstant> aquas = new List<CardConstant>();
        private List<CardConstant> pyros = new List<CardConstant>();
        private List<CardConstant> rocks = new List<CardConstant>();
        private List<CardConstant> plants = new List<CardConstant>();

        private void setupDeckEditorTab()
        {
            SetupDeckarchetypes();
            trunkCardConstantBinding = new BindingListView<CardConstant>(CardConstant.List);
            setupDeckEditDataGridView();
            deckEditRemoveSelectedMenuItem.Click += DeckEditRemoveSelectedMenuItem_Click;
            loadDeckData();
            LoadDeckTypeData();
            setUpBanList();
            setUpThemes();
            deckEditorDataGridView.CellDoubleClick += deckEditDataGridView_DoubleClick;
            trunkDataGridView.CellDoubleClick += trunkDataGridView_DoubleClick;
            trunkContextMenuStrip.Opening += TrunkContextMenuStrip_Opening;
        }

        private void TrunkContextMenuStrip_Opening(object sender, CancelEventArgs e)
        {
            DataGridViewSelectedRowCollection selectedRows = trunkDataGridView.SelectedRows;

            trunkContextMenuStrip.Items[0].Enabled = selectedRows.Count == 1;
        }

        private void loadDeckData()
        {
            byte[][][] deckBytes = dataAccess.LoadDecks();
            deckList = Deck.LoadDeckListFromBytes(deckBytes);
            deckListBinding.DataSource = deckList;
            deckDropdown.DataSource = deckListBinding;

            deckEditDeckLeaderRankComboBox.DataSource = DeckLeaderRank.RankList();
            deckEditDeckLeaderRankComboBox.SelectedIndex = ((Deck)deckDropdown.SelectedItem).DeckLeader.Rank.Index;

            refreshDeckInfoLabels();
        }

        private void LoadDeckTypeData()
        {
            List<DeckType> deckTypes = new List<DeckType>();

            deckTypes.Add(new DeckType(0, "random"));
            deckTypes.Add(new DeckType(1, "themed"));
            deckTypes.Add(new DeckType(2, "averaged"));
            deckTypes.Add(new DeckType(3, "completely random"));
            deckTypes.Add(new DeckType(4, "balanced"));

            deckTypeComboBox.DataSource = deckTypes;

            SetupPowerupCards();
        }

        private void formatCardTable(DataGridView table)
        {
            table.DataBindingComplete += this.FormatCardConstantTable;
            table.DefaultCellStyle.Font = new Font("OpenSans", 9.75F, FontStyle.Regular);
            table.AutoGenerateColumns = false;
            table.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            MainForm.EnableDoubleBuffering(table);
        }

        private void setupDeckEditDataGridView()
        {
            this.formatCardTable(this.trunkDataGridView);
            this.formatCardTable(this.deckEditorDataGridView);
            this.trunkDataGridView.DataSource = trunkCardConstantBinding;
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            Deck selectedDeck = (Deck)deckDropdown.SelectedItem;
            deckCardListBinding = new SortableBindingList<DeckCard>(selectedDeck.CardList);
            deckEditorDataGridView.DataSource = deckCardListBinding;
            deckEditDeckLeaderRankComboBox.SelectedValue = ((Deck)deckDropdown.SelectedItem).DeckLeader.Rank.Index;
            refreshDeckInfoLabels();
        }

        private void applyTrunkFilter()
        {
            string searchTerm = trunkFilterTextBox.Text.ToLower().Trim();

            if (searchTerm == string.Empty)
            {
                trunkCardConstantBinding.RemoveFilter();
                return;
            }

            trunkCardConstantBinding.ApplyFilter(delegate (CardConstant cardConstant) { return cardConstant.Name.ToLower().Contains(searchTerm); });
        }

        private void trunkSearchButton_Click(object sender, EventArgs e)
        {
            applyTrunkFilter();
        }

        private void trunkClearButton_Click(object sender, EventArgs e)
        {
            trunkFilterTextBox.Clear();
            trunkCardConstantBinding.RemoveFilter();
        }

        private void trunkFilterTextbox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                applyTrunkFilter();
                e.Handled = true;
                e.SuppressKeyPress = true;
            }
        }

        private void deckEditDataGridView_DoubleClick(Object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0)
            {
                return;
            }

            //DeckCard deckCard = (DeckCard)deckCardListBinding[e.RowIndex];
            //deckCardListBinding.Remove(deckCard);
            removeAllCards();
            refreshDeckInfoLabels();
        }

        private void removeAllCards()
        {
            for (int i = 0; i < 40; i++)
            {
                DeckCard deckCard = (DeckCard)deckCardListBinding[0];
                deckCardListBinding.Remove(deckCard);
            }
        }
        private void refreshDeckCardCountLabel()
        {
            Deck deck = (Deck)deckDropdown.SelectedItem;
            deckCardCountLabel.Text = $"Cards: {deck.CardList.Count}/40";

            if (deck.CardList.Count == 40)
            {
                deckCardCountLabel.ForeColor = Color.Black;
            }
            else
            {
                deckCardCountLabel.ForeColor = Color.Red;
            }
        }

        private void refreshDeckCostLabel()
        {
            Deck deck = (Deck)deckDropdown.SelectedItem;
            deckEditDeckCostLabel.Text = $"{deck.DeckCost} DC";
        }

        private void refreshDeckInfoLabels()
        {
            refreshDeckCardCountLabel();
            refreshDeckCostLabel();
        }

        private void DeckEditRemoveSelectedMenuItem_Click(object sender, EventArgs e)
        {
            //foreach (DataGridViewRow row in deckEditorDataGridView.SelectedRows)
            //{
            //    DeckCard deckCard = (DeckCard)row.DataBoundItem;
            //    deckCardListBinding.Remove(deckCard);
            //}

            //refreshDeckInfoLabels();
            EmptyDeck();
        }

        private void deckEditSaveButton_Click(object sender, EventArgs e)
        {
            try
            {
                saveDeck();
                MessageBox.Show("Deck saved.", "Save successful");
            }
            catch (InvalidOperationException error)
            {
                MessageBox.Show(error.Message, "Error");
            }

        }
        private void saveDeck()
        {
            Deck deck = (Deck)deckDropdown.SelectedItem;

            deck.Save(dataAccess);
            deckCardListBinding.ResetBindings();
        }

        private void deckEditDeckLeaderRankComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Prevent invalid setting of rank, usually happens on form load when SelectedIndex is initially null
            if (deckEditDeckLeaderRankComboBox.SelectedIndex < 1)
            {
                return;
            }

            Deck selectedDeck = (Deck)deckDropdown.SelectedItem;
            selectedDeck.DeckLeader.Rank = new DeckLeaderRank(deckEditDeckLeaderRankComboBox.SelectedIndex);
        }
        private void deckTypeComboBoxSelectedValueChanged(object sender, EventArgs e)
        {
            if (deckTypeComboBox.SelectedIndex == 0)
            {
                deckStrengthMultiplierTrackBar.Show();
                deckStrengthMultiplierTrackBarLabel.Show();
                averageDeckCost.Hide();
                averageDeckCostLabel.Hide();
            }
            if (deckTypeComboBox.SelectedIndex == 1)
            {
                deckStrengthMultiplierTrackBar.Show();
                deckStrengthMultiplierTrackBarLabel.Show();
                averageDeckCost.Hide();
                averageDeckCostLabel.Hide();
            }
            if (deckTypeComboBox.SelectedIndex == 2)
            {
                deckStrengthMultiplierTrackBar.Hide();
                deckStrengthMultiplierTrackBarLabel.Hide();
                averageDeckCost.Show();
                averageDeckCostLabel.Show();
            }
            if (deckTypeComboBox.SelectedIndex == 3)
            {
                deckStrengthMultiplierTrackBar.Hide();
                deckStrengthMultiplierTrackBarLabel.Hide();
                averageDeckCost.Hide();
                averageDeckCostLabel.Hide();
            }
            if (deckTypeComboBox.SelectedIndex == 4)
            {
                deckStrengthMultiplierTrackBar.Hide();
                deckStrengthMultiplierTrackBarLabel.Hide();
                averageDeckCost.Hide();
                averageDeckCostLabel.Hide();
            }
        }
        private void randomizeAllDecks(object sender, EventArgs e)
        {
            for (int i = 0; i < 17; i++)
            {
                Deck selectedDeck = (Deck)deckDropdown.Items[i];
                deckCardListBinding = new SortableBindingList<DeckCard>(selectedDeck.CardList);
                deckEditorDataGridView.DataSource = deckCardListBinding;
                deckEditDeckLeaderRankComboBox.SelectedValue = ((Deck)deckDropdown.SelectedItem).DeckLeader.Rank.Index;
                removeAllCards();
                NewDeck();
                deckDropdown.SelectedItem = selectedDeck;
                RandomizeDeckLeader();
                saveDeck();
            }
            refreshDeckInfoLabels();
        }
        private void averageDeckCostChanges(object sender, EventArgs e)
        {
            averageDeckCostLabel.Text = "Deck cost: " + CalculateAverageDeckCost().ToString();
        }
        private void deckStrengthValueChanged(object sender, EventArgs e)
        {
            deckStrengthMultiplier = CalculateDeckStrengthMultiplier();
            deckStrengthMultiplierTrackBarLabel.Text = "Deckstrength multiplier: " + CalculateDeckStrengthMultiplier();
        }
        private void setUpBanList()
        {
            var path = Path.Combine(Directory.GetCurrentDirectory(), "Banlist.txt");
            Console.WriteLine(path);
            string[] lines = File.ReadAllLines(path);
            for (int i = 0; i < lines.Length; i++)
            {
                bannedCardIds.Add(Int32.Parse(lines[i]));
            }
        }
        private void setUpThemes()
        {
            var path = Path.Combine(Directory.GetCurrentDirectory(), "Themes.txt");
            string [] lines = File.ReadAllLines(path);
            for(int i = 0; i < lines.Length; i++)
            {
                themes.Add(new List<CardConstant>());
                string[] ids = lines[i].Split(',');
                for (int x = 0; x < ids.Length; x++)
                {
                    int cardId = -1;
                    int.TryParse(ids[x], out cardId);

                    if(cardId > -1)
                    {
                        themes[i].Add(CardConstant.List[cardId]);
                    }
                }
            }
        }
        private void LoadDefaultThemes()
        {
            themes[0] = new List<CardConstant>();
            for(int i = 0; i < dragons.Count; i++)
            {
                themes[0].Add(dragons[i]);
            }
            themes[1] = new List<CardConstant>();
            for (int i = 0; i <spellcasters.Count; i++)
            {
                themes[1].Add(spellcasters[i]);
            }
            themes[2] = new List<CardConstant>();
            for (int i = 0; i < zombies.Count; i++)
            {
                themes[2].Add(zombies[i]);
            }
            themes[3] = new List<CardConstant>();
            for (int i = 0; i < warriors.Count; i++)
            {
                themes[3].Add(warriors[i]);
            }
            themes[4] = new List<CardConstant>();
            for (int i = 0; i < beastwarriors.Count; i++)
            {
                themes[4].Add(beastwarriors[i]);
            }
            themes[5] = new List<CardConstant>();
            for (int i = 5; i < beasts.Count; i++)
            {
                themes[5].Add(beasts[i]);
            }
            themes[6] = new List<CardConstant>();
            for (int i = 0; i < wingedbeasts.Count; i++)
            {
                themes[6].Add(wingedbeasts[i]);
            }
            themes[7] = new List<CardConstant>();
            for (int i = 0; i < fiends.Count; i++)
            {
                themes[7].Add(fiends[i]);
            }

            var path = "";
#if DEBUG
            path = @"../../Themes.txt";

#else
            path = Path.Combine(Directory.GetCurrentDirectory(), "Themes.txt");
#endif
            List<string> l = themes.Select(x => x.ToString()).ToList();
            string[] lines = l.ToArray();
#if DEBUG
            File.WriteAllLines(@"../../Themes.txt", lines);
#else
            File.WriteAllLines(path, lines);
#endif
        }
        private void saveBanList(object sender, EventArgs e)
        {
            var path = "";
#if DEBUG
            path = @"../../Banlist.txt";
            
#else
            path = Path.Combine(Directory.GetCurrentDirectory(), "Banlist.txt");
#endif
            Console.WriteLine(path);
            List<string> l = bannedCardIds.Select(x => x.ToString()).ToList();
            string[] lines = l.ToArray();
#if DEBUG
            File.WriteAllLines(@"../../Banlist.txt", lines);
#else
            File.WriteAllLines(path, lines);
#endif
        }
        private void makeDeckLeaderToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CardConstant selectedCard = ((ObjectView<CardConstant>)trunkDataGridView.SelectedRows[0].DataBoundItem).Object;
            Deck deck = (Deck)deckDropdown.SelectedItem;

            deck.DeckLeader = new DeckCard(selectedCard, deck.DeckLeader.Rank);
            deckListBinding.ResetBindings(false);
        }
        private void trunkDataGridView_DoubleClick(Object sender, DataGridViewCellEventArgs e)
        {
            // avoid errors when doubleclicking column headers
            if (e.RowIndex < 0)
            {
                return;
            }

            List<DataGridViewRow> rows = new List<DataGridViewRow> { trunkDataGridView.Rows[e.RowIndex] };
            //addSingleCardToDeck(rows);
            NewDeck();

            RandomizeDeckLeader();

        }
        private void addSelectedCardsToDeckToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DataGridViewSelectedRowCollection selectedRows = trunkDataGridView.SelectedRows;
            List<DataGridViewRow> rows = new List<DataGridViewRow> { };

            foreach (DataGridViewRow row in selectedRows)
            {
                rows.Add(row);
            }

            //addCardsToDeck(rows);
            NewDeck();
        }
        private void addSingleCardToDeck(List<DataGridViewRow> rows)
        {
            foreach (DataGridViewRow row in rows)
            {
                //Random rand = new Random();
                //int i = rand.Next(0, CardConstant.List.Count);
                //Console.WriteLine(i);
                CardConstant cardConstant = /*CardConstant.List[i];*/((ObjectView<CardConstant>)row.DataBoundItem).Object;
                DeckLeaderRank rank = new DeckLeaderRank((int)DeckLeaderRankType.NCO);
                DeckCard deckCard = new DeckCard(cardConstant, rank);
                deckCardListBinding.Add(deckCard);
            }
            refreshDeckInfoLabels();

        }
        private void NewDeck()
        {
            DeckType d = (DeckType)deckTypeComboBox.SelectedItem;
            if (d.Index == 0)
                AddRandomDeck();
            else if (d.Index == 1)
                AddThemedDeck();
            else if (d.Index == 2)
                AddWeakDeck();
            else if (d.Index == 3)
                AddCompletelyRandomDeck();
            else if (d.Index == 4)
            {
                AddBalancedDeck();

                while (GetDeckCost() > 850)
                {
                    Console.WriteLine(GetDeckCost());
                    removeAllCards();
                    AddBalancedDeck();
                }
            }
        }
        private void AddRandomDeck()
        {
            Random rand = new Random();
            CardConstant card;

            while (deckCardListBinding.Count < 40)
            {
                DeckLeaderRank rank = new DeckLeaderRank((int)DeckLeaderRankType.NCO);
                do
                {
                    card = GetRandomCardFromList(rand, CardConstant.List);
                } while (GetAmountOfCardDuplicatesInDeck(card.Index) >= 3 || IsCardBanned(card.Index) || (IsCardNonMonster(card) && IsDeckCapedOnNonMonsters()));

                DeckCard deckCard = new DeckCard(card, rank);
                deckCardListBinding.Add(deckCard);

                refreshDeckInfoLabels();
            }
        }
        private void AddThemedDeck()
        {
            Random rand = new Random();
            CardConstant card;
            List<CardConstant> monsterList = GetRandomMonsterList();

            for (int i = 0; i < 17; i++)
            {
                DeckLeaderRank rank = new DeckLeaderRank((int)DeckLeaderRankType.NCO);
                do
                {
                    card = GetRandomCardFromList(rand, monsterList);
                } while (GetAmountOfCardDuplicatesInDeck(card.Index) >= 3 || IsCardBanned(card.Index) || (IsCardNonMonster(card) && IsDeckCapedOnNonMonsters()));

                DeckCard deckCard = new DeckCard(card, rank);
                deckCardListBinding.Add(deckCard);

                refreshDeckInfoLabels();
            }

            while (deckCardListBinding.Count < 40)
            {
                DeckLeaderRank rank = new DeckLeaderRank((int)DeckLeaderRankType.NCO);
                do
                {
                    card = GetRandomCardFromList(rand, CardConstant.List);
                } while (GetAmountOfCardDuplicatesInDeck(card.Index) >= 3 || IsCardBanned(card.Index) || (IsCardNonMonster(card) && IsDeckCapedOnNonMonsters()));

                DeckCard deckCard = new DeckCard(card, rank);
                deckCardListBinding.Add(deckCard);

                refreshDeckInfoLabels();
            }
        }
        private void AddWeakDeck()
        {
            Random random = new Random();
            int deckCost = averageDeckCost.Value * 100 + 200;

            if (random.Next(0, 100) > 75)
                AddRandomRitualToDeck();

            CardConstant card;

            while (deckCardListBinding.Count < 40)
            {
                do
                {
                    card = GetRandomWheightedCard(random, deckCost - GetDeckCost(), CardConstant.List, random.Next(0, 15));
                    if (card.Kind.Id == 64 || IsCardMassPowerup(card.Index))
                    {
                        card = GetRandomWheightedCard(random, deckCost - GetDeckCost(), CardConstant.List, random.Next(0, 15));
                    }
                } while (GetAmountOfCardDuplicatesInDeck(card.Index) >= 3 || IsCardBanned(card.Index) || (IsCardNonMonster(card) && IsDeckCapedOnNonMonsters()));

                DeckLeaderRank rank = new DeckLeaderRank((int)DeckLeaderRankType.NCO);
                DeckCard deckCard = new DeckCard(card, rank);
                deckCardListBinding.Add(deckCard);

                refreshDeckInfoLabels();
            }
        }
        private void AddCompletelyRandomDeck()
        {
            while (deckCardListBinding.Count < 40)
            {
                CardConstant card;
                DeckLeaderRank rank = new DeckLeaderRank((int)DeckLeaderRankType.NCO);
                do
                {
                    card = GetRandomCard();
                } while (GetAmountOfCardDuplicatesInDeck(card.Index) >= 3);

                DeckCard dCard = new DeckCard(card, rank);
                deckCardListBinding.Add(dCard);

                refreshDeckInfoLabels();
            }
        }
        private void AddBalancedDeck()
        {
            int amountOfPowerups = rand.Next(3, 5);

            for (int i = 200; i <= 1300; i += 100)
            {
                for (int x = 0; x < 2; x++)
                {
                    int Atk = i;
                    CardConstant card;
                    DeckLeaderRank rank = new DeckLeaderRank((int)DeckLeaderRankType.NCO);

                    do
                    {
                        card = GetRandomCardAtXAttack(Atk, true);
                        if (card == null)
                            Atk -= 100;
                    } while (card == null || GetAmountOfCardDuplicatesInDeck(card.Index) >= 3);

                    DeckCard dCard = new DeckCard(card, rank);
                    deckCardListBinding.Add(dCard);
                }

            }
            for (int i = 1400; i <= 2000; i += 100)
            {
                CardConstant card;
                DeckLeaderRank rank = new DeckLeaderRank((int)DeckLeaderRankType.NCO);
                do
                {
                    card = GetRandomCardAtXAttack(i, true);
                } while (GetAmountOfCardDuplicatesInDeck(card.Index) >= 3);

                DeckCard dCard = new DeckCard(card, rank);
                deckCardListBinding.Add(dCard);
            }

            for (int i = 0; i < amountOfPowerups; i++)
            {
                CardConstant card;
                DeckLeaderRank rank = new DeckLeaderRank((int)DeckLeaderRankType.NCO);

                card = GetRandomPowerup(40);

                if (GetAmountOfCardDuplicatesInDeck(card.Index) < 3)
                {
                    DeckCard dCard = new DeckCard(card, rank);
                    deckCardListBinding.Add(dCard);
                }
            }


            while (deckCardListBinding.Count < 40)
            {
                CardConstant card;
                DeckLeaderRank rank = new DeckLeaderRank((int)DeckLeaderRankType.NCO);

                card = GetRandomNonPowerupSpellOrTrap(40);

                if (GetAmountOfCardDuplicatesInDeck(card.Index) < 3)
                {
                    DeckCard dCard = new DeckCard(card, rank);
                    deckCardListBinding.Add(dCard);
                }

                refreshDeckInfoLabels();
            }
        }
        private int GetDeckCost()
        {
            int points = 0;
            foreach (DeckCard card in deckCardListBinding)
            {
                points += card.DeckCost;
            }
            return points;
        }
        private void EmptyDeck()
        {
            foreach (DataGridViewRow row in deckEditorDataGridView.SelectedRows)
            {
                DeckCard deckCard = (DeckCard)row.DataBoundItem;
                deckCardListBinding.Remove(deckCard);
            }

            refreshDeckInfoLabels();
        }
        private void RandomizeDeckLeader(bool includeBannedMonsters = false)
        {
            CardConstant newDeckLeaderCard;
            if (includeBannedMonsters)
                newDeckLeaderCard = CardConstant.Monsters[rand.Next(0, CardConstant.Monsters.Count)];
            else
            {
                newDeckLeaderCard = GetRandomNonbannedMonster();
            }

            Deck deck = (Deck)deckDropdown.SelectedItem;
            deck.DeckLeader = new DeckCard(newDeckLeaderCard, deck.DeckLeader.Rank);
            deckListBinding.ResetBindings(false);
        }
        private void AddRandomRitualToDeck()
        {
            List<CardConstant> cards = new List<CardConstant>();
            int ritualId = rand.Next(830, 853);

            while (IsRitualBanned(ritualId))
            {
                ritualId = rand.Next(830, 853);
            }
            Console.WriteLine(ritualId);
            switch (ritualId)
            {
                case 830:
                    cards.Add(CardConstant.List[830]);
                    for (int i = 0; i < 3; i++)
                    {
                        CardConstant cardToAdd;
                        do
                        {
                            cardToAdd = GetRandomCardAtOrOverXDefence(2000);
                        } while (GetAmountOfCardDuplicatesInDeck(cardToAdd.Index) >= 3);
                        cards.Add(cardToAdd);
                    }
                    break;
                case 831:
                    cards.Add(CardConstant.List[831]);
                    cards.Add(GetRandomCardOfType(4));
                    cards.Add(GetRandomCardOfType(0));
                    cards.Add(GetRandomCardOfType(17));
                    break;
                case 832:
                    cards.Add(CardConstant.List[832]);
                    cards.Add(CardConstant.List[573]);
                    cards.Add(CardConstant.List[99]);
                    cards.Add(CardConstant.List[534]);
                    break;
                case 833:
                    cards.Add(CardConstant.List[833]);
                    cards.Add(GetRandomCardAtOrBelowXAttack(1500));
                    cards.Add(GetRandomCardAtOrBelowXAttack(1500));
                    cards.Add(CardConstant.List[155]);
                    break;
                case 834:
                    cards.Add(CardConstant.List[834]);
                    cards.Add(GetRandomCardOfType(7));
                    cards.Add(GetRandomCardOfType(7));
                    cards.Add(CardConstant.List[300]);
                    break;
                case 835:
                    cards.Add(CardConstant.List[835]);
                    cards.Add(GetRandomCardOfType(5));
                    cards.Add(GetRandomCardOfType(5));
                    cards.Add(CardConstant.List[249]);
                    break;
                case 836:
                    cards.Add(CardConstant.List[836]);
                    cards.Add(GetRandomCardOfType(7));
                    cards.Add(CardConstant.List[329]);
                    cards.Add(CardConstant.List[289]);
                    break;
                case 837:
                    cards.Add(CardConstant.List[837]);
                    cards.Add(CardConstant.List[0]);
                    cards.Add(CardConstant.List[0]);
                    cards.Add(CardConstant.List[0]);
                    break;
                case 838:
                    cards.Add(CardConstant.List[838]);
                    cards.Add(GetRandomCardOfType(3));
                    cards.Add(GetRandomCardOfType(6));
                    cards.Add(CardConstant.List[566]);
                    break;
                case 839:
                    cards.Add(CardConstant.List[839]);
                    cards.Add(CardConstant.List[210]);
                    cards.Add(CardConstant.List[336]);
                    cards.Add(CardConstant.List[661]);
                    break;
                case 840:
                    cards.Add(CardConstant.List[840]);
                    cards.Add(GetRandomCardOfType(4));
                    cards.Add(GetRandomCardOfType(4));
                    cards.Add(CardConstant.List[633]);
                    break;
                case 841:
                    cards.Add(CardConstant.List[841]);
                    cards.Add(GetRandomCardOfType(3));
                    cards.Add(GetRandomCardOfType(3));
                    cards.Add(CardConstant.List[35]);
                    break;
                case 842:
                    cards.Add(CardConstant.List[842]);
                    cards.Add(CardConstant.List[6]);
                    cards.Add(CardConstant.List[293]);
                    cards.Add(CardConstant.List[5]);
                    break;
                case 843:
                    cards.Add(CardConstant.List[843]);
                    cards.Add(GetRandomCardOfType(3));
                    cards.Add(GetRandomCardOfType(6));
                    cards.Add(CardConstant.List[8]);
                    break;
                case 844:
                    cards.Add(CardConstant.List[844]);
                    cards.Add(GetRandomCardOfType(16));
                    cards.Add(GetRandomCardOfType(16));
                    cards.Add(CardConstant.List[584]);
                    break;
                case 845:
                    cards.Add(CardConstant.List[845]);
                    cards.Add(GetRandomCardOfType(1));
                    cards.Add(GetRandomCardOfType(1));
                    cards.Add(CardConstant.List[305]);
                    break;
                case 846:
                    cards.Add(CardConstant.List[846]);
                    cards.Add(GetRandomCardAtOrBelowXAttack(1000));
                    cards.Add(GetRandomCardAtOrBelowXAttack(1000));
                    cards.Add(CardConstant.List[330]);
                    break;
                case 847:
                    cards.Add(CardConstant.List[847]);
                    cards.Add(GetRandomCardOfType(14));
                    cards.Add(GetRandomCardOfType(14));
                    cards.Add(CardConstant.List[173]);
                    break;
                case 848:
                    cards.Add(CardConstant.List[848]);
                    cards.Add(GetRandomCardOfType(9));
                    cards.Add(GetRandomCardOfType(9));
                    cards.Add(CardConstant.List[398]);
                    break;
                case 849:
                    cards.Add(CardConstant.List[849]);
                    cards.Add(GetRandomCardOfType(3));
                    cards.Add(GetRandomCardOfType(3));
                    cards.Add(CardConstant.List[146]);
                    break;
                case 850:
                    cards.Add(CardConstant.List[850]);
                    cards.Add(CardConstant.List[667]);
                    cards.Add(CardConstant.List[207]);
                    cards.Add(CardConstant.List[79]);
                    break;
                case 851:
                    cards.Add(CardConstant.List[851]);
                    cards.Add(GetRandomCardOfType(2));
                    cards.Add(GetRandomCardOfType(14));
                    cards.Add(GetRandomCardOfType(9));
                    break;
                case 852:
                    cards.Add(CardConstant.List[852]);
                    cards.Add(GetRandomCardOfType(12));
                    cards.Add(GetRandomCardOfType(12));
                    cards.Add(CardConstant.List[532]);
                    break;
                case 853:
                    cards.Add(CardConstant.List[853]);
                    cards.Add(GetRandomCardAtOrBelowXAttack(1500));
                    cards.Add(GetRandomCardAtOrBelowXAttack(1500));
                    cards.Add(CardConstant.List[60]);
                    break;
            }

            foreach (CardConstant card in cards)
            {
                if (GetAmountOfCardDuplicatesInDeck(card.Index) < 3)
                {
                    DeckLeaderRank rank = new DeckLeaderRank((int)DeckLeaderRankType.NCO);
                    deckCardListBinding.Add(new DeckCard(card, rank));
                }
            }
        }
        private CardConstant GetRandomNonPowerupSpellOrTrap(int maxDc = 99)
        {
            List<CardConstant> cards = new List<CardConstant>();
            foreach (CardConstant card in CardConstant.List)
            {
                if ((card.Kind.Id == 32 || card.Kind.Id == 96 || card.Kind.Id == 128) && card.DeckCost <= maxDc)
                {
                    cards.Add(card);
                }
            }
            return cards[rand.Next(0, cards.Count)];
        }
        private void SetupPowerupCards()
        {
            for (int i = 0; i < CardConstant.List.Count; i++)
            {
                if (CardConstant.List[i].Kind.Id == 64 || IsCardMassPowerup(i))
                    powerupCardIds.Add(i);
            }
        }
        private CardConstant GetRandomPowerup(int maxDc = 99)
        {
            int cardId;
            CardConstant card;
            do
            {
                cardId = powerupCardIds[rand.Next(0, powerupCardIds.Count)];
                card = CardConstant.List[cardId];
            } while (card.DeckCost > maxDc);
            return card;
        }
        private CardConstant GetRandomNonbannedMonster()
        {
            CardConstant card;
            do
            {
                card = CardConstant.Monsters[rand.Next(0, CardConstant.Monsters.Count)];
            }
            while (IsCardBanned(card.Index));
            return card;
        }
        private CardConstant GetRandomCardOfType(int typeId, int maxAttack = 1500, int maxDc = 30, int minDc = 0)
        {
            CardConstant card;
            do
            {
                card = CardConstant.List[rand.Next(0, CardConstant.List.Count)];
            }
            while (card.Kind.Id != typeId || card.Attack > maxAttack || card.DeckCost > maxDc || card.DeckCost < minDc);
            return card;
        }
        private CardConstant GetRandomCardAtOrBelowXAttack(int maxAttack)
        {
            CardConstant card = null;
            List<CardConstant> cardsBelowXAtk = new List<CardConstant>();

            for (int i = 0; i < CardConstant.Monsters.Count; i++)
            {
                if (CardConstant.Monsters[i].Attack <= maxAttack)
                    cardsBelowXAtk.Add(CardConstant.Monsters[i]);
            }

            card = cardsBelowXAtk[rand.Next(0, cardsBelowXAtk.Count)];
            return card;
        }
        private CardConstant GetRandomCardAtOrOverXDefence(int minDefence)
        {
            CardConstant card = null;
            List<CardConstant> cardsWithXDef = new List<CardConstant>();

            for (int i = 0; i < CardConstant.Monsters.Count; i++)
            {
                if (CardConstant.Monsters[i].Defense >= minDefence)
                    cardsWithXDef.Add(CardConstant.Monsters[i]);
            }

            card = cardsWithXDef[rand.Next(0, cardsWithXDef.Count)];
            return card;
        }
        private CardConstant GetRandomCardAtXAttack(int attack, bool include50AttackHigher = false)
        {
            CardConstant card = null;
            List<CardConstant> cardsWithXAtk = new List<CardConstant>();

            for (int i = 0; i < CardConstant.Monsters.Count; i++)
            {
                if ((CardConstant.Monsters[i].Attack == attack || CardConstant.Monsters[i].Attack == attack + 50) && !IsCardBanned(i) && CardConstant.Monsters[i].Kind.Id != 64 && CardConstant.Monsters[i].Kind.Id != 20)
                    cardsWithXAtk.Add(CardConstant.Monsters[i]);
            }
            if (cardsWithXAtk.Count > 0)
                card = cardsWithXAtk[rand.Next(0, cardsWithXAtk.Count)];
            return card;
        }
        private CardConstant GetRandomCard()
        {

            CardConstant card;
            do
            {
                card = CardConstant.List[rand.Next(0, CardConstant.List.Count)];
            } while (card.Index == 671);

            return card;
        }
        private bool IsCardMassPowerup(int cardId)
        {
            switch (cardId)
            {
                case 735:
                    return true;
                case 738:
                    return true;
                case 739:
                    return true;
                case 740:
                    return true;
                case 741:
                    return true;
                case 742:
                    return true;
            }
            return false;
        }
        private int GetAmountOfCardDuplicatesInDeck(int cardIndex)
        {
            int duplicates = 0;
            for (int i = 0; i < deckCardListBinding.Count; i++)
            {
                if (deckCardListBinding[i].CardConstant.Index == cardIndex)
                {
                    duplicates++;
                }
            }

            return duplicates;
        }
        private bool IsRitualBanned(int ritualId)
        {
            if (bannedRitualIds.Contains(ritualId))
                return true;

            return false;
        }
        private CardParameters GetRandomCardParameters(int cardStrength)
        {
            Console.WriteLine(cardStrength);
            if (cardStrength < 50)
            {
                return new CardParameters((int)(1000 * deckStrengthMultiplier), (int)(0 * deckStrengthMultiplier), (int)(20 * deckStrengthMultiplier), (int)(0 * deckStrengthMultiplier));
            }
            else if (cardStrength < 83)
            {
                return new CardParameters((int)(1500 * deckStrengthMultiplier), (int)(0 * deckStrengthMultiplier), (int)(33 * deckStrengthMultiplier), (int)(0 * deckStrengthMultiplier));
            }
            else if (cardStrength < 95)
            {
                return new CardParameters((int)(1800 * deckStrengthMultiplier), (int)(800 * deckStrengthMultiplier), (int)(41 * deckStrengthMultiplier), (int)(15 * deckStrengthMultiplier));
            }
            else
            {
                return new CardParameters((int)(2300 * deckStrengthMultiplier), (int)(1500 * deckStrengthMultiplier), (int)(47 * deckStrengthMultiplier), (int)(21 * deckStrengthMultiplier));
            }
            //else
            //{
            //    return new CardParameters((int)(2800 * deckStrengthMultiplier),(int)(2100 * deckStrengthMultiplier), (int)(70 * deckStrengthMultiplier),(int)( 35 * deckStrengthMultiplier));
            //}
        }
        private CardConstant GetRandomWheightedCard(Random rand, int remaingingDc, List<CardConstant> cardlist, int maximumAllowedExtraCost = 0)
        {
            int maxCardDc = Math.Max((remaingingDc / (40 - deckCardListBinding.Count)) + maximumAllowedExtraCost, 10);

            CardConstant card = CardConstant.List[416];
            List<CardConstant> cards = GetEligableCards(cardlist, maxCardDc, maxCardDc / 2, 2500, false);
            if (cards.Count == 0)
            {
                return card;
            }


            card = cards[rand.Next(0, cards.Count)];


            //int counter = 0;
            //do
            //{
            //    counter++;
            //    card = CardConstant.List[rand.Next(0, CardConstant.List.Count)];
            //    Console.WriteLine(card.Name);
            //} while (card.DeckCost > maxCardDc || card.DeckCost < maxCardDc / 2 || card.KindName == "Immortal" || card.CardColor == CardColorType.Ritual || counter > 5000);

            return card;
        }
        private List<CardConstant> GetEligableCards(List<CardConstant> cardlist, int maxDc = 30, int minDc = 0, int maxAttack = 2000, bool includeImmortals = false)
        {

            List<CardConstant> cards = new List<CardConstant>();

            for (int i = 0; i < cardlist.Count; i++)
            {
                CardConstant card = cardlist[i];
                if (!(card.DeckCost > maxDc || card.DeckCost < minDc || card.Attack > maxAttack || IsCardBanned(card.Index) || (!includeImmortals && card.KindName == "Immortal" || card.CardColor == CardColorType.Ritual)) || GetAmountOfCardDuplicatesInDeck(card.Index) >= 3 || (IsCardNonMonster(card) && IsDeckCapedOnNonMonsters()))
                {
                    cards.Add(card);
                }

            }
            return cards;
        }
        private CardConstant GetRandomCardFromList(Random rand, List<CardConstant> cardList)
        {
            bool cardFound = false;
            CardConstant cardConstant = cardList[0];
            int cardStrength;
            CardParameters cardPara;
            do
            {
                cardStrength = rand.Next(0, 100);
                cardPara = GetRandomCardParameters(cardStrength);
            } while (!cardList.Exists(x => (x.CardColor == CardColorType.EffectMonster || x.CardColor == CardColorType.NormalMonster) ? x.Attack <= cardPara.maxAttack && x.Attack >= cardPara.minAttack && x.DeckCost <= cardPara.maxDeckCost && x.DeckCost >= cardPara.minDeckCost : x.DeckCost <= cardPara.maxDeckCost && x.DeckCost >= cardPara.minDeckCost));

            while (!cardFound)
            {
                int i = rand.Next(0, cardList.Count);

                if (!IsCardBanned(i))
                {
                    cardConstant = cardList[i];

                    if ((cardConstant.CardColor == CardColorType.EffectMonster || cardConstant.CardColor == CardColorType.NormalMonster) && cardConstant.Attack <= cardPara.maxAttack && cardConstant.Attack >= cardPara.minAttack && cardConstant.DeckCost <= cardPara.maxDeckCost && cardConstant.DeckCost >= cardPara.minDeckCost && cardConstant.KindName != "Immortal")
                    {
                        cardFound = true;
                        //Console.WriteLine("Card id: " + i + " card strength: " + cardStrength);
                    }
                    if ((cardConstant.CardColor == CardColorType.Magic || cardConstant.CardColor == CardColorType.Trap) && cardConstant.DeckCost <= cardPara.maxDeckCost && cardConstant.DeckCost >= cardPara.minDeckCost)
                    {
                        if (cardConstant.Kind.Id == 64)
                        {
                            if (rand.Next(0, 100) > 50)
                            {
                                cardFound = true;
                            }
                        }
                        //Console.WriteLine("Card id: " + i + " card strength: " + cardStrength);
                    }
                }

            }
            return cardConstant;
        }
        private List<CardConstant> GetRandomMonsterList()
        {
            Random rand = new Random();
            int list = rand.Next(0, 16);

            switch (list)
            {
                case (0):
                    return dragons;
                case (1):
                    return spellcasters;
                case (2):
                    return zombies;
                case (3):
                    List<CardConstant> fighters = new List<CardConstant>();
                    fighters.AddRange(warriors);
                    fighters.AddRange(beastwarriors);
                    fighters.AddRange(beastwarriors);
                    return fighters;
                case (4):
                    List<CardConstant> Creatures = new List<CardConstant>();
                    Creatures.AddRange(beastwarriors);
                    Creatures.AddRange(beasts);
                    Creatures.AddRange(wingedbeasts);
                    return Creatures;
                case (5):
                    return beasts;
                case (6):
                    return wingedbeasts;
                case (7):
                    return fiends;
                case (8):
                    return fairies;
                case (9):
                    return insects;
                case (10):
                    return dinosaurs;
                case (11):
                    return reptiles;
                case (12):
                    List<CardConstant> waters = new List<CardConstant>();
                    waters.AddRange(aquas);
                    waters.AddRange(seaserpents);
                    waters.AddRange(seaserpents);
                    waters.AddRange(fishes);
                    waters.AddRange(fishes);
                    return waters;
                case (13):
                    return machines;
                case (14):
                    List<CardConstant> smallerTypes = new List<CardConstant>();
                    smallerTypes.AddRange(thunders);
                    smallerTypes.AddRange(rocks);
                    smallerTypes.AddRange(pyros);
                    return smallerTypes;
                case (15):
                    return plants;
                default:
                    return dragons;
            }
        }
        private bool IsCardBanned(int cardId)
        {
            if (bannedCardIds.Contains(cardId))
            {
                return true;
            }
            return false;
        }
        private bool IsCardNonMonster(CardConstant card)
        {
            if (card.CardColor == CardColorType.Magic || card.CardColor == CardColorType.Trap || card.CardColor == CardColorType.Ritual)
            {
                return true;
            }
            return false;
        }
        private bool IsDeckCapedOnNonMonsters()
        {
            int nonmonsters = 0;
            foreach (DeckCard card in deckCardListBinding)
            {
                if (card.CardConstant.CardColor == CardColorType.Magic || card.CardConstant.CardColor == CardColorType.Trap || card.CardConstant.CardColor == CardColorType.Ritual)
                {
                    nonmonsters++;
                }
            }
            if (nonmonsters >= 9)
            {
                return true;
            }
            return false;
        }
        private void SetupDeckarchetypes()
        {

            for (int i = 0; i < 670; i++)
            {
                switch (i)
                {
                    case int n when n < 35:
                        dragons.Add(CardConstant.Monsters[i]);
                        break;
                    case int n when n < 101:
                        spellcasters.Add(CardConstant.Monsters[i]);
                        break;
                    case int n when n < 133:
                        zombies.Add(CardConstant.Monsters[i]);
                        break;
                    case int n when n < 208:
                        warriors.Add(CardConstant.Monsters[i]);
                        break;
                    case int n when n < 224:
                        beastwarriors.Add(CardConstant.Monsters[i]);
                        break;
                    case int n when n < 268:
                        beasts.Add(CardConstant.Monsters[i]);
                        break;
                    case int n when n < 292:
                        wingedbeasts.Add(CardConstant.Monsters[i]);
                        break;
                    case int n when n < 368:
                        fiends.Add(CardConstant.Monsters[i]);
                        break;
                    case int n when n < 395:
                        fairies.Add(CardConstant.Monsters[i]);
                        break;
                    case int n when n < 433:
                        insects.Add(CardConstant.Monsters[i]);
                        break;
                    case int n when n < 446:
                        dinosaurs.Add(CardConstant.Monsters[i]);
                        break;
                    case int n when n < 459:
                        reptiles.Add(CardConstant.Monsters[i]);
                        break;
                    case int n when n < 476:
                        fishes.Add(CardConstant.Monsters[i]);
                        break;
                    case int n when n < 481:
                        seaserpents.Add(CardConstant.Monsters[i]);
                        break;
                    case int n when n < 533:
                        machines.Add(CardConstant.Monsters[i]);
                        break;
                    case int n when n < 548:
                        thunders.Add(CardConstant.Monsters[i]);
                        break;
                    case int n when n < 613:
                        aquas.Add(CardConstant.Monsters[i]);
                        break;
                    case int n when n < 624:
                        pyros.Add(CardConstant.Monsters[i]);
                        break;
                    case int n when n < 647:
                        rocks.Add(CardConstant.Monsters[i]);
                        break;
                    case int n when n < 671:
                        plants.Add(CardConstant.Monsters[i]);
                        break;
                }
            }
        }
        private int CalculateAverageDeckCost()
        {
            return averageDeckCost.Value * 100 + 200;
        }
        private List<CardConstant> RemoveBannedCards(List<CardConstant> incList)
        {
            List<CardConstant> newList = new List<CardConstant>();

            foreach (CardConstant card in incList)
            {
                if (!IsCardBanned(card.Index))
                    newList.Add(card);
            }
            return newList;
        }
        private float CalculateDeckStrengthMultiplier()
        {
            return (deckStrengthMultiplierTrackBar.Value * 0.1f + 0.5f);
        }
        public struct CardParameters
        {
            public int maxAttack { get; }
            public int minAttack { get; }
            public int maxDeckCost { get; }
            public int minDeckCost { get; }
            public CardParameters(int maxAtk, int minAtk, int maxDC, int minDC)
            {
                maxAttack = maxAtk;
                minAttack = minAtk;
                maxDeckCost = maxDC;
                minDeckCost = minDC;
            }
        }
    }
}