using System;
using System.Collections.Generic;
using System.Linq;
using Foundation;
using UIKit;

namespace DYAlertViewPicker
{
    public class DyAlertPickViewSource : UITableViewSource
    {
        private const string CellIdentifier = "picker_view_identifier";

        private readonly List<string> _items;
        public NSIndexPath SelectedIndexPath { get; set; }

        public Action<string> OnConfirm;

        public DyAlertPickViewSource(List<string> items)
        {
            _items = items;
        }

        public DyAlertPickViewSource(IEnumerable<string> items)
        {
            _items = items.ToList();
        }
        
        public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
        {
            var cell = tableView.DequeueReusableCell(CellIdentifier) ?? new UITableViewCell(UITableViewCellStyle.Default, CellIdentifier);

            if (SelectedIndexPath != null && Equals(SelectedIndexPath, indexPath))
                cell.Accessory = UITableViewCellAccessory.Checkmark;
            else
                cell.Accessory = UITableViewCellAccessory.None;

            cell.TextLabel.Text = _items[indexPath.Row];
            return cell;
        }

        public override void RowSelected(UITableView tableView, NSIndexPath indexPath)
        {
            var cell = tableView.CellAt(indexPath);
            if (SelectedIndexPath != null)
            {
                var prevCell = tableView.CellAt(SelectedIndexPath);
                if (prevCell != null)
                    prevCell.Accessory = UITableViewCellAccessory.None;

            }
            cell.Accessory = UITableViewCellAccessory.Checkmark;
            SelectedIndexPath = indexPath;
            tableView.DeselectRow(indexPath, true);


          OnConfirm?.Invoke(_items[indexPath.Row]);
        }

        public override nint RowsInSection(UITableView tableView, nint section)
        {
            return _items.Count;
        }
    }
}
