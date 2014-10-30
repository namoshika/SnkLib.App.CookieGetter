using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace SunokoLibrary.Application
{
    public class BrowserSelector : INotifyPropertyChanged
    {
        public BrowserSelector(Func<ICookieImporter, BrowserItem> itemGenerator)
        {
            _itemGenerator = itemGenerator;
            _selectedIndex = -1;
            _isAllBrowserMode = false;
            Items = new ObservableCollection<BrowserItem>();
        }
        System.Threading.SemaphoreSlim _updateSyncer = new System.Threading.SemaphoreSlim(1);
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
                currentGetter = await GetSelectedImporter();
                currentConfig = currentGetter != null ? currentGetter.Config : null;
                //Items更新
                await _updateSyncer.WaitAsync();
                _addedCustom = false;
                IsUpdating = true;
                Items.Clear();
                var browserItems = await Task.Factory.ContinueWhenAll((await CookieGetters.CreateInstancesAsync(!IsAllBrowserMode)).Select(async getter =>
                    {
                        BrowserItem item;
                        try
                        {
                            item = _itemGenerator(getter);
                            await item.Initialize();
                            return item;
                        }
                        catch (Exception e)
                        {
                            throw new CookieImportException(
                                string.Format("{0}の生成に失敗しました。", typeof(BrowserItem).Name), ImportResult.UnknownError, e);
                        }
                    }).ToArray(), tsks => tsks.Select(tsk => tsk.Result));
                foreach (var item in browserItems)
                    Items.Add(item);
            }
            catch (CookieImportException e)
            {
                Items.Clear();
                System.Diagnostics.Trace.TraceInformation("選択中のブラウザの設定カスタマイズに失敗。", e);
            }
            finally
            {
                IsUpdating = false;
                _updateSyncer.Release();
            }
            //更新前に選択していた項目を再選択させる
            if (currentConfig != null)
                await SetConfigAsync(currentConfig);
        }
        /// <summary>
        /// 選択中のブラウザのICookieImporterを取得します。
        /// </summary>
        public async Task<ICookieImporter> GetSelectedImporter()
        {
            try
            {
                await _updateSyncer.WaitAsync();
                var browserItem = SelectedIndex >= 0 && SelectedIndex < Items.Count ? Items[SelectedIndex] : null; ;
                var getter = browserItem != null ? browserItem.Getter : null;
                return getter;
            }
            finally
            { _updateSyncer.Release(); }
        }
        public async Task SetConfigAsync(BrowserConfig config)
        {
            try
            {
                await _updateSyncer.WaitAsync();
                IsUpdating = true;

                //引数configが使えるGetterを取得する。無い場合は適当なのを見繕う
                //取得したGetterのItems内での場所を検索する。
                //idxがどのItemsも指定していない場合はカスタム設定を生成
                var getter = await CookieGetters.CreateInstanceAsync(config);
                var idx = Items.Select(item => item.Getter.Config).TakeWhile(conf => conf != getter.Config).Count();
                if (idx == Items.Count)
                {
                    BrowserItem customItem;
                    try
                    {
                        customItem = _itemGenerator(getter);
                        await customItem.Initialize();
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
            catch (CookieImportException e)
            { System.Diagnostics.Trace.TraceInformation("選択中のブラウザの設定カスタマイズに失敗。", e); }
            finally
            {
                IsUpdating = false;
                _updateSyncer.Release();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged = (sender, e) => { };
        protected virtual void OnPropertyChanged([CallerMemberName]string memberName = null)
        { PropertyChanged(this, new System.ComponentModel.PropertyChangedEventArgs(memberName)); }
    }
    public abstract class BrowserItem : INotifyPropertyChanged
    {
        public BrowserItem(ICookieImporter getter)
        {
            Getter = getter;
            BrowserName = getter.Config.BrowserName;
            IsCustomized = getter.Config.IsCustomized;
        }
        bool _isCustomized;
        string _browserName;

        public ICookieImporter Getter { get; private set; }
        public bool IsCustomized
        {
            get { return _isCustomized; }
            private set
            {
                _isCustomized = value;
                OnPropertyChanged();
            }
        }
        public string BrowserName
        {
            get { return _browserName; }
            private set
            {
                _browserName = value;
                OnPropertyChanged();
            }
        }
        public abstract string DisplayText { get; protected set; }
        public abstract Task Initialize();

        public event PropertyChangedEventHandler PropertyChanged = (sender, e) => { };
        protected virtual void OnPropertyChanged([CallerMemberName]string memberName = null)
        { PropertyChanged(this, new System.ComponentModel.PropertyChangedEventArgs(memberName)); }
    }
}
