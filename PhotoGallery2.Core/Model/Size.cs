using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace PhotoGallery2.Core
{
    [DebuggerDisplay("{Width}x{Height}")]
    [DataContract]
    public struct Size
    {
        [DataMember]
        public int Width;

        [DataMember]
        public int Height;

        public Size(int w, int h)
        {
            Width = w;
            Height = h;
        }

        public static Size Zero
        {
            get
            {
                return new Size()
                {
                    Width = 0,
                    Height = 0,
                };
            }
        }

        public override string ToString()
        {
            return $"{Width}x{Height}";
        }
    }
}
