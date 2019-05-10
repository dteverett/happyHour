﻿using Contacts.ProximityScanner;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Drawing;
using System.Net;
using System.Net.Cache;
using System.Windows.Forms;
using EventAttendanceApp;

namespace EventAttendanceApp
{
	public partial class EventAttendanceForm : Form
    {
	    public campusEntities _ce = new campusEntities();
        private List<EmployeeModel> _employees = new List<EmployeeModel>();

        public EventAttendanceForm()
        {
            InitializeComponent();
            LoadEmployeeList();
        }

        private void LoadEmployeeList()
        {
            _employees = SqliteDataAccess.LoadEmployees();
        }

        private void EventAttendanceForm_Load(object sender, EventArgs e)
        {
            IProximityConnector ipc = new ProximityConnector();
            ipc.Connect();
            ipc.OnScan += ipc_OnScan;
            this.WindowState = FormWindowState.Maximized;
            FormBorderStyle = FormBorderStyle.None;
            lblWelcome.Visible = false;
            pbPicture.Visible = false;
	        acceptBtn.Visible = false;
        }

        void ipc_OnScan(object sender, ProximityConnectorEventArgs e)
        {
            if (LblScanBadge.Visible == false)
                return;

            if (this.InvokeRequired)
            {
                this.Invoke((Action)(() => ipc_OnScan(sender, e)));
                return;
            }
            var t = new RestClient();
            var badgeNumber = e.BadgeNumber.ToString();
			//var jsonStr = t.Get(ConfigurationManager.AppSettings["CardUrl"] + badgeNumber);
			var jsonStr = t.Get(ConfigurationManager.AppSettings["CardUrl"] + "12003164227");
			var user = JsonConvert.DeserializeObject<CardUser>(jsonStr);

            if (user != null)
            {
                //Hide Scan Badge
                LblScanBadge.Visible = false;

                //Get Image
                //string filename = @ConfigurationManager.AppSettings["PictureLocation"] + user.PicturePath;
	            string filename = @"\\ctac\service\Pictures\P1960-Darian Everett.jpg";
				pbPicture.Image = Bitmap.FromFile(filename);
                pbPicture.Visible = true;

                //Show Name
                lblWelcome.Text = "Welcome " + user.FirstName + " " + user.LastName;
                lblWelcome.Left = (ClientSize.Width - lblWelcome.Width) / 2;
                lblWelcome.Visible = true;

	            acceptBtn.Visible = true;

                // db work
                var emp = new EmployeeModel
                {
                    EmployeeId = user.CampusId, EmployeeName = user.FirstName + " " + user.LastName
                };

                emp.DrinksToday++;
                emp.LastLogin = DateTime.Today;

                SqliteDataAccess.SaveEmployee(emp);
            }
        }

        private void EventAttendanceForm_Resize(object sender, EventArgs e)
        {
            pbLogo.Left = (this.ClientSize.Width - pbLogo.Width) / 2;
            LblScanBadge.Left = pbLogo.Left + ((pbLogo.Width - LblScanBadge.Width) / 2);
            LblScanBadge.Top = this.ClientSize.Height / 2;
            LblScanBadge.Visible = true;
            lblWelcome.Left = (this.ClientSize.Width - lblWelcome.Width) / 2;
            pbPicture.Left = (this.ClientSize.Width - pbPicture.Width) / 2;
			acceptBtn.Left = ((this.ClientSize.Width - pbPicture.Width) / 2) -15;
		}

		private void button1_Click(object sender, EventArgs e)
		{
			pbPicture.Visible = false;
			acceptBtn.Visible = false;
			lblWelcome.Visible = false;
			LblScanBadge.Visible = true;
        }
	}
	public class RestClient
    {
        public string Get(string uri)
        {
	        string result;

            using (var webClient = new WebClient { UseDefaultCredentials = true, CachePolicy = new RequestCachePolicy(RequestCacheLevel.BypassCache) })
            {
                webClient.Encoding = System.Text.Encoding.UTF8;
                try
                {
                    result = webClient.DownloadString(uri);
                }
                catch
                {
                    return "";
                }
            }

            return result;
        }
    }
    public class CardUser
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string PicturePath { get; set; }
        public int? FitnessId { get; set; }
        public int CampusId { get; set; }
        public bool IsAdmin { get; set; }
        public string AccountName { get; set; }
        public string Sid { get; set; }
    }
}