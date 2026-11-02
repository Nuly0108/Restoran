using System;
using System.Collections.Generic;

namespace WpfApp1.Models;

public partial class Broni
{
    public int Id { get; set; }

    public int Idtables { get; set; }

    public string? Fio { get; set; }

    public string? Phone { get; set; }

    public DateTime Nachalo { get; set; }

    public DateTime End { get; set; }

    public string? Status { get; set; }

    public string? Komentari { get; set; }

    public int Idusers { get; set; }

    public virtual Table IdtablesNavigation { get; set; } = null!;

    public virtual User IdusersNavigation { get; set; } = null!;
}
