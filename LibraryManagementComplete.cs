using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Drawing;

// ==================== MODELS ====================
namespace LibraryManagement.Models
{
    public class Book
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Author { get; set; }
        public string ISBN { get; set; }
        public int PublicationYear { get; set; }
        public int TotalCopies { get; set; }
        public int AvailableCopies { get; set; }
        public string Category { get; set; }
        public string Publisher { get; set; }
        public decimal Price { get; set; }

        public override string ToString()
        {
            return $"{Title} - {Author}";
        }
    }

    public class Member
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public DateTime JoinDate { get; set; }
        public bool IsActive { get; set; }
        public string Address { get; set; }
        public string MembershipType { get; set; }

        public override string ToString()
        {
            return $"{Name} - {Email}";
        }
    }

    public class Loan
    {
        public int Id { get; set; }
        public int BookId { get; set; }
        public int MemberId { get; set; }
        public string BookTitle { get; set; }
        public string MemberName { get; set; }
        public DateTime LoanDate { get; set; }
        public DateTime DueDate { get; set; }
        public DateTime? ReturnDate { get; set; }
        public bool IsReturned { get; set; }
        public decimal Fine { get; set; }
    }
}

// ==================== SERVICES ====================
namespace LibraryManagement.Services
{
    using LibraryManagement.Models;

    public class LibraryService
    {
        private List<Book> books = new List<Book>();
        private List<Member> members = new List<Member>();
        private List<Loan> loans = new List<Loan>();
        private int bookIdCounter = 1;
        private int memberIdCounter = 1;
        private int loanIdCounter = 1;

        // ===== BOOK MANAGEMENT =====
        public void AddBook(string title, string author, string isbn, int year, int copies, string category, string publisher, decimal price)
        {
            var book = new Book
            {
                Id = bookIdCounter++,
                Title = title,
                Author = author,
                ISBN = isbn,
                PublicationYear = year,
                TotalCopies = copies,
                AvailableCopies = copies,
                Category = category,
                Publisher = publisher,
                Price = price
            };
            books.Add(book);
        }

        public List<Book> GetAllBooks()
        {
            return new List<Book>(books);
        }

        public Book GetBookById(int id)
        {
            return books.FirstOrDefault(b => b.Id == id);
        }

        public List<Book> SearchBooks(string searchTerm)
        {
            return books.Where(b =>
                b.Title.ToLower().Contains(searchTerm.ToLower()) ||
                b.Author.ToLower().Contains(searchTerm.ToLower()) ||
                b.ISBN.ToLower().Contains(searchTerm.ToLower())
            ).ToList();
        }

        public List<Book> GetBooksByCategory(string category)
        {
            return books.Where(b => b.Category.ToLower() == category.ToLower()).ToList();
        }

        public List<string> GetAllCategories()
        {
            return books.Select(b => b.Category).Distinct().ToList();
        }

        public void UpdateBook(int bookId, string title, string author, string isbn, int year, int copies, string category, string publisher, decimal price)
        {
            var book = GetBookById(bookId);
            if (book != null)
            {
                book.Title = title;
                book.Author = author;
                book.ISBN = isbn;
                book.PublicationYear = year;
                book.Category = category;
                book.Publisher = publisher;
                book.Price = price;
                book.AvailableCopies += (copies - book.TotalCopies);
                book.TotalCopies = copies;
            }
        }

        public void DeleteBook(int bookId)
        {
            var book = GetBookById(bookId);
            if (book != null)
            {
                books.Remove(book);
            }
        }

        // ===== MEMBER MANAGEMENT =====
        public void AddMember(string name, string email, string phone, string address, string membershipType)
        {
            var member = new Member
            {
                Id = memberIdCounter++,
                Name = name,
                Email = email,
                Phone = phone,
                Address = address,
                MembershipType = membershipType,
                JoinDate = DateTime.Now,
                IsActive = true
            };
            members.Add(member);
        }

        public List<Member> GetAllMembers()
        {
            return new List<Member>(members);
        }

        public Member GetMemberById(int id)
        {
            return members.FirstOrDefault(m => m.Id == id);
        }

        public List<Member> SearchMembers(string searchTerm)
        {
            return members.Where(m =>
                m.Name.ToLower().Contains(searchTerm.ToLower()) ||
                m.Email.ToLower().Contains(searchTerm.ToLower()) ||
                m.Phone.Contains(searchTerm)
            ).ToList();
        }

        public void UpdateMember(int memberId, string name, string email, string phone, string address, string membershipType)
        {
            var member = GetMemberById(memberId);
            if (member != null)
            {
                member.Name = name;
                member.Email = email;
                member.Phone = phone;
                member.Address = address;
                member.MembershipType = membershipType;
            }
        }

        public void DeactivateMember(int memberId)
        {
            var member = GetMemberById(memberId);
            if (member != null)
            {
                member.IsActive = false;
            }
        }

        public void ActivateMember(int memberId)
        {
            var member = GetMemberById(memberId);
            if (member != null)
            {
                member.IsActive = true;
            }
        }

        // ===== LOAN MANAGEMENT =====
        public bool BorrowBook(int memberId, int bookId)
        {
            var member = GetMemberById(memberId);
            var book = GetBookById(bookId);

            if (member == null || book == null || !member.IsActive || book.AvailableCopies <= 0)
                return false;

            var loan = new Loan
            {
                Id = loanIdCounter++,
                BookId = bookId,
                MemberId = memberId,
                BookTitle = book.Title,
                MemberName = member.Name,
                LoanDate = DateTime.Now,
                DueDate = DateTime.Now.AddDays(14),
                IsReturned = false
            };

            loans.Add(loan);
            book.AvailableCopies--;
            return true;
        }

        public bool ReturnBook(int loanId)
        {
            var loan = loans.FirstOrDefault(l => l.Id == loanId);
            if (loan == null || loan.IsReturned)
                return false;

            var book = GetBookById(loan.BookId);
            loan.ReturnDate = DateTime.Now;
            loan.IsReturned = true;

            int daysLate = (int)(loan.ReturnDate.Value - loan.DueDate).TotalDays;
            if (daysLate > 0)
            {
                loan.Fine = daysLate * 5m;
            }

            book.AvailableCopies++;
            return true;
        }

        public List<Loan> GetAllLoans()
        {
            return new List<Loan>(loans);
        }

        public List<Loan> GetMemberLoans(int memberId)
        {
            return loans.Where(l => l.MemberId == memberId).ToList();
        }

        public List<Loan> GetActiveMemberLoans(int memberId)
        {
            return loans.Where(l => l.MemberId == memberId && !l.IsReturned).ToList();
        }

        public List<Loan> GetOverdueLoans()
        {
            return loans.Where(l => !l.IsReturned && l.DueDate < DateTime.Now).ToList();
        }

        public Loan GetLoanById(int loanId)
        {
            return loans.FirstOrDefault(l => l.Id == loanId);
        }

        // ===== STATISTICS =====
        public int GetTotalBooks()
        {
            return books.Count;
        }

        public int GetTotalMembers()
        {
            return members.Count;
        }

        public int GetActiveLoans()
        {
            return loans.Count(l => !l.IsReturned);
        }

        public int GetAvailableBooks()
        {
            return books.Sum(b => b.AvailableCopies);
        }

        public decimal GetTotalFines()
        {
            return loans.Where(l => l.IsReturned && l.Fine > 0).Sum(l => l.Fine);
        }
    }
}

// ==================== FORMS ====================
namespace LibraryManagement.Forms
{
    using LibraryManagement.Models;
    using LibraryManagement.Services;

    // ===== MAIN FORM =====
    public partial class MainForm : Form
    {
        private LibraryService libraryService;
        private Button btnBooks, btnMembers, btnLoans, btnReports, btnExit;

        public MainForm()
        {
            InitializeComponent();
            libraryService = new LibraryService();
            LoadSampleData();
            ApplyTheme();
        }

        private void ApplyTheme()
        {
            this.BackColor = Color.FromArgb(240, 240, 240);
            this.Font = new Font("Segoe UI", 10);
        }

        private void LoadSampleData()
        {
            libraryService.AddBook("الحرب والسلام", "ليو تولستوي", "978-1234567890", 1869, 3, "روايات", "دار النشر", 150);
            libraryService.AddBook("الجريمة والعقاب", "فيودور دوستويفسكي", "978-0987654321", 1866, 2, "روايات", "دار النشر", 120);
            libraryService.AddBook("البرمجة بلغة C#", "محمد علي", "978-1111111111", 2022, 5, "تقنية", "دار التقنية", 200);
            libraryService.AddBook("تعلم قواعد الإنجليزية", "أحمد محمود", "978-2222222222", 2021, 4, "لغات", "دار اللغات", 100);

            libraryService.AddMember("علي محمد", "ali@example.com", "0501234567", "الرياض", "فضي");
            libraryService.AddMember("فاطمة أحمد", "fatima@example.com", "0509876543", "جدة", "ذهبي");
            libraryService.AddMember("محمود عبدالله", "mahmoud@example.com", "0505555555", "الدمام", "فضي");
        }

        private void btnBooks_Click(object sender, EventArgs e)
        {
            BooksForm booksForm = new BooksForm(libraryService);
            booksForm.ShowDialog();
        }

        private void btnMembers_Click(object sender, EventArgs e)
        {
            MembersForm membersForm = new MembersForm(libraryService);
            membersForm.ShowDialog();
        }

        private void btnLoans_Click(object sender, EventArgs e)
        {
            LoansForm loansForm = new LoansForm(libraryService);
            loansForm.ShowDialog();
        }

        private void btnReports_Click(object sender, EventArgs e)
        {
            ReportsForm reportsForm = new ReportsForm(libraryService);
            reportsForm.ShowDialog();
        }

        private void btnExit_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();

            this.AutoScaleDimensions = new SizeF(8F, 16F);
            this.AutoScaleMode = AutoScaleMode.Font;
            this.ClientSize = new Size(900, 600);
            this.Name = "MainForm";
            this.Text = "نظام إدارة المكتبة الذكي";
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Font = new Font("Segoe UI", 12F);

            var panelTitle = new Panel();
            panelTitle.BackColor = Color.FromArgb(0, 102, 204);
            panelTitle.Dock = DockStyle.Top;
            panelTitle.Height = 80;

            var lblTitle = new Label();
            lblTitle.Text = "نظام إدارة المكتبة الذكي";
            lblTitle.Font = new Font("Segoe UI", 24, FontStyle.Bold);
            lblTitle.ForeColor = Color.White;
            lblTitle.Dock = DockStyle.Fill;
            lblTitle.TextAlign = ContentAlignment.MiddleCenter;

            panelTitle.Controls.Add(lblTitle);

            var panelButtons = new Panel();
            panelButtons.Dock = DockStyle.Fill;
            panelButtons.Padding = new Padding(20);

            btnBooks = new Button();
            btnBooks.Text = "📚 إدارة الكتب";
            btnBooks.Size = new Size(200, 100);
            btnBooks.Location = new Point(50, 50);
            btnBooks.Font = new Font("Segoe UI", 14, FontStyle.Bold);
            btnBooks.BackColor = Color.FromArgb(76, 175, 80);
            btnBooks.ForeColor = Color.White;
            btnBooks.Cursor = Cursors.Hand;
            btnBooks.Click += btnBooks_Click;

            btnMembers = new Button();
            btnMembers.Text = "👥 إدارة الأعضاء";
            btnMembers.Size = new Size(200, 100);
            btnMembers.Location = new Point(350, 50);
            btnMembers.Font = new Font("Segoe UI", 14, FontStyle.Bold);
            btnMembers.BackColor = Color.FromArgb(33, 150, 243);
            btnMembers.ForeColor = Color.White;
            btnMembers.Cursor = Cursors.Hand;
            btnMembers.Click += btnMembers_Click;

            btnLoans = new Button();
            btnLoans.Text = "📖 إدارة الإعارات";
            btnLoans.Size = new Size(200, 100);
            btnLoans.Location = new Point(650, 50);
            btnLoans.Font = new Font("Segoe UI", 14, FontStyle.Bold);
            btnLoans.BackColor = Color.FromArgb(255, 152, 0);
            btnLoans.ForeColor = Color.White;
            btnLoans.Cursor = Cursors.Hand;
            btnLoans.Click += btnLoans_Click;

            btnReports = new Button();
            btnReports.Text = "📊 التقارير";
            btnReports.Size = new Size(200, 100);
            btnReports.Location = new Point(50, 250);
            btnReports.Font = new Font("Segoe UI", 14, FontStyle.Bold);
            btnReports.BackColor = Color.FromArgb(156, 39, 176);
            btnReports.ForeColor = Color.White;
            btnReports.Cursor = Cursors.Hand;
            btnReports.Click += btnReports_Click;

            btnExit = new Button();
            btnExit.Text = "🚪 خروج";
            btnExit.Size = new Size(200, 100);
            btnExit.Location = new Point(350, 250);
            btnExit.Font = new Font("Segoe UI", 14, FontStyle.Bold);
            btnExit.BackColor = Color.FromArgb(244, 67, 54);
            btnExit.ForeColor = Color.White;
            btnExit.Cursor = Cursors.Hand;
            btnExit.Click += btnExit_Click;

            panelButtons.Controls.Add(btnBooks);
            panelButtons.Controls.Add(btnMembers);
            panelButtons.Controls.Add(btnLoans);
            panelButtons.Controls.Add(btnReports);
            panelButtons.Controls.Add(btnExit);

            this.Controls.Add(panelButtons);
            this.Controls.Add(panelTitle);

            this.ResumeLayout(false);
        }
    }

    // ===== BOOKS FORM =====
    public partial class BooksForm : Form
    {
        private LibraryService libraryService;
        private DataGridView dgvBooks;
        private TextBox txtSearch;
        private Button btnAdd, btnEdit, btnDelete, btnRefresh, btnClose;

        public BooksForm(LibraryService service)
        {
            libraryService = service;
            InitializeComponent();
            LoadBooks();
        }

        private void InitializeComponent()
        {
            this.AutoScaleDimensions = new SizeF(8F, 16F);
            this.AutoScaleMode = AutoScaleMode.Font;
            this.ClientSize = new Size(1000, 600);
            this.Name = "BooksForm";
            this.Text = "إدارة الكتب";
            this.StartPosition = FormStartPosition.CenterParent;

            var panelSearch = new Panel();
            panelSearch.Dock = DockStyle.Top;
            panelSearch.Height = 50;
            panelSearch.Padding = new Padding(10);

            var lblSearch = new Label();
            lblSearch.Text = "بحث:";
            lblSearch.AutoSize = true;
            lblSearch.Location = new Point(10, 15);

            txtSearch = new TextBox();
            txtSearch.Location = new Point(50, 12);
            txtSearch.Size = new Size(300, 25);
            txtSearch.TextChanged += (s, e) => SearchBooks();

            panelSearch.Controls.Add(lblSearch);
            panelSearch.Controls.Add(txtSearch);

            dgvBooks = new DataGridView();
            dgvBooks.Dock = DockStyle.Fill;
            dgvBooks.AutoGenerateColumns = false;
            dgvBooks.AllowUserToAddRows = false;
            dgvBooks.ReadOnly = true;
            dgvBooks.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvBooks.MultiSelect = false;

            dgvBooks.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Id", HeaderText = "رقم", Width = 50 });
            dgvBooks.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Title", HeaderText = "العنوان", Width = 200 });
            dgvBooks.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Author", HeaderText = "المؤلف", Width = 150 });
            dgvBooks.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Category", HeaderText = "الفئة", Width = 100 });
            dgvBooks.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "AvailableCopies", HeaderText = "النسخ المتاحة", Width = 80 });
            dgvBooks.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "TotalCopies", HeaderText = "إجمالي", Width = 60 });

            var panelButtons = new Panel();
            panelButtons.Dock = DockStyle.Bottom;
            panelButtons.Height = 50;
            panelButtons.Padding = new Padding(10);

            btnAdd = new Button { Text = "إضافة", Location = new Point(10, 10), Size = new Size(100, 30) };
            btnAdd.Click += BtnAdd_Click;

            btnEdit = new Button { Text = "تعديل", Location = new Point(120, 10), Size = new Size(100, 30) };
            btnEdit.Click += BtnEdit_Click;

            btnDelete = new Button { Text = "حذف", Location = new Point(230, 10), Size = new Size(100, 30) };
            btnDelete.Click += BtnDelete_Click;

            btnRefresh = new Button { Text = "تحديث", Location = new Point(340, 10), Size = new Size(100, 30) };
            btnRefresh.Click += (s, e) => LoadBooks();

            btnClose = new Button { Text = "إغلاق", Location = new Point(890, 10), Size = new Size(100, 30) };
            btnClose.Click += (s, e) => this.Close();

            panelButtons.Controls.Add(btnAdd);
            panelButtons.Controls.Add(btnEdit);
            panelButtons.Controls.Add(btnDelete);
            panelButtons.Controls.Add(btnRefresh);
            panelButtons.Controls.Add(btnClose);

            this.Controls.Add(dgvBooks);
            this.Controls.Add(panelButtons);
            this.Controls.Add(panelSearch);
        }

        private void LoadBooks()
        {
            dgvBooks.DataSource = libraryService.GetAllBooks();
        }

        private void SearchBooks()
        {
            string searchTerm = txtSearch.Text;
            if (string.IsNullOrEmpty(searchTerm))
                LoadBooks();
            else
                dgvBooks.DataSource = libraryService.SearchBooks(searchTerm);
        }

        private void BtnAdd_Click(object sender, EventArgs e)
        {
            AddBookForm addForm = new AddBookForm();
            if (addForm.ShowDialog() == DialogResult.OK)
            {
                libraryService.AddBook(
                    addForm.Title, addForm.Author, addForm.ISBN,
                    addForm.Year, addForm.Copies, addForm.Category,
                    addForm.Publisher, addForm.Price);
                LoadBooks();
            }
        }

        private void BtnEdit_Click(object sender, EventArgs e)
        {
            if (dgvBooks.SelectedRows.Count == 0)
            {
                MessageBox.Show("اختر كتاباً لتعديله");
                return;
            }

            int bookId = (int)dgvBooks.SelectedRows[0].Cells[0].Value;
            var book = libraryService.GetBookById(bookId);

            AddBookForm editForm = new AddBookForm(book);
            if (editForm.ShowDialog() == DialogResult.OK)
            {
                libraryService.UpdateBook(
                    bookId, editForm.Title, editForm.Author, editForm.ISBN,
                    editForm.Year, editForm.Copies, editForm.Category,
                    editForm.Publisher, editForm.Price);
                LoadBooks();
            }
        }

        private void BtnDelete_Click(object sender, EventArgs e)
        {
            if (dgvBooks.SelectedRows.Count == 0)
            {
                MessageBox.Show("اختر كتاباً لحذفه");
                return;
            }

            int bookId = (int)dgvBooks.SelectedRows[0].Cells[0].Value;
            if (MessageBox.Show("هل متأكد من حذف هذا الكتاب؟", "تأكيد", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                libraryService.DeleteBook(bookId);
                LoadBooks();
            }
        }
    }

    // ===== ADD BOOK FORM =====
    public partial class AddBookForm : Form
    {
        public string Title { get; set; }
        public string Author { get; set; }
        public string ISBN { get; set; }
        public int Year { get; set; }
        public int Copies { get; set; }
        public string Category { get; set; }
        public string Publisher { get; set; }
        public decimal Price { get; set; }

        private TextBox txtTitle, txtAuthor, txtISBN, txtYear, txtCopies, txtCategory, txtPublisher, txtPrice;
        private Button btnOK, btnCancel;

        public AddBookForm(Book book = null)
        {
            InitializeComponent();
            if (book != null)
            {
                txtTitle.Text = book.Title;
                txtAuthor.Text = book.Author;
                txtISBN.Text = book.ISBN;
                txtYear.Text = book.PublicationYear.ToString();
                txtCopies.Text = book.TotalCopies.ToString();
                txtCategory.Text = book.Category;
                txtPublisher.Text = book.Publisher;
                txtPrice.Text = book.Price.ToString();
                this.Text = "تعديل الكتاب";
            }
        }

        private void InitializeComponent()
        {
            this.AutoScaleDimensions = new SizeF(8F, 16F);
            this.AutoScaleMode = AutoScaleMode.Font;
            this.ClientSize = new Size(500, 400);
            this.Name = "AddBookForm";
            this.Text = "إضافة كتاب";
            this.StartPosition = FormStartPosition.CenterParent;

            int yPos = 20;

            AddLabel("العنوان:", 20, yPos);
            txtTitle = new TextBox { Location = new Point(20, yPos + 25), Size = new Size(450, 25) };
            yPos += 60;

            AddLabel("المؤلف:", 20, yPos);
            txtAuthor = new TextBox { Location = new Point(20, yPos + 25), Size = new Size(450, 25) };
            yPos += 60;

            AddLabel("ISBN:", 20, yPos);
            txtISBN = new TextBox { Location = new Point(20, yPos + 25), Size = new Size(450, 25) };
            yPos += 60;

            AddLabel("السنة:", 20, yPos);
            txtYear = new TextBox { Location = new Point(20, yPos + 25), Size = new Size(450, 25) };
            yPos += 60;

            AddLabel("عدد النسخ:", 20, yPos);
            txtCopies = new TextBox { Location = new Point(20, yPos + 25), Size = new Size(450, 25) };
            yPos += 60;

            AddLabel("الفئة:", 20, yPos);
            txtCategory = new TextBox { Location = new Point(20, yPos + 25), Size = new Size(450, 25) };
            yPos += 60;

            AddLabel("الناشر:", 20, yPos);
            txtPublisher = new TextBox { Location = new Point(20, yPos + 25), Size = new Size(450, 25) };
            yPos += 60;

            AddLabel("السعر:", 20, yPos);
            txtPrice = new TextBox { Location = new Point(20, yPos + 25), Size = new Size(450, 25) };

            btnOK = new Button { Text = "موافق", DialogResult = DialogResult.OK, Location = new Point(200, 350), Size = new Size(100, 30) };
            btnCancel = new Button { Text = "إلغاء", DialogResult = DialogResult.Cancel, Location = new Point(310, 350), Size = new Size(100, 30) };

            btnOK.Click += (s, e) =>
            {
                Title = txtTitle.Text;
                Author = txtAuthor.Text;
                ISBN = txtISBN.Text;
                Year = int.Parse(txtYear.Text ?? "2024");
                Copies = int.Parse(txtCopies.Text ?? "1");
                Category = txtCategory.Text;
                Publisher = txtPublisher.Text;
                Price = decimal.Parse(txtPrice.Text ?? "0");
            };

            this.Controls.Add(txtTitle);
            this.Controls.Add(txtAuthor);
            this.Controls.Add(txtISBN);
            this.Controls.Add(txtYear);
            this.Controls.Add(txtCopies);
            this.Controls.Add(txtCategory);
            this.Controls.Add(txtPublisher);
            this.Controls.Add(txtPrice);
            this.Controls.Add(btnOK);
            this.Controls.Add(btnCancel);
        }

        private void AddLabel(string text, int x, int y)
        {
            var lbl = new Label { Text = text, Location = new Point(x, y), AutoSize = true };
            this.Controls.Add(lbl);
        }
    }

    // ===== MEMBERS FORM =====
    public partial class MembersForm : Form
    {
        private LibraryService libraryService;
        private DataGridView dgvMembers;
        private TextBox txtSearch;
        private Button btnAdd, btnEdit, btnDelete, btnRefresh, btnClose;

        public MembersForm(LibraryService service)
        {
            libraryService = service;
            InitializeComponent();
            LoadMembers();
        }

        private void InitializeComponent()
        {
            this.AutoScaleDimensions = new SizeF(8F, 16F);
            this.AutoScaleMode = AutoScaleMode.Font;
            this.ClientSize = new Size(1000, 600);
            this.Name = "MembersForm";
            this.Text = "إدارة الأعضاء";
            this.StartPosition = FormStartPosition.CenterParent;

            var panelSearch = new Panel();
            panelSearch.Dock = DockStyle.Top;
            panelSearch.Height = 50;
            panelSearch.Padding = new Padding(10);

            var lblSearch = new Label();
            lblSearch.Text = "بحث:";
            lblSearch.AutoSize = true;
            lblSearch.Location = new Point(10, 15);

            txtSearch = new TextBox();
            txtSearch.Location = new Point(50, 12);
            txtSearch.Size = new Size(300, 25);
            txtSearch.TextChanged += (s, e) => SearchMembers();

            panelSearch.Controls.Add(lblSearch);
            panelSearch.Controls.Add(txtSearch);

            dgvMembers = new DataGridView();
            dgvMembers.Dock = DockStyle.Fill;
            dgvMembers.AutoGenerateColumns = false;
            dgvMembers.AllowUserToAddRows = false;
            dgvMembers.ReadOnly = true;

            dgvMembers.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Id", HeaderText = "رقم", Width = 50 });
            dgvMembers.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Name", HeaderText = "الاسم", Width = 150 });
            dgvMembers.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Email", HeaderText = "البريد", Width = 200 });
            dgvMembers.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Phone", HeaderText = "الهاتف", Width = 150 });
            dgvMembers.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "MembershipType", HeaderText = "نوع العضوية", Width = 100 });

            var panelButtons = new Panel();
            panelButtons.Dock = DockStyle.Bottom;
            panelButtons.Height = 50;
            panelButtons.Padding = new Padding(10);

            btnAdd = new Button { Text = "إضافة", Location = new Point(10, 10), Size = new Size(100, 30) };
            btnAdd.Click += BtnAdd_Click;

            btnEdit = new Button { Text = "تعديل", Location = new Point(120, 10), Size = new Size(100, 30) };
            btnEdit.Click += BtnEdit_Click;

            btnDelete = new Button { Text = "حذف", Location = new Point(230, 10), Size = new Size(100, 30) };
            btnDelete.Click += BtnDelete_Click;

            btnRefresh = new Button { Text = "تحديث", Location = new Point(340, 10), Size = new Size(100, 30) };
            btnRefresh.Click += (s, e) => LoadMembers();

            btnClose = new Button { Text = "إغلاق", Location = new Point(890, 10), Size = new Size(100, 30) };
            btnClose.Click += (s, e) => this.Close();

            panelButtons.Controls.Add(btnAdd);
            panelButtons.Controls.Add(btnEdit);
            panelButtons.Controls.Add(btnDelete);
            panelButtons.Controls.Add(btnRefresh);
            panelButtons.Controls.Add(btnClose);

            this.Controls.Add(dgvMembers);
            this.Controls.Add(panelButtons);
            this.Controls.Add(panelSearch);
        }

        private void LoadMembers()
        {
            dgvMembers.DataSource = libraryService.GetAllMembers();
        }

        private void SearchMembers()
        {
            string searchTerm = txtSearch.Text;
            if (string.IsNullOrEmpty(searchTerm))
                LoadMembers();
            else
                dgvMembers.DataSource = libraryService.SearchMembers(searchTerm);
        }

        private void BtnAdd_Click(object sender, EventArgs e)
        {
            AddMemberForm addForm = new AddMemberForm();
            if (addForm.ShowDialog() == DialogResult.OK)
            {
                libraryService.AddMember(addForm.Name, addForm.Email, addForm.Phone, addForm.Address, addForm.MembershipType);
                LoadMembers();
            }
        }

        private void BtnEdit_Click(object sender, EventArgs e)
        {
            if (dgvMembers.SelectedRows.Count == 0)
            {
                MessageBox.Show("اختر عضواً لتعديله");
                return;
            }

            int memberId = (int)dgvMembers.SelectedRows[0].Cells[0].Value;
            var member = libraryService.GetMemberById(memberId);

            AddMemberForm editForm = new AddMemberForm(member);
            if (editForm.ShowDialog() == DialogResult.OK)
            {
                libraryService.UpdateMember(memberId, editForm.Name, editForm.Email, editForm.Phone, editForm.Address, editForm.MembershipType);
                LoadMembers();
            }
        }

        private void BtnDelete_Click(object sender, EventArgs e)
        {
            if (dgvMembers.SelectedRows.Count == 0)
            {
                MessageBox.Show("اختر عضواً لحذفه");
                return;
            }

            int memberId = (int)dgvMembers.SelectedRows[0].Cells[0].Value;
            if (MessageBox.Show("هل متأكد من حذف هذا العضو؟", "تأكيد", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                libraryService.DeactivateMember(memberId);
                LoadMembers();
            }
        }
    }

    // ===== ADD MEMBER FORM =====
    public partial class AddMemberForm : Form
    {
        public string Name { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Address { get; set; }
        public string MembershipType { get; set; }

        private TextBox txtName, txtEmail, txtPhone, txtAddress;
        private ComboBox cmbMembershipType;
        private Button btnOK, btnCancel;

        public AddMemberForm(Member member = null)
        {
            InitializeComponent();
            if (member != null)
            {
                txtName.Text = member.Name;
                txtEmail.Text = member.Email;
                txtPhone.Text = member.Phone;
                txtAddress.Text = member.Address;
                cmbMembershipType.SelectedItem = member.MembershipType;
                this.Text = "تعديل العضو";
            }
        }

        private void InitializeComponent()
        {
            this.AutoScaleDimensions = new SizeF(8F, 16F);
            this.AutoScaleMode = AutoScaleMode.Font;
            this.ClientSize = new Size(500, 350);
            this.Name = "AddMemberForm";
            this.Text = "إضافة عضو";
            this.StartPosition = FormStartPosition.CenterParent;

            int yPos = 20;

            AddLabel("الاسم:", 20, yPos);
            txtName = new TextBox { Location = new Point(20, yPos + 25), Size = new Size(450, 25) };
            yPos += 60;

            AddLabel("البريد الإلكتروني:", 20, yPos);
            txtEmail = new TextBox { Location = new Point(20, yPos + 25), Size = new Size(450, 25) };
            yPos += 60;

            AddLabel("الهاتف:", 20, yPos);
            txtPhone = new TextBox { Location = new Point(20, yPos + 25), Size = new Size(450, 25) };
            yPos += 60;

            AddLabel("العنوان:", 20, yPos);
            txtAddress = new TextBox { Location = new Point(20, yPos + 25), Size = new Size(450, 25) };
            yPos += 60;

            AddLabel("نوع العضوية:", 20, yPos);
            cmbMembershipType = new ComboBox { Location = new Point(20, yPos + 25), Size = new Size(450, 25) };
            cmbMembershipType.Items.AddRange(new[] { "فضي", "ذهبي", "بلاتيني" });
            cmbMembershipType.SelectedIndex = 0;

            btnOK = new Button { Text = "موافق", DialogResult = DialogResult.OK, Location = new Point(200, 310), Size = new Size(100, 30) };
            btnCancel = new Button { Text = "إلغاء", DialogResult = DialogResult.Cancel, Location = new Point(310, 310), Size = new Size(100, 30) };

            btnOK.Click += (s, e) =>
            {
                Name = txtName.Text;
                Email = txtEmail.Text;
                Phone = txtPhone.Text;
                Address = txtAddress.Text;
                MembershipType = cmbMembershipType.SelectedItem.ToString();
            };

            this.Controls.Add(txtName);
            this.Controls.Add(txtEmail);
            this.Controls.Add(txtPhone);
            this.Controls.Add(txtAddress);
            this.Controls.Add(cmbMembershipType);
            this.Controls.Add(btnOK);
            this.Controls.Add(btnCancel);
        }

        private void AddLabel(string text, int x, int y)
        {
            var lbl = new Label { Text = text, Location = new Point(x, y), AutoSize = true };
            this.Controls.Add(lbl);
        }
    }

    // ===== LOANS FORM =====
    public partial class LoansForm : Form
    {
        private LibraryService libraryService;
        private DataGridView dgvLoans;
        private Button btnBorrow, btnReturn, btnRefresh, btnClose;

        public LoansForm(LibraryService service)
        {
            libraryService = service;
            InitializeComponent();
            LoadLoans();
        }

        private void InitializeComponent()
        {
            this.AutoScaleDimensions = new SizeF(8F, 16F);
            this.AutoScaleMode = AutoScaleMode.Font;
            this.ClientSize = new Size(1100, 600);
            this.Name = "LoansForm";
            this.Text = "إدارة الإعارات";
            this.StartPosition = FormStartPosition.CenterParent;

            dgvLoans = new DataGridView();
            dgvLoans.Dock = DockStyle.Fill;
            dgvLoans.AutoGenerateColumns = false;
            dgvLoans.AllowUserToAddRows = false;
            dgvLoans.ReadOnly = true;

            dgvLoans.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Id", HeaderText = "رقم الإعارة", Width = 80 });
            dgvLoans.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "MemberName", HeaderText = "اسم العضو", Width = 150 });
            dgvLoans.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "BookTitle", HeaderText = "عنوان الكتاب", Width = 200 });
            dgvLoans.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "LoanDate", HeaderText = "تاريخ الإعارة", Width = 120 });
            dgvLoans.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "DueDate", HeaderText = "تاريخ الاستحقاق", Width = 120 });
            dgvLoans.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "ReturnDate", HeaderText = "تاريخ الإرجاع", Width = 120 });
            dgvLoans.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "IsReturned", HeaderText = "الحالة", Width = 80 });

            var panelButtons = new Panel();
            panelButtons.Dock = DockStyle.Bottom;
            panelButtons.Height = 50;
            panelButtons.Padding = new Padding(10);

            btnBorrow = new Button { Text = "إعارة", Location = new Point(10, 10), Size = new Size(100, 30) };
            btnBorrow.Click += BtnBorrow_Click;

            btnReturn = new Button { Text = "إرجاع", Location = new Point(120, 10), Size = new Size(100, 30) };
            btnReturn.Click += BtnReturn_Click;

            btnRefresh = new Button { Text = "تحديث", Location = new Point(230, 10), Size = new Size(100, 30) };
            btnRefresh.Click += (s, e) => LoadLoans();

            btnClose = new Button { Text = "إغلاق", Location = new Point(990, 10), Size = new Size(100, 30) };
            btnClose.Click += (s, e) => this.Close();

            panelButtons.Controls.Add(btnBorrow);
            panelButtons.Controls.Add(btnReturn);
            panelButtons.Controls.Add(btnRefresh);
            panelButtons.Controls.Add(btnClose);

            this.Controls.Add(dgvLoans);
            this.Controls.Add(panelButtons);
        }

        private void LoadLoans()
        {
            dgvLoans.DataSource = libraryService.GetAllLoans();
        }

        private void BtnBorrow_Click(object sender, EventArgs e)
        {
            BorrowForm borrowForm = new BorrowForm(libraryService);
            if (borrowForm.ShowDialog() == DialogResult.OK)
            {
                libraryService.BorrowBook(borrowForm.MemberId, borrowForm.BookId);
                LoadLoans();
            }
        }

        private void BtnReturn_Click(object sender, EventArgs e)
        {
            if (dgvLoans.SelectedRows.Count == 0)
            {
                MessageBox.Show("اختر إعارة لإرجاعها");
                return;
            }

            int loanId = (int)dgvLoans.SelectedRows[0].Cells[0].Value;
            if (libraryService.ReturnBook(loanId))
            {
                MessageBox.Show("تم إرجاع الكتاب بنجاح");
                LoadLoans();
            }
            else
            {
                MessageBox.Show("فشل إرجاع الكتاب");
            }
        }
    }

    // ===== BORROW FORM =====
    public partial class BorrowForm : Form
    {
        public int MemberId { get; set; }
        public int BookId { get; set; }

        private ComboBox cmbMembers, cmbBooks;
        private Button btnOK, btnCancel;

        public BorrowForm(LibraryService service)
        {
            InitializeComponent();

            var members = service.GetAllMembers();
            var books = service.GetAllBooks();

            foreach (var member in members)
            {
                cmbMembers.Items.Add($"{member.Id} - {member.Name}");
            }

            foreach (var book in books)
            {
                if (book.AvailableCopies > 0)
                    cmbBooks.Items.Add($"{book.Id} - {book.Title}");
            }

            if (cmbMembers.Items.Count > 0)
                cmbMembers.SelectedIndex = 0;
            if (cmbBooks.Items.Count > 0)
                cmbBooks.SelectedIndex = 0;
        }

        private void InitializeComponent()
        {
            this.AutoScaleDimensions = new SizeF(8F, 16F);
            this.AutoScaleMode = AutoScaleMode.Font;
            this.ClientSize = new Size(500, 250);
            this.Name = "BorrowForm";
            this.Text = "إعارة كتاب";
            this.StartPosition = FormStartPosition.CenterParent;

            cmbMembers = new ComboBox();
            cmbBooks = new ComboBox();

            var lblMember = new Label { Text = "العضو:", Location = new Point(20, 20), AutoSize = true };
            cmbMembers.Location = new Point(20, 45);
            cmbMembers.Size = new Size(450, 25);

            var lblBook = new Label { Text = "الكتاب:", Location = new Point(20, 90), AutoSize = true };
            cmbBooks.Location = new Point(20, 115);
            cmbBooks.Size = new Size(450, 25);

            btnOK = new Button { Text = "إعارة", DialogResult = DialogResult.OK, Location = new Point(200, 190), Size = new Size(100, 30) };
            btnCancel = new Button { Text = "إلغاء", DialogResult = DialogResult.Cancel, Location = new Point(310, 190), Size = new Size(100, 30) };

            btnOK.Click += (s, e) =>
            {
                string memberStr = cmbMembers.SelectedItem?.ToString() ?? "";
                string bookStr = cmbBooks.SelectedItem?.ToString() ?? "";

                if (int.TryParse(memberStr.Split('-')[0].Trim(), out int mid))
                    MemberId = mid;
                if (int.TryParse(bookStr.Split('-')[0].Trim(), out int bid))
                    BookId = bid;
            };

            this.Controls.Add(lblMember);
            this.Controls.Add(cmbMembers);
            this.Controls.Add(lblBook);
            this.Controls.Add(cmbBooks);
            this.Controls.Add(btnOK);
            this.Controls.Add(btnCancel);
        }
    }

    // ===== REPORTS FORM =====
    public partial class ReportsForm : Form
    {
        private LibraryService libraryService;
        private DataGridView dgvOverdue;

        public ReportsForm(LibraryService service)
        {
            libraryService = service;
            InitializeComponent();
            LoadData();
        }

        private void InitializeComponent()
        {
            this.AutoScaleDimensions = new SizeF(8F, 16F);
            this.AutoScaleMode = AutoScaleMode.Font;
            this.ClientSize = new Size(1000, 600);
            this.Name = "ReportsForm";
            this.Text = "التقارير والإحصائيات";
            this.StartPosition = FormStartPosition.CenterParent;

            var panelStats = new Panel();
            panelStats.Dock = DockStyle.Top;
            panelStats.Height = 150;
            panelStats.BackColor = Color.FromArgb(240, 240, 240);
            panelStats.BorderStyle = BorderStyle.FixedSingle;
            panelStats.Padding = new Padding(20);

            var lblTitle = new Label { Text = "إحصائيات المكتبة", Font = new Font("Segoe UI", 14, FontStyle.Bold), Location = new Point(20, 10), AutoSize = true };

            var lblBooks = new Label { Text = "إجمالي الكتب:", Location = new Point(20, 50), AutoSize = true };
            var lblBooksValue = new Label { Text = libraryService.GetTotalBooks().ToString(), Font = new Font("Segoe UI", 12, FontStyle.Bold), ForeColor = Color.Blue, Location = new Point(150, 50), AutoSize = true };

            var lblMembers = new Label { Text = "إجمالي الأعضاء:", Location = new Point(400, 50), AutoSize = true };
            var lblMembersValue = new Label { Text = libraryService.GetTotalMembers().ToString(), Font = new Font("Segoe UI", 12, FontStyle.Bold), ForeColor = Color.Green, Location = new Point(550, 50), AutoSize = true };

            var lblActiveLoans = new Label { Text = "الإعارات النشطة:", Location = new Point(20, 90), AutoSize = true };
            var lblActiveLoansValue = new Label { Text = libraryService.GetActiveLoans().ToString(), Font = new Font("Segoe UI", 12, FontStyle.Bold), ForeColor = Color.Orange, Location = new Point(150, 90), AutoSize = true };

            var lblAvailable = new Label { Text = "الكتب المتاحة:", Location = new Point(400, 90), AutoSize = true };
            var lblAvailableValue = new Label { Text = libraryService.GetAvailableBooks().ToString(), Font = new Font("Segoe UI", 12, FontStyle.Bold), ForeColor = Color.Green, Location = new Point(550, 90), AutoSize = true };

            panelStats.Controls.Add(lblTitle);
            panelStats.Controls.Add(lblBooks);
            panelStats.Controls.Add(lblBooksValue);
            panelStats.Controls.Add(lblMembers);
            panelStats.Controls.Add(lblMembersValue);
            panelStats.Controls.Add(lblActiveLoans);
            panelStats.Controls.Add(lblActiveLoansValue);
            panelStats.Controls.Add(lblAvailable);
            panelStats.Controls.Add(lblAvailableValue);

            var panelOverdue = new Panel();
            panelOverdue.Dock = DockStyle.Fill;

            var lblOverdueTitle = new Label { Text = "الكتب المتأخرة", Font = new Font("Segoe UI", 12, FontStyle.Bold), Dock = DockStyle.Top, Height = 30, TextAlign = ContentAlignment.MiddleLeft, Padding = new Padding(10, 0, 0, 0) };

            dgvOverdue = new DataGridView();
            dgvOverdue.Dock = DockStyle.Fill;
            dgvOverdue.AutoGenerateColumns = false;
            dgvOverdue.AllowUserToAddRows = false;
            dgvOverdue.ReadOnly = true;

            dgvOverdue.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "MemberName", HeaderText = "اسم العضو", Width = 150 });
            dgvOverdue.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "BookTitle", HeaderText = "عنوان الكتاب", Width = 300 });
            dgvOverdue.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "DueDate", HeaderText = "تاريخ الاستحقاق", Width = 150 });
            dgvOverdue.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Fine", HeaderText = "الغرامة", Width = 100 });

            panelOverdue.Controls.Add(dgvOverdue);
            panelOverdue.Controls.Add(lblOverdueTitle);

            var btnClose = new Button { Text = "إغلاق", Location = new Point(900, 560), Size = new Size(100, 30) };
            btnClose.Click += (s, e) => this.Close();

            this.Controls.Add(panelOverdue);
            this.Controls.Add(panelStats);
            this.Controls.Add(btnClose);
        }

        private void LoadData()
        {
            dgvOverdue.DataSource = libraryService.GetOverdueLoans();
        }
    }
}

// ==================== MAIN PROGRAM ====================
namespace LibraryManagement
{
    using LibraryManagement.Forms;

    static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }
    }
}
