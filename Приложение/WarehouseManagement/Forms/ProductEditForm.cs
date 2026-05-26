using WarehouseManagement.Models;
using WarehouseManagement.Repositories;
using WarehouseManagement.Controls;

namespace WarehouseManagement.Forms;

public partial class ProductEditForm : Form
{
    private readonly ProductRepository _productRepo = new();
    private readonly CategoryRepository _categoryRepo = new();
    private readonly SupplierRepository _supplierRepo = new();
    private Product? _product;
    private bool _isNewProduct = false;

    public ProductEditForm(Product? product = null)
    {
        InitializeComponent();
        _product = product;
        _isNewProduct = product == null;
        SetupUI();
        if (product != null)
        {
            LoadProductData(product);
        }
    }

    private void InitializeComponent()
    {
        this.SuspendLayout();
        this.AutoScaleDimensions = new SizeF(7F, 15F);
        this.AutoScaleMode = AutoScaleMode.Font;
        this.ResumeLayout(false);
    }

    private void SetupUI()
    {
        this.Text = _isNewProduct ? "Добавить товар" : "Редактировать товар";
        this.Size = new Size(500, 700);
        this.StartPosition = FormStartPosition.CenterParent;
        this.FormBorderStyle = FormBorderStyle.FixedDialog;
        this.MaximizeBox = false;
        this.MinimizeBox = false;
        this.BackColor = Color.FromArgb(15, 23, 42);

        var mainPanel = new Panel { Dock = DockStyle.Fill, BackColor = Color.FromArgb(15, 23, 42), Padding = new Padding(20), AutoScroll = true };

        var titleLabel = new Label
        {
            Text = _isNewProduct ? "📦 Новый товар" : "✏️ Редактирование товара",
            Font = new Font("Segoe UI", 16, FontStyle.Bold),
            ForeColor = Color.FromArgb(59, 130, 246),
            AutoSize = true,
            Location = new Point(0, 0)
        };

        int y = 40;

        var articleLabel = new Label { Text = "Артикул:", Font = new Font("Segoe UI", 10, FontStyle.Bold), ForeColor = Color.White, AutoSize = true, Location = new Point(0, y) };
        var articleTextBox = new RoundedTextBox { Name = "ArticleTextBox", Location = new Point(0, y + 25), Size = new Size(460, 40), Font = new Font("Segoe UI", 11), BackColor = Color.FromArgb(30, 41, 59), ForeColor = Color.White, BorderRadius = 8 };
        y += 75;

        var nameLabel = new Label { Text = "Название:", Font = new Font("Segoe UI", 10, FontStyle.Bold), ForeColor = Color.White, AutoSize = true, Location = new Point(0, y) };
        var nameTextBox = new RoundedTextBox { Name = "NameTextBox", Location = new Point(0, y + 25), Size = new Size(460, 40), Font = new Font("Segoe UI", 11), BackColor = Color.FromArgb(30, 41, 59), ForeColor = Color.White, BorderRadius = 8 };
        y += 75;

        var categoryLabel = new Label { Text = "Категория:", Font = new Font("Segoe UI", 10, FontStyle.Bold), ForeColor = Color.White, AutoSize = true, Location = new Point(0, y) };
        var categoryCombo = new RoundedComboBox { Name = "CategoryCombo", Location = new Point(0, y + 25), Size = new Size(460, 40), Font = new Font("Segoe UI", 11), BackColor = Color.FromArgb(30, 41, 59), ForeColor = Color.White, BorderRadius = 8 };
        var categories = _categoryRepo.GetAll();
        foreach (var cat in categories)
        {
            categoryCombo.Items.Add(new { cat.Id, cat.Name });
        }
        categoryCombo.DisplayMember = "Name";
        y += 75;

        var priceLabel = new Label { Text = "Цена (руб):", Font = new Font("Segoe UI", 10, FontStyle.Bold), ForeColor = Color.White, AutoSize = true, Location = new Point(0, y) };
        var priceTextBox = new RoundedTextBox { Name = "PriceTextBox", Location = new Point(0, y + 25), Size = new Size(460, 40), Font = new Font("Segoe UI", 11), BackColor = Color.FromArgb(30, 41, 59), ForeColor = Color.White, BorderRadius = 8 };
        y += 75;

        var quantityLabel = new Label { Text = "Количество (шт):", Font = new Font("Segoe UI", 10, FontStyle.Bold), ForeColor = Color.White, AutoSize = true, Location = new Point(0, y) };
        var quantityTextBox = new RoundedTextBox { Name = "QuantityTextBox", Location = new Point(0, y + 25), Size = new Size(460, 40), Font = new Font("Segoe UI", 11), BackColor = Color.FromArgb(30, 41, 59), ForeColor = Color.White, BorderRadius = 8 };
        y += 75;

        var minQuantityLabel = new Label { Text = "Минимальное кол-во:", Font = new Font("Segoe UI", 10, FontStyle.Bold), ForeColor = Color.White, AutoSize = true, Location = new Point(0, y) };
        var minQuantityTextBox = new RoundedTextBox { Name = "MinQuantityTextBox", Location = new Point(0, y + 25), Size = new Size(460, 40), Font = new Font("Segoe UI", 11), BackColor = Color.FromArgb(30, 41, 59), ForeColor = Color.White, BorderRadius = 8 };
        y += 75;

        var saveButton = new RoundedButton
        {
            Text = "✓ Сохранить",
            Location = new Point(0, y),
            Size = new Size(220, 45),
            Font = new Font("Segoe UI", 11, FontStyle.Bold),
            BackColor = Color.FromArgb(59, 130, 246),
            ForeColor = Color.White,
            Cursor = Cursors.Hand,
            BorderRadius = 8
        };
        saveButton.MouseEnter += (s, e) => saveButton.BackColor = Color.FromArgb(37, 99, 235);
        saveButton.MouseLeave += (s, e) => saveButton.BackColor = Color.FromArgb(59, 130, 246);
        saveButton.Click += (s, e) => SaveProduct(articleTextBox, nameTextBox, categoryCombo, priceTextBox, quantityTextBox, minQuantityTextBox);

        var cancelButton = new RoundedButton
        {
            Text = "✕ Отмена",
            Location = new Point(240, y),
            Size = new Size(220, 45),
            Font = new Font("Segoe UI", 11, FontStyle.Bold),
            BackColor = Color.FromArgb(100, 116, 139),
            ForeColor = Color.White,
            Cursor = Cursors.Hand,
            BorderRadius = 8
        };
        cancelButton.MouseEnter += (s, e) => cancelButton.BackColor = Color.FromArgb(120, 136, 159);
        cancelButton.MouseLeave += (s, e) => cancelButton.BackColor = Color.FromArgb(100, 116, 139);
        cancelButton.Click += (s, e) => { this.DialogResult = DialogResult.Cancel; this.Close(); };

        mainPanel.Controls.Add(titleLabel);
        mainPanel.Controls.Add(articleLabel);
        mainPanel.Controls.Add(articleTextBox);
        mainPanel.Controls.Add(nameLabel);
        mainPanel.Controls.Add(nameTextBox);
        mainPanel.Controls.Add(categoryLabel);
        mainPanel.Controls.Add(categoryCombo);
        mainPanel.Controls.Add(priceLabel);
        mainPanel.Controls.Add(priceTextBox);
        mainPanel.Controls.Add(quantityLabel);
        mainPanel.Controls.Add(quantityTextBox);
        mainPanel.Controls.Add(minQuantityLabel);
        mainPanel.Controls.Add(minQuantityTextBox);
        mainPanel.Controls.Add(saveButton);
        mainPanel.Controls.Add(cancelButton);

        this.Controls.Add(mainPanel);
    }

    private void LoadProductData(Product product)
    {
        var articleBox = this.Controls.Find("ArticleTextBox", true).FirstOrDefault() as TextBox;
        var nameBox = this.Controls.Find("NameTextBox", true).FirstOrDefault() as TextBox;
        var categoryCombo = this.Controls.Find("CategoryCombo", true).FirstOrDefault() as ComboBox;
        var priceBox = this.Controls.Find("PriceTextBox", true).FirstOrDefault() as TextBox;
        var quantityBox = this.Controls.Find("QuantityTextBox", true).FirstOrDefault() as TextBox;

        if (articleBox != null) articleBox.Text = product.Article;
        if (nameBox != null) nameBox.Text = product.Name;
        if (priceBox != null) priceBox.Text = product.Price.ToString("F2");
        if (quantityBox != null) quantityBox.Text = product.Quantity.ToString();
        if (categoryCombo != null)
        {
            for (int i = 0; i < categoryCombo.Items.Count; i++)
            {
                dynamic item = categoryCombo.Items[i];
                if (item.Id == product.CategoryId)
                {
                    categoryCombo.SelectedIndex = i;
                    break;
                }
            }
        }
    }

    private void SaveProduct(TextBox articleBox, TextBox nameBox, ComboBox categoryCombo, TextBox priceBox, TextBox quantityBox, TextBox minQuantityBox)
    {
        if (string.IsNullOrWhiteSpace(articleBox.Text))
        {
            MessageBox.Show("⚠️ Заполните артикул", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            articleBox.Focus();
            return;
        }

        if (string.IsNullOrWhiteSpace(nameBox.Text))
        {
            MessageBox.Show("⚠️ Заполните название товара", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            nameBox.Focus();
            return;
        }

        if (categoryCombo.SelectedIndex < 0)
        {
            MessageBox.Show("⚠️ Выберите категорию", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            categoryCombo.Focus();
            return;
        }

        if (!decimal.TryParse(priceBox.Text, out var price) || price < 0)
        {
            MessageBox.Show("⚠️ Введите корректную цену (число >= 0)", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            priceBox.Focus();
            return;
        }

        if (!int.TryParse(quantityBox.Text, out var quantity) || quantity < 0)
        {
            MessageBox.Show("⚠️ Введите корректное количество (целое число >= 0)", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            quantityBox.Focus();
            return;
        }

        if (!int.TryParse(minQuantityBox.Text, out var minQuantity) || minQuantity < 0)
        {
            MessageBox.Show("⚠️ Введите корректное минимальное количество (целое число >= 0)", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            minQuantityBox.Focus();
            return;
        }

        dynamic selectedCategory = categoryCombo.SelectedItem;
        int categoryId = selectedCategory.Id;

        try
        {
            if (_isNewProduct)
            {
                var newProduct = new Product
                {
                    Article = articleBox.Text.Trim(),
                    Name = nameBox.Text.Trim(),
                    CategoryId = categoryId,
                    Price = price,
                    Quantity = quantity,
                    MinQuantity = minQuantity,
                    IsActive = true
                };
                _productRepo.Insert(newProduct);
                MessageBox.Show("✓ Товар успешно добавлен", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else if (_product != null)
            {
                _product.Article = articleBox.Text.Trim();
                _product.Name = nameBox.Text.Trim();
                _product.CategoryId = categoryId;
                _product.Price = price;
                _product.Quantity = quantity;
                _product.MinQuantity = minQuantity;
                _productRepo.Update(_product);
                MessageBox.Show("✓ Товар успешно обновлен", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }

            this.DialogResult = DialogResult.OK;
            this.Close();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"✗ Ошибка при сохранении: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
