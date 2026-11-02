using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Win32;
using WpfApp1.Models;

namespace WpfApp1
{
    public partial class MenuWindow : Window
    {
        private int _userId;
        private string _fio;
        private string _rol;
        private int _editId = 0;
        private string _fotoPath = "";

        public MenuWindow(int userId, string fio, string rol)
        {
            InitializeComponent();

            _userId = userId;
            _fio = fio;
            _rol = rol.Trim();

            RolText.Text = "Роль сотрудника: " + _rol + " (" + _fio + ")";

            if (_rol != "admin")
            {
                BtnEdit.Visibility = Visibility.Collapsed;
            }

            LoadBlyuda(0);
        }

        public class BlyudoItem
        {
            public int Id { get; set; }
            public string Name { get; set; } = "";
            public string PriceText { get; set; } = "";
            public string Ves { get; set; } = "";
            public string Opisanie { get; set; } = "";
            public string Foto { get; set; } = "";
            public bool CanEdit { get; set; }
        }

        private void GoMenu_Click(object sender, RoutedEventArgs e)
        {
        }

        private void GoBron_Click(object sender, RoutedEventArgs e)
        {
            new BronWindow(_userId, _fio, _rol).Show();
            this.Close();
        }

        private void GoZakaz_Click(object sender, RoutedEventArgs e)
        {
            new ZakazWindow(_userId, _fio, _rol).Show();
            this.Close();
        }

        private void Logout_Click(object sender, RoutedEventArgs e)
        {
            new MainWindow().Show();
            this.Close();
        }

        // Загрузка блюд
        private void LoadBlyuda(int catId)
        {
            var list = new List<BlyudoItem>();

            using (var db = new RestoranContext())
            {
                var blyuda = db.Blyda.OrderBy(b => b.Name).ToList();

                foreach (var b in blyuda)
                {
                    if (catId > 0 && b.IdkategoriiMenu != catId)
                    {
                        continue;
                    }

                    BlyudoItem item = new BlyudoItem();
                    item.Id = b.Id;
                    item.Name = b.Name ?? "";
                    item.PriceText = b.Prise.ToString("0") + " ₽";
                    item.Ves = b.Ves ?? "";
                    item.Opisanie = b.Opisanie ?? "";
                    item.Foto = b.Foto ?? "";
                    item.CanEdit = (_rol == "admin");

                    list.Add(item);
                }
            }

            BlyudaGrid.ItemsSource = list;
        }

        // Клик по категории
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var btn = (Button)sender;
            int catId = 0;

            if (btn.Tag != null)
            {
                int.TryParse(btn.Tag.ToString(), out catId);
            }

            LoadBlyuda(catId);
        }

        private void BtnEdit_Click(object sender, RoutedEventArgs e)
        {
            if (_rol != "admin")
            {
                return;
            }

            _editId = 0;
            _fotoPath = "";
            FotoLabel.Text = "не выбрана";

            ClearForm();
            LoadCategoriesToCombo();

            if (Form.Visibility == Visibility.Collapsed)
            {
                Form.Visibility = Visibility.Visible;
            }
            else
            {
                Form.Visibility = Visibility.Collapsed;
            }
        }

        private void Blyudo_DoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (_rol != "admin")
            {
                return;
            }

            var border = (Border)sender;
            if (border.Tag == null)
            {
                return;
            }

            int id = (int)border.Tag;
            LoadBlyudoToForm(id);
        }

        private void LoadBlyudoToForm(int id)
        {
            using (var db = new RestoranContext())
            {
                var b = db.Blyda.FirstOrDefault(x => x.Id == id);
                if (b == null)
                {
                    return;
                }

                _editId = b.Id;
                NewName.Text = b.Name ?? "";
                NewOpisanie.Text = b.Opisanie ?? "";
                NewPrice.Text = b.Prise.ToString("0");
                NewVes.Text = b.Ves ?? "";

                LoadCategoriesToCombo();
                NewCat.SelectedValue = b.IdkategoriiMenu;

                _fotoPath = b.Foto ?? "";

                if (_fotoPath == "")
                {
                    FotoLabel.Text = "не выбрана";
                }
                else
                {
                    FotoLabel.Text = System.IO.Path.GetFileName(_fotoPath);
                }
            }

            FormError.Text = "";
            Form.Visibility = Visibility.Visible;
        }

        private void LoadCategoriesToCombo()
        {
            NewCat.Items.Clear();

            using (var db = new RestoranContext())
            {
                var cats = db.KategoriiMenus.OrderBy(c => c.Id).ToList();

                foreach (var c in cats)
                {
                    NewCat.Items.Add(new { Id = c.Id, Name = c.Name });
                }
            }

            NewCat.DisplayMemberPath = "Name";
            NewCat.SelectedValuePath = "Id";

            if (NewCat.Items.Count > 0)
            {
                NewCat.SelectedIndex = 0;
            }
        }

        private void PickFoto_Click(object sender, RoutedEventArgs e)
        {
            var a1 = new OpenFileDialog();
            a1.Filter = "Картинки|*.jpg;*.jpeg;*.png;*.bmp";
            a1.Title = "Выберите картинку блюда";

            if (a1.ShowDialog() != true)
            {
                return;
            }

            try
            {
                string imagesDir = System.IO.Path.Combine(
                    AppDomain.CurrentDomain.BaseDirectory, "Images");

                System.IO.Directory.CreateDirectory(imagesDir);

                string fileName = System.IO.Path.GetFileName(a1.FileName);
                string destPath = System.IO.Path.Combine(imagesDir, fileName);

                System.IO.File.Copy(a1.FileName, destPath, true);

                _fotoPath = "/Images/" + fileName;
                FotoLabel.Text = fileName;
            }
            catch (Exception ex)
            {
                FormError.Text = "Ошибка загрузки картинки: " + ex.Message;
            }
        }

        // Cохранить
        private void SaveNew_Click(object sender, RoutedEventArgs e)
        {
            if (_rol != "admin")
            {
                return;
            }

            FormError.Text = "";

            if (NewName.Text.Trim() == "")
            {
                FormError.Text = "Введите название";
                return;
            }

            if (NewCat.SelectedValue == null)
            {
                FormError.Text = "Выберите категорию";
                return;
            }

            using (var db = new RestoranContext())
            {
                Blydum b;

                if (_editId == 0)
                {
                    b = new Blydum();
                    db.Blyda.Add(b);
                }
                else
                {
                    b = db.Blyda.First(x => x.Id == _editId);
                }

                b.Name = NewName.Text.Trim();
                b.Opisanie = NewOpisanie.Text.Trim();
                b.Prise = decimal.Parse(NewPrice.Text.Trim());
                b.Ves = NewVes.Text.Trim();
                b.IdkategoriiMenu = (int)NewCat.SelectedValue;
                b.Foto = _fotoPath;

                db.SaveChanges();
            }

            _editId = 0;
            ClearForm();
            Form.Visibility = Visibility.Collapsed;
            LoadBlyuda(0);
        }

        //отмена
        private void CancelNew_Click(object sender, RoutedEventArgs e)
        {
            _editId = 0;
            ClearForm();
            Form.Visibility = Visibility.Collapsed;
        }

        private void DeleteBlyudo_Click(object sender, RoutedEventArgs e)
        {
            if (_rol != "admin")
            {
                return;
            }

            var btn = (Button)sender;
            if (btn.Tag == null)
            {
                return;
            }

            int id = (int)btn.Tag;

            using (var db = new RestoranContext())
            {
                var b = db.Blyda.FirstOrDefault(x => x.Id == id);

                if (b != null)
                {
                    db.Blyda.Remove(b);
                    db.SaveChanges();
                }
            }

            LoadBlyuda(0);
        }

        private void ClearForm()
        {
            NewName.Text = "";
            NewOpisanie.Text = "";
            NewPrice.Text = "";
            NewVes.Text = "";
            FormError.Text = "";

            _fotoPath = "";
            FotoLabel.Text = "не выбрана";
        }
    }
}