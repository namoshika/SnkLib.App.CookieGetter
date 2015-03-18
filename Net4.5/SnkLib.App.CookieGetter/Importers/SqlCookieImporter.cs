﻿using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;

namespace SunokoLibrary.Application.Browsers
{
    /// <summary>
    /// Cookieの保存にSQLiteを使用するタイプのブラウザからCookieを取得します。
    /// </summary>
    public abstract class SqlCookieImporter : CookieImporterBase
    {
#pragma warning disable 1591
        public SqlCookieImporter(CookieSourceInfo info, int primaryLevel) : base(info, CookiePathType.File, primaryLevel) { }
#pragma warning restore 1591

        /// <summary>
        /// DBから指定したクエリでCookieを取得します。
        /// </summary>
        /// <param name="path">参照先DBファイル</param>
        /// <param name="query">実行するクエリ</param>
        /// <param name="dataConverter">レコードからCookieへ変換するメソッド</param>
        /// <returns>取得されたCookies</returns>
        /// <exception cref="CookieImportException" />
        protected static IEnumerable<Cookie> LookupCookies(string path, string query, Func<object[], Cookie> dataConverter)
        {
            return LookupEntry(path, query)
                .Select(record => dataConverter(record)).Where(cookie => cookie != null);
        }
        /// <summary>
        /// DBに対してエントリ照会を行います。
        /// </summary>
        /// <param name="path">参照先DBファイル</param>
        /// <param name="query">実行するクエリ</param>
        /// <exception cref="CookieImportException">一時ファイル生成失敗。DB照会失敗。</exception>
        protected static List<object[]> LookupEntry(string path, string query)
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
                  "Cookieを取得中、一時ファイルの生成に失敗しました。", CookieImportState.AccessError, ex);
            }
            catch (SQLiteException ex)
            {
                throw new CookieImportException(
                  "Cookieを取得中、Sqliteアクセスでエラーが発生しました。", CookieImportState.ConvertError, ex);
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
