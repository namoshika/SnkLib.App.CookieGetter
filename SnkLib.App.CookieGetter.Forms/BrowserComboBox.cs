using System;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SunokoLibrary.Windows.Forms
{
    using SunokoLibrary.Application;
    using SunokoLibrary.Windows.ViewModels;

    /// <summary>
    /// アカウント一覧の表示用コンボボックス。
    /// </summary>
    public class BrowserComboBox : ComboBox
    {
        OpenFileDialog openFileDialog = new OpenFileDialog();
        FolderBrowserDialog openFolderDialog = new FolderBrowserDialog();

        /// <summary>
        /// ブラウザ選択のViewModelを取得、設定します。
        /// </summary>
        [Browsable(false), DefaultValue(null)]
        public BrowserSelector Selector { get; private set; }

        /// <summary>
        /// 指定したViewModelでコントロールを初期化します。
        /// </summary>
        public void Initialize(BrowserSelector viewModel)
        {
            if (DesignMode)
                return;
            if (Selector != null)
            {
                Selector.PropertyChanged -= _selector_PropertyChanged;
                Selector.Items.CollectionChanged -= _selector_Items_CollectionChanged;
            }
            Selector = viewModel;
            Items.Clear();
            if (Selector != null)
            {
                Selector.PropertyChanged += _selector_PropertyChanged;
                Selector.Items.CollectionChanged += _selector_Items_CollectionChanged;
                SelectedIndex = Selector.SelectedIndex;
                var tsk = Selector.UpdateAsync();
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
            if (Selector != null)
                Selector.SelectedIndex = SelectedIndex;
            base.OnSelectedIndexChanged(e);
        }
        protected override void InitLayout()
        {
            base.InitLayout();
            DropDownStyle = ComboBoxStyle.DropDownList;
        }

        void _selector_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch(e.PropertyName)
            {
                case "IsUpdating":
                    Enabled = !Selector.IsUpdating;
                    if (Selector.IsUpdating)
                        BeginUpdate();
                    else
                        EndUpdate();
                    break;
                case "SelectedIndex":
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
                        item.PropertyChanged += _selector_item_PropertyChanged;
                    }
                    break;
                case NotifyCollectionChangedAction.Remove:
                    for (var i = 0; i < e.OldItems.Count; i++)
                    {
                        var item = (BrowserItem)e.OldItems[i];
                        item.PropertyChanged -= _selector_item_PropertyChanged;
                        Items.RemoveAt(e.OldStartingIndex + i);
                    }
                    break;
                case NotifyCollectionChangedAction.Replace:
                    var oldItem = (BrowserItem)e.OldItems[0];
                    var newItem = (BrowserItem)e.NewItems[0];
                    oldItem.PropertyChanged -= _selector_item_PropertyChanged;
                    newItem.PropertyChanged += _selector_item_PropertyChanged;
                    Items[e.NewStartingIndex] = newItem.DisplayText ?? string.Empty;
                    break;
                case NotifyCollectionChangedAction.Move:
                    var mvItem = (string)Items[e.NewStartingIndex];
                    Items.RemoveAt(e.OldStartingIndex);
                    Items.Insert(e.NewStartingIndex, mvItem);
                    break;
            }
        }
        void _selector_item_PropertyChanged(object sender, PropertyChangedEventArgs e)
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