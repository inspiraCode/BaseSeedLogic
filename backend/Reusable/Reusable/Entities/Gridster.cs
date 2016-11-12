namespace Reusable
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("Gridster")]
    public partial class Gridster : BaseEntity
    {
        [Key]
        public int GridsterKey { get; set; }

        public int Gridster_Entity_ID { get; set; }

        [Required]
        [StringLength(50)]
        public string Gridster_Entity_Kind { get; set; }

        public int? Gridster_User_ID { get; set; }

        public DateTime? Gridster_Edited_On { get; set; }

        public int? Gridster_ManyToMany_ID { get; set; }

        public int cols { get; set; }
        public int rows { get; set; }
        public int y { get; set; }
        public int x { get; set; }

        public virtual User User { get; set; }

        public decimal? FontSize { get; set; }

        public bool IsShared { get; set; }

        public override int id { get { return GridsterKey; } }

        public override object Clone()
        {
            return new Gridster()
            {
                Gridster_Entity_ID = Gridster_Entity_ID,
                cols = cols,
                EF_State = EF_State,
                Gridster_Edited_On = DateTime.Now,
                Gridster_Entity_Kind = Gridster_Entity_Kind,
                Gridster_ManyToMany_ID = Gridster_ManyToMany_ID,
                Gridster_User_ID = Gridster_User_ID,
                rows = rows,
                x = x,
                y = y,
                FontSize = FontSize,
                IsShared = IsShared
            };
        }
    }
}
