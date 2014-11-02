using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace SunokoLibrary.Application
{
    /// <summary>
    /// ブラウザ選択UI用ViewModel。CookieGettersとUIの間を取り持ち、UI側の状態遷移を保持します。
    /// </summary>
    public class BrowserSelector : INotifyPropertyChanged
    {
        /// <summary>
        /// 内容を指定してインスタンスを生成
        /// </summary>
        /// <param name="itemGenerator">Cookie取得用インスタンスからUI上でのブラウザ選択項目を生成します。</param>
        public BrowserSelector(Func<ICookieImporter, BrowserItem> itemGenerator)
        {
            _itemGenerator = itemGenerator;
            _selectedIndex = -1;
            _isAllBrowserMode = false;
            Items = new ObservableCollection<BrowserItem>();
        }
        System.Threading.SemaphoreSlim _updateSem = new System.Threading.SemaphoreSlim(1);
        object _updaterSyn = new object();
        bool _isUpdating, _isAllBrowserMode, _addedCustom;
        int _selectedIndex;
        Func<ICookieImporter, BrowserItem> _itemGenerator;

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
                    var getter = browserItem != null ? browserItem.Getter : null;
                    return getter;
                }
            }
        }
        /// <summary>
        /// 使用可能なブラウザを取得します。
        /// </summary>
        public ObservableCollection<BrowserItem> Items { get; private set; }
        /// <summary>
        /// Itemsを更新します。
        /// </summary>
        public async Task UpdateAsync()
        {
            ICookieImporter currentGetter = null;
            BrowserConfig currentConfig = null;
            try
            {
                //設定復元用に選択中のブラウザを取得。
                currentGetter = SelectedImporter;
                currentConfig = currentGetter != null ? currentGetter.Config : null;
                //Items更新
                await _updateSem.WaitAsync();
                _addedCustom = false;
                IsUpdating = true;
                for (var i = Items.Count - 1; i >= 0; i--)
                    Items.RemoveAt(i);
                var browserItems = (await CookieGetters.GetInstancesAsync(!IsAllBrowserMode))
                    .Select(getter =>
                        {
                            try
                            {
                                var item = _itemGenerator(getter);
                                item.Initialize();
                                return item;
                            }
                            catch (Exception e)
                            {
                                throw new CookieImportException(
                                    string.Format("{0}の生成に失敗しました。", typeof(BrowserItem).Name), ImportResult.UnknownError, e);
                            }
                        });
                lock (_updaterSyn)
                    foreach (var item in browserItems)
                        Items.Add(item);
            }
            catch (CookieImportException e)
            {
                for (var i = Items.Count - 1; i >= 0; i--)
                    Items.RemoveAt(i);
                System.Diagnostics.Trace.TraceInformation("選択中のブラウザの設定カスタマイズに失敗。", e);
            }
            finally
            {
                IsUpdating = false;
                _updateSem.Release();
            }
            //更新前に選択していた項目を再選択させる
            if (currentConfig != null)
                await SetConfigAsync(currentConfig);
        }
        /// <summary>
        /// 任意のブラウザ構成を設定します。カスタム設定の構成も設定可能です。
        /// </summary>
        /// <param name="config">ブラウザの構成設定</param>
        /// <returns></returns>
        public async Task SetConfigAsync(BrowserConfig config)
        {
            try
            {
                await _updateSem.WaitAsync();
                IsUpdating = true;

                //引数configが使えるGetterを取得する。無い場合は適当なのを見繕う
                //取得したGetterのItems内での場所を検索する。
                //idxがどのItemsも指定していない場合はカスタム設定を生成
                var getter = await CookieGetters.GetInstanceAsync(config);
                lock (_updaterSyn)
                {
                    var idx = Items.Select(item => item.Getter.Config).TakeWhile(conf => conf != getter.Config).Count();
                    if (idx == Items.Count)
                    {
                        BrowserItem customItem;
                        try
                        {
                            customItem = _itemGenerator(getter);
                            customItem.Initialize();
                        }
                        catch (Exception e)
                        {
                            throw new CookieImportException(
                                string.Format("{0}の生成に失敗しました。", typeof(BrowserItem).Name), ImportResult.UnknownError, e);
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
            catch (CookieImportException e)
            { System.Diagnostics.Trace.TraceInformation("選択中のブラウザの設定カスタマイズに失敗。", e); }
            finally
            {
                IsUpdating = false;
                _updateSem.Release();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged = (sender, e) => { };
        protected virtual void OnPropertyChanged([CallerMemberName]string memberName = null)
        { PropertyChanged(this, new System.ComponentModel.PropertyChangedEventArgs(memberName)); }
    }
    /// <summary>
    /// ブラウザ選択UIにおける各ブラウザ項目用ViewModel。任意のICookieImporterを持ち、UI上での項目表示を保持します。
    /// </summary>
    public abstract class BrowserItem : INotifyPropertyChanged
    {
        /// <summary>
        /// 内容を指定してインスタンスを生成
        /// </summary>
        /// <param name="getter">任意のCookie取得用インスタンス</param>
        public BrowserItem(ICookieImporter getter)
        {
            Getter = getter;
            BrowserName = getter.Config.BrowserName;
            ProfileName = getter.Config.ProfileName;
            IsCustomized = getter.Config.IsCustomized;
        }
        bool _isCustomized;
        string _browserName, _profileName;

        /// <summary>
        /// Cookie取得用インスタンスを取得します。
        /// </summary>
        public ICookieImporter Getter { get; private set; }
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

        public event PropertyChangedEventHandler PropertyChanged = (sender, e) => { };
        protected virtual void OnPropertyChanged([CallerMemberName]string memberName = null)
        { PropertyChanged(this, new System.ComponentModel.PropertyChangedEventArgs(memberName)); }
    }
}
