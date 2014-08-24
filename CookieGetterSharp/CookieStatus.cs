using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hal.CookieGetterSharp
{
    using SunokoLibrary.Application;
    using SunokoLibrary.Application.Browsers;

    public class CookieStatus
    {
#pragma warning disable 0618 //Obsolete属性の警告を無効化"
        public CookieStatus(CookieGetter owner, IBrowserManager manager)
        {
            _owner = owner;
            BrowserType = manager.BrowserType;
        }
        CookieGetter _owner;
        string _displayName;

        public BrowserType BrowserType { get; private set; }
        public PathType PathType
        { get { return (PathType)Enum.Parse(typeof(PathType), GetValue(() => _owner.Importer.CookiePathType).ToString()); } }
        public bool IsAvailable
        { get { return GetValue(() => _owner.Importer.IsAvailable); } }
        public string Name
        {
            get { return GetValue(() => _owner.Importer.Config.BrowserName); }
            internal set { Refresh(value); }
        }
        public string CookiePath
        {
            get { return GetValue(() => _owner.Importer.Config.CookiePath); }
            set { Refresh(cookiePath: value); }
        }
        public string DisplayName
        {
            get { return string.IsNullOrEmpty(_displayName) ? Name : _displayName; }
            set { _displayName = this.Name.Equals(value) ? null : value; }
        }

        TResult GetValue<TResult>(Func<TResult> getter)
        {
            try { return getter(); }
            catch(CookieImportException e)
            { throw new CookieGetterException(e); }
        }
        void Refresh(string name = null, string profileName = null, string cookiePath = null)
        {
            try
            {
                _owner.Importer = _owner.Importer.Generate(
                    _owner.Importer.Config.GenerateCopy(name, profileName, cookiePath));
            }
            catch(CookieImportException e)
            { throw new CookieGetterException(e); }
        }
        public override bool Equals(object obj)
        {
            if (Name == null || CookiePath == null || obj == null || !(obj is CookieStatus))
                return false;
            var bi = (CookieStatus)obj;
            return Name.Equals(bi.Name) && CookiePath.Equals(bi.CookiePath);
        }
        public override int GetHashCode() { return (Name + CookiePath).GetHashCode(); }
        public override string ToString() { return this.DisplayName; }
#pragma warning restore 0618
    }
    public enum PathType { File, Directory }
}