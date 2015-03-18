using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace SunokoLibrary.Windows.ViewModels
{
    using SunokoLibrary.Application;

    /// <summary>
    /// ブラウザ選択UI用ViewModel。CookieImportersとUIの間を取り持ち、UI側の状態遷移を保持します。
    /// </summary>
    public class CookieSourceSelector : INotifyPropertyChanged
    {
        /// <summary>
        /// 指定されたManagerからの項目を使用するインスタンスを生成します。
        /// </summary>
        /// <param name="importerManager">使用するManager</param>
        /// <param name="itemGenerator">取得されたICookieImporterからブラウザ項目のViewModelを生成するメソッド</param>
        public CookieSourceSelector(ICookieImporterManager importerManager, Func<ICookieImporter, CookieSourceItem> itemGenerator)
        {
            _importerManager = importerManager;
            _itemGenerator = itemGenerator;
            _selectedIndex = -1;
            _isAllBrowserMode = false;
            Items = new ObservableCollection<CookieSourceItem>();
        }
        System.Threading.SemaphoreSlim _updateSem = new System.Threading.SemaphoreSlim(1);
        object _updaterSyn = new object();
        bool _isUpdating, _isAllBrowserMode, _addedCustom;
        int _selectedIndex;
        ICookieImporterManager _importerManager;
        Func<ICookieImporter, CookieSourceItem> _itemGenerator;

        /// <summary>
        /// メンバの内容の更新中であるかを取得します。
        /// </summary>
        public bool IsUpdating
        {
            get { return _isUpdating; }
            private set
            {
                _isUpdating = value;
                OnPropertyChanged();
            }
        }
        /// <summary>
        /// 使用可能なブラウザのみを取得するかを取得、設定します。
        /// </summary>
        public bool IsAllBrowserMode
        {
            get { return _isAllBrowserMode; }
            set
            {
                if (_isAllBrowserMode == value)
                    return;
                _isAllBrowserMode = value;
                OnPropertyChanged();
                var tsk = UpdateAsync();
            }
        }
        /// <summary>
        /// 選択中のブラウザのインデックスを取得、設定します。
        /// </summary>
        public int SelectedIndex
        {
            get { return _selectedIndex; }
            set
            {
                if (_selectedIndex == value)
                    return;
                _selectedIndex = value;
                OnPropertyChanged();
            }
        }
        /// <summary>
        /// 選択中のブラウザのICookieImporterを取得します。
        /// </summary>
        public ICookieImporter SelectedImporter
        {
            get
            {
                lock (_updaterSyn)
                {
                    var browserItem = SelectedIndex >= 0 && SelectedIndex < Items.Count ? Items[SelectedIndex] : null; ;
                    var importer = browserItem != null ? browserItem.Importer : null;
                    return importer;
                }
            }
        }
        /// <summary>
        /// 使用可能なブラウザを取得します。
        /// </summary>
        public ObservableCollection<CookieSourceItem> Items { get; private set; }
        /// <summary>
        /// Itemsを更新します。
        /// </summary>
        public async Task UpdateAsync()
        {
            ICookieImporter currentImporter = null;
            CookieSourceInfo currentInfo = null;
            try
            {
                //設定復元用に選択中のブラウザを取得。
                await _updateSem.WaitAsync();
                currentImporter = SelectedImporter;
                currentInfo = currentImporter != null ? currentImporter.SourceInfo : null;
                SelectedIndex = -1;

                IsUpdating = true;
                var browserItems = (await _importerManager.GetInstancesAsync(!IsAllBrowserMode))
                    .ToArray()
                    .OrderBy(importer => importer, _importerComparer)
                    .Select(importer =>
                        {
                            try
                            {
                                var item = _itemGenerator(importer);
                                item.Initialize();
                                return item;
                            }
                            catch (Exception e)
                            {
                                throw new CookieImportException(
                                    string.Format("{0}の生成に失敗しました。", typeof(CookieSourceItem).Name), CookieImportState.UnknownError, e);
                            }
                        });

                
                lock (_updaterSyn)
                {
                    _addedCustom = false;
                    for (var i = Items.Count - 1; i >= 0; i--)
                        Items.RemoveAt(i);
                    foreach (var item in browserItems)
                        Items.Add(item);
                }
                //更新前に選択していた項目を再選択させる
                if (currentInfo != null)
                    await PrivateSetInfoAsync(currentInfo);
            }
            catch (CookieImportException e)
            {
                lock (_updaterSyn)
                    for (var i = Items.Count - 1; i >= 0; i--)
                        Items.RemoveAt(i);
                System.Diagnostics.Trace.TraceInformation("選択中のブラウザの設定カスタマイズに失敗。", e);
            }
            finally
            {
                _updateSem.Release();
                IsUpdating = false;
            }
        }
        /// <summary>
        /// 任意のブラウザ構成を設定します。カスタム設定の構成も設定可能です。
        /// </summary>
        /// <param name="info">ブラウザの構成設定</param>
        public async Task SetInfoAsync(CookieSourceInfo info)
        {
            try
            {
                await _updateSem.WaitAsync();
                IsUpdating = true;
                await PrivateSetInfoAsync(info);
            }
            catch (CookieImportException e)
            { System.Diagnostics.Trace.TraceInformation("選択中のブラウザの設定カスタマイズに失敗。", e); }
            finally
            {
                IsUpdating = false;
                _updateSem.Release();
            }
        }
        async Task PrivateSetInfoAsync(CookieSourceInfo info)
        {
            //引数infoが使えるImporterを取得する。無い場合は適当なのを見繕う
            var importer = await _importerManager.GetInstanceAsync(info, true);
            lock (_updaterSyn)
            {
                //取得したImporterのItems内での場所を検索する。
                //idxがどのItemsも指定していない場合はカスタム設定を生成
                var idx = Items.Select(item => item.Importer.SourceInfo).TakeWhile(conf => conf != importer.SourceInfo).Count();
                if (idx == Items.Count)
                {
                    CookieSourceItem customItem;
                    try
                    {
                        customItem = _itemGenerator(importer);
                        customItem.Initialize();
                    }
                    catch (Exception e)
                    {
                        throw new CookieImportException(
                            string.Format("{0}の生成に失敗しました。", typeof(CookieSourceItem).Name), CookieImportState.UnknownError, e);
                    }
                    if (_addedCustom)
                        Items[Items.Count - 1] = customItem;
                    else
                    {
                        Items.Add(customItem);
                        _addedCustom = true;
                    }
                }
                SelectedIndex = idx;
            }
        }

        /// <summary>
        /// プロパティが更新された事を通知します。
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged = (sender, e) => { };
        /// <summary>
        /// PropertyChangedイベントを起こします。
        /// </summary>
        protected virtual void OnPropertyChanged([CallerMemberName]string memberName = null)
        { PropertyChanged(this, new System.ComponentModel.PropertyChangedEventArgs(memberName)); }

        static ImporterComparer _importerComparer = new ImporterComparer();
        class ImporterComparer : IComparer<ICookieImporter>
        {
            public int Compare(ICookieImporter x, ICookieImporter y)
            {
                if (x == y)
                    return 0;
                else if (x == null)
                    return -1;
                else if (y == null)
                    return 1;
                else if (x.PrimaryLevel != y.PrimaryLevel)
                    return x.PrimaryLevel - y.PrimaryLevel;
                else
                {
                    var xIdx = x.SourceInfo.BrowserName.IndexOf(' ');
                    var yIdx = y.SourceInfo.BrowserName.IndexOf(' ');
                    var xName = xIdx >= 0 ? x.SourceInfo.BrowserName.Substring(0, xIdx) : x.SourceInfo.BrowserName;
                    var yName = yIdx >= 0 ? y.SourceInfo.BrowserName.Substring(0, yIdx) : y.SourceInfo.BrowserName;
                    return string.Compare(xName, yName);
                }
            }
        }
    }
    /// <summary>
    /// ブラウザ選択UIにおける各ブラウザ項目用ViewModel。可視化対象のICookieImporterを持ち、UI上での項目表示を保持します。
    /// </summary>
    public abstract class CookieSourceItem : INotifyPropertyChanged
    {
        /// <summary>
        /// 指定されたICookieImporterからインスタンスを生成します。
        /// </summary>
        /// <param name="importer">対象のブラウザ</param>
        public CookieSourceItem(ICookieImporter importer)
        {
            Importer = importer;
            BrowserName = importer.SourceInfo.BrowserName;
            ProfileName = importer.SourceInfo.ProfileName;
            IsCustomized = importer.SourceInfo.IsCustomized;
        }
        bool _isCustomized;
        string _browserName, _profileName;

        /// <summary>
        /// Cookie取得用インスタンスを取得します。
        /// </summary>
        public ICookieImporter Importer { get; private set; }
        /// <summary>
        /// 既存の項目に設定変更を行って生成した項目かどうかを取得します。
        /// </summary>
        public bool IsCustomized
        {
            get { return _isCustomized; }
            private set
            {
                _isCustomized = value;
                OnPropertyChanged();
            }
        }
        /// <summary>
        /// ブラウザの名前を取得します。
        /// </summary>
        public string BrowserName
        {
            get { return _browserName; }
            private set
            {
                _browserName = value;
                OnPropertyChanged();
            }
        }
        /// <summary>
        /// 識別名を取得します。
        /// </summary>
        public string ProfileName
        {
            get { return _profileName; }
            private set
            {
                _profileName = value;
                OnPropertyChanged();
            }
        }
        /// <summary>
        /// 画面上で表示される文字列を取得します。
        /// </summary>
        public abstract string DisplayText { get; protected set; }
        /// <summary>
        /// 初期化を行う際に呼び出されます。呼び出す必要はありません。オーバーライドして使用してください。
        /// </summary>
        public abstract void Initialize();

        /// <summary>
        /// プロパティが更新された事を通知します。
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged = (sender, e) => { };
        /// <summary>
        /// PropertyChangedイベントを起こします。
        /// </summary>
        protected virtual void OnPropertyChanged([CallerMemberName]string memberName = null)
        { PropertyChanged(this, new System.ComponentModel.PropertyChangedEventArgs(memberName)); }
    }
}
