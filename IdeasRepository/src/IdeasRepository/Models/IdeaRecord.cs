using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Net;
using System.Text.RegularExpressions;

namespace IdeasRepository.Models
{
    public class IdeaRecord
    {
        public string ID { get; set; }

        [Required]
        [StringLength(150)]
        public string Title { get; set; }

        [DataType(DataType.Html)]
        [Required]
        public string Text { get; set; }

        public DateTime Date { get; set; }

        [ForeignKey("Author")]
        public string AuthorId { get; set; }

        public virtual ApplicationUser Author { get; set; }

        public DateTime? EditeDate { get; set; }

        [ForeignKey("Editor")]
        public string EditorId { get; set; }

        public virtual ApplicationUser Editor { get; set; }

        public DateTime StatusDate { get; set; }

        public IdeaRecordStatusEnum Status { get; set; }

        [StringLength(30)]
        public string Prewiew { get; set; }

        public static string GetPrewiew(string html)
        {
            var prewiew = WebUtility.HtmlDecode(Regex.Replace(html, "<[^>]*(>|$)", string.Empty)).Trim();
            if (prewiew.Length > 30)
            {
                prewiew = prewiew.Substring(0, 27) + "...";
            }
            return prewiew;
        }
    }
}
