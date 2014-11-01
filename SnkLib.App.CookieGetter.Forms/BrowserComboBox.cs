using System;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SunokoLibrary.Windows.Forms
{
    using SunokoLibrary.Application;

    /// <summary>
    /// アカウント一覧の表示用コンボボックス。
    /// </summary>
    public class BrowserComboBox : ComboBox
    {
        public BrowserComboBox()
            : base()
        {
            IsAllBrowserMode = false;
            DropDownStyle = ComboBoxStyle.DropDownList;
        }
        OpenFileDialog openFileDialog = new OpenFileDialog();
        FolderBrowserDialog openFolderDialog = new FolderBrowserDialog();
        BrowserSelector _selector;
        bool _isAllBrowserMode;

        /// <summary>
        /// ブラウザ選択のViewModelを取得、設定します。
        /// </summary>
        [Browsable(false)]
        public BrowserSelector Selector
        {
            get { return _selector; }
            set
            {
                if (!DesignMode && _selector != null)
                {
                    _selector.PropertyChanged -= _selector_PropertyChanged;
                    _selector.Items.CollectionChanged -= _selector_Items_CollectionChanged;
                }
                _selector = value;
                Items.Clear();
                if (!DesignMode && _selector != null)
                {
                    _selector.PropertyChanged += _selector_PropertyChanged;
                    _selector.Items.CollectionChanged += _selector_Items_CollectionChanged;
                    _isAllBrowserMode = _selector.IsAllBrowserMode;
                    SelectedIndex = _selector.SelectedIndex;
                    var tsk = _selector.UpdateAsync();
                }
            }
        }
        /// <summary>
        /// インストールされているブラウザーのみ表示するかどうかを取得、設定します。
        /// </summary>
        [Category("動作"), DefaultValue(true)]
        [Description("インストールされているブラウザーのみ表示するかどうかを取得、設定します。")]
        public bool IsAllBrowserMode
        {
            get { return !DesignMode && _selector != null ? _selector.IsAllBrowserMode : _isAllBrowserMode; }
            set
            {
                _isAllBrowserMode = value;
                if (!DesignMode && _selector != null && _selector.IsAllBrowserMode != value)
                    _selector.IsAllBrowserMode = value;
            }
        }

        /// <summary>
        /// 任意のCookieファイルを指定するためのファイル選択ダイアログを表示する。
        /// </summary>
        public async Task ShowCookieDialogAsync()
        {
            var currentGetter = Selector.SelectedImporter;
            var currentCookiePath = currentGetter.Config.CookiePath;
            BrowserConfig newConfig = null;
            DialogResult res;
            switch (currentGetter.CookiePathType)
            {
                case PathType.Directory:
                    if (System.IO.Directory.Exists(currentCookiePath))
                        openFolderDialog.SelectedPath = currentGetter.Config.CookiePath;
                    if ((res = openFolderDialog.ShowDialog()) == DialogResult.OK)
                    {
                        currentCookiePath = openFolderDialog.SelectedPath;
                        newConfig = currentGetter.Config.GenerateCopy(cookiePath: currentCookiePath);
                    }
                    break;
                case PathType.File:
                    if (System.IO.File.Exists(currentCookiePath))
                    {
                        openFileDialog.InitialDirectory = System.IO.Path.GetDirectoryName(currentCookiePath);
                        openFileDialog.FileName = currentCookiePath;
                    }
                    if ((res = openFileDialog.ShowDialog()) == DialogResult.OK)
                    {
                        currentCookiePath = openFileDialog.FileName;
                        newConfig = currentGetter.Config.GenerateCopy(cookiePath: currentCookiePath);
                    }
                    break;
                default:
                    return;
            }
            if (res == System.Windows.Forms.DialogResult.OK)
                await Selector.SetConfigAsync(newConfig);
        }
        protected override void OnSelectedIndexChanged(EventArgs e)
        {
            if (!DesignMode && Selector != null)
                Selector.SelectedIndex = SelectedIndex;
            base.OnSelectedIndexChanged(e);
        }
        void _selector_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch(e.PropertyName)
            {
                case "IsAvailableOnly":
                    IsAllBrowserMode = Selector.IsAllBrowserMode;
                    break;
                case "IsUpdating":
                    if (Enabled != !Selector.IsUpdating)
                        Enabled = !Selector.IsUpdating;
                    if (Selector.IsUpdating == false)
                        this.RefreshItems();
                    break;
                case "SelectedIndex":
                    if (SelectedIndex != Selector.SelectedIndex)
                        SelectedIndex = Selector.SelectedIndex;
                    break;
            }
        }
        void _selector_Items_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            switch(e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    for (var i = 0; i < e.NewItems.Count; i++)
                    {
                        var item = (BrowserItem)e.NewItems[i];
                        Items.Insert(e.NewStartingIndex + i, item.DisplayText ?? string.Empty);
                        item.PropertyChanged += item_PropertyChanged;
                    }
                    break;
                case NotifyCollectionChangedAction.Remove:
                    for (var i = 0; i < e.OldItems.Count; i++)
                    {
                        var item = (BrowserItem)e.OldItems[i];
                        item.PropertyChanged -= item_PropertyChanged;
                        Items.RemoveAt(e.OldStartingIndex + i);
                    }
                    break;
                case NotifyCollectionChangedAction.Replace:
                    var oldItem = (BrowserItem)e.OldItems[0];
                    var newItem = (BrowserItem)e.NewItems[0];
                    oldItem.PropertyChanged -= item_PropertyChanged;
                    newItem.PropertyChanged += item_PropertyChanged;
                    Items[e.NewStartingIndex] = newItem.DisplayText ?? string.Empty;
                    break;
                case NotifyCollectionChangedAction.Move:
                    var mvItem = (string)Items[e.NewStartingIndex];
                    Items.RemoveAt(e.OldStartingIndex);
                    Items.Insert(e.NewStartingIndex, mvItem);
                    break;
            }
        }
        void item_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch(e.PropertyName)
            {
                case "DisplayText":
                    var item = (BrowserItem)sender;
                    var idx = Selector.Items.IndexOf(item);
                    Items[idx] = item.DisplayText;
                    break;
            }
        }
    }
}