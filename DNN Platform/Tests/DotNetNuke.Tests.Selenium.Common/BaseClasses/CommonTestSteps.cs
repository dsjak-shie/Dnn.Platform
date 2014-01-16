﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Eventing;
using System.IO;
using System.Reflection;
using DNNSelenium.Common.CorePages;
using NUnit.Framework;
using OpenQA.Selenium;

namespace DNNSelenium.Common.BaseClasses
{
	public class CommonTestSteps : TestBase
	{
		protected IWebDriver _driver = null;
		protected string _baseUrl = null;

		public const string DNNSeleniumPrefix = "DNNSelenium.";
		public string _logContent;
		public string _physicalPath = null;

		public BasePage OpenPage(string assyName, string pageClassName, string openMethod)
		{
			Trace.WriteLine(BasePage.TraceLevelComposite + "'Navigation To " + pageClassName + "'");

			string fullAssyName = DNNSeleniumPrefix + assyName;
			Type pageClassType = Type.GetType(fullAssyName + "." + pageClassName + ", " + fullAssyName);
			object navToPage = Activator.CreateInstance(pageClassType, new object[] { _driver });

			MethodInfo miOpen = pageClassType.GetMethod(openMethod);
			if (miOpen != null)
			{
				miOpen.Invoke(navToPage, new object[] { _baseUrl });
			}
			else
			{
				Trace.WriteLine(BasePage.RunningTestKeyWord + "ERROR: cannot call " + openMethod + "for class " + pageClassName);
			}

			return (BasePage)navToPage;
		}

		public void OpenMainPageAndLoginAsHost()
		{
			var mainPage = new MainPage(_driver);
			mainPage.OpenUsingUrl(_baseUrl);

			LoginPage loginPage = new LoginPage(_driver);
			loginPage.LoginAsHost(_baseUrl);
		}

        #region BN added
        public bool Login(string usrname, string password)
        {
            try
            {
                Trace.WriteLine(BasePage.TraceLevelComposite + "Login user: " + usrname);
                var mainPage = new MainPage(_driver);
                mainPage.OpenUsingUrl(_baseUrl);

                LoginPage loginPage = new LoginPage(_driver);
                loginPage.LoginUsingDirectUrl(_baseUrl, usrname, password);

                string xpath = "//span[contains(text(), 'Login Failed.')]";
                if (loginPage.ElementPresent(By.XPath(xpath))) { return false; }
                var mp = new MainPage(_driver);
                xpath = "//div[contains(@class, 'ui-dialog-content')]";
                if(mp.ElementPresent(By.XPath(xpath)) && mp.FindElement(By.XPath(xpath)).Displayed)
                {
                    //BN: WRITE CODE LATER
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine("ERROR in Login: " + usrname + "\n\r" + ex.ToString());
                return false;
            }
            return true;
        }

        public void LoginCreateIfNeeded(string usrname, string password, string displayName, string email)
        {
            if(!Login(usrname,password))
            {
                OpenMainPageAndLoginAsHost();
                System.Threading.Thread.Sleep(1000);
                CreateAdminAndLoginAsAdmin(usrname, displayName, email, password);
                LoginPage lp = new LoginPage(_driver);
                lp.LetMeOut();
                if (!Login(usrname, password))
                {
                    Trace.WriteLine("CANNOT create and login as " + usrname);
                }
            }
        }

        public void Logout()
        {
            LoginPage lp = new LoginPage(_driver);
            lp.LetMeOut();
        }
        #endregion BN added

        public void CreateAdminAndLoginAsAdmin(string userName, string displayName, string email, string password)

		public void CreateAdmin(string userName, string displayName, string email, string password)
		{
			Trace.WriteLine(BasePage.TraceLevelComposite + "'Create Admin User'");

			ManageUsersPage manageUsersPage = new ManageUsersPage(_driver);
			manageUsersPage.OpenUsingControlPanel(_baseUrl);
			manageUsersPage.AddNewUser(userName, displayName, email, password);
			manageUsersPage.ManageRoles(userName);
			manageUsersPage.AssignRoleToUser("Administrators");

		}

		public void CreateAdminAndLoginAsAdmin(string userName, string displayName, string email, string password)
		{
			Trace.WriteLine(BasePage.TraceLevelComposite + "'Create Admin User and Login as Admin user'");

			ManageUsersPage manageUsersPage = new ManageUsersPage(_driver);
			manageUsersPage.OpenUsingControlPanel(_baseUrl);

			manageUsersPage.AddNewUser(userName, displayName, email, password);
			manageUsersPage.ManageRoles(userName);
			manageUsersPage.AssignRoleToUser("Administrators");

			MainPage mainPage = new MainPage(_driver);
			mainPage.OpenUsingUrl(_baseUrl);

			LoginPage loginPage = new LoginPage(_driver);
			loginPage.LoginUsingLoginLink(userName, password);
		}

		public string LogContent()
		{
			/*HostSettingsPage hostSettingsPage = new HostSettingsPage(_driver);
			hostSettingsPage.SetDictionary("en");
			hostSettingsPage.OpenUsingUrl(_baseUrl);

			hostSettingsPage.WaitAndClick(By.XPath(HostSettingsPage.LogsTab));
			SlidingSelect.SelectByValue(_driver, By.XPath("//a[contains(@id, '" + HostSettingsPage.LogFilesDropDownArrow + "')]"),
													By.XPath(HostSettingsPage.LogFilesDropDownList),
													HostSettingsPage.OptionOne,
													SlidingSelect.SelectByValueType.Contains);
			hostSettingsPage.WaitForElement(By.XPath(HostSettingsPage.LogContent), 30);

			return hostSettingsPage.FindElement(By.XPath(HostSettingsPage.LogContent)).Text.ToLower();*/

			if (_physicalPath == null)
			{
				var loginPage = new LoginPage(_driver);
				loginPage.LoginAsHost(_baseUrl);

				HostSettingsPage hostSettingsPage = new HostSettingsPage(_driver);
				hostSettingsPage.OpenUsingButtons(_baseUrl);
				_physicalPath = hostSettingsPage.FindElement(By.XPath(HostSettingsPage.PhysicalPath)).Text;
			}

			string[] file = Directory.GetFiles(_physicalPath + @"\Portals\_default\Logs", "*.log.resources", SearchOption.TopDirectoryOnly);

			string fileName = Path.GetFileName(file[0]);

			return new FileInfo(_physicalPath + @"\Portals\_default\Logs\" + fileName).Length.ToString();
		}

		public void VerifyLogs(string logContentBeforeTests)
		{
			Trace.WriteLine(BasePage.TraceLevelComposite + "'Verify Logs on Host Settings page: '");

			string logContentAfterTests = LogContent();

			Trace.WriteLine("log Content Before Tests " + logContentBeforeTests);
			Trace.WriteLine("log Content After Tests " + logContentAfterTests);
			//StringAssert.AreEqualIgnoringCase(logContentBeforeTests, logContentAfterTests, "ERROR in the Log");

			if (logContentBeforeTests != logContentAfterTests)
			{
				var loginPage = new LoginPage(_driver);
				loginPage.LoginAsHost(_baseUrl);

				HostSettingsPage hostSettingsPage = new HostSettingsPage(_driver);
				hostSettingsPage.SetDictionary("en");
				hostSettingsPage.OpenUsingUrl(_baseUrl);

				hostSettingsPage.WaitAndClick(By.XPath(HostSettingsPage.LogsTab));
				SlidingSelect.SelectByValue(_driver, By.XPath("//a[contains(@id, '" + HostSettingsPage.LogFilesDropDownArrow + "')]"),
														By.XPath(HostSettingsPage.LogFilesDropDownList),
														HostSettingsPage.OptionOne,
														SlidingSelect.SelectByValueType.Contains);
				hostSettingsPage.WaitForElement(By.XPath(HostSettingsPage.LogContent), 30);

				Trace.WriteLine(hostSettingsPage.FindElement(By.XPath(HostSettingsPage.LogContent)).Text);
				StringAssert.AreEqualIgnoringCase(logContentBeforeTests, logContentAfterTests, "ERROR in the Log");

			}
		}

		public void DisablePopups(string url)
		{
			Trace.WriteLine(BasePage.TraceLevelComposite + "'Disable Popups: '");

			var adminSiteSettingsPage = new AdminSiteSettingsPage(_driver);
			adminSiteSettingsPage.OpenUsingButtons(url);
			adminSiteSettingsPage.DisablePopups();
		}

		public void CreateChildSiteAndPrepareSettings(string childSiteName, string childSiteTitle)
		{
			Trace.WriteLine(BasePage.TraceLevelComposite + "'Create Child Site And Prepare Settings: '");

			HostSiteManagementPage hostSiteMgmtPage = new HostSiteManagementPage(_driver);
			hostSiteMgmtPage.OpenUsingButtons(_baseUrl);
			hostSiteMgmtPage.AddNewChildSite(_baseUrl, childSiteName, childSiteTitle, "Default Website");

			DisablePopups(_baseUrl + "/" + childSiteName);
		}

		public void CreateParentSiteAndPrepareSettings(string parentSiteName, string parentSiteTitle)
		{
			Trace.WriteLine(BasePage.TraceLevelComposite + "'Create Parent Site And Prepare Settings: '");

			HostSiteManagementPage hostSiteMgmtPage = new HostSiteManagementPage(_driver);
			hostSiteMgmtPage.OpenUsingButtons(_baseUrl);
			hostSiteMgmtPage.AddNewParentSite(parentSiteName, parentSiteTitle, "Default Website");

			LoginPage loginPage = new LoginPage(_driver);
			loginPage.OpenUsingUrl(parentSiteName);
			loginPage.DoLogin("host", "dnnhost");
			
			DisablePopups(parentSiteName);
		}

		public void CreatePageAndSetViewPermission(string pageName, string option, string permissionOption)
		{
			Trace.WriteLine(BasePage.TraceLevelComposite + "'Create A Page And Set View Permissions: '");

			var blankPage = new BlankPage(_driver);
			blankPage.OpenAddNewPageFrameUsingControlPanel(_baseUrl);

			blankPage.AddNewPage(pageName);

			//blankPage.OpenUsingUrl(_baseUrl, pageName);
			blankPage.SetPageViewPermissions(option, permissionOption);

			//blankPage.CloseEditMode();
		}

		public void CreateNewPage(string pageName)
		{
			var blankPage = new BlankPage(_driver);
			blankPage.OpenAddNewPageFrameUsingControlPanel(_baseUrl);
			blankPage.AddNewPage(pageName);
		}

		public void AddModule(string pageName, Dictionary<string, Modules.ModuleIDs> modulesDescription, string moduleName, string pane)
		{
			Trace.WriteLine(BasePage.TraceLevelComposite + "Add a new Module to Page: '");

			var blankPage = new BlankPage(_driver);
			blankPage.OpenUsingUrl(_baseUrl, pageName);
			
			var module = new Modules(_driver);
			string moduleNameOnPage = modulesDescription[moduleName].IdWhenOnPage;
			string moduleNameOnBanner = modulesDescription[moduleName].IdWhenOnBanner;

			module.OpenModulePanelUsingControlPanel();
			module.AddNewModuleUsingMenu(moduleNameOnBanner, moduleNameOnPage, pane);

			blankPage.CloseEditMode();
		}

		public void RemoveUsedPage(string pageName)
		{
			var page = new BlankPage(_driver);
			page.OpenUsingUrl(_baseUrl, pageName);
			page.DeletePage(pageName);

			var adminRecycleBinPage = new AdminRecycleBinPage(_driver);
			adminRecycleBinPage.OpenUsingButtons(_baseUrl);
			adminRecycleBinPage.EmptyRecycleBin();
		}

		public void CreateFolder(string folderType, string folderName)
		{
			Trace.WriteLine(BasePage.TraceLevelComposite + "'Create Folder:'");

			var adminFileManagementPage = new AdminFileManagementPage(_driver);
			adminFileManagementPage.OpenUsingButtons(_baseUrl);

			adminFileManagementPage.CreateFolder(folderType, folderName);
		}

		public void UploadFileToFolder(string rootFolderName, string folderName, string fileNameToUpload)
		{
			Trace.WriteLine(BasePage.TraceLevelComposite + "'Upload File to Folder:'");

			var adminFileManagementPage = new AdminFileManagementPage(_driver);
			adminFileManagementPage.OpenUsingButtons(_baseUrl);

			adminFileManagementPage.SelectFolderFromTreeView(rootFolderName, folderName);

			//adminFileManagementPage.SetItemsPerPage("All");

			adminFileManagementPage.UploadFileToFolder(folderName, fileNameToUpload);
		}

		public void DeleteFolder(string folderName)
		{
			Trace.WriteLine(BasePage.TraceLevelComposite + "'Delete Folder from Treeview:'" + folderName);

			var adminFileManagementPage = new AdminFileManagementPage(_driver);
			adminFileManagementPage.OpenUsingButtons(_baseUrl);

			adminFileManagementPage.DeleteFolderFromTreeView(folderName);
		}
	}
}
