using System;
using System.Collections.Generic;

namespace WpfApp1.Models;

public partial class KategoriiMenu
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public virtual ICollection<Blydum> Blyda { get; set; } = new List<Blydum>();
}
