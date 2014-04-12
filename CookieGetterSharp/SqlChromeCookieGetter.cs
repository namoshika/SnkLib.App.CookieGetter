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
	abstract class SqlChromeCookieGetter : SqlCookieGetter {

		private readonly string SELECT_QUERY_VERSION = "SELECT value FROM meta WHERE key='version';";
		private readonly int VERSION = 7;
		private readonly string SELECT_QUERY_V7 = "SELECT value, name, host_key, path, expires_utc, encrypted_value FROM cookies";

		public SqlChromeCookieGetter(CookieStatus status)
			: base(status) {
		}
			    
		protected override System.Net.CookieContainer GetCookies(string path, string query) {
			System.Net.CookieContainer container = new System.Net.CookieContainer();

			if(string.IsNullOrEmpty(path) || !System.IO.File.Exists(path)) {
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

				using(SQLiteConnection sqlConnection = new SQLiteConnection(string.Format(CONNECTIONSTRING_FORMAT, temp))) {
					sqlConnection.Open();

					try {
						using(SQLiteCommand commandVersion = sqlConnection.CreateCommand()) {
							commandVersion.Connection = sqlConnection;
							commandVersion.CommandText = SELECT_QUERY_VERSION;
							using(SQLiteDataReader sdrv = commandVersion.ExecuteReader()) {
								if(!sdrv.Read()) {
									return base.GetCookies(path, query);
								}

								int version = int.Parse(sdrv[0].ToString());
								if(version < VERSION) {
									return base.GetCookies(path, query);
								}
							}
						}
					}
					catch {
						return base.GetCookies(path, query);
					}

					// スマートにならないのかなー
					query = query.Replace("expires_utc FROM cookies", "expires_utc, encrypted_value FROM cookies");

					using(SQLiteCommand command = sqlConnection.CreateCommand()) {
						command.Connection = sqlConnection;
						command.CommandText = query;
						using(SQLiteDataReader sdr = command.ExecuteReader()) {

							while(sdr.Read()) {
								List<object> items = new List<object>();

								for(int i = 0; i < sdr.FieldCount; i++) {
									items.Add(sdr[i]);
								}

								System.Net.Cookie cookie = DataToCookie(items.ToArray());
								try {
									Utility.AddCookieToContainer(container, cookie);
								}
								catch(Exception ex) {
									CookieGetter.Exceptions.Enqueue(ex);
									Console.WriteLine(string.Format("Invalid Format! domain:{0},key:{1},value:{2}", cookie.Domain, cookie.Name, cookie.Value));
								}

							}
						}
					}

					sqlConnection.Close();
				}

			}
			catch(Exception ex) {
				throw new CookieGetterException("クッキーを取得中、Sqliteアクセスでエラーが発生しました。", ex);
			}
			finally {
				if(temp != null) {
					System.IO.File.Delete(temp);
				}
			}

			return container;
		}

		protected string makeWhere(Uri url) {
			Stack<string> hostStack = new Stack<string>(url.Host.Split('.'));
			StringBuilder hostBuilder = new StringBuilder('.' + hostStack.Pop());
			string[] pathes = url.Segments;

			StringBuilder sb = new StringBuilder();
			sb.Append(" WHERE (");

			bool needOr = false;
			while(hostStack.Count != 0) {
				if(needOr) {
					sb.Append(" OR");
				}

				if(hostStack.Count != 1) {
					hostBuilder.Insert(0, '.' + hostStack.Pop());
					sb.AppendFormat(" host_key = \"{0}\"", hostBuilder.ToString());
				}
				else {
					hostBuilder.Insert(0, '%' + hostStack.Pop());
					sb.AppendFormat(" host_key LIKE \"{0}\"", hostBuilder.ToString());
				}

				needOr = true;
			}

			sb.Append(')');
			return sb.ToString();
		}

		protected override string MakeQuery() {
			return SELECT_QUERY_V7 + " ORDER BY creation_utc DESC";
		}

		protected override string MakeQuery(Uri url) {
			return string.Format("{0} {1} ORDER BY creation_utc DESC", SELECT_QUERY_V7, makeWhere(url));
		}

		protected override string MakeQuery(Uri url, string key) {
			return string.Format("{0} {1} AND name = \"{2}\" ORDER BY creation_utc DESC", SELECT_QUERY_V7, makeWhere(url), key);
		}
	}
}
