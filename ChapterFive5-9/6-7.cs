// 7 - 1

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

namespace ChapterSix6_1
{
    public partial class frmPhoneDB : Form
    {
        SqlConnection authorsConnection;
        SqlCommand authorsCommand;
        SqlDataAdapter authorsAdaptor;
        DataTable authorsTable;
        CurrencyManager authorsManager;

        // 6-2 
        string myState;
        int myBookmark; 

        public frmPhoneDB()
        {
            InitializeComponent();
        }

        private void frmPhoneDB_Load(object sender, EventArgs e)
        {

            SetState("View"); 

            authorsConnection = new SqlConnection(@"Data Source=.\SQLEXPRESS; 
                                                AttachDbFilename=c:\Users\mholmes022726\source\repos\ChapterSix6-1\ChapterSix6-1\bin\Debug\netcoreapp3.1\SQLPhoneDB.mdf; 
                                                Integrated Security=True; Connect Timeout=30; User Instance=True");
            authorsConnection.Open();

            // establish command object   
            authorsCommand = new SqlCommand("Select * from phoneTable ORDER BY ContactName", authorsConnection);

            // estbalish data adapter/data table 
            authorsAdaptor = new SqlDataAdapter();
            authorsAdaptor.SelectCommand = authorsCommand;
            authorsTable = new DataTable();
            authorsAdaptor.Fill(authorsTable);

            // bind controls to the data table
            txtAuthorId.DataBindings.Add("Text", authorsTable, "ContactID");
            txtAuthorName.DataBindings.Add("Text", authorsTable, "ContactName");
            txtYearBorn.DataBindings.Add("Text", authorsTable, "ContactName");

            // establish conncurency manager
            authorsManager = (CurrencyManager)this.BindingContext[authorsTable];

            // 6-4
            foreach(DataRow phoneRow in authorsTable.Rows) 
            {
                phoneRow["ContactNumber"] = "(206)" + phoneRow["ContactNumber"].ToString(); 
            }
        }

        private void frmPhoneDB_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (myState.Equals("Edit") || myState.Equals("Add"))
            {
                MessageBox.Show("You must finish the current edit before stopping the application",
                    "", MessageBoxButtons.OK, MessageBoxIcon.Information);
                e.Cancel = true;
            }
            else 
            {
                try 
                {
                    // save the updated phone table
                    SqlCommandBuilder phoneAdapterCommands = new SqlCommandBuilder(authorsAdaptor);
                    authorsAdaptor.Update(authorsTable);

                }
                catch(Exception ex) 
                {
                    MessageBox.Show("Error saving database to file:\r\n" + ex.Message, "Save Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            
            authorsConnection.Close();

            authorsConnection.Dispose();
            authorsCommand.Dispose();
            authorsAdaptor.Dispose();
            authorsTable.Dispose(); 
        }

        private void btnFirst_Click(object sender, EventArgs e)
        {
            authorsManager.Position = 0; 
        }

        private void btnPrevious_Click(object sender, EventArgs e)
        {
            authorsManager.Position--;
        }

        private void btnNext_Click(object sender, EventArgs e)
        {
            authorsManager.Position++;
        }

        private void btnLast_Click(object sender, EventArgs e)
        {
            authorsManager.Position = authorsManager.Count - 1; 
        }
        private void SetState(string appState) 
        {
            myState = appState;
            switch (appState) 
            {
                case "View":
                    btnFirst.Enabled = true;
                    btnPrevious.Enabled = true;
                    btnNext.Enabled = true;
                    btnLast.Enabled = true;
                    btnEdit.Enabled = true;
                    btnSave.Enabled = false;
                    btnCancel.Enabled = false;
                    btnAdd.Enabled = true; // 6-2
                    txtAuthorId.BackColor = Color.White;
                    txtAuthorId.ForeColor = Color.Black;
                    txtAuthorName.ReadOnly = true;
                    txtYearBorn.ReadOnly = true;
                    break;
                default:
                    btnFirst.Enabled = false;
                    btnPrevious.Enabled = false;
                    btnNext.Enabled = false;
                    btnLast.Enabled = false;
                    btnEdit.Enabled = false;
                    btnSave.Enabled = true;
                    btnCancel.Enabled = true;
                    btnAdd.Enabled = false; 
                    txtAuthorId.BackColor = Color.Red;
                    txtAuthorId.ForeColor = Color.White;
                    txtAuthorName.ReadOnly = false;
                    txtYearBorn.ReadOnly = false;
                    break;
            }
        }

        private void btnEdit_Click(object sender, EventArgs e)
        {
            SetState("Edit");
        }

        private bool ValidateData()
        {
            string message = "";
            int inputYear, currentYear;
            bool allOK = true;

            // Check for name
            if (txtAuthorName.Text.Trim().Equals(""))
            {
                message = "You must enter an Author Name." + "\r\n";
                txtAuthorName.Focus();
                allOK = false;
            }

            //Check length and range on Year Born
            if (!txtYearBorn.Text.Trim().Equals(""))
            {
                inputYear = Convert.ToInt32(txtYearBorn.Text);
                currentYear = DateTime.Now.Year;
                if (inputYear > currentYear || inputYear < currentYear - 150)
                {
                    message += "Year born must be between " + (currentYear - 150).ToString() + " and " + currentYear.ToString();
                    txtYearBorn.Focus();
                    allOK = false;
                }
            }

            if (!allOK)
            {
                MessageBox.Show(message, "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            return (allOK);
        }
        private void btnSave_Click(object sender, EventArgs e)
        {
            if (!ValidateData()) 
            {
                return; 
            }
            string savedName = txtAuthorName.Text;
            int savedRow;
            try
            {
                authorsManager.EndCurrentEdit();
                authorsTable.DefaultView.Sort = "Author";
                savedRow = authorsTable.DefaultView.Find(savedName);
                authorsManager.Position = savedRow;
                MessageBox.Show("Record saved.", "Save", MessageBoxButtons.OK, MessageBoxIcon.Information);
                SetState("View");
            }
            catch (Exception ex) 
            {
                MessageBox.Show("Error saving record.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            authorsManager.EndCurrentEdit();
            if (myState.Equals("Add")) // 6-2
            {
                authorsManager.Position = myBookmark;
            }
            SetState("View");
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            try
            {
                myBookmark = authorsManager.Position;
                SetState("Add");
                authorsManager.AddNew();
            }
            catch(Exception ex) 
            {
                MessageBox.Show("Error adding record", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        
        private void btnDelete_Click(object sender, EventArgs e)
        {
            DialogResult response;
            response = MessageBox.Show("Are you sure you want to delete this record?", "Delete Record", MessageBoxButtons.YesNo,
                MessageBoxIcon.Question, MessageBoxDefaultButton.Button2); 
            if (response == DialogResult.No) 
            {
                return;
            }
            try 
            {
                authorsManager.RemoveAt(authorsManager.Position); 
            }
            catch (Exception ex) 
            {
                MessageBox.Show("Error deleting record.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error); 
            }
            SetState("View"); 
        }

        private void btnDone_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btnFind_Click(object sender, EventArgs e)
        {
            if (txtFind.Text.Equals("")) 
            {
                return;
            }
            int savedRow = authorsManager.Position;
            DataRow[] foundRows;
            authorsTable.DefaultView.Sort = "Author";
            foundRows = authorsTable.Select("Author LIKE '" + txtFind + "*'");
            if (foundRows.Length == 0) 
            {
                authorsManager.Position = savedRow;
            }
            else 
            {
                authorsManager.Position = authorsTable.DefaultView.Find(foundRows[0]["Author"]);
            }
        }
    }
}
