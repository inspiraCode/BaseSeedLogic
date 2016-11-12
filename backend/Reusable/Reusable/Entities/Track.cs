namespace Reusable
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("Track")]
    public partial class Track : BaseEntity
    {
        [Key]
        public int TrackKey { get; set; }

        public int Entity_ID { get; set; }

        [Required]
        [StringLength(50)]
        public string Entity_Kind { get; set; }

        public int User_CreatedByKey { get; set; }

        public DateTime Date_CreatedOn { get; set; }

        public DateTime? Date_EditedOn { get; set; }

        public DateTime? Date_RemovedOn { get; set; }

        public DateTime? Date_LastTimeUsed { get; set; }

        public int? User_LastEditedByKey { get; set; }

        public int? User_RemovedByKey { get; set; }

        public int? User_AssignedToKey { get; set; }

        public int? User_AssignedByKey { get; set; }

        public virtual User User_CreatedBy { get; set; }

        public virtual User User_LastEditedBy { get; set; }

        public virtual User User_RemovedBy { get; set; }

        public virtual User User_AssignedTo { get; set; }

        public virtual User User_AssignedBy { get; set; }

        [NotMapped]
        public override int id { get { return TrackKey; } }
    }
}
