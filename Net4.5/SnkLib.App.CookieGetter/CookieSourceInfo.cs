using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Runtime.Serialization;
using System.Xml.Serialization;

namespace SunokoLibrary.Application
{
    /// <summary>
    /// Cookie取得用の項目を保持します。
    /// </summary>
    [TypeConverter(typeof(InfoConverter))]
    [DebuggerDisplay("{BrowserName,nq}({ProfileName,nq}): {CookiePath,nq}")]
    public class CookieSourceInfo : IXmlSerializable
    {
        /// <summary>
        /// 対象のブラウザの構成情報を指定してインスタンスを生成します。
        /// </summary>
        /// <param name="browserName">ブラウザの名前</param>
        /// <param name="profileName">対象の構成情報にブラウザ側で付けられた固有名称</param>
        /// <param name="cookiePath">Cookieファイルパス</param>
        /// <param name="engineId">ブラウザのエンジン識別子</param>
        /// <param name="isCustomized">ユーザ定義による設定かどうか</param>
        public CookieSourceInfo(string browserName, string profileName, string cookiePath, string engineId, bool isCustomized)
        {
            BrowserName = browserName;
            ProfileName = profileName;
            CookiePath = cookiePath;
            EngineId = engineId;
            IsCustomized = isCustomized;
        }
        /// <summary>
        /// シリアル化用。使用しないでください。
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public CookieSourceInfo() { }

        /// <summary>
        /// ユーザーによるカスタム設定かを取得します。
        /// </summary>
        public bool IsCustomized { get; private set; }
        /// <summary>
        /// ブラウザ名を取得します。
        /// </summary>
        public string BrowserName { get; private set; }
        /// <summary>
        /// プロフィール名を取得します。
        /// </summary>
        public string ProfileName { get; private set; }
        /// <summary>
        /// Cookieが保存されている場所を取得します。
        /// </summary>
        public string CookiePath { get; private set; }
        /// <summary>
        /// 使用されているブラウザエンジンの種類を取得します。
        /// </summary>
        public string EngineId { get; private set; }
        /// <summary>
        /// 引数で指定された値で上書きしたコピーを生成します。
        /// </summary>
        public CookieSourceInfo GenerateCopy(string name = null, string profileName = null, string cookiePath = null)
        { return new CookieSourceInfo(name ?? BrowserName, profileName ?? ProfileName, cookiePath ?? CookiePath, EngineId, true); }

#pragma warning disable 1591
        public override int GetHashCode()
        { return CookiePath == null ? 0 : CookiePath.GetHashCode(); }
        public override bool Equals(object obj)
        {
            var target = obj as CookieSourceInfo;
            if ((object)target == null)
                return false;

            //CookiePathが一致していれば同一と見なす。
            //しかし、null同士で一致していた場合は他の要素で確認する。
            return CookiePath == target.CookiePath &&
                (!string.IsNullOrEmpty(CookiePath) || BrowserName == target.BrowserName && ProfileName == target.ProfileName);
        }
        public static bool operator ==(CookieSourceInfo valueA, CookieSourceInfo valueB)
        {
            if(object.ReferenceEquals(valueA, valueB))
                return true;
            if((object)valueA == null)
                return false;
            return valueA.Equals(valueB);
        }
        public static bool operator !=(CookieSourceInfo valueA, CookieSourceInfo valueB)
        { return !(valueA == valueB); }

        System.Xml.Schema.XmlSchema IXmlSerializable.GetSchema() { return null; }
        void IXmlSerializable.ReadXml(System.Xml.XmlReader reader)
        {
            //空タグなら見なかったことにする
            if (reader.IsEmptyElement)
            {
                reader.Read();
                return;
            }
            //読み込み
            reader.ReadStartElement();
            for (var i = 0; i < 5 && reader.NodeType != System.Xml.XmlNodeType.EndElement; )
            {
                var name = reader.Name;
                reader.Read();
                var value = reader.Value;
                reader.Read();
                reader.ReadEndElement();

                //プロパティ5つ分。for文のiはこれらのプロパティ全てを読み込んだら
                //ループを抜けるためにカウンタとして用いられている。
                switch (name)
                {
                    case "IsCustomized":
                        IsCustomized = value == true.ToString();
                        i++;
                        break;
                    case "BrowserName":
                        BrowserName = value;
                        i++;
                        break;
                    case "ProfileName":
                        ProfileName = value;
                        i++;
                        break;
                    case "CookiePath":
                        CookiePath = value;
                        i++;
                        break;
                    case "EngineId":
                        EngineId = value;
                        i++;
                        break;
                }
            }
            reader.ReadEndElement();
        }
        void IXmlSerializable.WriteXml(System.Xml.XmlWriter writer)
        {
            foreach (var member in new Dictionary<string, string>()
                {
                    { "IsCustomized", IsCustomized.ToString() },
                    { "BrowserName", BrowserName },
                    { "ProfileName", ProfileName },
                    { "CookiePath", CookiePath },
                    { "EngineId", EngineId },
                })
            {
                writer.WriteStartElement(member.Key);
                writer.WriteString(member.Value);
                writer.WriteEndElement();
            }
        }
#pragma warning restore 1591

        class InfoConverter : TypeConverter
        {
            public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
            { return sourceType == typeof(string); }
            public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
            {
                const string BOM = "0123456789_IC";
                var text = value as string;
                if (text == null || text.StartsWith(BOM) == false)
                    return base.ConvertFrom(context, culture, value);

                text = text.Substring(BOM.Length);
                var restoredValues = new Dictionary<string, string>();
                for (var i = 0; i < text.Length; i++)
                {
                    var nmCronIdx = text.IndexOf(':', i);
                    var name = text.Substring(i, nmCronIdx - i);
                    var lenCronIdx = text.IndexOf(':', nmCronIdx + 1);
                    var valLen = int.Parse(text.Substring(nmCronIdx + 1, lenCronIdx - nmCronIdx - 1));
                    var val = text.Substring(lenCronIdx + 1, valLen);
                    //項目の末尾まで移動
                    i = lenCronIdx + valLen;
                    restoredValues.Add(name, val);
                }
                //値を展開
                bool isCustom = false;
                string browserName = null, profileName = null, cookiePath = null, engineId = null;
                foreach (var pair in restoredValues)
                    switch (pair.Key)
                    {
                        case "IsCustomized": isCustom = pair.Value == true.ToString(); break;
                        case "BrowserName": browserName = pair.Value; break;
                        case "ProfileName": profileName = pair.Value; break;
                        case "CookiePath": cookiePath = pair.Value; break;
                        case "EngineId": engineId = pair.Value; break;
                    }
                var info = new CookieSourceInfo(browserName, profileName, cookiePath, engineId, isCustom);
                return info;
            }
            public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
            {
                if (destinationType == typeof(string))
                {
                    var info = value as CookieSourceInfo;
                    var res = "0123456789_IC" + string.Join(string.Empty, new Dictionary<string, string>()
                        {
                            { "IsCustomized", info.IsCustomized ? true.ToString() : false.ToString() },
                            { "BrowserName", info.BrowserName },
                            { "ProfileName", info.ProfileName },
                            { "CookiePath", info.CookiePath },
                            { "EngineId", info.EngineId },
                        }
                        .Select(pair =>
                            string.Format("{0}:{1}:{2}", pair.Key, (pair.Value ?? string.Empty).Length, pair.Value))
                        .ToArray());
                    return res;
                }
                return base.ConvertTo(context, culture, value, destinationType);
            }
        }
    }
}
