﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace DocServiceCore.Models
{
    public class Doc
    {
        [Required()]
        public string Title { get; set; }
        public string FileName { get; set; }
        public Guid Id { get; set; }
        public DateTimeOffset Created { get; set; }
    }
}