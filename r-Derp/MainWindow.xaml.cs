using System.Collections.Generic;
using System.Windows;
using System.DirectoryServices;
using System.Threading.Tasks;
using System;
using System.IO;
using System.Reflection;
using Newtonsoft.Json.Linq;
using System.Diagnostics;
using System.Collections.ObjectModel;

namespace r_Derp
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            LoadData();
            CheckDomain();
        }

        static void CheckDomain()
        {
            try
            {
                GetComputers();
                GetOrganizationalUnits();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                Environment.Exit(0);
            }
        }

        //https://social.technet.microsoft.com/wiki/contents/articles/5392.active-directory-ldap-syntax-filters.aspx


        static Dictionary <string, string> computerNames = new();

        static List <string> organizationalUnits = new();

        static ObservableCollection <string> userNames = new();

        static Dictionary<string, string> userPrincipalName = new();

        public static Dictionary <string, string> ComputerNames
        {
            get 
            { 
                return computerNames; 
            }
            set 
            { 
                computerNames = value; 
            }
        }

        public static List <string> OrganizationalUnits
        {
            get 
            {
                return organizationalUnits; 
            }
            set 
            { 
                organizationalUnits = value; 
            }
        }

        public static ObservableCollection <string> UserNames
        {
            get
            {
                return userNames;
            }
            set
            {
                userNames = value;
            }
        }

        public static Dictionary<string, string> UserPrincipalName
        {
            get
            {
                return userPrincipalName;
            }
            set
            {
                userPrincipalName = value;
            }
        }

        static string? domain;
        static string? domainType;
        static string? computerNameFilter;

        static void LoadData()
        {
            JObject config = JObject.Parse(File.ReadAllText(@AppDomain.CurrentDomain.BaseDirectory + "LDAP.json"));

            domain = config?["domain"]?.Value<string>();
            domainType = config?["domainType"]?.Value<string>();
            computerNameFilter = config?["computerNameFilter"]?.Value<string>();
        }

        static Dictionary<string, string> GetComputers()
        {
            computerNames.Clear();
            using (DirectoryEntry entry = new("LDAP://" + domain))
            {
                using DirectorySearcher mySearcher = new(entry);
                mySearcher.Filter = ("(&(objectclass=computer)(cn=" + computerNameFilter + "))");

                // No size limit, reads all objects
                mySearcher.SizeLimit = 0;

                // Read data in pages of 250 objects. Make sure this value is below the limit configured in your AD domain (if there is a limit)
                mySearcher.PageSize = 250;

                // Let searcher know which properties are going to be used, and only load those
                mySearcher.PropertiesToLoad.Add("name");
                mySearcher.PropertiesToLoad.Add("description");

                foreach (SearchResult resEnt in mySearcher.FindAll())
                {
                    // Note: Properties can contain multiple values.
                    if (resEnt.Properties["name"].Count > 0 && resEnt.Properties["description"].Count > 0)
                    {
                        computerNames.Add((string)resEnt.Properties["name"][0], (string)resEnt.Properties["name"][0] + " (" + (string)resEnt.Properties["description"][0] + ")");
                    }
                }
            }
            return computerNames;
        }

        static List <string> GetOrganizationalUnits()
        {
            organizationalUnits.Clear();
            using (DirectoryEntry entry = new("LDAP://" + domain))
            {
                using DirectorySearcher mySearcher = new(entry);
                mySearcher.Filter = ("(&(objectCategory=organizationalUnit)(!(name=Domain Controllers)))");

                // No size limit, reads all objects
                mySearcher.SizeLimit = 0;

                // Read data in pages of 250 objects. Make sure this value is below the limit configured in your AD domain (if there is a limit)
                mySearcher.PageSize = 250;

                // Let searcher know which properties are going to be used, and only load those
                mySearcher.PropertiesToLoad.Add("name");

                foreach (SearchResult resEnt in mySearcher.FindAll())
                {
                    // Note: Properties can contain multiple values.
                    if (resEnt.Properties["name"].Count > 0)
                    {
                        organizationalUnits.Add((string)resEnt.Properties["name"][0]);
                    }
                }
            }
            return organizationalUnits;
        }

        static ObservableCollection <string> GetUserNames()
        {
            userNames.Clear();
            using (DirectoryEntry entry = new("LDAP://OU=" + SelectedOrganizationalUnit + ", DC=" + domain + ",DC=" + domainType))
            {
                using DirectorySearcher mySearcher = new(entry);
                mySearcher.Filter = ("(objectclass=user)");

                // No size limit, reads all objects
                mySearcher.SizeLimit = 0;

                // Read data in pages of 250 objects. Make sure this value is below the limit configured in your AD domain (if there is a limit)
                mySearcher.PageSize = 250;

                // Let searcher know which properties are going to be used, and only load those
                mySearcher.PropertiesToLoad.Add("name");

                foreach (SearchResult resEnt in mySearcher.FindAll())
                {
                    // Note: Properties can contain multiple values.
                    if (resEnt.Properties["name"].Count > 0)
                    {
                        userNames.Add((string)resEnt.Properties["name"][0]);
                    }
                }
            }
            return userNames;
        }

        static Dictionary<string, string> GetUserPrincipalName()
        {
            userPrincipalName.Clear();
            using (DirectoryEntry entry = new("LDAP://OU=" + SelectedOrganizationalUnit + ", DC=" + domain + ",DC=" + domainType))
            {
                using DirectorySearcher mySearcher = new(entry);
                mySearcher.Filter = ("(objectclass=user)");

                // No size limit, reads all objects
                mySearcher.SizeLimit = 0;

                // Read data in pages of 250 objects. Make sure this value is below the limit configured in your AD domain (if there is a limit)
                mySearcher.PageSize = 250;

                // Let searcher know which properties are going to be used, and only load those
                mySearcher.PropertiesToLoad.Add("name");
                mySearcher.PropertiesToLoad.Add("userPrincipalName");

                foreach (SearchResult resEnt in mySearcher.FindAll())
                {
                    // Note: Properties can contain multiple values.
                    if (resEnt.Properties["name"].Count > 0)
                    {
                        userPrincipalName.Add((string)resEnt.Properties["name"][0], (string)resEnt.Properties["userPrincipalName"][0]);
                    }
                }
            }
            return userPrincipalName;
        }

        static string? SelectedComputerName;
        static string? SelectedOrganizationalUnit;
        static string? SelectedPrincipalName;

        void SelectComputerName_SelectionChanged(object sender, System.EventArgs e)
        {
            if (SelectComputerName.SelectedValue != null)
            {
                SelectedComputerName = SelectComputerName.SelectedValue.ToString();
            }
        }

        void SelectOrganizationalUnit_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (SelectOrganizationalUnit.SelectedValue != null)
            {
                SelectedOrganizationalUnit = SelectOrganizationalUnit.SelectedValue.ToString();
                SelectUserName.IsEnabled = true;
                GetUserNames();
                GetUserPrincipalName();
            }
        }

        void SelectUserName_SelectionChanged(object sender, System.EventArgs e)
        {
            if (SelectUserName.SelectedValue != null)
            {
                string? SelectedUserName = SelectUserName.SelectedValue.ToString();
                if (SelectUserName.SelectedValue != null && SelectedUserName != null)
                {
                    userPrincipalName.TryGetValue(SelectedUserName, out SelectedPrincipalName);
                }
            }
        }

        static readonly string directory = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\" + Assembly.GetExecutingAssembly().GetName().Name;
        static readonly string path = directory + "\\temp.rdp";

        async void Connect_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedComputerName != null && SelectedOrganizationalUnit != null && SelectedPrincipalName != null)
            {
                await GenerateRDP();
                Process.Start(new ProcessStartInfo(path) { UseShellExecute = true });
                this.Close();
            }
        }

        static async Task GenerateRDP()
        {
            string[] rows =
            {
                "full address:s:"+SelectedComputerName,
                "username:s:"+SelectedPrincipalName,
                "prompt for credentials:i:1"
            };

            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            };
            await File.WriteAllLinesAsync(path, rows);
        }
    }
}
