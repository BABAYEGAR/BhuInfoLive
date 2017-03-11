using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc.Html;

namespace BhuInfo.Data.Objects.Entities
{
    public class Video
    {
        public long VideoId { get; set; }
        public string Name { get; set; }
        public string Link { get; set; }
        [DisplayName("Created By")]
        public long CreatedById { get; set; }
        [DisplayName("Modified By")]
        public long LastModifiedById { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime DateLastModified { get; set; }
    }
}
