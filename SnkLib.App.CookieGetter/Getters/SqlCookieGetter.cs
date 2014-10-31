﻿using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace SunokoLibrary.Application.Browsers
{
    /// <summary>
    /// SQLiteを利用してクッキーを保存するタイプのブラウザからクッキーを取得するクラス
    /// </summary>
    public abstract class SqlCookieGetter : CookieGetterBase
    {
        public SqlCookieGetter(BrowserConfig config) : base(config, PathType.File) { }

        /// <summary>
        /// DBから指定したクエリでCookieを取得する
        /// </summary>
        /// <param name="path">参照先DBファイル</param>
        /// <param name="query">実行するクエリ</param>
        /// <returns>取得されたCookies</returns>
        /// <exception cref="CookieImportException" />
        protected async Task<CookieCollection> LookupCookiesAsync(string path, string query)
        {
            var result = new CookieCollection();
            foreach (var record in await LookupEntryAsync(path, query))
            {
                var cookie = DataToCookie(record);
                if (cookie == null)
                    continue;
                result.Add(cookie);
            }
            return result;
        }
        /// <summary>
        /// SQLから取得したデータをクッキーに変換する
        /// </summary>
        /// <param name="data">指定されたQueryで取得した１行分のデータ</param>
        /// <exception cref="CookieImportException">未知の形式のレコードを入力された。</exception>
        protected abstract Cookie DataToCookie(object[] data);
        /// <summary>
        /// DBに対してエントリ照会を行う
        /// </summary>
        /// <param name="path">参照先DBファイル</param>
        /// <param name="query">実行するクエリ</param>
        /// <exception cref="CookieImportException">一時ファイル生成失敗。DB照会失敗。</exception>
        protected static async Task<List<object[]>> LookupEntryAsync(string path, string query)
        {
            if (File.Exists(path) == false)
                throw new InvalidOperationException(string.Format("ファイルが存在しません。{0}", path));

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
                await Task.Delay(5);
                SQLiteConnection sqlConnection = null;
                try
                {
                    sqlConnection = new SQLiteConnection(string.Format("Data Source={0}", temp));
                    sqlConnection.Open();
                    var command = sqlConnection.CreateCommand();
                    command.Connection = sqlConnection;
                    command.CommandText = query;
                    SQLiteDataReader sdr = null;
                    try
                    {
                        sdr = command.ExecuteReader();
                        while (sdr.Read())
                        {
                            var items = new object[sdr.FieldCount];
                            for (int i = 0; i < sdr.FieldCount; i++)
                                items[i] = sdr[i];
                            results.Add(items);
                        }
                    }
                    finally
                    {
                        if (sdr != null)
                            sdr.Close();
                    }
                }
                finally
                {
                    if (sqlConnection != null)
                        sqlConnection.Close();
                }
                return results;
            }
            catch (IOException ex)
            {
                throw new CookieImportException(
                  "クッキーを取得中、一時ファイルの生成に失敗しました。", ImportResult.AccessError, ex);
            }
            catch (SQLiteException ex)
            {
                throw new CookieImportException(
                  "クッキーを取得中、Sqliteアクセスでエラーが発生しました。", ImportResult.ConvertError, ex);
            }
            finally
            {
                if (temp != null)
                    try { System.IO.File.Delete(temp); }
                    catch (IOException) { }
            }
        }
    }
}
