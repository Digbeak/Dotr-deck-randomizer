using DOTR_MODDING_TOOL.Classes;
using Equin.ApplicationFramework;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DOTR_Modding_Tool
{
    public partial class MainForm : Form
    {
        private SortableBindingList<CardConstant> draftDecklist;
        private List<int> mediumStrongSmallEffectMonsterIds = new List<int>()
        {
            41,
            42,
            53,
            62,
            67,
            72,
            74,
            80,
            89,
            95,
            98,
            112,
            116,
            118,
            123,
            140,
            152,
            156,
            163,
            181,
            206,
            216,
            224,
            228,
            247,
            255,
            268,
            274,
            276,
            295,
            297,
            305,
            311,
            316,
            317,
            325,
            346,
            349,
            361,
            364,
            374,
            382,
            384,
            388,
            389,
            404,
            415,
            456,
            470,
            493,
            495,
            498,
            500,
            571,
            582,
            591,
            613,
            621,
            627,
            629,
            635,
            642,
            652
        };
        private List<int> strongSmallEffectMonsterIds = new List<int>()
        {
            25,
            36,
            35,
            43,
            104,
            111,
            113,
            115,
            149,
            164,
            175,
            191,
            326,
            371,
            374,
            377,
            380,
            390,
            417,
            418,
            427,
            428,
            447,
            461,
            506,
            509,
            530,
            541,
            545,
            552,
            561,
            567,
            568,
            570,
            574,
            589,
            599,
            639,
            647,
            655,
            659
        };
        private List<int> regularMonsterIds = new List<int>();

        private List<CardConstant> mediumStrongSmallEffectMonsters = new List<CardConstant>();
        private List<CardConstant> strongSmallEffectMonsters = new List<CardConstant>();
        private List<CardConstant> regularMonsters = new List<CardConstant>();

        private void SetupDraftingList()
        {
            formatCardTable(draftTrunkDataGridView);
            formatCardTable(draftDataGridView);

            SetupDraftLists();

            NewDraftCards();

            List<CardConstant> list = new List<CardConstant>();

            draftDecklist = new SortableBindingList<CardConstant>(list);

            draftDataGridView.DataSource = draftDecklist;

            draftTrunkDataGridView.CellDoubleClick += draftTrunkDataGridView_DoubleClick;
        }

        private void SetupDraftLists()
        {
             foreach(CardConstant card in CardConstant.Monsters)
            {
                if(!mediumStrongSmallEffectMonsterIds.Contains(card.Index) && !strongSmallEffectMonsterIds.Contains(card.Index))
                {
                    regularMonsterIds.Add(card.Index);
                }
            }
            foreach(int id in mediumStrongSmallEffectMonsterIds)
            {
                mediumStrongSmallEffectMonsters.Add(CardConstant.List[id]);
            }
            foreach(int id in strongSmallEffectMonsterIds)
            {
                strongSmallEffectMonsters.Add(CardConstant.List[id]);
            }
            foreach(int id in regularMonsterIds)
            {
                regularMonsters.Add(CardConstant.List[id]);
            }

        }

        private void draftTrunkDataGridView_DoubleClick(Object sender, DataGridViewCellEventArgs e)
        {
            // avoid errors when doubleclicking column headers
            if (e.RowIndex < 0 || draftDecklist.Count >= 40)
            {
                return;
            }

            List<DataGridViewRow> rows = new List<DataGridViewRow> { draftTrunkDataGridView.Rows[e.RowIndex] };
            addCardToDraftDeck(rows);
            draftCardAmountLabel.Text = GetDraftDeckCardAmountString();
            draftDeckCostLable.Text = "Dc:" + GetDraftDeckCost().ToString();

            if (draftDecklist.Count >= 40)
            {
                SaveDraftDeck();
            }
        }
        private int GetDraftDeckCost()
        {
            int dc = 0;
            foreach(CardConstant card in draftDecklist)
            {
                dc += card.DeckCost;
            }
            return dc;
        }
        private void addCardToDraftDeck(List<DataGridViewRow> rows)
        {
            foreach (DataGridViewRow row in rows)
            {
                CardConstant card = ((ObjectView<CardConstant>)row.DataBoundItem).Object;
                draftDecklist.Add(card);
            }
            NewDraftCards();
        }

        private void NewDraftCards()
        {
            List<CardConstant> draftableCards = new List<CardConstant>();

            int chance = rand.Next(0, 100);
            List<CardConstant> cardlist = new List<CardConstant>();
            int maxMonsterAttack = 0;

            if(chance < 50)
            {
                cardlist = regularMonsters;
                maxMonsterAttack = 1500;
            }
            else if(chance < 68)
            {
                cardlist = mediumStrongSmallEffectMonsters;
                maxMonsterAttack = 2000;
            }
            else if (chance < 77)
            {
                cardlist =  GetListOfMonstersWithAttackXOrMore(1500, regularMonsters);
                maxMonsterAttack = 1900;
            }
            else if (chance < 80)
            {
                cardlist = strongSmallEffectMonsters;
                maxMonsterAttack = 2000;
            }
            else if (chance < 92)
            {
                cardlist = GetListOfRandomFullRangeTrap(35, 0);
                List<CardConstant> cards = GetListOfRandomLimitedRangeTrap(20, 0);
                foreach(CardConstant card in cards)
                {
                    cardlist.Add(card);
                }
                for (int i = 0; i < 3; i++)
                {
                    draftableCards.Add(cardlist[rand.Next(0, cardlist.Count)]);
                }
            }
            else
            {
                for (int i = 0; i < 3; i++)
                {
                    draftableCards.Add(GetRandomPowerup());
                }
            }

            if(chance < 80)
            {
                for (int i = 0; i < 3; i++)
                {
                    CardConstant card = GetRandomCardAtOrBelowXAttack(maxMonsterAttack, cardlist);
                    draftableCards.Add(card);
                }
            }

       

            draftTrunkDataGridView.DataSource = new BindingListView<CardConstant>(draftableCards);
        }

        private string GetDraftDeckCardAmountString()
        {
            if(draftDecklist == null)
            {
                return "0/40 cards";
            }
            return draftDecklist.Count.ToString() + "/40 cards";
        }

        private void SaveDraftDeck()
        {
            //SortableBindingList<DeckCard> deck = new SortableBindingList<DeckCard>();
            //foreach(CardConstant card in draftDecklist)
            //{
            //    DeckLeaderRank rank = new DeckLeaderRank((int)DeckLeaderRankType.NCO);
            //    DeckCard deckCard = new DeckCard(card, rank);
            //    deck.Add(deckCard);
            //}
            for (int i = 0; i < 17; i++)
            {
                Deck selectedDeck = (Deck)deckDropdown.Items[i];
                deckCardListBinding = new SortableBindingList<DeckCard>(selectedDeck.CardList);
                deckEditorDataGridView.DataSource = deckCardListBinding;

                removeAllCards();

                foreach(CardConstant card in draftDecklist)
                {
                    DeckLeaderRank rank = new DeckLeaderRank((int)DeckLeaderRankType.NCO);
                    DeckCard deckCard = new DeckCard(card, rank);
                    deckCardListBinding.Add(deckCard);
                }

                deckEditDeckLeaderRankComboBox.SelectedValue = ((Deck)deckDropdown.SelectedItem).DeckLeader.Rank.Index;
                deckDropdown.SelectedItem = selectedDeck;
                RandomizeDeckLeader();
                saveDeck();
            }
            refreshDeckInfoLabels();
        }
    }
}
