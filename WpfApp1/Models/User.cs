using System;
using System.Collections.Generic;

namespace WpfApp1.Models;

public partial class User
{
    public int Id { get; set; }

    public string Login { get; set; } = null!;

    public string Parol { get; set; } = null!;

    public string Fio { get; set; } = null!;

    public string Rol { get; set; } = null!;

    public string Phone { get; set; } = null!;

    public DateOnly DataSosdania { get; set; }

    public virtual ICollection<Broni> Bronis { get; set; } = new List<Broni>();

    public virtual ICollection<Oplaty> Oplaties { get; set; } = new List<Oplaty>();

    public virtual ICollection<Zakazy> Zakazies { get; set; } = new List<Zakazy>();
}
