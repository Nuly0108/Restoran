using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using WpfApp1.Models;

namespace WpfApp1
{
    public partial class ZakazWindow : Window
    {
        private int _userId;
        private string _fio;
        private string _rol;

        // Списки для текущего заказа
        private List<int> _ids = new List<int>();
        private List<string> _names = new List<string>();
        private List<decimal> _prices = new List<decimal>();

        // Для формы оплаты
        private int _payOrderId = 0;
        private decimal _payAmount = 0;

        public ZakazWindow(int userId, string fio, string rol)
        {
            InitializeComponent();

            _userId = userId;
            _fio = fio;
            _rol = rol.Trim();

            RolText.Text = "Роль сотрудника: " + _rol + " (" + _fio + ")";

            LoadZakazy();
            LoadCategories();
        }

        // Класс для плитки блюда
        public class BlyudoRow
        {
            public int Id { get; set; }
            public string Name { get; set; } = "";
            public string PriceText { get; set; } = "";
            public decimal Price { get; set; }
            public string Foto { get; set; } = "";
        }

        // Класс для строки заказа
        public class ZakazRow
        {
            public int Id { get; set; }
            public string Info { get; set; } = "";
        }

        // ========== СПИСОК ЗАКАЗОВ ==========
        private void LoadZakazy()
        {
            var rows = new List<ZakazRow>();

            using (var db = new RestoranContext())
            {
                var zakazy = db.Zakazies.ToList();
                zakazy.Reverse();

                foreach (var z in zakazy)
                {
                    string stol = "?";
                    var stoly = db.Tables.ToList();
                    foreach (var t in stoly)
                    {
                        if (t.Id == z.IdTables)
                        {
                            stol = t.Number.ToString();
                            break;
                        }
                    }

                    string ofitsiant = "—";
                    var users = db.Users.ToList();
                    foreach (var u in users)
                    {
                        if (u.Id == z.IdUsers)
                        {
                            ofitsiant = u.Fio;
                            break;
                        }
                    }

                    ZakazRow row = new ZakazRow();
                    row.Id = z.Id;
                    row.Info = "Заказ №" + z.Id + " | " + z.Nachalo + " | Стол " + stol + " | " + z.SummaItogo + " ₽ | " + z.Status + " | " + ofitsiant;
                    rows.Add(row);
                }
            }

            ZakazyGrid.ItemsSource = rows;
        }

        // ========== УДАЛИТЬ ЗАКАЗ ==========
        private void DeleteOrder_Click(object sender, RoutedEventArgs e)
        {
            Button btn = (Button)sender;
            if (btn.Tag == null) return;

            int id = (int)btn.Tag;

            using (var db = new RestoranContext())
            {
                var poz = db.PozitsiaZakazas.ToList();
                foreach (var p in poz)
                {
                    if (p.IdZakazy == id)
                    {
                        db.PozitsiaZakazas.Remove(p);
                    }
                }

                var opl = db.Oplaties.ToList();
                foreach (var o in opl)
                {
                    if (o.IdZakazy == id)
                    {
                        db.Oplaties.Remove(o);
                    }
                }

                Zakazy заказ = null;
                var все = db.Zakazies.ToList();
                foreach (var z in все)
                {
                    if (z.Id == id)
                    {
                        заказ = z;
                        break;
                    }
                }

                if (заказ == null) return;

                var stoly = db.Tables.ToList();
                foreach (var t in stoly)
                {
                    if (t.Id == заказ.IdTables)
                    {
                        t.Status = "svoboden";
                        break;
                    }
                }

                db.Zakazies.Remove(заказ);
                db.SaveChanges();
            }

            LoadZakazy();
        }

        // ========== ОПЛАТА ==========
        private void PayOrder_Click(object sender, RoutedEventArgs e)
        {
            Button btn = (Button)sender;
            if (btn.Tag == null) return;

            int id = (int)btn.Tag;

            using (var db = new RestoranContext())
            {
                Zakazy заказ = null;
                var все = db.Zakazies.ToList();
                foreach (var z in все)
                {
                    if (z.Id == id)
                    {
                        заказ = z;
                        break;
                    }
                }

                if (заказ == null) return;

                _payOrderId = id;
                _payAmount = заказ.SummaItogo;
            }

            PaySumText.Text = _payAmount.ToString("0") + " ₽";
            PayTips.Text = "0";
            PayMethod.SelectedIndex = 0;
            PayError.Text = "";
            PayForm.Visibility = Visibility.Visible;
        }
        private void ConfirmPay_Click(object sender, RoutedEventArgs e)
        {
            PayError.Text = "";

            decimal tips = 0;
            decimal.TryParse(PayTips.Text.Trim(), out tips);

            string method = "Наличные";
            if (PayMethod.SelectedItem != null)
            {
                ComboBoxItem item = (ComboBoxItem)PayMethod.SelectedItem;
                method = item.Content.ToString();
            }

            try
            {
                using (var db = new RestoranContext())
                {
                    Oplaty o = new Oplaty();
                    o.IdZakazy = _payOrderId;
                    o.IdUsers = _userId;
                    o.Summa = _payAmount;
                    o.Chaevye = tips;
                    o.SposobOplate = method;
                    o.DataOplat = DateTime.Now;
                    db.Oplaties.Add(o);

                    var все = db.Zakazies.ToList();
                    foreach (var z in все)
                    {
                        if (z.Id == _payOrderId)
                        {
                            z.Status = "zakryt";
                            break;
                        }
                    }

                    db.SaveChanges();
                }

                PayForm.Visibility = Visibility.Collapsed;
                _payOrderId = 0;
                _payAmount = 0;
                LoadZakazy();
            }
            catch (Exception ex)
            {
                PayError.Text = "Ошибка: " + ex.Message;
            }
        }

        private void CancelPay_Click(object sender, RoutedEventArgs e)
        {
            PayForm.Visibility = Visibility.Collapsed;
            _payOrderId = 0;
            _payAmount = 0;
            PayError.Text = "";
        }

        // ========== СТОЛЫ ==========
        private void LoadTables()
        {
            TableBox.Items.Clear();

            using (var db = new RestoranContext())
            {
                var stoly = db.Tables.ToList();

                foreach (var t in stoly)
                {
                    bool занят = false;
                    var заказы = db.Zakazies.ToList();
                    foreach (var z in заказы)
                    {
                        if (z.IdTables == t.Id && z.Status == "otkryt")
                        {
                            занят = true;
                            break;
                        }
                    }

                    if (занят) continue;

                    TableBox.Items.Add(new { Id = t.Id, Name = "Стол " + t.Number });
                }
            }

            TableBox.DisplayMemberPath = "Name";
            TableBox.SelectedValuePath = "Id";

            if (TableBox.Items.Count > 0)
            {
                TableBox.SelectedIndex = 0;
            }
        }

        // ========== КАТЕГОРИИ ==========
        private void LoadCategories()
        {
            CatBox.Items.Clear();
            CatBox.Items.Add(new { Id = 0, Name = "Все" });

            using (var db = new RestoranContext())
            {
                var cats = db.KategoriiMenus.ToList();

                foreach (var c in cats)
                {
                    CatBox.Items.Add(new { Id = c.Id, Name = c.Name });
                }
            }

            CatBox.DisplayMemberPath = "Name";
            CatBox.SelectedValuePath = "Id";

            if (CatBox.Items.Count > 0)
            {
                CatBox.SelectedIndex = 0;
            }
        }

        // ========== БЛЮДА ==========
        private void LoadBlyuda()
        {
            int catId = 0;
            if (CatBox.SelectedValue != null)
            {
                catId = (int)CatBox.SelectedValue;
            }

            var rows = new List<BlyudoRow>();

            using (var db = new RestoranContext())
            {
                var blyuda = db.Blyda.ToList();

                foreach (var b in blyuda)
                {
                    if (catId > 0 && b.IdkategoriiMenu != catId)
                    {
                        continue;
                    }

                    BlyudoRow row = new BlyudoRow();
                    row.Id = b.Id;
                    row.Name = b.Name;
                    row.PriceText = b.Prise.ToString("0") + " ₽";
                    row.Price = b.Prise;
                    row.Foto = b.Foto ?? "";
                    rows.Add(row);
                }
            }

            BlyudaList.ItemsSource = rows;
        }

        private void CatBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            LoadBlyuda();
        }

        // ========== НОВЫЙ ЗАКАЗ ==========
        private void NewOrder_Click(object sender, RoutedEventArgs e)
        {
            _ids.Clear();
            _names.Clear();
            _prices.Clear();

            GuestsBox.Text = "2";
            OrderError.Text = "";
            RefreshCart();

            LoadTables();
            LoadBlyuda();

            OrderForm.Visibility = Visibility.Visible;
            ZakazyList.Visibility = Visibility.Collapsed;
        }

        // ========== ДОБАВИТЬ БЛЮДО ==========
        private void AddToOrder_Click(object sender, RoutedEventArgs e)
        {
            Button btn = (Button)sender;
            if (btn.Tag == null) return;

            int id = (int)btn.Tag;

            List<BlyudoRow> rows = BlyudaList.ItemsSource as List<BlyudoRow>;
            if (rows == null) return;

            foreach (BlyudoRow r in rows)
            {
                if (r.Id == id)
                {
                    _ids.Add(r.Id);
                    _names.Add(r.Name);
                    _prices.Add(r.Price);
                    break;
                }
            }

            RefreshCart();
        }

        // ========== УДАЛИТЬ ПОЗИЦИЮ ==========
        private void RemoveItem_Click(object sender, RoutedEventArgs e)
        {
            Button btn = (Button)sender;

            string text = "";
            if (btn.Tag != null)
            {
                text = btn.Tag.ToString();
            }

            if (text == "") return;

            for (int i = 0; i < _names.Count; i++)
            {
                string line = _names[i] + " — " + _prices[i].ToString("0") + " ₽";

                if (line == text)
                {
                    _ids.RemoveAt(i);
                    _names.RemoveAt(i);
                    _prices.RemoveAt(i);
                    break;
                }
            }

            RefreshCart();
        }

        // ========== ОБНОВИТЬ ПАНЕЛЬ ==========
        private void RefreshCart()
        {
            var lines = new List<string>();

            for (int i = 0; i < _names.Count; i++)
            {
                string line = _names[i] + " — " + _prices[i].ToString("0") + " ₽";
                lines.Add(line);
            }

            CurrentItems.ItemsSource = null;
            CurrentItems.ItemsSource = lines;

            decimal sum = 0;
            foreach (decimal p in _prices)
            {
                sum = sum + p;
            }

            TotalText.Text = "Итого: " + sum.ToString("0") + " ₽";
        }

        
        private void CreateOrder_Click(object sender, RoutedEventArgs e)
        {
            OrderError.Text = "";

            if (TableBox.SelectedValue == null)
            {
                OrderError.Text = "Выберите стол";
                return;
            }

            if (_ids.Count == 0)
            {
                OrderError.Text = "Добавьте блюда";
                return;
            }

            int guests = 0;
            if (!int.TryParse(GuestsBox.Text.Trim(), out guests) || guests < 1)
            {
                OrderError.Text = "Введите гостей";
                return;
            }

            int tableId = (int)TableBox.SelectedValue;

            decimal sum = 0;
            foreach (decimal p in _prices)
            {
                sum = sum + p;
            }

            try
            {
                using (var db = new RestoranContext())
                {
                    Zakazy z = new Zakazy();
                    z.IdTables = tableId;
                    z.IdUsers = _userId;
                    z.KolVo = guests;
                    z.Nachalo = DateTime.Now;
                    z.Status = "otkryt";
                    z.SummaItogo = sum;
                    db.Zakazies.Add(z);
                    db.SaveChanges();

                    for (int i = 0; i < _ids.Count; i++)
                    {
                        PozitsiaZakaza p = new PozitsiaZakaza();
                        p.IdZakazy = z.Id;
                        p.IdBlyda = _ids[i];
                        p.Kolvo = 1;
                        p.Prise = _prices[i];
                        p.Status = "otpravleno";
                        db.PozitsiaZakazas.Add(p);
                    }
                    db.SaveChanges();
                }

                CancelOrder_Click(null, null);
                LoadZakazy();
            }
            catch (Exception ex)
            {
                OrderError.Text = "Ошибка: " + ex.Message;
            }
        }

        // ========== ОТМЕНА ==========
        private void CancelOrder_Click(object sender, RoutedEventArgs e)
        {
            _ids.Clear();
            _names.Clear();
            _prices.Clear();

            OrderError.Text = "";
            OrderForm.Visibility = Visibility.Collapsed;
            ZakazylistVisibilityRestore();
        }

        private void ZakazylistVisibilityRestore()
        {
            ZakazyList.Visibility = Visibility.Visible;
        }

        private void GoMenu_Click(object sender, RoutedEventArgs e)
        {
            MenuWindow окно = new MenuWindow(_userId, _fio, _rol);
            окно.Show();
            this.Close();
        }

        private void GoBron_Click(object sender, RoutedEventArgs e)
        {
            BronWindow окно = new BronWindow(_userId, _fio, _rol);
            окно.Show();
            this.Close();
        }

        private void GoZakaz_Click(object sender, RoutedEventArgs e) { }

        private void Logout_Click(object sender, RoutedEventArgs e)
        {
            MainWindow окно = new MainWindow();
            окно.Show();
            this.Close();
        }
    }
}