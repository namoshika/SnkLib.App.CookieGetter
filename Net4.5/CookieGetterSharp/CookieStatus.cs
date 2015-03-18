using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace Hal.CookieGetterSharp
{
    using SunokoLibrary.Application;
    using SunokoLibrary.Application.Browsers;

    [Serializable]
    public class CookieStatus : ISerializable
    {
#pragma warning disable 0618 //Obsolete属性の警告を無効化"
        internal CookieStatus(CookieGetter owner, BrowserType type)
        {
            _owner = owner;
            BrowserType = type;
        }
        CookieGetter _owner;
        string _name, _cookiePath, _displayName;
        bool _isStandalone, _isAvailable;
        PathType _pathType;

        public BrowserType BrowserType { get; private set; }
        public PathType PathType
        { get { return _isStandalone ? _pathType : (PathType)Enum.Parse(typeof(PathType), GetValue(() => _owner.Importer.CookiePathType).ToString()); } }
        public bool IsAvailable
        { get { return _isStandalone ? _isAvailable : GetValue(() => _owner.Importer.IsAvailable); } }
        public string Name
        {
            get
            {
                if (_isStandalone)
                    return _name;

                var browName = GetValue(() => _owner.Importer.SourceInfo.BrowserName);
                var profName = GetValue(() => _owner.Importer.SourceInfo.ProfileName);
                return profName == "Default" || string.IsNullOrEmpty(profName)
                    ? browName : string.Join(" ", new[] { browName, profName });
            }
            internal set
            {
                if (_isStandalone)
                    _name = value;
                else
                    Refresh(value);
            }
        }
        public string CookiePath
        {
            get { return _isStandalone ? _cookiePath : GetValue(() => _owner.Importer.SourceInfo.CookiePath); }
            set
            {
                if (_isStandalone)
                    _cookiePath = value;
                else
                    Refresh(cookiePath: value);
            }
        }
        public string DisplayName
        {
            get { return string.IsNullOrEmpty(_displayName) ? Name : _displayName; }
            set { _displayName = this.Name.Equals(value) ? null : value; }
        }

        TResult GetValue<TResult>(Func<TResult> importer)
        {
            try { return importer(); }
            catch (CookieImportException e)
            { throw new CookieGetterException(e); }
        }
        void Refresh(string name = null, string profileName = null, string cookiePath = null)
        {
            try
            {
                _owner.Importer = _owner.Importer.Generate(
                    _owner.Importer.SourceInfo.GenerateCopy(name, profileName, cookiePath));
            }
            catch (CookieImportException e)
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
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("BrowserType", BrowserType);
            info.AddValue("PathType", PathType);
            info.AddValue("IsAvailable", IsAvailable);
            info.AddValue("Name", Name);
            info.AddValue("CookiePath", CookiePath);
            info.AddValue("DisplayName", DisplayName);
        }
        public CookieStatus(SerializationInfo info, StreamingContext context)
        {
            _isStandalone = true;
            _pathType = (CookieGetterSharp.PathType)info.GetInt32("PathType");
            _isAvailable = info.GetBoolean("IsAvailable");
            _name = info.GetString("Name");
            _cookiePath = info.GetString("CookiePath");
            _displayName = info.GetString("DisplayName");
            BrowserType = (BrowserType)info.GetInt32("BrowserType");
        }
#pragma warning restore 0618
    }
    public enum PathType { File, Directory }
}