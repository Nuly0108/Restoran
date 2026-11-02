using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using WpfApp1.Models;

namespace WpfApp1
{
    public partial class BronWindow : Window
    {
        private int _userId;
        private string _fio;
        private string _rol;
        private int _selectedTableId = 0;
        private int _currentBronId = 0;   

        public BronWindow(int userId, string fio, string rol)
        {
            InitializeComponent();

            _userId = userId;
            _fio = fio;
            _rol = rol.Trim();

            RolText.Text = "Роль сотрудника: " + _rol + " (" + _fio + ")";

            BronDate.SelectedDate = DateTime.Today;

            LoadStoly();
            LoadBronList();
        }

        public class StolItem
        {
            public int Id { get; set; }
            public int Number { get; set; }
            public string Info { get; set; } = "";
            public string Color { get; set; } = "";
        }

        public class BronRow
        {
            public int Id { get; set; }
            public string Info { get; set; } = "";
        }

        private void LoadStoly()
        {
            var list = new List<StolItem>();

            using (var db = new RestoranContext())
            {
                var stoly = db.Tables.OrderBy(t => t.Number).ToList();

                foreach (var t in stoly)
                {
                    string info = "свободен";
                    string color = "#BBDEFB";

                    var zak = db.Zakazies.FirstOrDefault(z => z.IdTables == t.Id && z.Status == "otkryt");
                    var bron = db.Bronis.FirstOrDefault(b => b.Idtables == t.Id &&
                        (b.Status == "podtverzhdena" || b.Status == "posazhen"));

                    if (zak != null)
                    {
                        info = "Заказ №" + zak.Id;
                        color = "#FFAB91";
                    }
                    else if (bron != null)
                    {
                        info = "бронь: " + bron.Fio;
                        color = "#FFE0B2";
                    }

                    StolItem s = new StolItem();
                    s.Id = t.Id;
                    s.Number = t.Number;
                    s.Info = info;
                    s.Color = color;
                    list.Add(s);
                }
            }

            StolyGrid.ItemsSource = list;
        }

        private void LoadBronList()
        {
            var rows = new List<BronRow>();

            DateTime day = DateTime.Today;
            if (BronDate.SelectedDate != null)
            {
                day = BronDate.SelectedDate.Value.Date;
            }

            using (var db = new RestoranContext())
            {
                var все = db.Bronis.ToList();

                foreach (var b in все)
                {
                 
                    if (b.Nachalo.Date != day) continue;

                    if (b.Status != "podtverzhdena" && b.Status != "posazhen") continue;
                    string stol = "?";
                    var stoly = db.Tables.ToList();
                    foreach (var t in stoly)
                    {
                        if (t.Id == b.Idtables)
                        {
                            stol = t.Number.ToString();
                            break;
                        }
                    }

                    string time = b.Nachalo.ToString("HH:mm");

                    BronRow row = new BronRow();
                    row.Id = b.Id;
                    row.Info = time + " — Стол " + stol + " — " + b.Fio + " — " + b.Phone;
                    rows.Add(row);
                }
            }

            BronList.ItemsSource = null;
            BronList.ItemsSource = rows;

            if (rows.Count == 0)
            {
                BronEmptyText.Visibility = Visibility.Visible;
            }
            else
            {
                BronEmptyText.Visibility = Visibility.Collapsed;
            }
        }

        private void BronDate_Changed(object sender, SelectionChangedEventArgs e)
        {
            LoadBronList();
        }

        private void Stol_Click(object sender, MouseButtonEventArgs e)
        {
            var border = (Border)sender;
            if (border.Tag == null) return;

            int id = (int)border.Tag;

            
            if (e.ClickCount == 2)
            {
                DeleteBron(id);
                return;
            }

            _selectedTableId = id;
            BronError.Text = "";
            Broni бронь = null;
            using (var db = new RestoranContext())
            {
                бронь = db.Bronis.FirstOrDefault(b => b.Idtables == id &&
                    (b.Status == "podtverzhdena" || b.Status == "posazhen"));
            }

            if (бронь != null)
            {
                _currentBronId = бронь.Id;

                BronName.Text = бронь.Fio;
                BronPhone.Text = бронь.Phone;
                BronTime.Text = бронь.Nachalo.ToString("HH:mm");

                FormTitle.Text = "Бронь на этот стол: " + бронь.Nachalo.ToString("dd.MM.yyyy HH:mm");
                B1.Content = "Обновить";
                BtnDeleteBron.Visibility = Visibility.Visible;
            }
            else
            {
                _currentBronId = 0;

                BronName.Text = "";
                BronPhone.Text = "";
                BronTime.Text = "19:00";

                FormTitle.Text = "Новая бронь";
                B1.Content = "Забронировать";
                BtnDeleteBron.Visibility = Visibility.Collapsed;
            }

            BronForm.Visibility = Visibility.Visible;
        }

        // ========== ЗАБРОНИРОВАТЬ / ОБНОВИТЬ ==========
        private void B1_Click(object sender, RoutedEventArgs e)
        {
            BronError.Text = "";

            if (BronName.Text.Trim() == "")
            {
                BronError.Text = "Введите имя гостя";
                return;
            }

            // Дата из календаря
            DateTime day = DateTime.Today;
            if (BronDate.SelectedDate != null)
            {
                day = BronDate.SelectedDate.Value.Date;
            }

            // Время
            TimeSpan time;
            if (!TimeSpan.TryParse(BronTime.Text.Trim(), out time))
            {
                BronError.Text = "Формат времени: 19:00";
                return;
            }

            DateTime start = day + time;

            try
            {
                using (var db = new RestoranContext())
                {
                    Broni b;

                    if (_currentBronId == 0)
                    {
                        // Новая бронь
                        b = new Broni();
                        b.Idtables = _selectedTableId;
                        b.Status = "podtverzhdena";
                        b.Komentari = "";
                        b.Idusers = _userId;
                        db.Bronis.Add(b);

                        var t = db.Tables.First(x => x.Id == _selectedTableId);
                        t.Status = "bron";
                    }
                    else
                    {
                        // Обновляем существующую
                        b = db.Bronis.First(x => x.Id == _currentBronId);
                    }

                    b.Fio = BronName.Text.Trim();
                    b.Phone = BronPhone.Text.Trim();
                    b.Nachalo = start;
                    b.End = start.AddHours(2);

                    db.SaveChanges();
                }

                BronForm.Visibility = Visibility.Collapsed;
                _selectedTableId = 0;
                _currentBronId = 0;
                LoadStoly();
                LoadBronList();
            }
            catch (Exception ex)
            {
                BronError.Text = "Ошибка: " + ex.Message;
            }
        }

        // ========== УДАЛИТЬ БРОНЬ ПО КНОПКЕ ==========
        private void DeleteBron_Click(object sender, RoutedEventArgs e)
        {
            if (_currentBronId == 0) return;

            try
            {
                using (var db = new RestoranContext())
                {
                    var бронь = db.Bronis.FirstOrDefault(x => x.Id == _currentBronId);
                    if (бронь == null) return;

                    // Освобождаем стол
                    var t = db.Tables.FirstOrDefault(x => x.Id == бронь.Idtables);
                    if (t != null) t.Status = "svoboden";

                    db.Bronis.Remove(бронь);
                    db.SaveChanges();
                }

                BronForm.Visibility = Visibility.Collapsed;
                _selectedTableId = 0;
                _currentBronId = 0;
                LoadStoly();
                LoadBronList();
            }
            catch (Exception ex)
            {
                BronError.Text = "Ошибка: " + ex.Message;
            }
        }

        // ========== УДАЛИТЬ БРОНЬ (двойной клик по столу) ==========
        private void DeleteBron(int tableId)
        {
            using (var db = new RestoranContext())
            {
                var broni = db.Bronis.Where(x => x.Idtables == tableId).ToList();
                foreach (var b in broni)
                {
                    db.Bronis.Remove(b);
                }

                var t = db.Tables.First(x => x.Id == tableId);
                t.Status = "svoboden";

                db.SaveChanges();
            }

            LoadStoly();
            LoadBronList();
        }

        // ========== КЛИК ПО СТРОКЕ СПИСКА БРОНЕЙ ==========
        private void BronRow_Click(object sender, MouseButtonEventArgs e)
        {
            Border b = (Border)sender;
            if (b.Tag == null) return;

            int id = (int)b.Tag;

            using (var db = new RestoranContext())
            {
                Broni бронь = null;
                var все = db.Bronis.ToList();
                foreach (var x in все)
                {
                    if (x.Id == id)
                    {
                        бронь = x;
                        break;
                    }
                }

                if (бронь == null) return;

                // Освобождаем стол
                var stoly = db.Tables.ToList();
                foreach (var t in stoly)
                {
                    if (t.Id == бронь.Idtables)
                    {
                        t.Status = "svoboden";
                        break;
                    }
                }

                db.Bronis.Remove(бронь);
                db.SaveChanges();
            }

            LoadStoly();
            LoadBronList();
        }

       
        private void B2_Click(object sender, RoutedEventArgs e)
        {
            BronForm.Visibility = Visibility.Collapsed;
            _selectedTableId = 0;
            _currentBronId = 0;
            BronError.Text = "";
        }

        // ========== ПЕРЕХОДЫ ==========
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            new MenuWindow(_userId, _fio, _rol).Show();
            this.Close();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e) { }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            new ZakazWindow(_userId, _fio, _rol).Show();
            this.Close();
        }

        private void Logout_Click(object sender, RoutedEventArgs e)
        {
            new MainWindow().Show();
            this.Close();
        }
    }
}