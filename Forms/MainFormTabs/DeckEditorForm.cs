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
            //this.formatCardTable(this.trunkDataGridView);
            this.formatCardTable(this.deckEditorDataGridView);
            //this.trunkDataGridView.DataSource = trunkCardConstantBinding;
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            Deck selectedDeck = (Deck)deckDropdown.SelectedItem;
            deckCardListBinding = new SortableBindingList<DeckCard>(selectedDeck.CardList);
            deckEditorDataGridView.DataSource = deckCardListBinding;
            deckEditDeckLeaderRankComboBox.SelectedValue = ((Deck)deckDropdown.SelectedItem).DeckLeader.Rank.Index;
            refreshDeckInfoLabels();
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

            //Deck selectedsDeck = (Deck)deckDropdown.Items[27];
            //deckCardListBinding = new SortableBindingList<DeckCard>(selectedsDeck.CardList);
            //deckEditorDataGridView.DataSource = deckCardListBinding;
            //deckEditDeckLeaderRankComboBox.SelectedValue = ((Deck)deckDropdown.SelectedItem).DeckLeader.Rank.Index;
            //removeAllCards();
            //NewDeck();
            //deckDropdown.SelectedItem = selectedsDeck;
            //RandomizeDeckLeader();
            //saveDeck();

            //refreshDeckInfoLabels();

        }
        private void averageDeckCostChanges(object sender, EventArgs e)
        {
            averageDeckCostLabel.Text = "Deck cost: " + CalculateAverageDeckCost().ToString();
        }
        private void minSpellsChanged(object sender, EventArgs e)
        {
            minSpellsSliderLabel.Text = "Minimum amount:" + minSpellsSlider.Value.ToString();
        }
        private void maxSpellsChanged(object sender, EventArgs e)
        {
            maxSpellsSliderLabel.Text = "Maximum amount:" + maxSpellsSlider.Value.ToString();
        }
        private void ritualschanceChanged(object sender, EventArgs e)
        {
            ritualchancesliderlabel.Text = ritualchanceslider.Value.ToString() + "%";
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
            string[] lines = File.ReadAllLines(path);
            for (int i = 0; i < lines.Length; i++)
            {
                themes.Add(new List<CardConstant>());
                string[] ids = lines[i].Split(',');
                for (int x = 0; x < ids.Length; x++)
                {
                    int cardId = -1;
                    int.TryParse(ids[x], out cardId);

                    if (cardId > -1)
                    {
                        themes[i].Add(CardConstant.List[cardId]);
                    }
                }
            }
        }
        private void LoadDefaultThemes()
        {
            themes[0] = new List<CardConstant>();
            for (int i = 0; i < dragons.Count; i++)
            {
                themes[0].Add(dragons[i]);
            }
            themes[1] = new List<CardConstant>();
            for (int i = 0; i < spellcasters.Count; i++)
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
            AddRitual();
            AddSpellsAndTraps();
            AddRandomMonsters();

        }
        private void AddRitual()
        {
            if (ritualsRadioButton.Checked || (ritualsRadioButton2.Checked && rand.Next(0, 100) <= ritualchanceslider.Value))
            {
                AddRandomRitualToDeck();
            }
        }
        private void AddRandomMonsters()
        {
            CardConstant card;

            while (deckCardListBinding.Count < 40)
            {
                DeckLeaderRank rank = new DeckLeaderRank((int)DeckLeaderRankType.NCO);
                do
                {
                    card = GetRandomNonbannedMonster();
                } while (GetAmountOfCardDuplicatesInDeck(card.Index) >= 3 || IsCardBanned(card.Index) || (IsCardNonMonster(card)));

                DeckCard deckCard = new DeckCard(card, rank);
                deckCardListBinding.Add(deckCard);

                refreshDeckInfoLabels();
            }
        }
        private void AddSpellsAndTraps()
        {
            CardConstant card;

            int amountOfSpellsAndTraps = rand.Next(minSpellsSlider.Value, maxSpellsSlider.Value);
            int maxDc = (int)(deckStrengthMultiplier * 33);

            int spellscounter = 0;
            while (spellscounter < amountOfSpellsAndTraps)
            {
                DeckLeaderRank rank = new DeckLeaderRank((int)DeckLeaderRankType.NCO);
                int x = rand.Next(0, 100);

                if (x < 25)
                {
                    do
                    {
                        card = GetRandomPowerup(maxDc);
                    } while (GetAmountOfCardDuplicatesInDeck(card.Index) >= 3 || IsCardBanned(card.Index));
                }
                else
                {
                    do
                    {
                        card = GetRandomNonPowerupSpellOrTrap(maxDc);
                    } while (GetAmountOfCardDuplicatesInDeck(card.Index) >= 3 || IsCardBanned(card.Index));
                }

                DeckCard deckCard = new DeckCard(card, rank);
                deckCardListBinding.Add(deckCard);

                spellscounter++;
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
                    card = GetRandomCardFromList(monsterList);
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
                    card = GetRandomCardFromList(CardConstant.List);
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
                    card = GetRandomWheightedCard(deckCost - GetDeckCost(), CardConstant.List, random.Next(0, 15));
                    if (card.Kind.Id == 64 || IsCardMassPowerup(card.Index))
                    {
                        card = GetRandomWheightedCard(deckCost - GetDeckCost(), CardConstant.List, random.Next(0, 15));
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
        private void randomizeEnemyDecks(object sender, EventArgs e)
        {
            for (int i = 27; i < 48; i++)
            {
                Deck selectedDeck = (Deck)deckDropdown.Items[i];
                deckCardListBinding = new SortableBindingList<DeckCard>(selectedDeck.CardList);
                deckEditorDataGridView.DataSource = deckCardListBinding;
                deckEditDeckLeaderRankComboBox.SelectedValue = ((Deck)deckDropdown.SelectedItem).DeckLeader.Rank.Index;
                removeAllCards();
                AddEnemyDeck(i);
                deckDropdown.SelectedItem = selectedDeck;
                saveDeck();
            }
            refreshDeckInfoLabels();


            //      { 27, "Seto" },
            //{ 28, "Weevil Underwood" },
            //{ 29, "Rex Raptor" },
            //{ 30, "Keith" },
            //{ 31, "Ishtar" },
            //{ 32, "Necromancer" },
            //{ 33, "Darkness-ruler" },
            //{ 34, "Labyrinth-ruler" },
            //{ 35, "Pegasus Crawford" },
            //{ 36, "Richard Slysheen of York" },
            //{ 37, "Tea" },
            //{ 38, "T. Tristan Grey" },
            //{ 39, "Margaret Mai Beaufort" },
            //{ 40, "Mako" },
            //{ 41, "Joey" },
            //{ 42, "J. Shadi Morton" },
            //{ 43, "Jasper Dice Tudor" },
            //{ 44, "Bakura" },
            //{ 45, "Yugi" },
            //{ 46, "Manawyddan fab Llyr (vs White Rose)" },
            //{ 47, "Manawyddan fab Llyr (vs Red Rose)" },
        }
        private void AddEnemyDeck(int enemy)
        {
            switch (enemy)
            {
                case 27:
                    AddSetoDeck();
                    break;
                case 28:
                    AddWeevilDeck();
                    break;
                case 29:
                    AddRexDeck();
                    break;
                case 30:
                    AddKeithDeck();
                    break;
                case 31:
                    AddIshtarDeck();
                    break;
                case 32:
                    AddNecromancerDeck();
                    break;
                case 33:
                    AddDarknessrulerDeck();
                    break;
                case 34:
                    AddLabyrinthrulerDeck();
                    break;
                case 35:
                    AddPegasusDeck();
                    break;
                case 36:
                    AddRichardDeck();
                    break;
                case 37:
                    AddTeaDeck();
                    break;
                case 38:
                    AddTristanDeck();
                    break;
                case 39:
                    AddMaiDeck();
                    break;
                case 40:
                    AddMakoDeck();
                    break;
                case 41:
                    AddJoeyDeck();
                    break;
                case 42:
                    AddShadiDeck();
                    break;
                case 43:
                    AddGrandpaDeck();
                    break;
                case 44:
                    AddBakuraDeck();
                    break;
                case 45:
                    AddYugiDeck();
                    break;
                case 46:
                    AddManawyddanWhite();
                    break;
                case 47:
                    AddManawyddanRed();
                    break;
            }

        }
        private void AddSetoDeck()
        {
            List<CardConstant> possibleCards = new List<CardConstant>();

            List<int> guaranteedCards = new List<int>()
            {
                0,
                0,
                0,
                83,
                146,
                154,
                211,
                233,
                685,
                766,
                766,
                837
            };

            List<int> extracards = new List<int>()
            {
                59,
                84,
                136,
                137,
                172,
                178,
                191,
                205,
                209,
                215,
                219,
                228,
                339,
                340,
                360,
                361,
                522,
                527,
                535,
                545,
                708,
                722,
                726,
                743,
                752,
                755,
                778,
                780,
                780,
                780,
                781,
                793,
                796,
                802,
                802,
                803,
                805,
                806,
                809,
                810,
                813,
                821,
                823,
                825,
                828
            };

            List<CardConstant> dragons = GetListOfMonstersWithAttackXOrMore(1500, GetListOfMonstersWithAttackXOrLess(3000, GetListOfMonstersOfType(0)));

            foreach (int guaranteedCard in guaranteedCards)
            {
                if (!IsCardBanned(guaranteedCard) && GetAmountOfCardDuplicatesInDeck(guaranteedCard) <= 3)
                {
                    DeckLeaderRank rank = new DeckLeaderRank((int)DeckLeaderRankType.NCO);
                    CardConstant card = CardConstant.List[guaranteedCard];
                    DeckCard deckCard = new DeckCard(card, rank);
                    deckCardListBinding.Add(deckCard);
                }
            }
            foreach (CardConstant card in dragons)
            {
                if (!IsCardBanned(card.Index))
                {
                    possibleCards.Add(card);
                }

            }
            foreach (int cardid in extracards)
            {
                if (!IsCardBanned(cardid))
                {
                    possibleCards.Add(CardConstant.List[cardid]);
                }
            }

            while (deckCardListBinding.Count < 40)
            {
                DeckLeaderRank rank = new DeckLeaderRank((int)DeckLeaderRankType.NCO);
                CardConstant cardToAdd;
                do
                {
                    cardToAdd = possibleCards[rand.Next(0, possibleCards.Count)];
                } while (GetAmountOfCardDuplicatesInDeck(cardToAdd.Index) >= 3);

                DeckCard deckCard = new DeckCard(cardToAdd, rank);
                deckCardListBinding.Add(deckCard);


            }

            refreshDeckInfoLabels();
        }
        private void AddWeevilDeck()
        {
            List<CardConstant> possibleCards = new List<CardConstant>();

            List<int> guaranteedCards = new List<int>()
            {
                401,
                419,
                756,
                757,
                798,
                848
            };

            List<int> extraCards = new List<int>()
            {
                689,
                703,
                705,
                716,
                756,
                756,
                757,
                757,
                773,
                798,
                804,
                805,
                808,
                809,
                817,
                828
            };

            List<CardConstant> insects = GetListOfMonstersWithAttackXOrLess(2300, GetListOfMonstersOfType(9));

            foreach (CardConstant card in insects)
            {
                if (!IsCardBanned(card.Index))
                {
                    possibleCards.Add(card);
                    possibleCards.Add(card);
                }
            }
            foreach (int cardid in extraCards)
            {
                if (!IsCardBanned(cardid))
                {
                    possibleCards.Add(CardConstant.List[cardid]);
                }
            }
            foreach (int guaranteedCard in guaranteedCards)
            {
                if (!IsCardBanned(guaranteedCard) && GetAmountOfCardDuplicatesInDeck(guaranteedCard) <= 3)
                {
                    DeckLeaderRank rank = new DeckLeaderRank((int)DeckLeaderRankType.NCO);
                    CardConstant card = CardConstant.List[guaranteedCard];
                    DeckCard deckCard = new DeckCard(card, rank);
                    deckCardListBinding.Add(deckCard);
                }
            }
            while (deckCardListBinding.Count < 40)
            {
                DeckLeaderRank rank = new DeckLeaderRank((int)DeckLeaderRankType.NCO);
                CardConstant cardToAdd;
                do
                {
                    cardToAdd = possibleCards[rand.Next(0, possibleCards.Count)];
                } while (GetAmountOfCardDuplicatesInDeck(cardToAdd.Index) >= 3);

                DeckCard deckCard = new DeckCard(cardToAdd, rank);
                deckCardListBinding.Add(deckCard);
            }
            refreshDeckInfoLabels();
        }
        private void AddRexDeck()
        {
            List<CardConstant> possibleCards = new List<CardConstant>();

            List<int> guaranteedCards = new List<int>()
            {
                775,
                775
            };

            List<int> extraCards = new List<int>()
            {
                446,
                446,
                451,
                451,
                453,
                453,
                690,
                704,
                704,
                709,
                709,
                721,
                722,
                724,
                725,
                741,
                741,
                773,
                773,
                775,
                775,
                807,
                807,
                809,
                809,
                813,
                822
            };

            List<CardConstant> dinosaurs = GetListOfMonstersOfType(10);

            foreach (CardConstant card in dinosaurs)
            {
                if (!IsCardBanned(card.Index))
                {
                    possibleCards.Add(card);
                    possibleCards.Add(card);
                    possibleCards.Add(card);
                    possibleCards.Add(card);
                }
            }
            foreach (int cardid in extraCards)
            {
                if (!IsCardBanned(cardid))
                {
                    possibleCards.Add(CardConstant.List[cardid]);
                }
            }
            foreach (int guaranteedCard in guaranteedCards)
            {
                if (!IsCardBanned(guaranteedCard) && GetAmountOfCardDuplicatesInDeck(guaranteedCard) <= 3)
                {
                    DeckLeaderRank rank = new DeckLeaderRank((int)DeckLeaderRankType.NCO);
                    CardConstant card = CardConstant.List[guaranteedCard];
                    DeckCard deckCard = new DeckCard(card, rank);
                    deckCardListBinding.Add(deckCard);
                }
            }
            while (deckCardListBinding.Count < 40)
            {
                DeckLeaderRank rank = new DeckLeaderRank((int)DeckLeaderRankType.NCO);
                CardConstant cardToAdd;
                do
                {
                    cardToAdd = possibleCards[rand.Next(0, possibleCards.Count)];
                } while (GetAmountOfCardDuplicatesInDeck(cardToAdd.Index) >= 3);

                DeckCard deckCard = new DeckCard(cardToAdd, rank);
                deckCardListBinding.Add(deckCard);
            }
            refreshDeckInfoLabels();
        }
        private void AddKeithDeck()
        {
            List<CardConstant> possibleCards = new List<CardConstant>();

            List<int> guaranteedCards = new List<int>()
            {
                342,
                505,
                738,
                774,
                774,
                774,
                787,
                787,
                799
            };

            List<int> extraCards = new List<int>()
            {
                810,
                814,
                815,
                823
            };

            List<CardConstant> machines = GetListOfMonstersWithAttackXOrLess(2600, GetListOfMonstersOfType(14));

            foreach (CardConstant card in machines)
            {
                if (!IsCardBanned(card.Index))
                {
                    possibleCards.Add(card);
                    possibleCards.Add(card);
                }
            }
            foreach (int cardid in extraCards)
            {
                if (!IsCardBanned(cardid))
                {
                    possibleCards.Add(CardConstant.List[cardid]);
                }
            }
            foreach (int guaranteedCard in guaranteedCards)
            {
                if (!IsCardBanned(guaranteedCard) && GetAmountOfCardDuplicatesInDeck(guaranteedCard) <= 3)
                {
                    DeckLeaderRank rank = new DeckLeaderRank((int)DeckLeaderRankType.NCO);
                    CardConstant card = CardConstant.List[guaranteedCard];
                    DeckCard deckCard = new DeckCard(card, rank);
                    deckCardListBinding.Add(deckCard);
                }
            }
            while (deckCardListBinding.Count < 40)
            {
                DeckLeaderRank rank = new DeckLeaderRank((int)DeckLeaderRankType.NCO);
                CardConstant cardToAdd;
                do
                {
                    cardToAdd = possibleCards[rand.Next(0, possibleCards.Count)];
                } while (GetAmountOfCardDuplicatesInDeck(cardToAdd.Index) >= 3);

                DeckCard deckCard = new DeckCard(cardToAdd, rank);
                deckCardListBinding.Add(deckCard);
            }
            refreshDeckInfoLabels();
        }
        private void AddIshtarDeck()
        {
            List<CardConstant> possibleCards = new List<CardConstant>();

            List<int> guaranteedCards = new List<int>()
            {
                476,
                584,
                593,
                742,
                777,
                777,
                783,
                783,
                829,
                844,
            };

            List<int> extraCards = new List<int>()
            {
                5,
                22,
                29,
                84,
                92,
                100,
                476,
                477,
                478,
                479,
                480,
                535,
                539,
                541,
                543,
                546,
                547,
                549,
                552,
                558,
                584,
                598,
                592,
                634,
                684,
                693,
                699,
                705,
                706,
                727,
                730,
                760,
                751,
                777,
                796,
                801,
                803,
                804,
                809,
                811,
                818,
                823
            };

            List<CardConstant> aquas = GetListOfMonstersWithAttackXOrLess(2100, GetListOfMonstersOfType(16));
            List<CardConstant> fishes = GetListOfMonstersWithAttackXOrLess(1700, GetListOfMonstersOfType(12));

            foreach (CardConstant card in aquas)
            {
                if (!IsCardBanned(card.Index))
                {
                    possibleCards.Add(card);
                }
            }
            foreach (CardConstant card in fishes)
            {
                if (!IsCardBanned(card.Index))
                {
                    possibleCards.Add(card);
                }
            }
            foreach (int cardid in extraCards)
            {
                if (!IsCardBanned(cardid))
                {
                    possibleCards.Add(CardConstant.List[cardid]);
                }
            }
            foreach (int guaranteedCard in guaranteedCards)
            {
                if (!IsCardBanned(guaranteedCard) && GetAmountOfCardDuplicatesInDeck(guaranteedCard) <= 3)
                {
                    DeckLeaderRank rank = new DeckLeaderRank((int)DeckLeaderRankType.NCO);
                    CardConstant card = CardConstant.List[guaranteedCard];
                    DeckCard deckCard = new DeckCard(card, rank);
                    deckCardListBinding.Add(deckCard);
                }
            }
            while (deckCardListBinding.Count < 40)
            {
                DeckLeaderRank rank = new DeckLeaderRank((int)DeckLeaderRankType.NCO);
                CardConstant cardToAdd;
                do
                {
                    cardToAdd = possibleCards[rand.Next(0, possibleCards.Count)];
                } while (GetAmountOfCardDuplicatesInDeck(cardToAdd.Index) >= 3);

                DeckCard deckCard = new DeckCard(cardToAdd, rank);
                deckCardListBinding.Add(deckCard);
            }
            refreshDeckInfoLabels();
        }
        private void AddNecromancerDeck()
        {
            List<CardConstant> possibleCards = new List<CardConstant>();

            List<int> guaranteedCards = new List<int>()
            {
                108,
                119,
                119,
                412,
                495,
                754,
                754,
                772,
                772,
                851
            };

            List<int> extraCards = new List<int>()
            {
                404,
                411,
                412,
                420,
                490,
                493,
                495,
                685,
                690,
                694,
                699,
                745,
                754,
                772,
                808,
                821,
                823
            };

            List<CardConstant> zombies = GetListOfMonstersWithAttackXOrLess(2300, GetListOfMonstersOfType(2));

            foreach (CardConstant card in zombies)
            {
                possibleCards.Add(card);
                possibleCards.Add(card);
            }
            foreach (int cardid in extraCards)
            {
                possibleCards.Add(CardConstant.List[cardid]);
            }
            foreach (int guaranteedCard in guaranteedCards)
            {
                if (GetAmountOfCardDuplicatesInDeck(guaranteedCard) <= 3)
                {
                    DeckLeaderRank rank = new DeckLeaderRank((int)DeckLeaderRankType.NCO);
                    CardConstant card = CardConstant.List[guaranteedCard];
                    DeckCard deckCard = new DeckCard(card, rank);
                    deckCardListBinding.Add(deckCard);
                }
            }
            while (deckCardListBinding.Count < 40)
            {
                DeckLeaderRank rank = new DeckLeaderRank((int)DeckLeaderRankType.NCO);
                CardConstant cardToAdd;
                do
                {
                    cardToAdd = possibleCards[rand.Next(0, possibleCards.Count)];
                } while (GetAmountOfCardDuplicatesInDeck(cardToAdd.Index) >= 3);

                DeckCard deckCard = new DeckCard(cardToAdd, rank);
                deckCardListBinding.Add(deckCard);
            }
            refreshDeckInfoLabels();
        }
        private void AddDarknessrulerDeck()
        {
            List<CardConstant> possibleCards = new List<CardConstant>();

            List<int> guaranteedCards = new List<int>()
            {
                300,
                300,
                754,
                770,
                782,
                782,
                834
            };

            List<int> extraCards = new List<int>()
            {
                684,
                746,
                749,
                754,
                770,
                782,
                795,
                817,
                818,
                819,
                823,
                828
            };

            List<CardConstant> fiends = GetListOfMonstersWithAttackXOrLess(2400, GetListOfMonstersOfType(7));
            List<CardConstant> limitedtraps = GetListOfRandomLimitedRangeTrap(60, 0);

            foreach (CardConstant card in fiends)
            {
                if (!IsCardBanned(card.Index))
                {
                    possibleCards.Add(card);
                }
            }
            foreach (CardConstant card in limitedtraps)
            {
                if (!IsCardBanned(card.Index))
                {
                    possibleCards.Add(card);
                    possibleCards.Add(card);
                }
            }
            foreach (int cardid in extraCards)
            {
                if (!IsCardBanned(cardid))
                {
                    possibleCards.Add(CardConstant.List[cardid]);
                }
            }
            foreach (int guaranteedCard in guaranteedCards)
            {
                if (!IsCardBanned(guaranteedCard) && GetAmountOfCardDuplicatesInDeck(guaranteedCard) <= 3)
                {
                    DeckLeaderRank rank = new DeckLeaderRank((int)DeckLeaderRankType.NCO);
                    CardConstant card = CardConstant.List[guaranteedCard];
                    DeckCard deckCard = new DeckCard(card, rank);
                    deckCardListBinding.Add(deckCard);
                }
            }
            while (deckCardListBinding.Count < 40)
            {
                DeckLeaderRank rank = new DeckLeaderRank((int)DeckLeaderRankType.NCO);
                CardConstant cardToAdd;
                do
                {
                    cardToAdd = possibleCards[rand.Next(0, possibleCards.Count)];
                } while (GetAmountOfCardDuplicatesInDeck(cardToAdd.Index) >= 3);

                DeckCard deckCard = new DeckCard(cardToAdd, rank);
                deckCardListBinding.Add(deckCard);
            }
            refreshDeckInfoLabels();
        }
        private void AddLabyrinthrulerDeck()
        {
            List<CardConstant> possibleCards = new List<CardConstant>();

            List<int> guaranteedCards = new List<int>()
            {
                99,
                123,
                123,
                409,
                486,
                534,
                573,
                697,
                832
            };

            List<int> extraCards = new List<int>()
            {
                13,
                21,
                37,
                67,
                88,
                99,
                111,
                123,
                123,
                140,
                142,
                144,
                156,
                158,
                162,
                162,
                164,
                169,
                172,
                174,
                182,
                190,
                237,
                240,
                288,
                297,
                325,
                349,
                371,
                382,
                402,
                408,
                408,
                409,
                409,
                409,
                413,
                417,
                422,
                425,
                426,
                429,
                445,
                495,
                503,
                513,
                527,
                528,
                534,
                536,
                567,
                573,
                644,
                652,
                661,
                663,
                668,
                681,
                697,
                698,
                700,
                701,
                705,
                706,
                711,
                712,
                716,
                723,
                729,
                734,
                734,
                736,
                746,
                750,
                756,
                757,
                780,
                780,
                784,
                796
            };

            List<CardConstant> limitedtraps = GetListOfRandomLimitedRangeTrap(40, 0);
            List<CardConstant> fulltraps = GetListOfRandomLimitedRangeTrap(50, 0);

            foreach (CardConstant card in limitedtraps)
            {
                if (!IsCardBanned(card.Index))
                {
                    possibleCards.Add(card);
                }
            }
            foreach (CardConstant card in fulltraps)
            {
                if (!IsCardBanned(card.Index))
                {
                    possibleCards.Add(card);
                }
            }
            foreach (int cardid in extraCards)
            {
                if (!IsCardBanned(cardid))
                {
                    possibleCards.Add(CardConstant.List[cardid]);
                    possibleCards.Add(CardConstant.List[cardid]);
                }
            }
            foreach (int guaranteedCard in guaranteedCards)
            {
                if (!IsCardBanned(guaranteedCard) && GetAmountOfCardDuplicatesInDeck(guaranteedCard) <= 3)
                {
                    DeckLeaderRank rank = new DeckLeaderRank((int)DeckLeaderRankType.NCO);
                    CardConstant card = CardConstant.List[guaranteedCard];
                    DeckCard deckCard = new DeckCard(card, rank);
                    deckCardListBinding.Add(deckCard);
                }
            }
            while (deckCardListBinding.Count < 40)
            {
                DeckLeaderRank rank = new DeckLeaderRank((int)DeckLeaderRankType.NCO);
                CardConstant cardToAdd;
                do
                {
                    cardToAdd = possibleCards[rand.Next(0, possibleCards.Count)];
                } while (GetAmountOfCardDuplicatesInDeck(cardToAdd.Index) >= 3);

                DeckCard deckCard = new DeckCard(cardToAdd, rank);
                deckCardListBinding.Add(deckCard);
            }
            refreshDeckInfoLabels();
        }
        private void AddPegasusDeck()
        {
            List<CardConstant> possibleCards = new List<CardConstant>();

            List<int> guaranteedCards = new List<int>()
            {
                16,
                17,
                29,
                41,
                188,
                225,
                270,
                341,
                362,
                454,
                611,
                695,
                713,
                713,
                713,
                781,
                781,
                782,
                782
            };

            List<int> extraCards = new List<int>()
            {
                16,
                17,
                29,
                37,
                41,
                59,
                61,
                146,
                148,
                188,
                188,
                225,
                225,
                270,
                270,
                341,
                362,
                454,
                452,
                611,
                611,
                613,
                683,
                687,
                688,
                695,
                700,
                715,
                732,
                733,
                734,
                780,
                781,
                782,
                801,
                802,
                803,
                804,
                810,
                812,
                813,
                815,
                823,
                830
            };

            foreach (int cardid in extraCards)
            {
                if (!IsCardBanned(cardid))
                {
                    possibleCards.Add(CardConstant.List[cardid]);
                }
            }
            foreach (int guaranteedCard in guaranteedCards)
            {
                if (!IsCardBanned(guaranteedCard) && GetAmountOfCardDuplicatesInDeck(guaranteedCard) <= 3)
                {
                    DeckLeaderRank rank = new DeckLeaderRank((int)DeckLeaderRankType.NCO);
                    CardConstant card = CardConstant.List[guaranteedCard];
                    DeckCard deckCard = new DeckCard(card, rank);
                    deckCardListBinding.Add(deckCard);
                }
            }
            while (deckCardListBinding.Count < 40)
            {
                DeckLeaderRank rank = new DeckLeaderRank((int)DeckLeaderRankType.NCO);
                CardConstant cardToAdd;
                do
                {
                    cardToAdd = possibleCards[rand.Next(0, possibleCards.Count)];
                } while (GetAmountOfCardDuplicatesInDeck(cardToAdd.Index) >= 3);

                DeckCard deckCard = new DeckCard(cardToAdd, rank);
                deckCardListBinding.Add(deckCard);
            }
            refreshDeckInfoLabels();
        }
        private void AddRichardDeck()
        {
            List<CardConstant> possibleCards = new List<CardConstant>();

            List<int> guaranteedCards = new List<int>()
            {
                752,
                752,
                753,
                755,
                755,
                778,
                778,
                796
            };

            List<int> extraCards = new List<int>()
            {
                140,
                163,
                175,
                191,
                674,
                692,
                702,
                705,
                706,
                752,
                753,
                773,
                778,
                791,
                796,
                820,
                823,
                824,
                827
            };

            List<CardConstant> warriors = GetListOfMonstersWithAttackXOrLess(2700, GetListOfMonstersWithAttackXOrMore(1200, GetListOfMonstersOfType(3)));
            List<CardConstant> beastwarriors = GetListOfMonstersWithAttackXOrMore(1700, GetListOfMonstersOfType(4));
            List<CardConstant> limitedtraps = GetListOfRandomLimitedRangeTrap(60, 15);

            foreach (CardConstant card in warriors)
            {
                if (!IsCardBanned(card.Index))
                {
                    possibleCards.Add(card);
                    possibleCards.Add(card);
                }
            }
            foreach (CardConstant card in beastwarriors)
            {
                if (!IsCardBanned(card.Index))
                {
                    possibleCards.Add(card);
                    possibleCards.Add(card);
                }
            }
            foreach (CardConstant card in limitedtraps)
            {
                if (!IsCardBanned(card.Index))
                {
                    possibleCards.Add(card);
                }
            }
            foreach (int cardid in extraCards)
            {
                possibleCards.Add(CardConstant.List[cardid]);
            }
            foreach (int guaranteedCard in guaranteedCards)
            {
                if (!IsCardBanned(guaranteedCard) && GetAmountOfCardDuplicatesInDeck(guaranteedCard) <= 3)
                {
                    DeckLeaderRank rank = new DeckLeaderRank((int)DeckLeaderRankType.NCO);
                    CardConstant card = CardConstant.List[guaranteedCard];
                    DeckCard deckCard = new DeckCard(card, rank);
                    deckCardListBinding.Add(deckCard);
                }
            }
            while (deckCardListBinding.Count < 40)
            {
                DeckLeaderRank rank = new DeckLeaderRank((int)DeckLeaderRankType.NCO);
                CardConstant cardToAdd;
                do
                {
                    cardToAdd = possibleCards[rand.Next(0, possibleCards.Count)];
                } while (GetAmountOfCardDuplicatesInDeck(cardToAdd.Index) >= 3);

                DeckCard deckCard = new DeckCard(cardToAdd, rank);
                deckCardListBinding.Add(deckCard);
            }
            refreshDeckInfoLabels();
        }
        private void AddTeaDeck()
        {
            List<CardConstant> possibleCards = new List<CardConstant>();

            List<int> guaranteedCards = new List<int>()
            {
                706,
                735,
                763,
                763
            };

            List<int> extraCards = new List<int>()
            {
                35,
                691,
                703,
                704,
                705,
                706,
                716,
                735,
                751,
                758,
                763,
                767,
                768,
                781,
                792,
                804,
                808,
                809
            };

            List<CardConstant> fairies = GetListOfMonstersOfType(8);

            foreach (CardConstant card in fairies)
            {
                if (!IsCardBanned(card.Index) && card.Index != 371)
                {
                    possibleCards.Add(card);
                    possibleCards.Add(card);
                }
            }
            foreach (int cardid in extraCards)
            {
                if (!IsCardBanned(cardid))
                {
                    possibleCards.Add(CardConstant.List[cardid]);
                }
            }
            foreach (int guaranteedCard in guaranteedCards)
            {
                if (!IsCardBanned(guaranteedCard) && GetAmountOfCardDuplicatesInDeck(guaranteedCard) <= 3)
                {
                    DeckLeaderRank rank = new DeckLeaderRank((int)DeckLeaderRankType.NCO);
                    CardConstant card = CardConstant.List[guaranteedCard];
                    DeckCard deckCard = new DeckCard(card, rank);
                    deckCardListBinding.Add(deckCard);
                }
            }
            while (deckCardListBinding.Count < 40)
            {
                DeckLeaderRank rank = new DeckLeaderRank((int)DeckLeaderRankType.NCO);
                CardConstant cardToAdd;
                do
                {
                    cardToAdd = possibleCards[rand.Next(0, possibleCards.Count)];
                } while (GetAmountOfCardDuplicatesInDeck(cardToAdd.Index) >= 3);

                DeckCard deckCard = new DeckCard(cardToAdd, rank);
                deckCardListBinding.Add(deckCard);
            }
            refreshDeckInfoLabels();
        }
        private void AddTristanDeck()
        {
            List<CardConstant> possibleCards = new List<CardConstant>();

            List<int> guaranteedCards = new List<int>()
            {
                310,
                325,
                330,
                330,
                846
            };

            List<int> extraCards = new List<int>()
            {
                317,
                329,
                330,
                323,
                440,
                731,
                750,
                751
            };

            List<CardConstant> monsters = GetListOfMonstersWithAttackXOrLess(2000, GetListOfMonstersWithAttackXOrMore(1300, CardConstant.Monsters));

            foreach (CardConstant card in monsters)
            {
                if (!IsCardBanned(card.Index) && card.Kind.Id != 20)
                {
                    possibleCards.Add(card);
                }
            }
            foreach (int cardid in extraCards)
            {
                if (!IsCardBanned(cardid))
                {
                    possibleCards.Add(CardConstant.List[cardid]);
                }
            }
            foreach (int guaranteedCard in guaranteedCards)
            {
                if (!IsCardBanned(guaranteedCard) && GetAmountOfCardDuplicatesInDeck(guaranteedCard) <= 3)
                {
                    DeckLeaderRank rank = new DeckLeaderRank((int)DeckLeaderRankType.NCO);
                    CardConstant card = CardConstant.List[guaranteedCard];
                    DeckCard deckCard = new DeckCard(card, rank);
                    deckCardListBinding.Add(deckCard);
                }
            }
            while (deckCardListBinding.Count < 40)
            {
                DeckLeaderRank rank = new DeckLeaderRank((int)DeckLeaderRankType.NCO);
                CardConstant cardToAdd;
                do
                {
                    cardToAdd = possibleCards[rand.Next(0, possibleCards.Count)];
                } while (GetAmountOfCardDuplicatesInDeck(cardToAdd.Index) >= 3);

                DeckCard deckCard = new DeckCard(cardToAdd, rank);
                deckCardListBinding.Add(deckCard);
            }
            refreshDeckInfoLabels();
        }
        private void AddMaiDeck()
        {
            List<CardConstant> possibleCards = new List<CardConstant>();

            List<int> guaranteedCards = new List<int>()
            {
                8,
                8,
                30,
                187,
                272,
                272,
                272,
                767,
                768,
                768,
                776,
                776,
                785,
                785,
                797,
                797,
                843
            };

            List<int> extraCards = new List<int>()
            {
                8,
                22,
                30,
                31,
                33,
                195,
                199,
                204,
                685,
                691,
                702,
                704,
                712,
                740,
                743,
                767,
                768,
                776,
                785,
                795,
                796,
                797,
                816,
                818,
                822,
                828,
            };

            List<CardConstant> wingedbeasts = GetListOfMonstersOfType(6);
            List<CardConstant> traps = GetListOfRandomLimitedRangeTrap(20, 10);

            foreach (CardConstant card in wingedbeasts)
            {
                if (!IsCardBanned(card.Index))
                {
                    possibleCards.Add(card);
                    possibleCards.Add(card);
                }
            }
            foreach (CardConstant card in traps)
            {
                if (!IsCardBanned(card.Index))
                {
                    possibleCards.Add(card);
                }
            }
            foreach (int cardid in extraCards)
            {
                if (!IsCardBanned(cardid))
                {
                    possibleCards.Add(CardConstant.List[cardid]);
                }
            }
            foreach (int guaranteedCard in guaranteedCards)
            {
                if (!IsCardBanned(guaranteedCard) && GetAmountOfCardDuplicatesInDeck(guaranteedCard) <= 3)
                {
                    DeckLeaderRank rank = new DeckLeaderRank((int)DeckLeaderRankType.NCO);
                    CardConstant card = CardConstant.List[guaranteedCard];
                    DeckCard deckCard = new DeckCard(card, rank);
                    deckCardListBinding.Add(deckCard);
                }
            }
            while (deckCardListBinding.Count < 40)
            {
                DeckLeaderRank rank = new DeckLeaderRank((int)DeckLeaderRankType.NCO);
                CardConstant cardToAdd;
                do
                {
                    cardToAdd = possibleCards[rand.Next(0, possibleCards.Count)];
                } while (GetAmountOfCardDuplicatesInDeck(cardToAdd.Index) >= 3);

                DeckCard deckCard = new DeckCard(cardToAdd, rank);
                deckCardListBinding.Add(deckCard);
            }
            refreshDeckInfoLabels();
        }
        private void AddMakoDeck()
        {
            List<CardConstant> possibleCards = new List<CardConstant>();

            List<int> guaranteedCards = new List<int>()
            {
                476,
                479,
                532,
                777,
                777,
                783,
                783,
                852,
            };

            List<int> extraCards = new List<int>()
            {
                532,
                693,
                701,
                703,
                705,
                711,
                713,
                742,
                747,
                777,
                783,
                784,
                804,
                805,
                807,
                809,
                810,
                819,
                820,
                828
            };

            List<CardConstant> aquas = GetListOfMonstersWithAttackXOrLess(2400, GetListOfMonstersOfType(16));
            List<CardConstant> fishes = GetListOfMonstersWithAttackXOrLess(2200, GetListOfMonstersOfType(12));
            List<CardConstant> seaserpents = GetListOfMonstersOfType(13);

            foreach (CardConstant card in aquas)
            {
                if (!IsCardBanned(card.Index))
                {
                    possibleCards.Add(card);
                }
            }
            foreach (CardConstant card in fishes)
            {
                if (!IsCardBanned(card.Index))
                {
                    possibleCards.Add(card);
                }
            }
            foreach (CardConstant card in seaserpents)
            {
                if (!IsCardBanned(card.Index))
                {
                    possibleCards.Add(card);
                }
            }
            foreach (int cardid in extraCards)
            {
                if (!IsCardBanned(cardid))
                {
                    possibleCards.Add(CardConstant.List[cardid]);
                }
            }
            foreach (int guaranteedCard in guaranteedCards)
            {
                if (!IsCardBanned(guaranteedCard) && GetAmountOfCardDuplicatesInDeck(guaranteedCard) <= 3)
                {
                    DeckLeaderRank rank = new DeckLeaderRank((int)DeckLeaderRankType.NCO);
                    CardConstant card = CardConstant.List[guaranteedCard];
                    DeckCard deckCard = new DeckCard(card, rank);
                    deckCardListBinding.Add(deckCard);
                }
            }
            while (deckCardListBinding.Count < 40)
            {
                DeckLeaderRank rank = new DeckLeaderRank((int)DeckLeaderRankType.NCO);
                CardConstant cardToAdd;
                do
                {
                    cardToAdd = possibleCards[rand.Next(0, possibleCards.Count)];
                } while (GetAmountOfCardDuplicatesInDeck(cardToAdd.Index) >= 3);

                DeckCard deckCard = new DeckCard(cardToAdd, rank);
                deckCardListBinding.Add(deckCard);
            }
            refreshDeckInfoLabels();
        }
        private void AddJoeyDeck()
        {
            List<CardConstant> possibleCards = new List<CardConstant>();

            List<int> guaranteedCards = new List<int>()
            {
                7,
                21,
                36,
                752,
                752,
                755,
                755,
                778,
                778,
                779,
                788,
            };

            List<int> extraCards = new List<int>()
            {
                1,
                4,
                7,
                11,
                16,
                21,
                24,
                26,
                34,
                77,
                94,
                103,
                245,
                261,
                264,
                485,
                491,
                497,
                529,
                614,
                615,
                616,
                618,
                686,
                687,
                701,
                704,
                708,
                711,
                712,
                727,
                729,
                734,
                736,
                750,
                752,
                752,
                755,
                755,
                766,
                778,
                779,
                780,
                792,
                799,
                801,
                804,
                804,
                805,
                809,
                810,
                811,
                818,
                823,
                824
            };

            List<CardConstant> warriors = GetListOfMonstersWithAttackXOrMore(800, GetListOfMonstersWithAttackXOrLess(2600, GetListOfMonstersOfType(3)));
            List<CardConstant> beastwarriors = GetListOfMonstersWithAttackXOrLess(2100, GetListOfMonstersOfType(4));

            foreach (CardConstant card in warriors)
            {
                if (!IsCardBanned(card.Index))
                {
                    possibleCards.Add(card);
                }
            }
            foreach (CardConstant card in beastwarriors)
            {
                if (!IsCardBanned(card.Index))
                {
                    possibleCards.Add(card);
                }
            }
            foreach (int cardid in extraCards)
            {
                if (!IsCardBanned(cardid))
                {
                    possibleCards.Add(CardConstant.List[cardid]);
                }
            }
            foreach (int guaranteedCard in guaranteedCards)
            {
                if (!IsCardBanned(guaranteedCard) && GetAmountOfCardDuplicatesInDeck(guaranteedCard) <= 3)
                {
                    DeckLeaderRank rank = new DeckLeaderRank((int)DeckLeaderRankType.NCO);
                    CardConstant card = CardConstant.List[guaranteedCard];
                    DeckCard deckCard = new DeckCard(card, rank);
                    deckCardListBinding.Add(deckCard);
                }
            }
            while (deckCardListBinding.Count < 40)
            {
                DeckLeaderRank rank = new DeckLeaderRank((int)DeckLeaderRankType.NCO);
                CardConstant cardToAdd;
                do
                {
                    cardToAdd = possibleCards[rand.Next(0, possibleCards.Count)];
                } while (GetAmountOfCardDuplicatesInDeck(cardToAdd.Index) >= 3);

                DeckCard deckCard = new DeckCard(cardToAdd, rank);
                deckCardListBinding.Add(deckCard);
            }
            refreshDeckInfoLabels();
        }
        private void AddShadiDeck()
        {
            List<CardConstant> possibleCards = new List<CardConstant>();

            List<int> guaranteedCards = new List<int>()
            {
                770,
                784,
                784,
                784,
                786,
            };

            List<int> extraCards = new List<int>()
            {
                47,
                72,
                89,
                274,
                292,
                333,
                338,
                342,
                343,
                345,
                349,
                352,
                364,
                367,
                489,
                498,
                712,
                713,
                770,
                773,
                780,
                781,
                782,
                782,
                786,
                796,
                801,
                802,
                803,
                804,
                810,
                810,
                811,
                814,
                828
            };

            List<CardConstant> thunders = GetListOfMonstersWithAttackXOrLess(3000, GetListOfMonstersOfType(15));
            List<CardConstant> pyros = GetListOfMonstersWithAttackXOrLess(3000, GetListOfMonstersOfType(17));
            List<CardConstant> rocks = GetListOfMonstersWithAttackXOrLess(3000, GetListOfMonstersOfType(18));

            foreach (CardConstant card in thunders)
            {
                if (!IsCardBanned(card.Index))
                {
                    possibleCards.Add(card);
                }
            }
            foreach (CardConstant card in pyros)
            {
                if (!IsCardBanned(card.Index))
                {
                    possibleCards.Add(card);
                }
            }
            foreach (CardConstant card in rocks)
            {
                if (!IsCardBanned(card.Index))
                {
                    possibleCards.Add(card);
                }
            }
            foreach (int cardid in extraCards)
            {
                if (!IsCardBanned(cardid))
                {
                    possibleCards.Add(CardConstant.List[cardid]);
                }
            }
            foreach (int guaranteedCard in guaranteedCards)
            {
                if (!IsCardBanned(guaranteedCard) && GetAmountOfCardDuplicatesInDeck(guaranteedCard) <= 3)
                {
                    DeckLeaderRank rank = new DeckLeaderRank((int)DeckLeaderRankType.NCO);
                    CardConstant card = CardConstant.List[guaranteedCard];
                    DeckCard deckCard = new DeckCard(card, rank);
                    deckCardListBinding.Add(deckCard);
                }
            }
            while (deckCardListBinding.Count < 40)
            {
                DeckLeaderRank rank = new DeckLeaderRank((int)DeckLeaderRankType.NCO);
                CardConstant cardToAdd;
                do
                {
                    cardToAdd = possibleCards[rand.Next(0, possibleCards.Count)];
                } while (GetAmountOfCardDuplicatesInDeck(cardToAdd.Index) >= 3);

                DeckCard deckCard = new DeckCard(cardToAdd, rank);
                deckCardListBinding.Add(deckCard);
            }
            refreshDeckInfoLabels();
        }
        private void AddGrandpaDeck()
        {
            List<CardConstant> possibleCards = new List<CardConstant>();

            List<int> guaranteedCards = new List<int>()
            {
                711,
                712,
                713,
                714,
                762,
                762,
                762,
                771,
                771,
            };

            List<int> extraCards = new List<int>()
            {
                486,
                684,
                687,
                688,
                694,
                699,
                700,
                702,
                705,
                707,
                711,
                712,
                713,
                713,
                714,
                714,
                715,
                731,
                737,
                747,
                749,
                771,
                780,
            };

            List<CardConstant> spellcasters = GetListOfMonstersWithAttackXOrLess(3000, GetListOfMonstersOfType(1));
            List<CardConstant> limitedtraps = GetListOfRandomLimitedRangeTrap(60, 15);
            List<CardConstant> fullrangetraps = GetListOfRandomFullRangeTrap(80, 10);

            foreach (CardConstant card in spellcasters)
            {
                if (!IsCardBanned(card.Index))
                {
                    possibleCards.Add(card);
                }
            }
            foreach (CardConstant card in limitedtraps)
            {
                if (!IsCardBanned(card.Index))
                {
                    possibleCards.Add(card);
                }
            }
            foreach (CardConstant card in fullrangetraps)
            {
                if (!IsCardBanned(card.Index))
                {
                    possibleCards.Add(card);
                }
            }
            foreach (int cardid in extraCards)
            {
                if (!IsCardBanned(cardid))
                {
                    possibleCards.Add(CardConstant.List[cardid]);
                }
            }
            foreach (int guaranteedCard in guaranteedCards)
            {
                if (!IsCardBanned(guaranteedCard) && GetAmountOfCardDuplicatesInDeck(guaranteedCard) <= 3)
                {
                    DeckLeaderRank rank = new DeckLeaderRank((int)DeckLeaderRankType.NCO);
                    CardConstant card = CardConstant.List[guaranteedCard];
                    DeckCard deckCard = new DeckCard(card, rank);
                    deckCardListBinding.Add(deckCard);
                }
            }
            while (deckCardListBinding.Count < 40)
            {
                DeckLeaderRank rank = new DeckLeaderRank((int)DeckLeaderRankType.NCO);
                CardConstant cardToAdd;
                do
                {
                    cardToAdd = possibleCards[rand.Next(0, possibleCards.Count)];
                } while (GetAmountOfCardDuplicatesInDeck(cardToAdd.Index) >= 3);

                DeckCard deckCard = new DeckCard(cardToAdd, rank);
                deckCardListBinding.Add(deckCard);
            }
            refreshDeckInfoLabels();
        }
        private void AddBakuraDeck()
        {
            List<CardConstant> possibleCards = new List<CardConstant>();

            List<int> guaranteedCards = new List<int>()
            {
                739,
                759,
                761,
                761,
                761,
                769
            };

            List<int> extraCards = new List<int>()
            {
                228,
                230,
                231,
                238,
                245,
                248,
                251,
                292,
                305,
                317,
                318,
                329,
                336,
                670,
                668,
                673,
                679,
                681,
                684,
                706,
                713,
                732,
                734,
                739,
                759,
                769,
                773,
                793,
                794,
                823,
                827
            };

            List<CardConstant> plants = GetListOfMonstersWithAttackXOrLess(1400, GetListOfMonstersOfType(19));
            List<CardConstant> limitedtraps = GetListOfRandomLimitedRangeTrap(60, 15);

            foreach (CardConstant card in plants)
            {
                if (!IsCardBanned(card.Index))
                {
                    possibleCards.Add(card);
                }
            }
            foreach (CardConstant card in limitedtraps)
            {
                if (!IsCardBanned(card.Index))
                {
                    possibleCards.Add(card);
                }
            }
            foreach (int cardid in extraCards)
            {
                possibleCards.Add(CardConstant.List[cardid]);
            }
            foreach (int guaranteedCard in guaranteedCards)
            {
                if (!IsCardBanned(guaranteedCard) && GetAmountOfCardDuplicatesInDeck(guaranteedCard) <= 3)
                {
                    DeckLeaderRank rank = new DeckLeaderRank((int)DeckLeaderRankType.NCO);
                    CardConstant card = CardConstant.List[guaranteedCard];
                    DeckCard deckCard = new DeckCard(card, rank);
                    deckCardListBinding.Add(deckCard);
                }
            }
            while (deckCardListBinding.Count < 40)
            {
                DeckLeaderRank rank = new DeckLeaderRank((int)DeckLeaderRankType.NCO);
                CardConstant cardToAdd;
                do
                {
                    cardToAdd = possibleCards[rand.Next(0, possibleCards.Count)];
                } while (GetAmountOfCardDuplicatesInDeck(cardToAdd.Index) >= 3);

                DeckCard deckCard = new DeckCard(cardToAdd, rank);
                deckCardListBinding.Add(deckCard);
            }
            refreshDeckInfoLabels();
        }
        private void AddYugiDeck()
        {
            List<CardConstant> possibleCards = new List<CardConstant>();

            List<int> guaranteedCards = new List<int>()
            {
                60,
                87,
                754,
                762
            };

            List<int> extraCards = new List<int>()
            {
                4,
                5,
                6,
                7,
                23,
                43,
                60,
                76,
                78,
                87,
                92,
                143,
                155,
                212,
                294,
                417,
                459,
                535,
                540,
                626,
                634,
                675,
                685,
                686,
                687,
                696,
                699,
                700,
                701,
                702,
                706,
                707,
                714,
                715,
                732,
                734,
                736,
                750,
                751,
                754,
                762,
                771,
                780,
                780,
                796,
            };

            List<CardConstant> limitedtraps = GetListOfRandomLimitedRangeTrap(60, 10);
            List<CardConstant> fullrangetraps = GetListOfRandomFullRangeTrap(99, 20);

            foreach (CardConstant card in limitedtraps)
            {
                if (!IsCardBanned(card.Index))
                {
                    possibleCards.Add(card);
                }
            }
            foreach (CardConstant card in fullrangetraps)
            {
                if (!IsCardBanned(card.Index))
                {
                    possibleCards.Add(card);
                }
            }
            foreach (int cardid in extraCards)
            {
                if (!IsCardBanned(cardid))
                {
                    possibleCards.Add(CardConstant.List[cardid]);
                }
            }
            foreach (int guaranteedCard in guaranteedCards)
            {
                if (!IsCardBanned(guaranteedCard) && GetAmountOfCardDuplicatesInDeck(guaranteedCard) <= 3)
                {
                    DeckLeaderRank rank = new DeckLeaderRank((int)DeckLeaderRankType.NCO);
                    CardConstant card = CardConstant.List[guaranteedCard];
                    DeckCard deckCard = new DeckCard(card, rank);
                    deckCardListBinding.Add(deckCard);
                }
            }
            while (deckCardListBinding.Count < 25)
            {
                DeckLeaderRank rank = new DeckLeaderRank((int)DeckLeaderRankType.NCO);
                CardConstant cardToAdd;
                do
                {
                    cardToAdd = possibleCards[rand.Next(0, possibleCards.Count)];
                } while (GetAmountOfCardDuplicatesInDeck(cardToAdd.Index) >= 3);

                DeckCard deckCard = new DeckCard(cardToAdd, rank);
                deckCardListBinding.Add(deckCard);
            }
            while (deckCardListBinding.Count < 40)
            {
                DeckLeaderRank rank = new DeckLeaderRank((int)DeckLeaderRankType.NCO);
                CardConstant cardToAdd;
                do
                {
                    cardToAdd = CardConstant.List[rand.Next(0, CardConstant.List.Count)];
                } while (GetAmountOfCardDuplicatesInDeck(cardToAdd.Index) >= 3 && cardToAdd.Kind.Id != 160);

                DeckCard deckCard = new DeckCard(cardToAdd, rank);
                deckCardListBinding.Add(deckCard);
            }
            refreshDeckInfoLabels();
        }
        private void AddManawyddanWhite()
        {
            List<CardConstant> possibleCards = new List<CardConstant>();

            List<int> guaranteedCards = new List<int>()
            {
                75,
                713,
                713,
                714,
                714,
                789
            };

            List<int> extraCards = new List<int>()
            {
                22,
                44,
                75,
                111,
                115,
                146,
                154,
                155,
                172,
                183,
                224,
                293,
                308,
                312,
                366,
                672,
                694,
                677,
                684,
                685,
                686,
                687,
                688,
                699,
                700,
                701,
                702,
                707,
                713,
                714,
                715,
                732,
                733,
                734,
                748,
                780,
                789,
                789,
                794,
                795,
                796
            };

            List<CardConstant> limitedtraps = GetListOfRandomLimitedRangeTrap(60, 30);
            List<CardConstant> fullrangetraps = GetListOfRandomFullRangeTrap(99, 30);

            List<CardConstant> monsters = GetListOfMonstersWithAttackXOrLess(3200, GetListOfMonstersWithAttackXOrMore(2500, CardConstant.Monsters));

            foreach (CardConstant card in limitedtraps)
            {
                if (!IsCardBanned(card.Index))
                {
                    possibleCards.Add(card);
                    possibleCards.Add(card);
                }
            }
            foreach (CardConstant card in fullrangetraps)
            {
                if (!IsCardBanned(card.Index) && card.Index != 825)
                {
                    possibleCards.Add(card);
                    possibleCards.Add(card);
                }
            }
            foreach (CardConstant card in monsters)
            {
                if (!IsCardBanned(card.Index) && card.Kind.Id != 20)
                {
                    possibleCards.Add(card);
                }
            }
            foreach (int cardid in extraCards)
            {
                if (!IsCardBanned(cardid))
                {
                    possibleCards.Add(CardConstant.List[cardid]);
                    possibleCards.Add(CardConstant.List[cardid]);
                }
            }
            foreach (int guaranteedCard in guaranteedCards)
            {
                if (!IsCardBanned(guaranteedCard) && GetAmountOfCardDuplicatesInDeck(guaranteedCard) <= 3)
                {
                    DeckLeaderRank rank = new DeckLeaderRank((int)DeckLeaderRankType.NCO);
                    CardConstant card = CardConstant.List[guaranteedCard];
                    DeckCard deckCard = new DeckCard(card, rank);
                    deckCardListBinding.Add(deckCard);
                }
            }
            while (deckCardListBinding.Count < 40)
            {
                DeckLeaderRank rank = new DeckLeaderRank((int)DeckLeaderRankType.NCO);
                CardConstant cardToAdd;
                do
                {
                    cardToAdd = possibleCards[rand.Next(0, possibleCards.Count)];
                } while (GetAmountOfCardDuplicatesInDeck(cardToAdd.Index) >= 3);

                DeckCard deckCard = new DeckCard(cardToAdd, rank);
                deckCardListBinding.Add(deckCard);
            }
            refreshDeckInfoLabels();
        }
        private void AddManawyddanRed()
        {
            List<CardConstant> possibleCards = new List<CardConstant>();

            List<int> guaranteedCards = new List<int>()
            {
                224,
                364,
                748,
                780,
                780,
                780,
                789
            };

            List<int> extraCards = new List<int>()
            {
                111,
                146,
                149,
                224,
                224,
                364,
                682,
                684,
                685,
                687,
                688,
                699,
                700,
                701,
                702,
                707,
                713,
                714,
                715,
                732,
                733,
                734,
                748,
                789,
                789,
                795,
                796
            };

            List<CardConstant> limitedtraps = GetListOfRandomLimitedRangeTrap(60, 30);
            List<CardConstant> fullrangetraps = GetListOfRandomFullRangeTrap(99, 30);

            List<CardConstant> monsters = GetListOfMonstersWithAttackXOrLess(5200, GetListOfMonstersWithAttackXOrMore(2500, CardConstant.Monsters));

            foreach (CardConstant card in limitedtraps)
            {
                if (!IsCardBanned(card.Index))
                {
                    possibleCards.Add(card);
                }
            }
            foreach (CardConstant card in fullrangetraps)
            {
                if (!IsCardBanned(card.Index))
                {
                    possibleCards.Add(card);
                }
            }
            foreach (CardConstant card in monsters)
            {
                if (!IsCardBanned(card.Index) && card.Kind.Id != 20)
                {
                    possibleCards.Add(card);
                }
            }
            foreach (int cardid in extraCards)
            {
                if (!IsCardBanned(cardid))
                {
                    possibleCards.Add(CardConstant.List[cardid]);
                    possibleCards.Add(CardConstant.List[cardid]);
                }
            }
            foreach (int guaranteedCard in guaranteedCards)
            {
                if (!IsCardBanned(guaranteedCard) && GetAmountOfCardDuplicatesInDeck(guaranteedCard) <= 3)
                {
                    DeckLeaderRank rank = new DeckLeaderRank((int)DeckLeaderRankType.NCO);
                    CardConstant card = CardConstant.List[guaranteedCard];
                    DeckCard deckCard = new DeckCard(card, rank);
                    deckCardListBinding.Add(deckCard);
                }
            }
            while (deckCardListBinding.Count < 40)
            {
                DeckLeaderRank rank = new DeckLeaderRank((int)DeckLeaderRankType.NCO);
                CardConstant cardToAdd;
                do
                {
                    cardToAdd = possibleCards[rand.Next(0, possibleCards.Count)];
                } while (GetAmountOfCardDuplicatesInDeck(cardToAdd.Index) >= 3);

                DeckCard deckCard = new DeckCard(cardToAdd, rank);
                deckCardListBinding.Add(deckCard);
            }
            refreshDeckInfoLabels();
        }
        private void AddEnemyDeck(List<int> guaranteedcards, List<int> cardkindids, List<int> extracardids)
        {
            for (int i = 0; i < guaranteedcards.Count; i++)
            {
                DeckLeaderRank rank = new DeckLeaderRank((int)DeckLeaderRankType.NCO);

                CardConstant card = CardConstant.List[guaranteedcards[i]];

                if (!(GetAmountOfCardDuplicatesInDeck(card.Index) >= 3) && !IsCardBanned(card.Index))
                {
                    DeckCard deckCard = new DeckCard(card, rank);
                    deckCardListBinding.Add(deckCard);
                }
            }
            List<CardConstant> possibleCardList = new List<CardConstant>();

            for (int i = 0; i < CardConstant.List.Count; i++)
            {
                foreach (int id in cardkindids)
                {
                    if (CardConstant.List[i].Kind.Id == id)
                    {
                        possibleCardList.Add(CardConstant.List[i]);
                    }
                }
                foreach (int id in extracardids)
                {
                    possibleCardList.Add(CardConstant.List[id]);
                }
            }

            while (deckCardListBinding.Count < 40)
            {
                DeckLeaderRank rank = new DeckLeaderRank((int)DeckLeaderRankType.NCO);
                CardConstant card;
                do
                {
                    card = GetRandomCardFromList(possibleCardList);
                } while (GetAmountOfCardDuplicatesInDeck(card.Index) >= 3 || IsCardBanned(card.Index));

                DeckCard deckCard = new DeckCard(card, rank);
                deckCardListBinding.Add(deckCard);

                refreshDeckInfoLabels();
            }

            //add guaranteed cards
            //slå samman kort av vald typ och extra kort
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
                            cardToAdd = GetRandomCardAtOrOverXDefence(2000, CardConstant.Monsters);
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
                    cards.Add(GetRandomCardAtOrBelowXAttack(1500, CardConstant.Monsters));
                    cards.Add(GetRandomCardAtOrBelowXAttack(1500, CardConstant.Monsters));
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
                    cards.Add(GetRandomCardAtOrBelowXAttack(1000, CardConstant.Monsters));
                    cards.Add(GetRandomCardAtOrBelowXAttack(1000, CardConstant.Monsters));
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
                    cards.Add(GetRandomCardAtOrBelowXAttack(1500, CardConstant.Monsters));
                    cards.Add(GetRandomCardAtOrBelowXAttack(1500, CardConstant.Monsters));
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
            while (IsCardBanned(card.Index) || card.DeckCost > deckStrengthMultiplier * 33);
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
        private CardConstant GetRandomCardAtOrBelowXAttack(int maxAttack, List<CardConstant> list)
        {
            CardConstant card = null;
            List<CardConstant> cardsBelowXAtk = new List<CardConstant>();

            for (int i = 0; i < list.Count; i++)
            {
                if (list[i].Attack <= maxAttack && !IsCardBanned(i))
                    cardsBelowXAtk.Add(list[i]);
            }

            card = cardsBelowXAtk[rand.Next(0, cardsBelowXAtk.Count)];
            return card;
        }
        private CardConstant GetRandomCardAtOrAboveXAttack(int minAttack, List<CardConstant> list)
        {
            CardConstant card = null;
            List<CardConstant> cardsAtOrAboveXAtk = new List<CardConstant>();

            for (int i = 0; i < list.Count; i++)
            {
                if (list[i].Attack >= minAttack && !IsCardBanned(i))
                    cardsAtOrAboveXAtk.Add(list[i]);
            }

            card = cardsAtOrAboveXAtk[rand.Next(0, cardsAtOrAboveXAtk.Count)];
            return card;
        }
        private CardConstant GetRandomCardAtOrOverXDefence(int minDefence, List<CardConstant> list)
        {
            CardConstant card = null;
            List<CardConstant> cardsWithXDef = new List<CardConstant>();

            for (int i = 0; i < list.Count; i++)
            {
                if (list[i].Defense >= minDefence && !IsCardBanned(i))
                    cardsWithXDef.Add(list[i]);
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
        private CardConstant GetRandomLimitedRangeTrap(int maxdc)
        {
            CardConstant returncard;
            List<CardConstant> traps = new List<CardConstant>();

            foreach (CardConstant card in CardConstant.List)
            {
                if (card.Kind.Id == 96 && card.DeckCost <= maxdc)
                {
                    traps.Add(card);
                }
            }
            returncard = traps[rand.Next(0, traps.Count())];
            return returncard;
        }
        private CardConstant GetRandomFullRangeTrap(int maxdc)
        {
            CardConstant returncard;
            List<CardConstant> traps = new List<CardConstant>();

            foreach (CardConstant card in CardConstant.List)
            {
                if (card.Kind.Id == 128 && card.DeckCost <= maxdc)
                {
                    traps.Add(card);
                }
            }
            returncard = traps[rand.Next(0, traps.Count())];
            return returncard;
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
        private CardConstant GetRandomWheightedCard(int remaingingDc, List<CardConstant> cardlist, int maximumAllowedExtraCost = 0)
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
        private CardConstant GetRandomCardFromList(List<CardConstant> cardList)
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
        private List<CardConstant> GetListOfMonstersWithAttackXOrMore(int minAttack, List<CardConstant> list)
        {
            List<CardConstant> monsters = new List<CardConstant>();
            foreach (CardConstant card in list)
            {
                if (card.Attack >= minAttack && !IsCardBanned(card.Index))
                {
                    monsters.Add(card);
                }
            }
            return monsters;
        }
        private List<CardConstant> GetListOfMonstersWithAttackXOrLess(int maxAttack, List<CardConstant> list)
        {
            List<CardConstant> monsters = new List<CardConstant>();
            foreach (CardConstant card in list)
            {
                if (card.Attack <= maxAttack && !IsCardBanned(card.Index))
                {
                    monsters.Add(card);
                }
            }
            return monsters;
        }
        private List<CardConstant> GetListOfMonstersWithDefenceXOrMore(int minDefence, List<CardConstant> list)
        {
            List<CardConstant> monsters = new List<CardConstant>();
            foreach (CardConstant card in list)
            {
                if (card.Defense >= minDefence && !IsCardBanned(card.Index))
                {
                    monsters.Add(card);
                }
            }
            return monsters;
        }
        private List<CardConstant> GetListOfMonstersWithDefenceXOrless(int maxDefence, List<CardConstant> list)
        {
            List<CardConstant> monsters = new List<CardConstant>();
            foreach (CardConstant card in list)
            {
                if (card.Defense <= maxDefence && !IsCardBanned(card.Index))
                {
                    monsters.Add(card);
                }
            }
            return monsters;
        }
        private List<CardConstant> GetListOfRandomLimitedRangeTrap(int maxdc, int mindc)
        {
            List<CardConstant> traps = new List<CardConstant>();

            foreach (CardConstant card in CardConstant.List)
            {
                if (card.Kind.Id == 96 && card.DeckCost <= maxdc && card.DeckCost >= mindc)
                {
                    traps.Add(card);
                }
            }
            return traps;
        }
        private List<CardConstant> GetListOfRandomFullRangeTrap(int maxdc, int mindc)
        {
            List<CardConstant> traps = new List<CardConstant>();

            foreach (CardConstant card in CardConstant.List)
            {
                if (card.Kind.Id == 128 && card.DeckCost <= maxdc && card.DeckCost >= mindc)
                {
                    traps.Add(card);
                }
            }
            return traps;
        }
        private bool IsCardBanned(int cardId)
        {
            if (bannedCardIds.Contains(cardId) || CardConstant.List[cardId].Kind.Id == 20)
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
        private bool IsCardFieldSpell(CardConstant card)
        {
            if (card.Index >= 689 && card.Index <= 696)
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
        private void RestoreDefaultEnemyDecks(object sender, EventArgs e)
        {
            var path = "";
            //path = @"../../DefaultEnemyDecks.txt";
            path = Path.Combine(Directory.GetCurrentDirectory(), "DefaultEnemyDecks.txt");

            string[] lines = File.ReadAllLines(path);

            int counter = 0;
            for (int i = 27; i < 47; i++)
            {

                Deck selectedDeck = (Deck)deckDropdown.Items[i];
                deckCardListBinding = new SortableBindingList<DeckCard>(selectedDeck.CardList);
                deckEditorDataGridView.DataSource = deckCardListBinding;
                deckEditDeckLeaderRankComboBox.SelectedValue = ((Deck)deckDropdown.SelectedItem).DeckLeader.Rank.Index;
                removeAllCards();

                for (int x = 0; x < 40; x++)
                {
                    int cardid = int.Parse(lines[counter]);
                    CardConstant card = CardConstant.List[cardid];

                    DeckLeaderRank rank = new DeckLeaderRank((int)DeckLeaderRankType.NCO);
                    DeckCard deckCard = new DeckCard(card, rank);
                    deckCardListBinding.Add(deckCard);

                    counter++;
                }

                deckDropdown.SelectedItem = selectedDeck;
                saveDeck();
                refreshDeckInfoLabels();
            }


        }
        private void SaveDefaultEnemyDecks()
        {
            var path = "";
            //path = @"../../DefaultEnemyDecks.txt";
            path = Path.Combine(Directory.GetCurrentDirectory(), "DefaultEnemyDecks.txt");

            List<string> originalenemydeckslist = new List<string>();

            for (int i = 27; i < 47; i++)
            {
                Deck deck = (Deck)deckDropdown.Items[i];
                for (int x = 0; x < 40; x++)
                {
                    originalenemydeckslist.Add(deck.CardList[x].CardConstant.Index.ToString());
                }
            }

            string[] lines = originalenemydeckslist.ToArray();
            File.WriteAllLines(path, lines);



            //            var path = "";
            //#if DEBUG
            //            path = @"../../Banlist.txt";

            //#else
            //            path = Path.Combine(Directory.GetCurrentDirectory(), "Banlist.txt");
            //#endif
            //            Console.WriteLine(path);
            //            List<string> l = bannedCardIds.Select(x => x.ToString()).ToList();
            //            string[] lines = l.ToArray();
            //#if DEBUG
            //            File.WriteAllLines(@"../../Banlist.txt", lines);
            //#else
            //            File.WriteAllLines(path, lines);
            //#endif
            //            ////////////////////////////////////
            //            var path = "";
            //#if DEBUG
            //            path = @"../../OriginalFusions.txt";

            //#else
            //            path = Path.Combine(Directory.GetCurrentDirectory(), "OriginalFusions.txt");
            //#endif
            //            string[] lines = File.ReadAllLines(path);
            //            for (int i = 0; i < lines.Length; i++)
            //            {
            //                string[] monsterIds = lines[i].Split(',');
            //                fusions.fusions[i].LowerCardIndex = Convert.ToUInt16(Int32.Parse(monsterIds[0]));
            //                fusions.fusions[i].UpperCardIndex = Convert.ToUInt16(Int32.Parse(monsterIds[1]));
            //                fusions.fusions[i].FusionCardIndex = Convert.ToUInt16(Int32.Parse(monsterIds[2]));
            //            }

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