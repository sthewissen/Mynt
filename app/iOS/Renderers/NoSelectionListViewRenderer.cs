using System;
using Mynt.iOS.Renderers;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;

[assembly: ExportRenderer(typeof(ViewCell), typeof(NoSelectionListViewRenderer))]
namespace Mynt.iOS.Renderers
{
    public class NoSelectionListViewRenderer : ViewCellRenderer
    {
        public override UIKit.UITableViewCell GetCell(Cell item, UIKit.UITableViewCell reusableCell, UIKit.UITableView tv)
        {
            var cell = base.GetCell(item, reusableCell, tv);

            cell.SelectedBackgroundView = new UIView() { BackgroundColor = UIColor.Clear };

            return cell;
        }
    }
}
