using System;
using System.Collections.Generic;

namespace WpfApp1.Models;

public partial class Zakazy
{
    public int Id { get; set; }

    public int IdTables { get; set; }

    public int IdUsers { get; set; }

    public int KolVo { get; set; }

    public DateTime Nachalo { get; set; }

    public DateTime End { get; set; }

    public string Status { get; set; } = null!;

    public decimal SummaItogo { get; set; }

    public virtual Table IdTablesNavigation { get; set; } = null!;

    public virtual User IdUsersNavigation { get; set; } = null!;

    public virtual ICollection<Oplaty> Oplaties { get; set; } = new List<Oplaty>();

    public virtual ICollection<PozitsiaZakaza> PozitsiaZakazas { get; set; } = new List<PozitsiaZakaza>();
}
