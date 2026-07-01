using System;
using System.Collections;
using System.Windows.Forms;

namespace FileRenamer
{
    public class ListViewItemComparer : IComparer
    {
        public enum ComparerMode
        {
            String,
            Integer,
            DateTime
        }

        private int _column;
        private SortOrder _order;
        private ComparerMode _mode;
        private ComparerMode[] _columnModes;

        public int Column
        {
            get { return _column; }
            set
            {
                if (_column == value)
                {
                    if (_order == SortOrder.Ascending)
                        _order = SortOrder.Descending;
                    else if (_order == SortOrder.Descending)
                        _order = SortOrder.Ascending;
                }
                _column = value;
            }
        }

        public SortOrder Order
        {
            get { return _order; }
            set { _order = value; }
        }

        public ComparerMode Mode
        {
            get { return _mode; }
            set { _mode = value; }
        }

        public ComparerMode[] ColumnModes
        {
            set { _columnModes = value; }
        }

        public ListViewItemComparer(int col, SortOrder ord, ComparerMode cmod)
        {
            _column = col;
            _order = ord;
            _mode = cmod;
        }

        public ListViewItemComparer()
        {
            _column = 0;
            _order = SortOrder.Ascending;
            _mode = ComparerMode.String;
        }

        public int Compare(object x, object y)
        {
            if (_order == SortOrder.None)
                return 0;

            int num = 0;
            ListViewItem listViewItem = (ListViewItem)x;
            ListViewItem listViewItem2 = (ListViewItem)y;

            if (_columnModes != null && _columnModes.Length > _column)
            {
                if (_column == 0) _mode = ComparerMode.Integer;
                else if (_column == 1) _mode = ComparerMode.String;
                else if (_column == 2) _mode = ComparerMode.String;
                else if (_column == 3) _mode = ComparerMode.DateTime;
                else if (_column == 4) _mode = ComparerMode.String;
            }

            switch (_mode)
            {
                case ComparerMode.String:
                    num = string.Compare(listViewItem.SubItems[_column].Text, listViewItem2.SubItems[_column].Text);
                    break;
                case ComparerMode.Integer:
                    num = int.Parse(listViewItem.SubItems[_column].Text).CompareTo(int.Parse(listViewItem2.SubItems[_column].Text));
                    break;
                case ComparerMode.DateTime:
                    num = DateTime.Compare(DateTime.Parse(listViewItem.SubItems[_column].Text), DateTime.Parse(listViewItem2.SubItems[_column].Text));
                    break;
            }

            if (_order == SortOrder.Descending)
                num = -num;

            return num;
        }
    }
}
