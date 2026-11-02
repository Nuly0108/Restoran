using System;
using System.Collections.Generic;

namespace WpfApp1.Models;

public partial class Oplaty
{
    public int Id { get; set; }

    public int IdZakazy { get; set; }

    public decimal Summa { get; set; }

    public decimal Chaevye { get; set; }

    public string SposobOplate { get; set; } = null!;

    public DateTime DataOplat { get; set; }

    public int? IdUsers { get; set; }

    public virtual User? IdUsersNavigation { get; set; }

    public virtual Zakazy IdZakazyNavigation { get; set; } = null!;
}
