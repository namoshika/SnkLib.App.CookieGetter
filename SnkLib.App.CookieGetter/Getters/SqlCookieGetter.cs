﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Data.SQLite;

namespace SunokoLibrary.Application.Browsers
{
    /// <summary>
    /// SQLiteを利用してクッキーを保存するタイプのブラウザからクッキーを取得するクラス
    /// </summary>
    public abstract class SqlCookieGetter : CookieGetterBase
    {
        public SqlCookieGetter(BrowserConfig status) : base(status, PathType.File) { }

        /// <summary>
        /// DBから指定したクエリでCookieを取得する
        /// </summary>
        /// <param name="path">参照先DBファイル</param>
        /// <param name="query">実行するクエリ</param>
        /// <returns>取得されたCookies</returns>
        protected CookieCollection LookupCookies(string path, string query)
        {
            var result = new CookieCollection();
            foreach (var record in LookupEntry(path, query))
            {
                var cookie = DataToCookie(record);
                if (cookie == null)
                    continue;
                result.Add(cookie);
            }
            return result;
        }
        /// <summary>
        /// DBに対してエントリ照会を行う
        /// </summary>
        /// <param name="path">参照先DBファイル</param>
        /// <param name="query">実行するクエリ</param>
        /// <exception cref="CookieImportException">一時ファイル生成失敗。DB照会失敗。</exception>
        protected static List<object[]> LookupEntry(string path, string query)
        {
            if (System.IO.File.Exists(path) == false)
                throw new CookieImportException("クッキーパスが正しく設定されていません。");

            string temp = null;
            try
            {
                temp = Path.GetTempFileName();
                File.Copy(path, temp, true);

                // SQLite3.7.x
                var pathshm = path + "-shm";
                var pathwal = path + "-wal";
                if (File.Exists(pathshm))
                {
                    File.Copy(pathwal, temp + "-wal", true);
                    File.Copy(pathshm, temp + "-shm", true);
                }

                var results = new List<object[]>();
                System.Threading.Thread.Sleep(5);
                using (var sqlConnection = new SQLiteConnection(string.Format("Data Source={0}", temp)))
                {
                    sqlConnection.Open();
                    var command = sqlConnection.CreateCommand();
                    command.Connection = sqlConnection;
                    command.CommandText = query;
                    using (var sdr = command.ExecuteReader())
                        while (sdr.Read())
                        {
                            var items = new object[sdr.FieldCount];
                            for (int i = 0; i < sdr.FieldCount; i++)
                                items[i] = sdr[i];
                            results.Add(items);
                        }
                }
                return results;
            }
            catch (IOException ex)
            { throw new CookieImportException("クッキーを取得中、一時ファイルの生成に失敗しました。", ex); }
            catch (SQLiteException ex)
            { throw new CookieImportException("クッキーを取得中、Sqliteアクセスでエラーが発生しました。", ex); }
            finally
            {
                if (temp != null)
                    try { System.IO.File.Delete(temp); }
                    catch (IOException) { }
            }
        }
        /// <summary>
        /// SQLから取得したデータをクッキーに変換する
        /// </summary>
        /// <param name="data">指定されたQueryで取得した１行分のデータ</param>
        protected abstract System.Net.Cookie DataToCookie(object[] data);
        /// <summary>
        /// 指定されたURLに関連したクッキーを取得するためのクエリーを生成する
        /// </summary>
        protected abstract string MakeQuery(Uri url);
    }
}
