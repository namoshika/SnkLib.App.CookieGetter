using System;
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
            DisplayMember = "DisplayText";
        }
        OpenFileDialog openFileDialog = new OpenFileDialog();
        FolderBrowserDialog openFolderDialog = new FolderBrowserDialog();
        BrowserSelector _selector;
        bool _isAllBrowserMode;

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
        [Category("動作"), Description("インストールされているブラウザーのみ表示するかどうかを示します。"), DefaultValue(true)]
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

        public async Task ShowCookieDialogAsync()
        {
            var currentGetter = await Selector.GetSelectedImporter();
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
        { DataSource = Selector.Items.ToArray(); }
    }
}