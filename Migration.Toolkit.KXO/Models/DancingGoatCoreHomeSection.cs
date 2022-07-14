﻿using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Migration.Toolkit.KXO.Models
{
    [Table("DancingGoatCore_HomeSection")]
    public partial class DancingGoatCoreHomeSection
    {
        [Key]
        [Column("HomeSectionID")]
        public int HomeSectionId { get; set; }
        [StringLength(200)]
        public string HomeSectionHeading { get; set; } = null!;
        public string HomeSectionText { get; set; } = null!;
        [StringLength(200)]
        public string? HomeSectionLinkText { get; set; }
    }
}
