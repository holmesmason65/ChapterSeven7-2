/* Mason Holmes
 * 10/4/21
 * This program allows you to print titles from a books database
 */

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data.SqlClient;
using System.IO;

namespace ChapterFive5_9
{
    public partial class frmPublishers : Form
    {
        SqlConnection booksConnection;
        SqlCommand publisherCommand;
        SqlDataAdapter publisherAdapter;
        DataTable publisherTable;
        CurrencyManager publisherManager;

        string myState;
        int myBookmark;

        public frmPublishers()
        {
            InitializeComponent();
        }

        private void frmPublishers_Load(object sender, EventArgs e)
        {
            try
            {
                hlpPublishers.HelpNamespace = Application.StartupPath + "\\Publishers.chm"; 
                string path = Path.GetFullPath("SQLBooksDB.mdf");

                booksConnection = new SqlConnection($@"Data Source=.\SQLEXPRESS; AttachDbFilename={path};
                                                    Integrated Security=True; Connect Timeout=30; User Instance=True");
                booksConnection.Open();

                // establish command object 
                publisherCommand = new SqlCommand("SELECT * FROM Publishers ORDER BY Name", booksConnection);

                // establish data adpater 
                publisherAdapter = new SqlDataAdapter();
                publisherAdapter.SelectCommand = publisherCommand;
                publisherTable = new DataTable();
                publisherAdapter.Fill(publisherTable);

                // bind controls to data table 
                txtPubID.DataBindings.Add("Text", publisherTable, "PubID");
                txtPubName.DataBindings.Add("Text", publisherTable, "Name");
                txtCompanyName.DataBindings.Add("Text", publisherTable, "Company_Name");
                txtPubCity.DataBindings.Add("Text", publisherTable, "City");
                txtPubAddress.DataBindings.Add("Text", publisherTable, "Address");
                txtPubState.DataBindings.Add("Text", publisherTable, "State");
                txtPubTelephone.DataBindings.Add("Text", publisherTable, "Telephone");
                txtPubFax.DataBindings.Add("Text", publisherTable, "FAX");
                txtPubComments.DataBindings.Add("Text", publisherTable, "Comments");

                // establish currency manager
                publisherManager = (CurrencyManager)this.BindingContext[publisherTable];
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error establishing Publisher Table.", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            this.Show();
            SetState("View");
        }

        private void frmPublishers_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (myState.Equals("Edit") || myState.Equals("Add"))
            {
                MessageBox.Show("You must finish the current edit before exiting.", "", MessageBoxButtons.OK, MessageBoxIcon.Information);
                e.Cancel = true;
            }
            else 
            {
                try 
                {
                    // save changes to the database
                    SqlCommandBuilder publishersCommandBuilder = new SqlCommandBuilder(publisherAdapter);
                    publisherAdapter.Update(publisherTable);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error saving database to file: \r\n" + ex.Message, "Save Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            
                // close the connection 
                booksConnection.Close();
                // dispose of the objects 
                booksConnection.Dispose();
                publisherCommand.Dispose();
                publisherAdapter.Dispose();
                publisherTable.Dispose();
            }
        }

        private void btnPrevious_Click(object sender, EventArgs e)
        {
            if (publisherManager.Position == 0)
            {
                Console.Beep();
            }
            publisherManager.Position--;
            SetText();
        }

        private void btnNext_Click(object sender, EventArgs e)
        {
            if (publisherManager.Position == publisherManager.Count - 1)
            {
                Console.Beep();
            }
            publisherManager.Position++;
            SetText();
        }

        private void SetState(string appState)
        {
            myState = appState;
            switch (appState)
            {
                case "View":
                    txtPubID.BackColor = Color.White;
                    txtPubID.ForeColor = Color.Black;
                    txtPubName.ReadOnly = true;
                    txtCompanyName.ReadOnly = true;
                    btnPrevious.Enabled = true;
                    btnAddNew.Enabled = true;
                    btnSave.Enabled = false;
                    btnCancel.Enabled = false;
                    btnExit.Enabled = true; // supposed to be exit
                    btnDelete.Enabled = true;
                    btnDone.Enabled = true;
                    txtPubName.Focus();
                    break;
                default: // Add or Edit if not in view 
                    txtPubID.BackColor = Color.Red;
                    txtPubID.ForeColor = Color.White;
                    txtPubName.ReadOnly = false;
                    txtCompanyName.ReadOnly = false;
                    btnPrevious.Enabled = false;
                    btnAddNew.Enabled = false;
                    btnSave.Enabled = true;
                    btnCancel.Enabled = true;
                    btnExit.Enabled = false; // should be edit 
                    btnDelete.Enabled = false;
                    btnDone.Enabled = false;
                    txtPubName.Focus();
                    break;
            }
        }

        private void txtPubName_KeyPress(object sender, KeyPressEventArgs e)
        {
            // accidental 
        }

        // added manually
        private void txtInput_KeyPress(object sender, KeyPressEventArgs e) // not wired up? not letting type enter
        {
            TextBox whichBox = (TextBox)sender;
            if((int) e.KeyChar == 13) 
            {
                switch (whichBox.Name) 
                {
                    case "txtPubName":
                        txtCompanyName.Focus();
                        break;
                    case "txtCompanyName":
                        txtPubAddress.Focus();
                        break;
                    case "txtPubAdress":
                        txtPubCity.Focus();
                        break;
                    case "txtPubCity":
                        txtPubState.Focus();
                        break;
                    case "txtPubState":
                        txtPubZip.Focus();
                        break;
                    case "txtPubZip":
                        txtPubTelephone.Focus();
                        break;
                    case "txtPubTelephone":
                        txtPubFax.Focus();
                        break;
                    case "txtPubFax":
                        txtPubComments.Focus();
                        break;
                    case "txtPubComments":
                        txtPubName.Focus();
                        break;
                }
            }
        }

        private bool ValidateData() 
        {
            string message = "";
            bool allOK = true;
            // check for name
            if (txtPubName.Text.Trim().Equals("")) 
            {
                message = "You must enter a Publisher Name" + "\r\n";
                txtPubName.Focus();
                allOK = false; 
            }
            if (!allOK) 
            {
                MessageBox.Show(message, "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            return allOK; 
        }

        private void btnHelp_Click(object sender, EventArgs e)
        {
            Help.ShowHelp(this, hlpPublishers.HelpNamespace); 
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            if (!ValidateData()) 
            {
                return;
            }
            string savedName = txtPubName.Text;
            int savedRow;
            try 
            {
                publisherManager.EndCurrentEdit();
                publisherTable.DefaultView.Sort = "Name";
                savedRow = publisherTable.DefaultView.Find(savedName);
                publisherManager.Position = savedRow;
                MessageBox.Show("Record Saved", "Save", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch 
            {
                MessageBox.Show("Error saving record.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            SetText();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            publisherManager.CancelCurrentEdit();
            if (myState.Equals("Add")) 
            {
                publisherManager.Position = myBookmark;
            }
            SetState("View");
        }

        private void btnAddNew_Click(object sender, EventArgs e)
        {
            try 
            {
                myBookmark = publisherManager.Position;
                publisherManager.AddNew();
                SetState("Add");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error adding record.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            SetText();
        }

        private void btnExit_Click(object sender, EventArgs e)
        {
            SetState("Edit");
            SetText();
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            DialogResult response;
            response = MessageBox.Show("Are you sure you want to delete this record?", "Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2);
            if(response == DialogResult.No) 
            {
                return;
            }
            try 
            {
                publisherManager.RemoveAt(publisherManager.Position);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error deleting record", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            SetText();
        }

        private void btnDone_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            publisherManager.Position = 0;
            SetText();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            publisherManager.Position = publisherManager.Count - 1;
            SetText();
        }

        private void btnFind_Click(object sender, EventArgs e)
        {
            if (txtFind.Text.Equals("")) 
            {
                return;
            }
            int savedRow = publisherManager.Position;
            DataRow[] foundRows;
            publisherTable.DefaultView.Sort = "Name";
            foundRows = publisherTable.Select("Name LIKE '" + txtFind.Text + "*'");
            if(foundRows.Length == 0) 
            {
                publisherManager.Position = savedRow;
            }
            else 
            {
                publisherManager.Position = publisherTable.DefaultView.Find(foundRows[0]["Name"]);
            }
            SetText();

        }
        private void SetText() 
        {
            this.Text = "Publishers - Records " + (publisherManager.Position + 1).ToString() + " of " + publisherManager.Count.ToString() + " records"; 
        }
    }
}
