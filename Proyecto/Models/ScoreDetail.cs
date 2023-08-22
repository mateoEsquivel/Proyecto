using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Proyecto.Models
{
    public class ScoreDetail
    {
        [Key]
        [Column(Order = 0)]
        public int IdScore { get; set; }

        [Key]
        [Column(Order = 1)]
        public int IdDetail { get; set; }

        [Display(Name = "Rúbro")]
        public string Name { get; set; }

        [Display(Name = "Nota")]
        public double Score { get; set; }

        [Display(Name = "Porcentaje")]
        public int Percentage { get; set; }
    }
}