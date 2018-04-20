/**
 * Google Sync Plugin for KeePass Password Safe
 * Copyright (C) 2012-2016  DesignsInnovate
 * Copyright (C) 2014-2016  Paul Voegler
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
**/

using System;
using System.Threading;
using System.Threading.Tasks;
using System.Reflection;
using System.Windows.Forms;

using KeePass.UI;
using KeePass.Plugins;
using KeePass.Forms;
using KeePass.DataExchange;

using KeePassLib;
using KeePassLib.Interfaces;
using KeePassLib.Serialization;
using KeePassLib.Security;
using KeePassLib.Collections;

using Google.Apis.Services;
using Google.Apis.Upload;
using Google.Apis.Drive.v3;
using Google.Apis.Drive.v3.Data;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Auth.OAuth2.Responses;
using Google.Apis.Auth.OAuth2.Requests;


namespace GoogleSyncPlugin
{
	public static class Defs
	{
		private static string m_productName;
		private static string m_productVersion;
		public static string ProductName()
		{
			if (m_productName == null)
			{
				Assembly assembly = Assembly.GetExecutingAssembly();
				AssemblyTitleAttribute assemblyTitle = assembly.GetCustomAttributes(typeof(AssemblyTitleAttribute), false)[0] as AssemblyTitleAttribute;
				m_productName = assemblyTitle.Title;
			}
			return m_productName;
		}
		public static string VersionString()
		{
			if (m_productVersion == null)
			{
				Version version = Assembly.GetExecutingAssembly().GetName().Version;
				m_productVersion = "v" + version.Major + "." + version.Minor + "." + version.Build + "." + version.Revision;
			}
			return m_productVersion;
		}
		public const string ConfigAutoSync = "GoogleSync.AutoSync";
		public const string ConfigUUID = "GoogleSync.AccountUUID";
		public const string ConfigClientId = "GoogleSync.ClientID";
		public const string ConfigClientSecret = "GoogleSync.ClientSecret";
		public const string ConfigRefreshToken = "GoogleSync.RefreshToken";
		public const string ConfigActiveAccount = "GoogleSync.ActiveAccount";
		public const string ConfigActiveAccountTrue = ConfigActiveAccount + ".TRUE";
		public const string ConfigActiveAccountFalse = ConfigActiveAccount + ".FALSE";
		public const string AccountSearchString = "accounts.google.com";
		public const string URLHome = "http://sourceforge.net/p/kp-googlesync";
		public const string URLHelp = "http://sourceforge.net/p/kp-googlesync/support";
		public const string URLGoogleDev = "https://console.developers.google.com/start";
		public const string UpdateUrl = "http://designsinnovate.com/googlesyncplugin/versioninfo.txt";
	}

	[Flags]
	public enum AutoSyncMode
	{
		DISABLED = 0,
		SAVE = 1,
		OPEN = 2
	}

	/// <summary>
	/// main plugin class
	/// </summary>
	public sealed class GoogleSyncPluginExt : Plugin
	{
		private IPluginHost m_host = null;

		private AutoSyncMode m_autoSync = AutoSyncMode.DISABLED;

		private PwEntry m_entry = null;
		private string m_clientId = string.Empty;
		private ProtectedString m_clientSecret = null;
		private ProtectedString m_refreshToken = null;

		private ToolStripSeparator m_tsSeparator = null;
		private ToolStripMenuItem m_tsmiPopup = null;
		private ToolStripMenuItem m_tsmiSync = null;
		private ToolStripMenuItem m_tsmiUpload = null;
		private ToolStripMenuItem m_tsmiDownload = null;
		private ToolStripMenuItem m_tsmiConfigure = null;
		private enum SyncCommand
		{
			DOWNLOAD = 1,
			SYNC = 2,
			UPLOAD = 3
		}

		private const string DefaultClientId = "579467001123-ee60b1ghffl38rgdk6pj7gjdjvagi9i7.apps.googleusercontent.com";
		// pseudosecret (pad is sha256 of DefaultClientId)
		private XorredBuffer DefaultClientSecret = new XorredBuffer(
			new byte[] {
				0x56, 0x26, 0xd7, 0x81, 0xcd, 0x45, 0x8c, 0xd2,
				0xee, 0x3d, 0x1a, 0x6a, 0x64, 0xec, 0x54, 0x5d,
				0xa0, 0x36, 0x24, 0x2c, 0xca, 0x27, 0x81, 0xec
			}, new byte[] {
				0x10, 0x65, 0x98, 0xec, 0xbd, 0x74, 0xe0, 0xe2,
				0xda, 0x72, 0x5f, 0x2b, 0x21, 0x85, 0x2d, 0x34,
				0xf2, 0x78, 0x5e, 0x7e, 0x8b, 0x74, 0xf4, 0xae
			}
		);

		/// <summary>
		/// URL of a version information file
		/// </summary>
		public override string UpdateUrl
		{
			get
			{
				return Defs.UpdateUrl;
			}
		}

		/// <summary>
		/// The <c>Initialize</c> function is called by KeePass when
		/// you should initialize your plugin (create menu items, etc.).
		/// </summary>
		/// <param name="host">Plugin host interface. By using this
		/// interface, you can access the KeePass main window and the
		/// currently opened database.</param>
		/// <returns>You must return <c>true</c> in order to signal
		/// successful initialization. If you return <c>false</c>,
		/// KeePass unloads your plugin (without calling the
		/// <c>Terminate</c> function of your plugin).</returns>
		public override bool Initialize(IPluginHost host)
		{
			if(host == null) return false;
			m_host = host;

			try
			{
				m_autoSync = (AutoSyncMode)Enum.Parse(typeof(AutoSyncMode), m_host.CustomConfig.GetString(Defs.ConfigAutoSync, AutoSyncMode.DISABLED.ToString()), true);
			}
			catch (Exception)
			{
				// support old boolean value (Sync on Save) (may be removed in later versions)
				if (m_host.CustomConfig.GetBool(Defs.ConfigAutoSync, false))
					m_autoSync = AutoSyncMode.SAVE;
			}

			// Get a reference to the 'Tools' menu item container
			ToolStripItemCollection tsMenu = m_host.MainWindow.ToolsMenu.DropDownItems;

			// Add a separator at the bottom
			m_tsSeparator = new ToolStripSeparator();
			tsMenu.Add(m_tsSeparator);

			// Add the popup menu item
			m_tsmiPopup = new ToolStripMenuItem();
			m_tsmiPopup.Text = Defs.ProductName();
			tsMenu.Add(m_tsmiPopup);

			m_tsmiSync = new ToolStripMenuItem();
			m_tsmiSync.Name = SyncCommand.SYNC.ToString();
			m_tsmiSync.Text = "同步";
			m_tsmiSync.Click += OnSyncWithGoogle;
			m_tsmiPopup.DropDownItems.Add(m_tsmiSync);

			m_tsmiUpload = new ToolStripMenuItem();
			m_tsmiUpload.Name = SyncCommand.UPLOAD.ToString();
			m_tsmiUpload.Text = "上传";
			m_tsmiUpload.Click += OnSyncWithGoogle;
			m_tsmiPopup.DropDownItems.Add(m_tsmiUpload);

			m_tsmiDownload = new ToolStripMenuItem();
			m_tsmiDownload.Name = SyncCommand.DOWNLOAD.ToString();
			m_tsmiDownload.Text = "下载";
			m_tsmiDownload.Click += OnSyncWithGoogle;
			m_tsmiPopup.DropDownItems.Add(m_tsmiDownload);

			m_tsmiConfigure = new ToolStripMenuItem();
			m_tsmiConfigure.Name = "CONFIG";
			m_tsmiConfigure.Text = "配置...";
			m_tsmiConfigure.Click += OnConfigure;
			m_tsmiPopup.DropDownItems.Add(m_tsmiConfigure);

			// We want a notification when the user tried to save the
			// current database or opened a new one.
			m_host.MainWindow.FileSaved += OnFileSaved;
			m_host.MainWindow.FileOpened += OnFileOpened;

			return true; // Initialization successful
		}

		/// <summary>
		/// The <c>Terminate</c> function is called by KeePass when
		/// you should free all resources, close open files/streams,
		/// etc. It is also recommended that you remove all your
		/// plugin menu items from the KeePass menu.
		/// </summary>
		public override void Terminate()
		{
			// Remove all of our menu items
			ToolStripItemCollection tsMenu = m_host.MainWindow.ToolsMenu.DropDownItems;
			tsMenu.Remove(m_tsSeparator);
			tsMenu.Remove(m_tsmiSync);
			tsMenu.Remove(m_tsmiUpload);
			tsMenu.Remove(m_tsmiDownload);
			tsMenu.Remove(m_tsmiConfigure);

			// Important! Remove event handlers!
			m_host.MainWindow.FileSaved -= OnFileSaved;
			m_host.MainWindow.FileOpened -= OnFileOpened;
		}

		/// <summary>
		/// Event handler to implement auto sync on save
		/// </summary>
		private void OnFileSaved(object sender, FileSavedEventArgs e)
		{
			if (e.Success && AutoSyncMode.SAVE == (m_autoSync & AutoSyncMode.SAVE))
			{
				if (Keys.Shift == (Control.ModifierKeys & Keys.Shift))
					ShowMessage("按下 Shift 键，跳过自动同步。", true);
				else if (!LoadConfiguration())
					ShowMessage("找不到有效的配置，跳过自动同步。", true);
				else
					syncWithGoogle(SyncCommand.SYNC, true);
			}
		}

		/// <summary>
		/// Event handler to implement auto sync on open
		/// </summary>
		private void OnFileOpened(object sender, FileOpenedEventArgs e)
		{
			if (AutoSyncMode.OPEN == (m_autoSync & AutoSyncMode.OPEN))
			{
				if (Keys.Shift == (Control.ModifierKeys & Keys.Shift))
					ShowMessage("按下 Shift 键，跳过自动同步。", true);
				else if (!LoadConfiguration())
					ShowMessage("找不到有效的配置，跳过自动同步。", true);
				else
					syncWithGoogle(SyncCommand.SYNC, true);
			}
		}

		/// <summary>
		/// Event handler for sync menu entries
		/// </summary>
		private void OnSyncWithGoogle(object sender, EventArgs e)
		{
			ToolStripItem item = (ToolStripItem)sender;
			SyncCommand syncCommand = (SyncCommand)Enum.Parse(typeof(SyncCommand), item.Name);
			syncWithGoogle(syncCommand, false);
		}

		/// <summary>
		/// Event handler for configuration menu entry
		/// </summary>
		private void OnConfigure(object sender, EventArgs e)
		{
			if (!m_host.Database.IsOpen)
			{
				ShowMessage("你需要先打开一个数据库");
				return;
			}

			if (AskForConfiguration())
				SaveConfiguration();
		}

		/// <summary>
		/// Sync the current database with Google Drive. Create a new file if it does not already exists
		/// </summary>
		private void syncWithGoogle(SyncCommand syncCommand, bool autoSync)
		{
			if (!m_host.Database.IsOpen)
			{
				ShowMessage("你需要先打开一个数据库");
				return;
			}
			else if (!m_host.Database.IOConnectionInfo.IsLocalFile())
			{
				ShowMessage("只支持本地数据库或网络共享数据库。\n" +
					"将您的数据库保存到本地或网络共享中，再重试。");
				return;
			}

			string status = "请稍后 ...";
			try
			{
				m_host.MainWindow.FileSaved -= OnFileSaved; // disable to not trigger when saving ourselves
				m_host.MainWindow.FileOpened -= OnFileOpened; // disable to not trigger when opening ourselves
				ShowMessage(status, true);
				m_host.MainWindow.Enabled = false;

				// abort when user cancelled or didn't provide config
				if (!GetConfiguration())
					throw new PlgxException(Defs.ProductName() + " 中止！");

				// Get Access Token / Authorization
				// Invoke async method GetAuthorization from thread pool to have no context to marshal back to
				// and thus making this call synchroneous without running into potential deadlocks.
				// Needed so that KeePass can't close the db or lock the workspace before we are done syncing
				UserCredential myCredential = Task.Run(() => GetAuthorization()).Result;

				// Create a new Google Drive API Service
				DriveService service = new DriveService(new BaseClientService.Initializer()
				{
					HttpClientInitializer = myCredential,
					ApplicationName = "Google Sync Plugin"
                });

				string filePath = m_host.Database.IOConnectionInfo.Path;
				string contentType = "application/x-keepass2";

				File file = getFile(service, filePath);
				if (file == null)
				{
					if (syncCommand == SyncCommand.DOWNLOAD)
					{
						status = "再 Google Drive 上找不到文件，请先上传或与 Google Drive 同步。";
					}
					else // upload
					{
						if (!autoSync)
							m_host.Database.Save(new NullStatusLogger());
						status = uploadFile(service, "KeePass 密码安全数据库", string.Empty, contentType, filePath);
					}
				}
				else
				{
					if (syncCommand == SyncCommand.UPLOAD)
					{
						if (!autoSync)
							m_host.Database.Save(new NullStatusLogger());
						status = updateFile(service, file, filePath, contentType);
					}
					else
					{
						string downloadFilePath = downloadFile(service, file, filePath);
						if (!String.IsNullOrEmpty(downloadFilePath))
						{
							if (syncCommand == SyncCommand.DOWNLOAD)
								status = replaceDatabase(filePath, downloadFilePath);
							else // sync
								status = String.Format("{0} {1}",
									syncFile(downloadFilePath),
									updateFile(service, file, filePath, contentType));
						}
						else
							status = "无法下载文件。";
					}
				}
			}
			catch (TokenResponseException ex)
			{
				string msg = string.Empty;
				switch (ex.Error.Error)
				{
					case "access_denied":
						msg = "访问授权被拒绝";
						break;
					case "invalid_request":
						msg = "客户端ID或密匙丢失";
						break;
					case "invalid_client":
						msg = "客户端ID或密匙无效";
						break;
					case "invalid_grant":
						msg = "提供的密匙无效或过期";
						break;
					case "unauthorized_client":
						msg = "未经授权的客户端";
						break;
					case "unsupported_grant_type":
						msg = "授权服务器不支持该授权类型";
						break;
					case "invalid_scope":
						msg = "请求的范围无效";
						break;
					default:
						msg = ex.Message;
						break;
				}

				status = "ERROR";
				ShowMessage(msg);
			}
			catch (Exception ex)
			{
				status = "ERROR";
				ShowMessage(ex.Message);
			}

			m_host.MainWindow.UpdateUI(false, null, true, null, true, null, false);
			ShowMessage(status, true);
			m_host.MainWindow.Enabled = true;
			m_host.MainWindow.FileSaved += OnFileSaved;
			m_host.MainWindow.FileOpened += OnFileOpened;
		}

		/// <summary>
		/// Download a file and return a string with its content.
		/// </summary>
		/// <param name="service">The Google Drive service</param>
		/// <param name="file">The Google Drive File instance</param>
		/// <param name="filePath">The local file name and path to download to (timestamp will be appended)</param>
		/// <returns>File's path if successful, null or empty otherwise.</returns>
		private string downloadFile(DriveService service, File file, string filePath)
		{
			if (file == null || String.IsNullOrEmpty(file.Id) || String.IsNullOrEmpty(filePath))
				return null;

			string downloadFilePath = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(filePath),
				System.IO.Path.GetFileNameWithoutExtension(filePath))
				+ DateTime.Now.ToString("_yyyyMMddHHmmss")
				+ System.IO.Path.GetExtension(filePath);

			FilesResource.GetRequest request = service.Files.Get(file.Id);
			using (System.IO.FileStream fileStream = new System.IO.FileStream(downloadFilePath, System.IO.FileMode.Create, System.IO.FileAccess.Write))
			{
				request.Download(fileStream);
			}

			return downloadFilePath;
		}

		/// <summary>
		/// Get File from Google Drive
		/// </summary>
		/// <param name="service">DriveService</param>
		/// <param name="filepath">Full path of the current database file</param>
		/// <returns>Return Google File</returns>
		private File getFile(DriveService service, string filepath)
		{
			string filename = System.IO.Path.GetFileName(filepath);
			FilesResource.ListRequest req = service.Files.List();
			req.Q = "name='" + filename.Replace("'", "\\'") + "' and trashed=false";
			FileList files = req.Execute();
			if (files.Files.Count < 1)
				return null;
			else if (files.Files.Count == 1)
				return files.Files[0];

			throw new PlgxException("在 Google Drive 发现多个重名文件'" + filename + "'\n\n请确保文件名在所有文件夹中都是唯一的。");
		}

		/// <summary>
		/// Sync Google Drive File with currently open Database file
		/// </summary>
		/// <param name="downloadFilePath">Full path of database file to sync with</param>
		/// <returns>Return status of the update</returns>
		private string syncFile(string downloadFilePath)
		{
			IOConnectionInfo connection = IOConnectionInfo.FromPath(downloadFilePath);
			bool? success = ImportUtil.Synchronize(m_host.Database, m_host.MainWindow, connection, true, m_host.MainWindow);

			System.IO.File.Delete(downloadFilePath);

			if (!success.HasValue)
				throw new PlgxException("同步失败。\n\n没有导入权限，请调整 KeePass 配置");
			if (!(bool)success)
				throw new PlgxException("同步失败。\n\n" +
                    "如果主密匙（密码）不正确，请使用 上传/下载 命令，而不是同步。\n\n" +
                    "或更改本地主密匙与远程数据库的密钥一致。");

			return "本地文件同步。";
		}

		/// <summary>
		/// Replace contents of the Google Drive File with currently open Database file
		/// </summary>
		/// <param name="service">DriveService</param>
		/// <param name="file">File from Google Drive</param>
		/// <param name="filePath">Full path of the current database file</param>
		/// <param name="contentType">Content type of the Database file</param>
		/// <returns>Return status of the update</returns>
		private string updateFile(DriveService service, File file, string filePath, string contentType)
		{
			byte[] byteArray = System.IO.File.ReadAllBytes(filePath);
			System.IO.MemoryStream stream = new System.IO.MemoryStream(byteArray);

			File temp = new File();
			FilesResource.UpdateMediaUpload request = service.Files.Update(temp, file.Id, stream, contentType);
			IUploadProgress progress = request.Upload();
			if (progress.Exception != null)
			{
				throw progress.Exception;
			}

			return string.Format("Google Drive 文件已更新，文件名：{0}，ID：{1}", file.Name, file.Id);
		}

		/// <summary>
		/// Upload a new file to Google Drive
		/// </summary>
		/// <param name="service">DriveService</param>
		/// <param name="description">File description</param>
		/// <param name="fileName">File name</param>
		/// <param name="contentType">File content type</param>
		/// <param name="filepath">Full path of the current database file</param>
		/// <returns>Return status of the upload</returns>
		private string uploadFile(DriveService service, string description, string fileName, string contentType, string filePath)
		{
			File temp = new File();
			if (string.IsNullOrEmpty(fileName))
				temp.Name = System.IO.Path.GetFileName(filePath);
			else
				temp.Name = fileName;
			temp.Description = description;
			temp.MimeType = contentType;

			byte[] byteArray = System.IO.File.ReadAllBytes(filePath);
			System.IO.MemoryStream stream = new System.IO.MemoryStream(byteArray);

			FilesResource.CreateMediaUpload request = service.Files.Create(temp, stream, contentType);
			IUploadProgress progress = request.Upload();
			if (progress.Exception != null)
			{
				throw progress.Exception;
			}

			File file = request.ResponseBody;
			return string.Format("Google Drive 文件已更新，文件名：{0}，ID：{1}", file.Name, file.Id);
		}

		/// <summary>
		/// Replace the current database file with a new file and open it
		/// </summary>
		/// <param name="downloadFilePath">Full path of the new database file</param>
		/// <returns>Status of the replacement</returns>
		private string replaceDatabase(string currentFilePath, string downloadFilePath)
		{
			string status = string.Empty;
			string tempFilePath = currentFilePath + ".gsyncbak";

			KeePassLib.Keys.CompositeKey pwKey = m_host.Database.MasterKey;
			m_host.Database.Close();

			try
			{
				System.IO.File.Move(currentFilePath, tempFilePath);
				System.IO.File.Move(downloadFilePath, currentFilePath);
				System.IO.File.Delete(tempFilePath);
				status = "下载文件替换当前数据库 '" + System.IO.Path.GetFileName(currentFilePath) + "'";
			}
			catch (Exception)
			{
				status = "替换当前数据库失败，下载文件在 '" + System.IO.Path.GetFileName(downloadFilePath) + "'";
			}

			// try to open new (or old in case of error) db
			try
			{
				// try to open with current MasterKey ...
				m_host.Database.Open(IOConnectionInfo.FromPath(currentFilePath), pwKey, new NullStatusLogger());
			}
			catch (KeePassLib.Keys.InvalidCompositeKeyException)
			{
				// ... MasterKey is different, let user enter the MasterKey
				m_host.MainWindow.OpenDatabase(IOConnectionInfo.FromPath(currentFilePath), null, true);
			}

			return status;
		}

		/// <summary>
		/// Get Access Token from Google OAuth 2.0 API
		/// </summary>
		/// <returns>The UserCredentials with Access Token and Refresh Token</returns>
		private async Task<UserCredential> GetAuthorization()
		{
			if (!LoadConfiguration())
				return null;

			// Set up the Installed App OAuth 2.0 Flow for Google APIs with a custom code receiver that uses the Browser inside a native Form.
			GoogleAuthorizationCodeFlow.Initializer myInitializer = new GoogleAuthorizationCodeFlow.Initializer();
			myInitializer.ClientSecrets = new ClientSecrets() { ClientId = m_clientId, ClientSecret = m_clientSecret.ReadString() };
			myInitializer.Scopes = new[] { DriveService.Scope.Drive };
			GoogleAuthorizationCodeFlow myFlow = new GoogleAuthorizationCodeFlow(myInitializer);
			//myFlow.HttpClient.Timeout = new TimeSpan(0, 0, 10); // 10s
			NativeCodeReceiver myCodeReceiver = new NativeCodeReceiver(m_entry.Strings.Get(PwDefs.UserNameField).ReadString(), m_entry.Strings.Get(PwDefs.PasswordField));
			AuthorizationCodeInstalledApp myAuth = new AuthorizationCodeInstalledApp(myFlow, myCodeReceiver);
			UserCredential myCredential = null;

			// Try using an existing Refresh Token to get a new Access Token
			if (m_refreshToken != null && !m_refreshToken.IsEmpty)
			{
				try
				{
					TokenResponse myTokenResponse = await myAuth.Flow.RefreshTokenAsync("user", m_refreshToken.ReadString(), CancellationToken.None);
					myCredential = new UserCredential(myFlow, "user", myTokenResponse);
				}
				catch (TokenResponseException ex)
				{
					switch (ex.Error.Error)
					{
						case "invalid_grant":
							myCredential = null; // Refresh Token is invalid. Get user authorization below.
							break;
						default:
							throw ex;
					}
				}
			}

			// Let the User authorize the access
			if (myCredential == null || String.IsNullOrEmpty(myCredential.Token.AccessToken))
			{
				myCredential = await myAuth.AuthorizeAsync("user", CancellationToken.None);

				// save the refresh token if new or different
				if (myCredential != null && !String.IsNullOrEmpty(myCredential.Token.RefreshToken) && (m_refreshToken == null || myCredential.Token.RefreshToken != m_refreshToken.ReadString()))
				{
					m_refreshToken = new ProtectedString(true, myCredential.Token.RefreshToken);
					SaveConfiguration();
				}
			}

			return myCredential;
		}

		/// <summary>
		/// Find active configured Google Accounts
		/// Should only return one account
		/// </summary>
		private PwObjectList<PwEntry> FindActiveAccounts()
		{
			if (!m_host.Database.IsOpen)
				return null;

			PwObjectList<PwEntry> accounts = new PwObjectList<PwEntry>();

			SearchParameters sp = new SearchParameters();
			sp.SearchString = Defs.ConfigActiveAccountTrue;
			sp.ComparisonMode = StringComparison.Ordinal;
			sp.RespectEntrySearchingDisabled = false;
			sp.SearchInGroupNames = false;
			sp.SearchInNotes = false;
			sp.SearchInOther = true;
			sp.SearchInPasswords = false;
			sp.SearchInTags = false;
			sp.SearchInTitles = false;
			sp.SearchInUrls = false;
			sp.SearchInUserNames = false;
			sp.SearchInUuids = false;
			m_host.Database.RootGroup.SearchEntries(sp, accounts);

			for (int idx = 0; idx < accounts.UCount; idx++)
			{
				PwEntry entry = accounts.GetAt((uint)idx);
				if (!(entry.Strings.Exists(Defs.ConfigActiveAccount) && entry.Strings.Get(Defs.ConfigActiveAccount).ReadString().Equals(Defs.ConfigActiveAccountTrue)))
					accounts.RemoveAt((uint)idx--);
			}

			return accounts;
		}

		/// <summary>
		/// Show the configuration form
		/// </summary>
		private bool AskForConfiguration()
		{
			if (!m_host.Database.IsOpen)
				return false;

			// find google accounts
			SearchParameters sp = new SearchParameters();
			sp.SearchString = Defs.AccountSearchString;
			sp.ComparisonMode = StringComparison.OrdinalIgnoreCase;
			sp.RespectEntrySearchingDisabled = false;
			sp.SearchInGroupNames = false;
			sp.SearchInNotes = false;
			sp.SearchInOther = false;
			sp.SearchInPasswords = false;
			sp.SearchInTags = false;
			sp.SearchInTitles = true;
			sp.SearchInUrls = true;
			sp.SearchInUserNames = false;
			sp.SearchInUuids = false;
			PwObjectList<PwEntry> accounts = new PwObjectList<PwEntry>();
			m_host.Database.RootGroup.SearchEntries(sp, accounts);

			// find the active account
			string strUuid = null;
			PwEntry entry = null;
			PwObjectList<PwEntry> activeAccounts = FindActiveAccounts();
			if (activeAccounts != null && activeAccounts.UCount == 1)
			{
				entry = activeAccounts.GetAt(0);
			}
			else
			{
				// alternatively try to find the active account in the config file (old configuration) (may be removed in later versions)
				strUuid = m_host.CustomConfig.GetString(Defs.ConfigUUID);
				try
				{
					entry = m_host.Database.RootGroup.FindEntry(new PwUuid(KeePassLib.Utility.MemUtil.HexStringToByteArray(strUuid)), true);
				}
				catch (ArgumentException) { }
			}

			// find configured entry in account list
			int idx = -1;
			if (entry != null)
			{
				idx = accounts.IndexOf(entry);
				// add configured entry to account list if not already present
				if (idx < 0)
				{
					accounts.Insert(0, entry);
					idx = 0;
				}
			}

			ConfigurationForm form1 = new ConfigurationForm(accounts, idx, m_autoSync);
			if (DialogResult.OK != UIUtil.ShowDialogAndDestroy(form1))
				return false;

			entry = null;
			strUuid = form1.Uuid;
			try
			{
				// will throw ArgumentException when Uuid is empty and association shall be removed
				entry = m_host.Database.RootGroup.FindEntry(new PwUuid(KeePassLib.Utility.MemUtil.HexStringToByteArray(strUuid)), true);
			}
			catch (ArgumentException) { }

			if (entry == null && !String.IsNullOrEmpty(strUuid))
			{
				ShowMessage("没有找到 UUID：'" + strUuid + "' 关联密码");
				return false;
			}

			m_entry = entry;
			m_clientId = form1.ClientId;
			m_clientSecret = new ProtectedString(true, form1.ClientSecrect);
			m_refreshToken = (m_entry != null) ? m_entry.Strings.Get(Defs.ConfigRefreshToken) : null;
			m_autoSync = form1.AutoSync;

			return true;
		}

		/// <summary>
		/// Load the current configuration
		/// </summary>
		private bool LoadConfiguration()
		{
			m_entry = null;
			m_clientId = string.Empty;
			m_clientSecret = null;
			m_refreshToken = null;

			if (!m_host.Database.IsOpen)
				return false;

			// find the active account
			PwObjectList<PwEntry> accounts = FindActiveAccounts();
			if (accounts != null && accounts.UCount == 1)
			{
				m_entry = accounts.GetAt(0);
			}
			else
			{
				// alternatively try to find the active account in the config file (old configuration) (may be removed in later versions)
				string strUuid = m_host.CustomConfig.GetString(Defs.ConfigUUID);
				try
				{
					m_entry = m_host.Database.RootGroup.FindEntry(new PwUuid(KeePassLib.Utility.MemUtil.HexStringToByteArray(strUuid)), true);
				}
				catch (ArgumentException) { }
			}

			if (m_entry == null)
				return false;

			// read OAuth 2.0 credentials
			ProtectedString pstr = m_entry.Strings.Get(Defs.ConfigClientId);
			if (pstr != null)
				m_clientId = pstr.ReadString();
			m_clientSecret = m_entry.Strings.Get(Defs.ConfigClientSecret);
			m_refreshToken = m_entry.Strings.Get(Defs.ConfigRefreshToken);

			// use default OAuth 2.0 credentials if missing
			if (String.IsNullOrEmpty(m_clientId))
			{
				m_clientId = DefaultClientId;
				m_clientSecret = new ProtectedString(true, DefaultClientSecret);
			}

			// something missing?
			if (m_entry == null || String.IsNullOrEmpty(m_clientId) || m_clientSecret == null || m_clientSecret.IsEmpty)
				return false;

			return true;
		}

		/// <summary>
		/// Load the current configuration or ask for configuration if missing
		/// </summary>
		private bool GetConfiguration()
		{
			// configuration not complete?
			if (!LoadConfiguration())
			{
				if (!AskForConfiguration())
					return false; // user cancelled or error

				SaveConfiguration();

				// user deleted configuration
				if (m_entry == null)
					return false;

				// use default OAuth 2.0 credentials if missing
				if (String.IsNullOrEmpty(m_clientId))
				{
					m_clientId = DefaultClientId;
					m_clientSecret = new ProtectedString(true, DefaultClientSecret);
				}
			}

			// only return true if in fact nothing is missing
			if (m_entry == null || String.IsNullOrEmpty(m_clientId) || m_clientSecret == null || m_clientSecret.IsEmpty)
				return false;

			return true;
		}

		/// <summary>
		/// Save the current configuration
		/// </summary>
		private bool SaveConfiguration()
		{
			m_host.CustomConfig.SetString(Defs.ConfigAutoSync, m_autoSync.ToString());

			// remove old uuid config (may be removed in later versions)
			if (m_host.CustomConfig.GetString(Defs.ConfigUUID) != null)
				m_host.CustomConfig.SetString(Defs.ConfigUUID, null);

			if (!m_host.Database.IsOpen)
				return false;

			// disable all currently active accounts but the selected (if any)
			PwObjectList<PwEntry> accounts = FindActiveAccounts();
			foreach (PwEntry entry in accounts)
			{
				if (!entry.Equals(m_entry) && entry.Strings.Exists(Defs.ConfigActiveAccount))
				{
					entry.Strings.Set(Defs.ConfigActiveAccount, new ProtectedString(false, Defs.ConfigActiveAccountFalse));
					entry.Touch(true);
				}
			}

			if (m_entry == null)
				return true;

			m_entry.Strings.Set(Defs.ConfigActiveAccount, new ProtectedString(false, Defs.ConfigActiveAccountTrue));

			if (m_clientId == DefaultClientId || String.IsNullOrEmpty(m_clientId))
			{
				m_entry.Strings.Remove(Defs.ConfigClientId);
				m_entry.Strings.Remove(Defs.ConfigClientSecret);
			}
			else
			{
				m_entry.Strings.Set(Defs.ConfigClientId, new ProtectedString(false, m_clientId));
				if (m_clientSecret != null)
					m_entry.Strings.Set(Defs.ConfigClientSecret, m_clientSecret);
			}
			if (m_refreshToken != null)
				m_entry.Strings.Set(Defs.ConfigRefreshToken, m_refreshToken);

			// mark entry as modified (important)
			m_entry.Touch(true);
			m_host.Database.Save(new NullStatusLogger());

			return true;
		}

		/// <summary>
		/// Show message as an alert or in the status bar
		/// </summary>
		/// <param name="msg"></param>
		/// <param name="isStatusMessage"></param>
		private void ShowMessage(string msg, bool isStatusMessage = false)
		{
			if (isStatusMessage)
			{
				m_host.MainWindow.SetStatusEx(Defs.ProductName() + ": " + msg);
			}
			else
			{
				MessageBox.Show(msg, Defs.ProductName());
			}
		}
	}

	public class NativeCodeReceiver : ICodeReceiver
	{
		private Uri m_authorizationUrl = null;
		private string m_email = string.Empty;
		private ProtectedString m_passwd = null;
		private bool m_success = false;
		private string m_code = "access_denied";

		public string RedirectUri
		{
			get { return GoogleAuthConsts.InstalledAppRedirectUri; }
		}

		public NativeCodeReceiver(string email, ProtectedString passwd)
		{
			m_email = email;
			m_passwd = passwd;
		}

		public Task<AuthorizationCodeResponseUrl> ReceiveCodeAsync(AuthorizationCodeRequestUrl url,
			CancellationToken taskCancellationToken)
		{
			TaskCompletionSource<AuthorizationCodeResponseUrl> tcs = new TaskCompletionSource<AuthorizationCodeResponseUrl>();

			if (url is GoogleAuthorizationCodeRequestUrl && !String.IsNullOrEmpty(m_email))
				((GoogleAuthorizationCodeRequestUrl)url).LoginHint = m_email;
			m_authorizationUrl = url.Build();

			Thread t = new Thread(new ThreadStart(RunBrowser));
			t.SetApartmentState(ApartmentState.STA);
			t.Start();
			do
			{
				Thread.Yield();
			} while (t.IsAlive);

			if (m_success)
				tcs.SetResult(new AuthorizationCodeResponseUrl() { Code = m_code });
			else
				tcs.SetResult(new AuthorizationCodeResponseUrl() { Error = m_code });

			return tcs.Task;
		}

		private void RunBrowser()
		{
			GoogleAuthenticateForm form1 = new GoogleAuthenticateForm(m_authorizationUrl, m_email, m_passwd);
			UIUtil.ShowDialogAndDestroy(form1);

			m_success = form1.Success;
			m_code = form1.Code;
		}
	}
}
