using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Data.SQLite;

namespace Hal.CookieGetterSharp
{
	/// <summary>
	/// SQLiteを利用してクッキーを保存するタイプのブラウザからクッキーを取得するクラス
	/// </summary>
	abstract class SqlCookieGetter : CookieGetter
	{
		protected readonly string CONNECTIONSTRING_FORMAT = "Data Source={0}";

		public SqlCookieGetter(CookieStatus status) : base(status) 
		{ 
		}

		public override System.Net.Cookie GetCookie(Uri url, string key)
		{
			System.Net.CookieContainer container = GetCookies(base.CookiePath, MakeQuery(url, key));
			System.Net.CookieCollection collection = container.GetCookies(Utility.AddSrashLast(url));
			return collection[key];
		}

		public override System.Net.CookieCollection GetCookieCollection(Uri url)
		{
			System.Net.CookieContainer container = GetCookies(base.CookiePath, MakeQuery(url));
			return container.GetCookies(Utility.AddSrashLast(url));
		}

		public override System.Net.CookieContainer GetAllCookies()
		{
			return GetCookies(base.CookiePath, MakeQuery());
		}

		protected virtual System.Net.CookieContainer GetCookies(string path, string query) {
			System.Net.CookieContainer container = new System.Net.CookieContainer();

			if (path == null || !System.IO.File.Exists(path)) {
				return container;
				throw new CookieGetterException("クッキーパスが正しく設定されていません。");
			}


			string temp = null;

			try {
				// 一時ファイルの取得ができない環境に対応
				temp = Utility.GetTempFilePath();
				System.IO.File.Copy(path, temp, true);

				// SQLite3.7.x
				string pathshm = path + "-shm";
				string pathwal = path + "-wal";
				if(File.Exists(pathshm)) {
					System.IO.File.Copy(pathwal, temp + "-wal", true);
					System.IO.File.Copy(pathshm, temp + "-shm", true);
				}

				System.Threading.Thread.Sleep(5);

				using (SQLiteConnection sqlConnection = new SQLiteConnection(string.Format(CONNECTIONSTRING_FORMAT, temp))) {
					sqlConnection.Open();

					SQLiteCommand command = sqlConnection.CreateCommand();
					command.Connection = sqlConnection;
					command.CommandText = query;
					SQLiteDataReader sdr = command.ExecuteReader();

					while (sdr.Read()) {
						List<object> items = new List<object>();

						for (int i = 0; i < sdr.FieldCount; i++) {
							items.Add(sdr[i]);
						}

						System.Net.Cookie cookie = DataToCookie(items.ToArray());
						try {
							Utility.AddCookieToContainer(container, cookie);
						} catch (Exception ex){
							CookieGetter.Exceptions.Enqueue(ex);
							Console.WriteLine(string.Format("Invalid Format! domain:{0},key:{1},value:{2}", cookie.Domain, cookie.Name, cookie.Value));
						}

					}

					sqlConnection.Close();
				}

			} catch (Exception ex) {
				throw new CookieGetterException("クッキーを取得中、Sqliteアクセスでエラーが発生しました。", ex);
			} finally {
				if (temp != null) {
					System.IO.File.Delete(temp);
				}
			}

			return container;
		}

		/// <summary>
		/// SQLから取得したデータをクッキーに変換する
		/// </summary>
		/// <param name="data">指定されたQueryで取得した１行分のデータ</param>
		/// <returns></returns>
		protected abstract System.Net.Cookie DataToCookie(object[] data);

		/// <summary>
		/// すべてのクッキーを取得するためのクエリーを生成する
		/// </summary>
		/// <returns></returns>
		protected abstract string MakeQuery();

		/// <summary>
		/// 指定されたURLに関連したクッキーを取得するためのクエリーを生成する
		/// </summary>
		/// <param name="url"></param>
		/// <returns></returns>
		protected abstract string MakeQuery(Uri url);

		/// <summary>
		/// 指定されたURLの名前がkeyであるクッキーを取得するためのクエリーを生成する
		/// </summary>
		/// <param name="url"></param>
		/// <param name="key"></param>
		/// <returns></returns>
		protected abstract string MakeQuery(Uri url, string key);
	}
}
