using System;
using System.Collections.Generic;

namespace WpfApp1.Models;

public partial class Table
{
    public int Id { get; set; }

    public int Number { get; set; }

    public int Vmestimost { get; set; }

    public string Status { get; set; } = null!;

    public virtual ICollection<Broni> Bronis { get; set; } = new List<Broni>();

    public virtual ICollection<Zakazy> Zakazies { get; set; } = new List<Zakazy>();
}
