using DOTR_MODDING_TOOL.Classes;
using Equin.ApplicationFramework;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;


namespace DOTR_Modding_Tool
{
    public partial class MainForm : Form
    {
        
        private SortableBindingList<CardConstant> cards;
        private void SetupBannedList()
        {
            formatCardTable(banlistTrunkDataGridView);
            formatCardTable(banlistDataGridView);
            banlistTrunkDataGridView.DataSource = new BindingListView<CardConstant>(CardConstant.List);

            List<CardConstant> list = new List<CardConstant>();

            foreach(int card in bannedCardIds)
            {
                list.Add(CardConstant.List[card]);
            }

            cards = new SortableBindingList<CardConstant>(list);

            banlistDataGridView.DataSource = cards;

            banlistTrunkDataGridView.CellDoubleClick += banlistTrunkDataGridView_DoubleClick;
            banlistDataGridView.CellDoubleClick += banlistDataGridView_DoubleClick;
        }

        private void banlistTrunkDataGridView_DoubleClick(Object sender, DataGridViewCellEventArgs e)
        {
            // avoid errors when doubleclicking column headers
            if (e.RowIndex < 0)
            {
                return;
            }

            List<DataGridViewRow> rows = new List<DataGridViewRow> { banlistTrunkDataGridView.Rows[e.RowIndex] };
            addCardToBanlist(rows);

        }

        private void banlistDataGridView_DoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            foreach (DataGridViewRow row in banlistDataGridView.SelectedRows)
            {
                CardConstant card = (CardConstant)row.DataBoundItem;
                cards.Remove(card);
                bannedCardIds.Remove(card.Index);
            }
        }

        private void addCardToBanlist(List<DataGridViewRow> rows)
        {
            foreach (DataGridViewRow row in rows)
            {
                CardConstant card = ((ObjectView<CardConstant>)row.DataBoundItem).Object;
                cards.Add(card);
                bannedCardIds.Add(card.Index);
                Console.WriteLine(card.Index);
            }

        }
    }
}
