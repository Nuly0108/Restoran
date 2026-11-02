using System;
using System.Collections.Generic;

namespace WpfApp1.Models;

public partial class PozitsiaZakaza
{
    public int Id { get; set; }

    public int IdZakazy { get; set; }

    public int IdBlyda { get; set; }

    public int Kolvo { get; set; }

    public decimal Prise { get; set; }

    public string? Status { get; set; }

    public string? Kommentari { get; set; }

    public virtual Blydum IdBlydaNavigation { get; set; } = null!;

    public virtual Zakazy IdZakazyNavigation { get; set; } = null!;
}
