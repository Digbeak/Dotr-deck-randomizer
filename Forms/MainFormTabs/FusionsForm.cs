namespace DOTR_Modding_Tool
{
    using Equin.ApplicationFramework;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.IO;
    using System.Linq;
    using System.Windows.Forms;

    public partial class MainForm : Form
    {
        private Fusions fusions;
        private BindingListView<Fusion> fusionsBinding;
        private bool fusionDataGridviewIsSetup = false;

        private void LoadFusionData()
        {
            this.fusionsDataGridView.DataSource = null;
            byte[] fusionBytes = this.dataAccess.LoadFusionData();
            this.fusions = new Fusions(fusionBytes);

            if (!this.fusionDataGridviewIsSetup)
            {
                this.SetupFusionDataGridView();
            }

            this.fusionsBinding = new BindingListView<Fusion>(this.fusions.fusions);
            this.fusionsDataGridView.DataSource = this.fusionsBinding;
        }

        private void SetupFusionDataGridView()
        {
            this.fusionsDataGridView.AutoGenerateColumns = false;

            this.FusionsDataGridViewLowerCard.DataPropertyName = "LowerCardIndex";
            this.FusionsDataGridViewLowerCard.ValueMember = "LowerCardIndex";
            this.FusionsDataGridViewLowerCard.DisplayMember = "LowerCardName";
            this.FusionsDataGridViewLowerCard.AutoComplete = true;
            this.FusionsDataGridViewLowerCard.FlatStyle = FlatStyle.Flat;

            this.FusionsDataGridViewUpperCard.DataPropertyName = "UpperCardIndex";
            this.FusionsDataGridViewUpperCard.ValueMember = "UpperCardIndex";
            this.FusionsDataGridViewUpperCard.DisplayMember = "UpperCardName";
            this.FusionsDataGridViewUpperCard.AutoComplete = true;
            this.FusionsDataGridViewUpperCard.FlatStyle = FlatStyle.Flat;


            this.FusionsDataGridViewFusionCard.DataPropertyName = "FusionCardIndex";
            this.FusionsDataGridViewFusionCard.ValueMember = "FusionCardIndex";
            this.FusionsDataGridViewFusionCard.DisplayMember = "FusionCardName";
            this.FusionsDataGridViewFusionCard.AutoComplete = true;
            this.FusionsDataGridViewFusionCard.FlatStyle = FlatStyle.Flat;

            foreach (CardConstant cardConstant in CardConstant.Monsters)
            {
                this.FusionsDataGridViewLowerCard.Items.Add(new { LowerCardName = cardConstant.Name, LowerCardIndex = cardConstant.Index });
                this.FusionsDataGridViewUpperCard.Items.Add(new { UpperCardName = cardConstant.Name, UpperCardIndex = cardConstant.Index });
                this.FusionsDataGridViewFusionCard.Items.Add(new { FusionCardName = cardConstant.Name, FusionCardIndex = cardConstant.Index });
            }

            this.fusionsDataGridView.EditingControlShowing += this.FusionEditControlShowing;
            this.fusionsDataGridView.ColumnHeaderMouseClick += this.fusionsDataGridView_SortColumns;
            MainForm.EnableDoubleBuffering(this.fusionsDataGridView);

            this.fusionDataGridviewIsSetup = true;
        }

        private void fusionsDataGridView_SortColumns(object sender, DataGridViewCellMouseEventArgs e)
        {
            string nextSortDirection = "";
            string sortColumn = "Index";

            switch (e.ColumnIndex)
            {
                case 1:
                    sortColumn = "LowerCardIndex";
                    break;
                case 2:
                    sortColumn = "LowerCardName";
                    break;
                case 3:
                    sortColumn = "UpperCardIndex";
                    break;
                case 4:
                    sortColumn = "UpperCardName";
                    break;
                case 5:
                    sortColumn = "FusionCardIndex";
                    break;
                case 6:
                    sortColumn = "FusionCardName";
                    break;
                default:
                    return;
            }

            if (this.fusionsBinding.SortProperty == null || this.fusionsBinding.SortProperty.Name != sortColumn)
            {
                nextSortDirection = "ASC";
            }
            else
            {
                switch (this.fusionsBinding.SortDirection)
                {
                    case ListSortDirection.Ascending:
                        nextSortDirection = "DESC";
                        break;
                    case ListSortDirection.Descending:
                        sortColumn = "Index";
                        nextSortDirection = "ASC";
                        break;
                    default:
                        nextSortDirection = "ASC";
                        break;
                }
            }

            this.fusionsBinding.Sort = $"{sortColumn} {nextSortDirection}";
        }

        private void FusionEditControlShowing(object sender, DataGridViewEditingControlShowingEventArgs e)
        {
            if (e.Control is DataGridViewComboBoxEditingControl)
            {
                ((ComboBox)e.Control).DropDownStyle = ComboBoxStyle.DropDown;
                ((ComboBox)e.Control).AutoCompleteSource = AutoCompleteSource.ListItems;
                ((ComboBox)e.Control).AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.Suggest;
            }
        }

        private void fusionSaveButton_Click(object sender, EventArgs e)
        {
            this.dataAccess.SaveFusionData(this.fusions.Bytes);
            this.LoadFusionData();
            MessageBox.Show("All fusion combinations saved.", "Save successful");
        }


        private void randomFusions(object sender, EventArgs e)
        {
            Console.WriteLine("rweigjfeoriugjer");
            List<CardConstant> potentialFusionResults = new List<CardConstant>();
            foreach (CardConstant card in CardConstant.Monsters)
            {
                if (card.Attack >= 1500 && card.Attack <= 2800 && card.KindName != "Immortal" && !IsCardBanned(card.Index))
                    potentialFusionResults.Add(card);
            }

            int counter = 0;
            while (counter < fusions.fusions.Count)
            {
                CardConstant resultMonster = potentialFusionResults[rand.Next(potentialFusionResults.Count)];
                //Console.WriteLine("Resultmonster: " + resultMonster.Name + " Kind name: " + resultMonster.Kind.Id + resultMonster.KindName);
                List<CardConstant> matchingTypeMonsters = RemoveMonsterWithMoreAtk(GetListOfMonstersOfType(resultMonster.Kind.Id), resultMonster.Attack - 1);
                //Console.WriteLine("Matching monsters: " + matchingTypeMonsters.Count);

                List<CardConstant> secondaryTypeMonsters = RemoveMonsterWithMoreAtk(GetListOfMonstersOfRandomType(), resultMonster.Attack - 1);
                //Console.WriteLine("secondary type monsters: " + secondaryTypeMonsters.Count);

                if (resultMonster.Attack > 2000)
                {
                    List<CardConstant> templist = RemoveMonsterWithLessAtk(matchingTypeMonsters, resultMonster.Attack - 1000);
                    matchingTypeMonsters = templist;
                    //Console.WriteLine("Matching monsters: " + matchingTypeMonsters.Count);
                }

                foreach (CardConstant card in matchingTypeMonsters)
                {
                    foreach (CardConstant secondary in secondaryTypeMonsters)
                    {
                        if (counter < fusions.fusions.Count)
                        {
                            fusions.fusions[counter].LowerCardIndex = card.Index;
                            fusions.fusions[counter].UpperCardIndex = secondary.Index;
                            fusions.fusions[counter].FusionCardIndex = resultMonster.Index;
                            //Console.WriteLine("counter: " + counter);
                            counter++;
                        }
                    }
                }



            }
        }

        private void SaveFusionsToOriginalList(object sender, EventArgs e)
        {
            string[] originalFusions = new string[fusions.fusions.Count];
            for(int i = 0; i < fusions.fusions.Count; i++)
            {
                string fusionLine = fusions.fusions[i].LowerCardIndex + "," + fusions.fusions[i].UpperCardIndex + "," + fusions.fusions[i].FusionCardIndex;
                originalFusions[i] = fusionLine;
            }
            File.WriteAllLines(@"../../OriginalFusions.txt", originalFusions);
            Console.WriteLine("done svaing og frusion");
        }

        private void RestoreOriginalFusions(object o, EventArgs e)
        {
            var path = "";
#if DEBUG
            path = @"../../OriginalFusions.txt";

#else
            path = Path.Combine(Directory.GetCurrentDirectory(), "OriginalFusions.txt");
#endif
            string[] lines = File.ReadAllLines(path);
            for(int i = 0; i < lines.Length; i++)
            {
                string[] monsterIds = lines[i].Split(',');
                fusions.fusions[i].LowerCardIndex = Convert.ToUInt16(Int32.Parse(monsterIds[0]));
                fusions.fusions[i].UpperCardIndex = Convert.ToUInt16(Int32.Parse(monsterIds[1]));
                fusions.fusions[i].FusionCardIndex = Convert.ToUInt16(Int32.Parse(monsterIds[2]));
            }
        }
        //private void setpBanList()
        //{
        //    var path = Path.Combine(Directory.GetCurrentDirectory(), "Banlist.txt");
        //    Console.WriteLine(path);
        //    string[] lines = File.ReadAllLines(path);
        //    for (int i = 0; i < lines.Length; i++)
        //    {
        //        bannedCardIds.Add(Int32.Parse(lines[i]));
        //    }
        //}

        //private void randomizeFusions(object sender, EventArgs e)
        //{
        //    for (int i = 0; i < fusions.fusions.Count; i++)
        //    {
        //        fusions.fusions[i].LowerCardIndex = CardConstant.Monsters[rand.Next(0, CardConstant.Monsters.Count)].Index;
        //        fusions.fusions[i].UpperCardIndex = CardConstant.Monsters[rand.Next(0, CardConstant.Monsters.Count)].Index;
        //        fusions.fusions[i].FusionCardIndex = CardConstant.Monsters[rand.Next(0, CardConstant.Monsters.Count)].Index;
        //    }
        //    this.dataAccess.SaveFusionData(this.fusions.Bytes);
        //    this.LoadFusionData();
        //}
        //private void randomizeFusionsBalanced(object sender, EventArgs e)
        //{

        //    int counter = 0;
        //    while (counter < fusions.fusions.Count)
        //    {
        //        int minAtk = rand.Next(500, 1600);
        //        int maxAtk = rand.Next(minAtk + 400, minAtk + 900);

        //        List<CardConstant> originalMonsterListOne = GetListOfMonstersOfRandomType();
        //        List<CardConstant> filteredMonsterListOne = RemoveMonsterWithLessAtk(RemoveMonsterWithMoreAtk(originalMonsterListOne, maxAtk), minAtk);
        //        List<CardConstant> monsterListTwo = RemoveMonsterWithMoreAtk(GetListOfMonstersOfRandomType(), maxAtk);

        //        if ((filteredMonsterListOne.Count * monsterListTwo.Count) < (fusions.fusions.Count - counter))
        //        {
        //            CardConstant resultMonster;
        //            List<CardConstant> resultMonsters = new List<CardConstant>();
        //            foreach (CardConstant card in originalMonsterListOne)
        //            {
        //                if (card.Attack >= maxAtk + 100 && card.Attack < 2900)
        //                {
        //                    resultMonsters.Add(card);
        //                }
        //            }
        //            if (resultMonsters.Count == 0)
        //            {
        //                resultMonsters = RemoveMonsterWithLessAtk(CardConstant.Monsters, maxAtk + 100);
        //            }
        //            resultMonster = resultMonsters[rand.Next(0, resultMonsters.Count)];

        //            foreach (CardConstant card in filteredMonsterListOne)
        //            {
        //                foreach (CardConstant monster in monsterListTwo)
        //                {
        //                    fusions.fusions[counter].LowerCardIndex = card.Index;
        //                    fusions.fusions[counter].UpperCardIndex = monster.Index;
        //                    fusions.fusions[counter].FusionCardIndex = resultMonster.Index;
        //                    Console.WriteLine(counter);
        //                    counter++;
        //                }
        //            }
        //        }
        //        else
        //        {
        //            CardConstant monster1 = filteredMonsterListOne[rand.Next(0, filteredMonsterListOne.Count)];
        //            CardConstant monster2 = monsterListTwo[rand.Next(0, monsterListTwo.Count)];
        //            List<CardConstant> fusionMonsters = originalMonsterListOne.Where(x => (x.Attack + 100) > maxAtk).ToList();
        //            if (fusionMonsters.Count == 0)
        //                fusionMonsters = CardConstant.Monsters.Where(x => x.Attack + 100 > maxAtk).ToList();
        //            CardConstant fusionMonster = fusionMonsters[rand.Next(0, fusionMonsters.Count)];

        //            fusions.fusions[counter].LowerCardIndex = monster1.Index;
        //            fusions.fusions[counter].UpperCardIndex = monster2.Index;
        //            fusions.fusions[counter].FusionCardIndex = fusionMonster.Index;
        //            Console.WriteLine("counter: " + counter);
        //            counter++;
        //        }



        //    }
        //    this.dataAccess.SaveFusionData(this.fusions.Bytes);
        //    this.LoadFusionData();



        //    //get random type/atk combo
        //    //check if ther are enough fusions left
        //    //if true
        //    //get result monster
        //    //if false
        //    //get new combo
        //    //repeat x times
        //    //if no new combos work
        //    //random fusions for the rest


        //}
        private List<CardConstant> RemoveMonsterWithMoreAtk(List<CardConstant> list, int maxAtk)
        {
            List<CardConstant> newList = new List<CardConstant>();
            int atk = maxAtk;

            do
            {
                foreach (CardConstant card in list)
                {
                    if (card.Attack < atk)
                        newList.Add(card);
                }
                atk++;
                Console.WriteLine(atk);
            } while (newList.Count == 0);


            return newList;
        }
        private List<CardConstant> RemoveMonsterWithLessAtk(List<CardConstant> list, int minAtk)
        {
            List<CardConstant> newList = new List<CardConstant>();
            int atk = minAtk;

            do
            {
                foreach (CardConstant card in list)
                {
                    if (card.Attack > atk)
                        newList.Add(card);
                }
                atk--;
                Console.WriteLine(atk);
            } while (newList.Count == 0);


            return newList;
        }
        private List<CardConstant> GetListOfMonstersOfType(int kindId)
        {
            List<CardConstant> cards = new List<CardConstant>();

            foreach (CardConstant card in CardConstant.Monsters)
            {
                if (card.Kind.Id == kindId)
                    cards.Add(card);
            }
            return cards;
        }
        private List<CardConstant> GetListOfMonstersOfRandomType()
        {
            int i = rand.Next(0, 19);
            switch (i)
            {
                case 0:
                    return RemoveBannedCards(dragons);
                case 1:
                    return RemoveBannedCards(spellcasters);
                case 2:
                    return RemoveBannedCards(zombies);
                case 3:
                    return RemoveBannedCards(warriors);
                case 4:
                    return RemoveBannedCards(beastwarriors);
                case 5:
                    return RemoveBannedCards(beasts);
                case 6:
                    return RemoveBannedCards(wingedbeasts);
                case 7:
                    return RemoveBannedCards(fiends);
                case 8:
                    return RemoveBannedCards(fairies);
                case 9:
                    return RemoveBannedCards(insects);
                case 10:
                    return RemoveBannedCards(dinosaurs);
                case 11:
                    return RemoveBannedCards(reptiles);
                case 12:
                    return RemoveBannedCards(fishes);
                case 13:
                    return RemoveBannedCards(seaserpents);
                case 14:
                    return RemoveBannedCards(machines);
                case 15:
                    return RemoveBannedCards(thunders);
                case 16:
                    return RemoveBannedCards(aquas);
                case 17:
                    return RemoveBannedCards(pyros);
                case 18:
                    return RemoveBannedCards(rocks);
                case 19:
                    return RemoveBannedCards(plants);
                default:
                    return CardConstant.Monsters;
            }
        }
    }
}
