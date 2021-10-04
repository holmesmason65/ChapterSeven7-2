﻿using System;
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
using System.Drawing.Printing;

namespace ChapterFive5_9
{
    public partial class frmTitles : Form
    {
        SqlConnection booksConnection;
        SqlCommand titlesCommand;
        SqlDataAdapter titlesAdapter; 
        DataTable titlesTable;
        CurrencyManager titlesManager;

        SqlCommand publishersCommand;
        SqlDataAdapter publishersAdapter;
        DataTable publishersTable; 

        string myState;
        int myBookmark;

        ComboBox[] authorsCombo = new ComboBox[4];
        SqlCommand authorsCommand;
        SqlDataAdapter authorsAdapter;
        DataTable[] authorsTable = new DataTable[4];

        SqlCommand ISBNAuthorsCommand;
        SqlDataAdapter ISBNAuthorsAdapter;
        DataTable ISBNAuthorsTable;

        int pageNumber;
        const int titlesPerPage = 45;

        public frmTitles()
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
                titlesCommand = new SqlCommand("SELECT * FROM Titles ORDER BY Title", booksConnection);

                // establish data adpater 
                titlesAdapter = new SqlDataAdapter();
                titlesAdapter.SelectCommand = titlesCommand;
                titlesTable = new DataTable();
                titlesAdapter.Fill(titlesTable);

                // bind controls to data table 
                txtTitle.DataBindings.Add("Text", titlesTable, "Title");
                txtYear.DataBindings.Add("Text", titlesTable, "Year_Published");
                txtISBN.DataBindings.Add("Text", titlesTable, "ISBN");
                txtNotes.DataBindings.Add("Text", titlesTable, "Notes");
                txtDescription.DataBindings.Add("Text", titlesTable, "Description");
                txtSubject.DataBindings.Add("Text", titlesTable, "Subject");
                txtComments.DataBindings.Add("Text", titlesTable, "Comments");
                //txtPubTelephone.DataBindings.Add("Text", titlesTable, "Telephone");
                //txtPubFax.DataBindings.Add("Text", titlesTable, "FAX");
                //txtPubComments.DataBindings.Add("Text", titlesTable, "Comments");

                // establish currency manager
                titlesManager = (CurrencyManager)this.BindingContext[titlesTable];

                // establish publishers table/combo box to pick publisher
                publishersCommand = new SqlCommand("Select * from Publishers ORDER BY Name", booksConnection);
                publishersAdapter = new SqlDataAdapter();
                publishersAdapter.SelectCommand = publishersCommand;
                publishersTable = new DataTable();
                publishersAdapter.Fill(publishersTable);
                cboPublisher.DataSource = publishersTable;
                cboPublisher.DisplayMember = "Name";
                cboPublisher.ValueMember = "PubID";
                cboPublisher.DataBindings.Add("SelectedValue", titlesTable, "PubID");

                // set up combo box array
                authorsCombo[0] = cboAuthor1;
                authorsCombo[1] = cboAuthor2;
                authorsCombo[2] = cboAuthor3;
                authorsCombo[3] = cboAuthor4;

                authorsCommand = new SqlCommand("Select * from Authors ORDER BY Author", booksConnection);
                authorsAdapter = new SqlDataAdapter();
                authorsAdapter.SelectCommand = authorsCommand;
                for(int i = 0; i < 4; i++) 
                {
                    // establish author table/combo boxes to pick author
                    authorsTable[i] = new DataTable();
                    authorsAdapter.Fill(authorsTable[i]);
                    authorsCombo[i].DataSource = authorsTable[i];
                    authorsCombo[i].DisplayMember = "Author";
                    authorsCombo[i].ValueMember = "Au_ID";
                    // set all to no selection 
                    authorsCombo[i].SelectedIndex = -1;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error establishing Publisher Table.", MessageBoxButtons.OK, MessageBoxIcon.Error);
                //return;
            }
            this.Show();
            SetState("View");
            SetText();
            GetAuthors();
        }

        private void frmTitles_FormClosing(object sender, FormClosingEventArgs e)
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
                    SqlCommandBuilder publishersCommandBuilder = new SqlCommandBuilder(titlesAdapter);
                    titlesAdapter.Update(titlesTable);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error saving database to file: \r\n" + ex.Message, "Save Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            
                // close the connection 
                booksConnection.Close();
                // dispose of the objects 
                booksConnection.Dispose();
                titlesCommand.Dispose();
                titlesAdapter.Dispose();
                titlesTable.Dispose();

                publishersCommand.Dispose();
                publishersAdapter.Dispose();
                publishersTable.Dispose();

                authorsCommand.Dispose();
                authorsAdapter.Dispose();
                authorsTable[0].Dispose();
                authorsTable[1].Dispose();
                authorsTable[2].Dispose();
                authorsTable[3].Dispose();

                ISBNAuthorsAdapter.Dispose();
                ISBNAuthorsCommand.Dispose();
                ISBNAuthorsTable.Dispose(); 
            }
        }

        private void btnPrevious_Click(object sender, EventArgs e)
        {
            if (titlesManager.Position == 0)
            {
                Console.Beep();
            }
            titlesManager.Position--;
            SetText();
            GetAuthors();
        }

        private void btnNext_Click(object sender, EventArgs e)
        {
            if (titlesManager.Position == titlesManager.Count - 1)
            {
                Console.Beep();
            }
            titlesManager.Position++;
            SetText();
            GetAuthors();
        }

        private void SetState(string appState)
        {
            myState = appState;
            switch (appState)
            {
                case "View":
                    txtTitle.ReadOnly = true;
                    grpFindTitle.Enabled = true;

                    cboAuthor1.Enabled = false;
                    cboAuthor2.Enabled = false;
                    cboAuthor3.Enabled = false;
                    cboAuthor4.Enabled = false;

                    txtTitle.BackColor = Color.White;
                    txtTitle.ForeColor = Color.Black;
                    txtYear.ReadOnly = true;

                    txtDescription.ReadOnly = true;
                    txtNotes.ReadOnly = true;
                    txtSubject.ReadOnly = true;
                    txtComments.ReadOnly = true;

                    txtISBN.ReadOnly = true;
                    txtISBN.BackColor = Color.White;
                    txtISBN.ForeColor = Color.Black;

                    btnAddNew.Enabled = true;
                    btnPrevious.Enabled = true;
                    btnSave.Enabled = false;
                    btnCancel.Enabled = false;
                    btnExit.Enabled = true; // supposed to be exit
                    btnDelete.Enabled = true;
                    btnDone.Enabled = true;
                    txtYear.Focus();
                    break;
                default: // Add or Edit if not in view 

                    txtTitle.ReadOnly = false;
                    grpFindTitle.Enabled = false;
                    
                    txtTitle.BackColor = Color.Red;
                    txtTitle.ForeColor = Color.White;
                    txtYear.ReadOnly = false;
                    txtISBN.ReadOnly = false;

                    cboAuthor1.Enabled = true;
                    cboAuthor2.Enabled = true;
                    cboAuthor3.Enabled = true;
                    cboAuthor4.Enabled = true;

                    if (myState.Equals("Edit")) 
                    {
                        txtISBN.BackColor = Color.Red;
                        txtISBN.ForeColor = Color.White;
                        txtISBN.ReadOnly = true;
                        txtISBN.TabStop = false;
                    }
                    else 
                    {
                        txtISBN.TabStop = true;
                    }
                    txtDescription.ReadOnly = false;
                    txtNotes.ReadOnly = false;
                    txtSubject.ReadOnly = false;
                    txtComments.ReadOnly = true;

                    btnPrevious.Enabled = false;
                    btnAddNew.Enabled = false;
                    btnSave.Enabled = true;
                    btnCancel.Enabled = true;
                    btnExit.Enabled = false; // should be edit 
                    btnDelete.Enabled = false;
                    btnDone.Enabled = false;
                    txtTitle.Focus();
                    break;
            }
        }

        private void txtPubName_KeyPress(object sender, KeyPressEventArgs e)
        {
            // accidental 
        }

        // 6-9 doesn't seem to mention modifying this method   
        /*
        // added manually
        private void txtInput_KeyPress(object sender, KeyPressEventArgs e) 
        {
            TextBox whichBox = (TextBox)sender;
            if((int) e.KeyChar == 13) 
            {
                switch (whichBox.Name) 
                {
                    case "txtPubName":
                        txtISBN.Focus();
                        break;
                    case "txtCompanyName":
                        txtDescription.Focus();
                        break;
                    case "txtPubAdress":
                        txtNotes.Focus();
                        break;
                    case "txtPubCity":
                        txtSubject.Focus();
                        break;
                    case "txtPubState":
                        txtComments.Focus();
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
                        txtYear.Focus();
                        break;
                }
            }
        }
        */

        private bool ValidateData() 
        {
            string message = "";
            bool allOK = true;
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
            string savedName = txtYear.Text;
            int savedRow;
            try 
            {
                titlesManager.EndCurrentEdit();
                SqlCommandBuilder ISBNCommandUpdate = new SqlCommandBuilder(ISBNAuthorsAdapter);
                // delete all rows of the data table then repopulate
                if(ISBNAuthorsTable.Rows.Count != 0) 
                {
                    for(int i = 0; i < ISBNAuthorsTable.Rows.Count; i++) 
                    {
                        ISBNAuthorsTable.Rows[i].Delete();
                    }
                }
                ISBNAuthorsAdapter.Update(ISBNAuthorsTable);
                
                for(int i = 0; i < 4; i++) 
                {
                    if(authorsCombo[i].SelectedIndex != -1) 
                    {
                        ISBNAuthorsTable.Rows.Add();
                        ISBNAuthorsTable.Rows[ISBNAuthorsTable.Rows.Count - 1]["ISBN"] = txtISBN.Text;
                        ISBNAuthorsTable.Rows[ISBNAuthorsTable.Rows.Count - 1]["Au_ID"] = authorsCombo[i].SelectedValue;
                    }
                }
                ISBNAuthorsAdapter.Update(ISBNAuthorsTable);

                titlesTable.DefaultView.Sort = "Title";
                savedRow = titlesTable.DefaultView.Find(savedName);
                titlesManager.Position = savedRow;
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
            titlesManager.CancelCurrentEdit();
            if (myState.Equals("Add")) 
            {
                titlesManager.Position = myBookmark;
            }
            SetState("View");
            SetText();
            GetAuthors();
        }

        private void btnAddNew_Click(object sender, EventArgs e)
        {
            try 
            {
                myBookmark = titlesManager.Position;
                titlesManager.AddNew();
                SetState("Add");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error adding record.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            SetText();
            cboAuthor1.SelectedIndex = -1;
            cboAuthor2.SelectedIndex = -1;
            cboAuthor3.SelectedIndex = -1;
            cboAuthor4.SelectedIndex = -1;
        }

        private void btnExit_Click(object sender, EventArgs e)
        {
            SetState("Edit");
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
                titlesManager.RemoveAt(titlesManager.Position);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error deleting record", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            SetText();
            GetAuthors();
        }

        private void btnDone_Click(object sender, EventArgs e)
        {
            this.Close();
        }
        // first 
        private void button1_Click(object sender, EventArgs e)
        {
            titlesManager.Position = 0;
            SetText();
            GetAuthors();
        }
        // last
        private void button2_Click(object sender, EventArgs e)
        {
            titlesManager.Position = titlesManager.Count - 1;
            SetText();
        }

        private void btnFind_Click(object sender, EventArgs e)
        {
            if (txtFind.Text.Equals("")) 
            {
                return;
            }
            int savedRow = titlesManager.Position;
            DataRow[] foundRows;
            titlesTable.DefaultView.Sort = "Title";
            foundRows = titlesTable.Select("Title LIKE '" + txtFind.Text + "*'");
            if(foundRows.Length == 0) 
            {
                titlesManager.Position = savedRow;
            }
            else 
            {
                titlesManager.Position = titlesTable.DefaultView.Find(foundRows[0]["Title"]);
            }
            SetText();
            GetAuthors();
        }

        private void SetText() 
        {
            this.Text = "Titles - Record " + (titlesManager.Count.ToString() + " Records");
        }

        private void btnPublishers_Click(object sender, EventArgs e)
        {
            frmPublishers pubForm = new frmPublishers();
            string pubSave = cboPublisher.Text;
            pubForm.ShowDialog();
            pubForm.Dispose();
            // need to regenorate publishers data
            booksConnection.Close();

            hlpPublishers.HelpNamespace = Application.StartupPath + "\\Publishers.chm";
            string path = Path.GetFullPath("SQLBooksDB.mdf");

            booksConnection = new SqlConnection($@"Data Source=.\SQLEXPRESS; AttachDbFilename={path};
                                                    Integrated Security=True; Connect Timeout=30; User Instance=True");
            booksConnection.Open();

            publishersAdapter.SelectCommand = publishersCommand;
            publishersTable = new DataTable();
            publishersAdapter.Fill(publishersTable);
            cboPublisher.DataSource = publishersTable;
            cboPublisher.Text = pubSave;
        }

        private void GetAuthors() 
        {
            string SQLStatment = "SELECT Title_Author.* FROM Title_Author WHERE Title_Author.ISBN = '" + txtISBN.Text + "'";
            for (int i = 0; i < 4; i++) 
            {
                authorsCombo[i].SelectedIndex = -1;
            }
            ISBNAuthorsCommand = new SqlCommand(SQLStatment, booksConnection);
            ISBNAuthorsAdapter = new SqlDataAdapter();
            ISBNAuthorsAdapter.SelectCommand = ISBNAuthorsCommand;
            ISBNAuthorsTable = new DataTable();
            ISBNAuthorsAdapter.Fill(ISBNAuthorsTable);
            if(ISBNAuthorsTable.Rows.Count == 0) 
            {
                return;
            }
            for (int i = 0; i < ISBNAuthorsTable.Rows.Count; i++) 
            {
                authorsCombo[i].SelectedValue = ISBNAuthorsTable.Rows[i]["Au_ID"].ToString();
            }
        }

        // not named because visual studio won't load the properties
        private void button3_Click(object sender, EventArgs e)
        {
            // declare the document 
            PrintDocument recordDocument;
            // create document and name it 
            recordDocument = new PrintDocument();
            recordDocument.DocumentName = "Titles Record";
            // add code handler
            recordDocument.PrintPage += new PrintPageEventHandler(this.PrintRecordPage);
            //recordDocument.Print();
            recordDocument.Dispose();
        }

    
        private void PrintRecordPage(object sender, PrintPageEventArgs e) 
        {
            Pen myPen = new Pen(Color.Black, 3);
            e.Graphics.DrawRectangle(myPen, e.MarginBounds.Left, e.MarginBounds.Top, e.MarginBounds.Width, 100);
            e.Graphics.DrawImage(picBooks.Image, e.MarginBounds.Left + 10, e.MarginBounds.Top + 10, 80, 80);
            string s = "BOOKS DATABASE";
            Font myFont = new Font("Ariel", 24, FontStyle.Bold);
            SizeF sSize = e.Graphics.MeasureString(s, myFont);
            e.Graphics.DrawString(s, myFont, Brushes.Black, e.MarginBounds.Left + 100 + Convert.ToInt32(0.5 * (e.MarginBounds.Width - 100 - sSize.Width)), e.MarginBounds.Top + Convert.ToInt32(0.5 * (100 - sSize.Height)));
            myFont = new Font("Ariel", 12, FontStyle.Regular);
            int y = 30;
            int dy = Convert.ToInt32(e.Graphics.MeasureString("S", myFont).Height);

            // print title 
            e.Graphics.DrawString("Title: " + txtTitle.Text, myFont, Brushes.Black, e.MarginBounds.Left, y);
            y += 2 * dy;
            // print authors 
            e.Graphics.DrawString("Author(s): ", myFont, Brushes.Black, e.MarginBounds.Left, y);
            int x = e.MarginBounds.Left + Convert.ToInt32(e.Graphics.MeasureString("Author(s): ", myFont).Width);
            MessageBox.Show(ISBNAuthorsTable.Rows.Count.ToString());
            if (ISBNAuthorsTable.Rows.Count != 0) 
            {
                for(int i = 0; i < ISBNAuthorsTable.Rows.Count; i++) 
                {
                    e.Graphics.DrawString(authorsCombo[i].Text, myFont, Brushes.Black, x, y);
                    y += dy;
                }
            }
            else 
            {
                e.Graphics.DrawString("None", myFont, Brushes.Black, x, y);
                y += dy;
            }
            x = e.MarginBounds.Left;
            y += dy;

            // print the other fields
            e.Graphics.DrawString("ISBN: " + txtISBN.Text, myFont, Brushes.Black, x, y);
            y += 2 * dy;
            e.Graphics.DrawString("Year Published: " + txtYear.Text, myFont, Brushes.Black, x, y);
            y += 2 * dy;
            e.Graphics.DrawString("Publisher: " + cboPublisher.Text, myFont, Brushes.Black, x, y);
            y += 2 * dy;
            e.Graphics.DrawString("Description: " + txtDescription.Text, myFont, Brushes.Black, x, y);
            y += 2 * dy;
            e.Graphics.DrawString("Notes: " +  txtNotes.Text, myFont, Brushes.Black, x, y);
            y += 2 * dy;
            e.Graphics.DrawString("Comments: " +  txtComments.Text, myFont, Brushes.Black, x, y);
            y += 2 * dy;
        }
        // btnPrintTitles
        private void button4_Click(object sender, EventArgs e)
           {
            // start printing process at the first record
            pageNumber = 1;
            button1.PerformClick();
            PrintDocument titlesDocument;

            // create the first document and name it
            titlesDocument = new PrintDocument();
            titlesDocument.DocumentName = "Titles Listing";
            // add handler code
            titlesDocument.PrintPage += new PrintPageEventHandler(this.PrintTilesPage);
            // print document
            dlgPreview.Document = titlesDocument;
            dlgPreview.ShowDialog();
            // dispose of document when done printing
            titlesDocument.Dispose();
        }

        private void PrintTilesPage(object sender, PrintPageEventArgs e)
        {
            // print headings
            Font myFont = new Font("Courier New", 14, FontStyle.Bold);
            e.Graphics.DrawString("Titles - Page" + pageNumber.ToString(), myFont, Brushes.Black, e.MarginBounds.Left, e.MarginBounds.Top);
            myFont = new Font("Courier New", 14, FontStyle.Underline);
            int y = Convert.ToInt32(e.MarginBounds.Top + 50);
            e.Graphics.DrawString("Title", myFont, Brushes.Black, e.MarginBounds.Left, y);
            e.Graphics.DrawString("Author", myFont, Brushes.Black, e.MarginBounds.Left + Convert.ToInt32(0.6 * (e.MarginBounds.Width)), y);
            y += Convert.ToInt32(2 * myFont.GetHeight());
            myFont = new Font("Courier New", 12, FontStyle.Regular);
            int iEnd = titlesPerPage + pageNumber;
            if(iEnd > titlesTable.Rows.Count)
            {
                iEnd = titlesTable.Rows.Count;
                e.HasMorePages = false;
            }
            else
            {
                e.HasMorePages = true;
            }
            for(int i = 1 + titlesPerPage * (pageNumber - 1); i <= iEnd; i++)
            {
                // programatically move through all the records
                if(txtTitle.Text.Length < 35)
                {
                    e.Graphics.DrawString(txtTitle.Text, myFont, Brushes.Black, e.MarginBounds.Left, y);
                }
                else
                {
                    e.Graphics.DrawString(txtTitle.Text.Substring(0, 35), myFont, Brushes.Black, e.MarginBounds.Left, y);
                }
                if(cboAuthor1.Text.Length < 20)
                {
                    e.Graphics.DrawString(cboAuthor1.Text, myFont, Brushes.Black, e.MarginBounds.Left + Convert.ToInt32(0.6 * (e.MarginBounds.Width)), y);
                }
                else
                {
                    e.Graphics.DrawString(cboAuthor1.Text.Substring(0, 20), myFont, Brushes.Black, e.MarginBounds.Left + Convert.ToInt32(0.6 * (e.MarginBounds.Width)), y);
                }
                btnNext.PerformClick();
                y += Convert.ToInt32(myFont.GetHeight());
            }
            if (e.HasMorePages)
                pageNumber++;
            else
                pageNumber = 1;
        }
    }
}
