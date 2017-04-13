using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CoreAnimation;
using CoreGraphics;
using Foundation;
using UIKit;

namespace DYAlertViewPicker
{

    public class DyAlertPickerView : UIView
    {
        #region const

        private const float DyBackgroundAlpha = 0.4f;
        private const double DyHeaderHeight = 44.0d;
        private const double DySwitchHeight = 35.0d;
        private const double DyFooterHeight = 40.0d;
        private const double DyTableViewCellHeight = 44.0d;

        #endregion

        #region Fields

        private UIView _backgroundMaskView;
        private UIView _containerView;
        private UIView _headerView;
        private UIView _switchView;
        private UIView _footerView;
        private UITableView _tableView;

        #endregion

        #region Properties

        public UIColor HeaderTitleColor { get; set; }
        public UIColor HeaderBackgroundColor { get; set; }
        public UIColor ConfirmButtonNormalColor { get; set; }
        public UIColor ConfirmButtonHighlightedColor { get; set; }
        public UIColor ConfirmButtonBackgroundColor { get; set; }
        public UIColor CancelButtonNormalColor { get; set; }
        public UIColor CancelButtonHighlightedColor { get; set; }
        public UIColor CancelButtonBackgroundColor { get; set; }

        public bool TapBackgroundToDismiss { get; set; } = true;
        public bool TapPickerViewItemToConfirm { get; set; } = true;
        public bool IsUiDeviceOrientation { get; set; }

        public string SwitchButtonTitle { get; set; }
        public string HeaderTitle { get; set; }
        public string CancelButtonTitle { get; set; }
        public string ConfirmButtonTitle { get; set; }

        public IReadOnlyList<string> ItemList { get; set; }
        public NSIndexPath SelectedIndexPath { get; set; }

        #endregion

        #region EventHandlers

        public event EventHandler OnCancel;

        public event EventHandler<string> OnConfirm;

        public event EventHandler<bool> OnSwitchChanged;

        #endregion


        #region ctor

        public DyAlertPickerView() { }

        public DyAlertPickerView(string headerTitle, string cancelButtonTitle, string confirmButtonTitle,
            string switchButtonTitle)
        {
            TapBackgroundToDismiss = true;
            HeaderTitle = headerTitle ?? string.Empty;
            HeaderTitleColor = UIColor.White;
            HeaderBackgroundColor = UIColor.FromRGB(51, 153, 255);

            // footer button
            ConfirmButtonTitle = confirmButtonTitle ?? string.Empty;
            ConfirmButtonNormalColor = UIColor.White;
            ConfirmButtonHighlightedColor = UIColor.Gray;
            ConfirmButtonBackgroundColor = UIColor.FromRGB(56, 185, 158);

            CancelButtonTitle = cancelButtonTitle ?? string.Empty;
            CancelButtonNormalColor = UIColor.White;
            CancelButtonHighlightedColor = UIColor.Gray;
            CancelButtonBackgroundColor = UIColor.FromRGB(255, 71, 25);

            SwitchButtonTitle = switchButtonTitle ?? string.Empty;
            TapPickerViewItemToConfirm = true;
            IsUiDeviceOrientation = false;
        }

        #endregion

        private async void OrientationChange(NSNotification notification)
        {
            if (IsUiDeviceOrientation)
                return;

            switch (UIDevice.CurrentDevice.Orientation)
            {
                case UIDeviceOrientation.Portrait:
                case UIDeviceOrientation.PortraitUpsideDown:
                case UIDeviceOrientation.LandscapeLeft:
                case UIDeviceOrientation.LandscapeRight:
                    break;
                case UIDeviceOrientation.Unknown:
                case UIDeviceOrientation.FaceUp:
                case UIDeviceOrientation.FaceDown:
                    return;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            IsUiDeviceOrientation = true;

            foreach (var subview in Subviews)
            {
                subview.RemoveFromSuperview();
            }

            Layer.Transform = CATransform3D.MakeScale(1, 1, 1);
            RemoveFromSuperview();

            //Request to stop receiving accelerometer event and turn off accelerometer
            NSNotificationCenter.DefaultCenter.RemoveObserver(this);
            UIDevice.CurrentDevice.EndGeneratingDeviceOrientationNotifications();
            Show();

            await Task.Delay(TimeSpan.FromSeconds(0.3));
            IsUiDeviceOrientation = false;
        }


        public void Show(int index = -1)
        {
            SetupSubViews();

            var mainWindow = UIApplication.SharedApplication.Delegate.GetWindow();
            Frame = mainWindow.Frame;
            mainWindow.AddSubview(this);

            _containerView.Layer.Opacity = 1f;
            Layer.Opacity = 0.5f;
            Layer.Transform = CATransform3D.MakeScale(1f, 1f, 1f);

            Animate(0.2d, 0.0d, UIViewAnimationOptions.CurveEaseIn, () =>
            {
                BackgroundColor = UIColor.FromRGBA(0, 0, 0, 0.4f);
                Layer.Opacity = 1f;
                _backgroundMaskView.Layer.Opacity = 5f;
                Layer.Transform = CATransform3D.MakeScale(1f, 1f, 1f);

            }, () =>
            {

                var numberOfRows = _tableView.NumberOfRowsInSection(0);
                if (index < 0 || index > numberOfRows) return;
                SelectedIndexPath = NSIndexPath.FromRowSection(index, 0);
                _tableView.ReloadData();
            });
        }

        /// <summary>
        /// Uses the On
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="eventArgs"></param>
        private void SwOnValueChanged(object sender, EventArgs eventArgs)
        {
            var uiSwitch = sender as UISwitch;
            if(uiSwitch !=null)
             OnSwitchChanged?.Invoke(sender,uiSwitch.On);
        }

        private void Confirm(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                CancelButtonPress();
                return;
            }
            Dismiss(() => OnConfirm?.Invoke(this, value));
        }

        private void ConfirmPressed()
        {
            var source = _tableView.Source as DyAlertPickViewSource;
            if (source?.SelectedIndexPath != null)
                Confirm(ItemList[source.SelectedIndexPath.Row]);
        }

        private void CancelButtonPress()
        {
            Dismiss(() => OnCancel?.Invoke(this, EventArgs.Empty));
        }

        private async void Dismiss(Action completion)
        {
            completion?.Invoke();
            IsUiDeviceOrientation = true;
            NSNotificationCenter.DefaultCenter.RemoveObserver(this);
            UIDevice.CurrentDevice.EndGeneratingDeviceOrientationNotifications();
            var delay = TapPickerViewItemToConfirm ? 0.5d : 0d;
            await Task.Delay(TimeSpan.FromSeconds(delay));

            Animate(0.4, 0.0, UIViewAnimationOptions.CurveEaseOut, () =>
                {
                    BackgroundColor = UIColor.Black.ColorWithAlpha(DyBackgroundAlpha);
                    Layer.Opacity = 0.1f;
                    Layer.Transform = CATransform3D.MakeScale(1f, 1f, 1f);

                },
                () =>
                {
                    foreach (var subview in Subviews)
                    {
                        subview.RemoveFromSuperview();
                    }
                    Layer.Transform = CATransform3D.MakeScale(1, 1, 1);
                    RemoveFromSuperview();
                    SetNeedsDisplay();
                });
        }

        #region --- Build views ----
        private void SetupSubViews()
        {
            var rect = UIScreen.MainScreen.Bounds;
            Frame = rect;

            _backgroundMaskView = BuildBackgroundMaskView();
            AddSubview(_backgroundMaskView);

            _containerView = BuildContainerView();
            AddSubview(_containerView);

            _tableView = BuildTableView();
            _containerView.AddSubview(_tableView);

            _headerView = BuildHeaderView();
            _containerView.AddSubview(_headerView);

            _switchView = BuildSwitchView();
            _containerView.AddSubview(_switchView);

            _footerView = BuildFooterView();
            _containerView.AddSubview(_footerView);

            var frame = _containerView.Frame;
            _containerView.Frame = new CGRect(
                frame.X,
                frame.Y,
                frame.Size.Width,
                _headerView.Frame.Size.Height + _tableView.Frame.Size.Height +
                _footerView.Frame.Size.Height + _switchView.Frame.Size.Height
            );

            _containerView.Center = Center;

            UIDevice.CurrentDevice.BeginGeneratingDeviceOrientationNotifications();
            NSNotificationCenter.DefaultCenter.AddObserver(UIDevice.OrientationDidChangeNotification, OrientationChange, null);
        }
        private UIView BuildContainerView()
        {
            var transform = new CGAffineTransform(0.8f, 0, 0.0f, 0.6f, 0, 0);
            var newRect = CGAffineTransform.CGRectApplyAffineTransform(Frame, transform);
            var bhv = new UIView(newRect);
            bhv.Layer.CornerRadius = 5f;
            bhv.ClipsToBounds = true;

            return bhv;
        }

        private UITableView BuildTableView()
        {
            var transform = new CGAffineTransform(0.8f, 0, 0.0f, 0.6f, 0, 0);
            var newRect = CGAffineTransform.CGRectApplyAffineTransform(Frame, transform);

            var n = ItemList.Count;

            CGRect tableRect;

            var heightOffset = DyHeaderHeight;
            heightOffset += string.IsNullOrEmpty(ConfirmButtonTitle) && string.IsNullOrEmpty(CancelButtonTitle)
                ? 0
                : DyFooterHeight;
            heightOffset += string.IsNullOrEmpty(SwitchButtonTitle) ? 0 : DySwitchHeight;

            if (n > 0)
            {
                var height = n * DyTableViewCellHeight;
                height = height > newRect.Size.Height - heightOffset ? newRect.Size.Height - heightOffset : height;
                tableRect = new CGRect(0, DyTableViewCellHeight, newRect.Size.Width, height);
            }
            else
            {
                tableRect = new CGRect(0, DyTableViewCellHeight, newRect.Size.Width,
                    newRect.Size.Height - heightOffset);
            }

            return new UITableView(tableRect, UITableViewStyle.Plain)
            {

                Source = new DyAlertPickViewSource(ItemList)
                {
                    OnConfirm = value =>
                    {
                        if (!TapPickerViewItemToConfirm)
                            Confirm(value);
                    }
                },
                SeparatorStyle = UITableViewCellSeparatorStyle.None
            };
        }

        private UIView BuildBackgroundMaskView()
        {
            var view = new UIView(Frame)
            {
                BackgroundColor = UIColor.Black.ColorWithAlpha(DyBackgroundAlpha)
            };

            if (!TapBackgroundToDismiss) return view;
            var tapRecognizer = new UITapGestureRecognizer();
            tapRecognizer.AddTarget(() =>
            {
                tapRecognizer.LocationInView(view);
                CancelButtonPress();
            });

            tapRecognizer.NumberOfTapsRequired = 1;
            tapRecognizer.NumberOfTouchesRequired = 1;
            view.AddGestureRecognizer(tapRecognizer);
            return view;

        }

        private UIView BuildFooterView()
        {
            //Check for FooterView()
            if (string.IsNullOrEmpty(CancelButtonTitle) && string.IsNullOrEmpty(ConfirmButtonTitle))
            {
                TapPickerViewItemToConfirm = false;
                return new UIView(new CGRect(0, 0, 0, 0));
            }

            var rect = string.IsNullOrEmpty(SwitchButtonTitle)
                ? _tableView.Frame
                : _switchView.Frame;

            var newRect = new CGRect(0, rect.Y + rect.Size.Height, rect.Size.Width, DyFooterHeight);
            var leftRect = CGRect.Empty;
            var rightRect = CGRect.Empty;

            if (string.IsNullOrEmpty(CancelButtonTitle))
            {
                rightRect = new CGRect(0, 0, newRect.Size.Width, DyFooterHeight);
            }
            else if (string.IsNullOrEmpty(ConfirmButtonTitle))
            {
                leftRect = new CGRect(0, 0, newRect.Size.Width, DyFooterHeight);
            }
            else
            {
                leftRect = new CGRect(0, 0, newRect.Size.Width / 2, DyFooterHeight);
                rightRect = new CGRect(newRect.Size.Width / 2, 0, newRect.Size.Width / 2, DyFooterHeight);
            }

            var bfv = new UIView(newRect) { BackgroundColor = UIColor.Black };

            if (leftRect.Size.Width > 0 && leftRect.Size.Height > 0)
            {
                var cancelButton = new UIButton(leftRect);
                cancelButton.SetTitle(CancelButtonTitle, UIControlState.Normal);
                cancelButton.SetTitleColor(CancelButtonNormalColor, UIControlState.Normal);
                cancelButton.SetTitleColor(CancelButtonHighlightedColor, UIControlState.Highlighted);
                cancelButton.TitleLabel.Font = UIFont.BoldSystemFontOfSize(16);
                cancelButton.TouchUpInside += (s, e) => CancelButtonPress();
                cancelButton.BackgroundColor = CancelButtonBackgroundColor;
                bfv.AddSubview(cancelButton);
            }

            if (rightRect.Size.Width <= 0 || rightRect.Size.Height <= 0) return bfv;
            {
                var confirmButton = new UIButton(rightRect);
                confirmButton.SetTitle(ConfirmButtonTitle, UIControlState.Normal);
                confirmButton.SetTitleColor(ConfirmButtonNormalColor, UIControlState.Normal);
                confirmButton.SetTitleColor(ConfirmButtonHighlightedColor, UIControlState.Highlighted);
                confirmButton.TitleLabel.Font = UIFont.BoldSystemFontOfSize(16);
                confirmButton.TouchUpInside += (s, e) => ConfirmPressed();
                confirmButton.BackgroundColor = ConfirmButtonBackgroundColor;
                bfv.AddSubview(confirmButton);
            }

            return bfv;

        }

        private UIView BuildHeaderView()
        {
            var bhv =
                new UIView(new CGRect(0, 0, _tableView.Frame.Size.Width, DyHeaderHeight))
                {
                    BackgroundColor = HeaderBackgroundColor
                };

            var label = new UILabel(bhv.Frame)
            {
                Lines = 0,
                AttributedText = new NSAttributedString(HeaderTitle, new UIStringAttributes
                {
                    Font = UIFont.BoldSystemFontOfSize(16),
                    ForegroundColor = HeaderTitleColor
                }.Dictionary)
            };
            label.SizeToFit();
            label.Center = bhv.Center;
            bhv.AddSubview(label);
            return bhv;
        }

        private UIView BuildSwitchView()
        {
            if (string.IsNullOrEmpty(SwitchButtonTitle))
            {
                return new UIView(new CGRect(0, 0, 0, 0));
            }

            var bsv = new UIView(new CGRect(0, _tableView.Frame.Y + _tableView.Frame.Height + 1,
                _tableView.Frame.Size.Width, DySwitchHeight))
            {
                BackgroundColor = UIColor.White
            };

            var sw = new UISwitch(CGRect.Empty);
            sw.Frame = new CGRect(_tableView.Frame.Size.Width - sw.Frame.Size.Width - 2,
                (bsv.Frame.Size.Width - sw.Frame.Size.Height) / 2, sw.Frame.Size.Width, sw.Frame.Size.Height);
            sw.ValueChanged += SwOnValueChanged;

            var lbl = new UILabel(new CGRect(15, 0, bsv.Frame.Size.Width - sw.Frame.Size.Width - 15,
                bsv.Frame.Size.Height))
            {
                Text = SwitchButtonTitle,
                TextColor = UIColor.DarkGray
            };
            bsv.AddSubview(lbl);
            bsv.AddSubview(sw);

            return bsv;
        }
    }

}



#endregion



