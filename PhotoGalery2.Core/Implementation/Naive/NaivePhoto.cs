﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhotoGalery2.Core.Implementation.Naive
{
    internal class NaivePhoto : Photo
    {
        private IEnumerable<IMetadata> _metatdata;

        internal Lazy<IEnumerable<IMetadata>> LazyMetadata { get; set; }

        public override IEnumerable<IMetadata> Metatdata
        {
            get
            {
                return _metatdata ?? LazyMetadata.Value;
            }

            set
            {
                _metatdata = value;
            }
        }
    }
}
