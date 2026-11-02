using System;
using System.Collections.Generic;

namespace WpfApp1.Models;

public partial class Blydum
{
    public int Id { get; set; }

    public int IdkategoriiMenu { get; set; }

    public string Name { get; set; } = null!;

    public string Opisanie { get; set; } = null!;

    public decimal Prise { get; set; }

    public string? Ves { get; set; }

    public string? Foto { get; set; }

    public virtual KategoriiMenu IdkategoriiMenuNavigation { get; set; } = null!;

    public virtual ICollection<PozitsiaZakaza> PozitsiaZakazas { get; set; } = new List<PozitsiaZakaza>();
}
